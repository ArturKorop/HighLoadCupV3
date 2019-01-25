using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HighLoadCupV3.Model.Dto;

namespace HighLoadCupV3.Model.InMemory
{
    public class MmfLikesStorage : ILikesStorage
    {
        private const string FromFileName = "from.txt";
        private const string ToFileName = "to.txt";

        private StreamWriter _fromFileWriter;
        private StreamWriter _toFileWriter;

        private List<LikeDto>[] _toBuffer;

        private int _maxToId = 0;

        private readonly int[,] _fromFilePositions;
        private readonly int[,] _toFilePositions;

        private int _lastFromPosition;
        private int _lastToPosition;

        private readonly Dictionary<int, List<LikeDto>> _postTo = new Dictionary<int, List<LikeDto>>();
        private readonly Dictionary<int, List<int>> _postFrom = new Dictionary<int, List<int>>();

        private MemoryMappedFile _mmfTo;
        private MemoryMappedFile _mmfFrom;

        private MemoryMappedViewAccessor _mmfVsTo;
        private MemoryMappedViewAccessor _mmfVsFrom;

        public MmfLikesStorage(int accountsCount)
        {
            _fromFileWriter = new StreamWriter(File.Create(FromFileName, 1024 * 1024, FileOptions.WriteThrough));
            _toBuffer = new List<LikeDto>[accountsCount];

            _toFilePositions = new int[accountsCount, 2];
            _fromFilePositions = new int[accountsCount, 2];
        }

        private void WriterLikesFrom(int id, IEnumerable<int> likes)
        {
            var data = string.Join(',', likes);

            _fromFileWriter.Write(data);
            _fromFilePositions[id, 0] = _lastFromPosition;
            _fromFilePositions[id, 1] = data.Length;

            _lastFromPosition += data.Length;
        }

        private void WriteLikesTo(int id, IEnumerable<LikeDto> likes)
        {
            var data = string.Join(',', likes.Select(x => $"{x.Id}-{x.TimeStamp}"));
            _toFileWriter.Write(data);
            _toFilePositions[id, 0] = _lastToPosition;
            _toFilePositions[id, 1] = data.Length;
            _lastToPosition += data.Length;
        }

        public void AddToBuffer(int id, LikeDto[] likes)
        {
            if (likes == null || likes.Length == 0) return;

            _postFrom[id] = likes.Select(x => x.Id).ToList();
            foreach (var like in likes)
            {
                if (!_postTo.ContainsKey(like.Id))
                {
                    _postTo[like.Id] = new List<LikeDto> { new LikeDto { Id = id, TimeStamp = like.TimeStamp } };
                }
                else
                {
                    _postTo[like.Id].Add(new LikeDto { Id = id, TimeStamp = like.TimeStamp });
                }
            }
        }

        public void UpdateBuffer(LikeUpdateDto dto)
        {
            if (!_postFrom.ContainsKey(dto.Liker))
            {
                _postFrom[dto.Liker] = new List<int> { dto.Likee };
            }
            else
            {
                _postFrom[dto.Liker].Add(dto.Likee);
            }

            if (!_postTo.ContainsKey(dto.Likee))
            {
                _postTo[dto.Likee] = new List<LikeDto> { new LikeDto { Id = dto.Liker, TimeStamp = dto.TimeStamp } };
            }
            else
            {
                _postTo[dto.Likee].Add(new LikeDto { Id = dto.Liker, TimeStamp = dto.TimeStamp });
            }
        }

        public void UpdateBuffer(LikeDto dto, int id)
        {
            if (!_postFrom.ContainsKey(id))
            {
                _postFrom[id] = new List<int> { dto.Id };
            }
            else
            {
                _postFrom[id].Add(dto.Id);
            }

            if (!_postTo.ContainsKey(dto.Id))
            {
                _postTo[dto.Id] = new List<LikeDto> { new LikeDto { Id = id, TimeStamp = dto.TimeStamp } };
            }
            else
            {
                _postTo[dto.Id].Add(new LikeDto { Id = id, TimeStamp = dto.TimeStamp });
            }
        }


        public void AddFromFile(int id, LikeDto[] likes)
        {
            if (likes == null || likes.Length == 0) return;

            WriterLikesFrom(id, likes.Select(x => x.Id));

            foreach (var like in likes)
            {
                if (_toBuffer[like.Id] == null)
                {
                    _toBuffer[like.Id] = new List<LikeDto> { new LikeDto { Id = id, TimeStamp = like.TimeStamp } };
                    _maxToId = Math.Max(_maxToId, like.Id);
                }
                else
                {
                    _toBuffer[like.Id].Add(new LikeDto { Id = id, TimeStamp = like.TimeStamp });
                }
            }
        }

        public void Flush()
        {
            _fromFileWriter.Flush();
            _fromFileWriter.Dispose();
            _fromFileWriter = null;
        }

        public void FillReverse(Stopwatch sw)
        {
            const int notificationCount = 100000;

            using (_toFileWriter = new StreamWriter(File.Create(ToFileName, 1024 * 1024, FileOptions.WriteThrough)))
            {
                for (int i = 1; i <= _maxToId; i++)
                {
                    if (_toBuffer[i] != null)
                    {
                        WriteLikesTo(i, _toBuffer[i]);
                    }

                    if (i % notificationCount == 0)
                    {
                        Console.WriteLine($"Reverse wrote {i} in {sw.ElapsedMilliseconds / 1000} seconds");
                    }

                }
            }

            _toBuffer = null;

            _mmfFrom = MemoryMappedFile.CreateFromFile(FromFileName, FileMode.Open);
            _mmfTo = MemoryMappedFile.CreateFromFile(ToFileName, FileMode.Open);

            _mmfVsFrom = _mmfFrom.CreateViewAccessor();
            _mmfVsTo = _mmfTo.CreateViewAccessor();
        }

        public async Task<List<int>> GetFromAsync(int id)
        {
            var result = new List<int>();
            if (_fromFilePositions[id, 1] != 0)
            {
                var count = _fromFilePositions[id, 1];
                var buffer = new byte[count];
                _mmfVsFrom.ReadArray(_fromFilePositions[id, 0], buffer, 0, count);
                var stringValue = Encoding.Default.GetString(buffer);
                var lines = stringValue.Split(',');
                result = lines.Select(int.Parse).ToList();
            }

            if (_postFrom.ContainsKey(id))
            {
                result.AddRange(_postFrom[id]);
            }

            return result.Count > 0 ? result : null;
        }

        public async Task<List<List<int>>> GetFromAsync(IEnumerable<int> ids)
        {
            var result = new List<List<int>>();

            foreach (var id in ids)
            {
                var current = new List<int>();
                if (_fromFilePositions[id, 1] != 0)
                {
                    var count = _fromFilePositions[id, 1];
                    var buffer = new byte[count];
                    _mmfVsFrom.ReadArray(_fromFilePositions[id,0], buffer, 0, count);
                    var stringValue = Encoding.Default.GetString(buffer);
                    var lines = stringValue.Split(',');
                    current = lines.Select(int.Parse).ToList();
                }

                if (_postFrom.ContainsKey(id))
                {
                    current.AddRange(_postFrom[id]);
                }

                result.Add(current);
            }

            return result;
        }

        public IEnumerable<List<int>> GetFrom(IEnumerable<int> ids)
        {
            foreach (var id in ids)
            {
                var current = new List<int>();
                if (_fromFilePositions[id, 1] != 0)
                {
                    var count = _fromFilePositions[id, 1];
                    var buffer = new byte[count];
                    _mmfVsFrom.ReadArray(_fromFilePositions[id, 0], buffer, 0, count);
                    var stringValue = Encoding.Default.GetString(buffer);
                    var lines = stringValue.Split(',');
                    current = lines.Select(int.Parse).ToList();
                }

                if (_postFrom.ContainsKey(id))
                {
                    current.AddRange(_postFrom[id]);
                }

                yield return current;
            }
        }

        public List<int> GetFrom(int id)
        {
            var result = new List<int>();
            if (_fromFilePositions[id, 1] != 0)
            {
                var count = _fromFilePositions[id, 1];
                var buffer = new byte[count];
                _mmfVsFrom.ReadArray(_fromFilePositions[id, 0], buffer, 0, count);
                var stringValue = Encoding.Default.GetString(buffer);
                var lines = stringValue.Split(',');
                result = lines.Select(int.Parse).ToList();
            }

            if (_postFrom.ContainsKey(id))
            {
                result.AddRange(_postFrom[id]);
            }

            return result.Count > 0 ? result : null;
        }

        public async Task<List<LikeDto>> GetToAsync(int id)
        {
            var result = new List<LikeDto>();

            if (_toFilePositions[id, 1] != 0)
            {
                var count = _toFilePositions[id, 1];
                var buffer = new byte[count];
                _mmfVsTo.ReadArray(_toFilePositions[id, 0], buffer, 0, count);
                var stringValue = Encoding.Default.GetString(buffer);
                var lines = stringValue.Split(',');
                result = lines.Select(x =>
                    {
                        var dtoParts = x.Split('-');
                        return new LikeDto {Id = int.Parse(dtoParts[0]), TimeStamp = int.Parse(dtoParts[1])};
                    })
                    .ToList();
            }

            if (_postTo.ContainsKey(id))
            {
                result.AddRange(_postTo[id]);
            }

            return result.Count > 0 ? result : null;
        }

        public async Task<List<List<LikeDto>>> GetToAsync(IEnumerable<int> ids)
        {
            var result = new List<List<LikeDto>>();
            foreach (var id in ids)
            {
                var current = new List<LikeDto>();
                if (_toFilePositions[id, 1] != 0)
                {
                    var count = _toFilePositions[id, 1];
                    var buffer = new byte[count];
                    _mmfVsTo.ReadArray(_toFilePositions[id, 0], buffer, 0, count);
                    var stringValue = Encoding.Default.GetString(buffer);
                    var lines = stringValue.Split(',');
                    current = lines.Select(x =>
                        {
                            var dtoParts = x.Split('-');
                            return new LikeDto {Id = int.Parse(dtoParts[0]), TimeStamp = int.Parse(dtoParts[1])};
                        })
                        .ToList();
                }

                if (_postTo.ContainsKey(id))
                {
                    current.AddRange(_postTo[id]);
                }

                result.Add(current);
            }


            return result;
        }

        public List<List<LikeDto>> GetTo(IEnumerable<int> ids)
        {
            var result = new List<List<LikeDto>>();
            foreach (var id in ids)
            {
                var current = new List<LikeDto>();
                if (_toFilePositions[id, 1] != 0)
                {
                    var count = _toFilePositions[id, 1];
                    var buffer = new byte[count];
                    _mmfVsTo.ReadArray(_toFilePositions[id, 0], buffer, 0, count);
                    var stringValue = Encoding.Default.GetString(buffer);
                    var lines = stringValue.Split(',');
                    current = lines.Select(x =>
                        {
                            var dtoParts = x.Split('-');
                            return new LikeDto {Id = int.Parse(dtoParts[0]), TimeStamp = int.Parse(dtoParts[1])};
                        })
                        .ToList();
                }

                if (_postTo.ContainsKey(id))
                {
                    current.AddRange(_postTo[id]);
                }

                result.Add(current);
            }

            return result;
        }


        public List<LikeDto> GetTo(int id)
        {
            var result = new List<LikeDto>();

            if (_toFilePositions[id, 1] != 0)
            {
                var count = _toFilePositions[id, 1];
                var buffer = new byte[count];
                _mmfVsTo.ReadArray(_toFilePositions[id, 0], buffer, 0, count);
                var stringValue = Encoding.Default.GetString(buffer);
                var lines = stringValue.Split(',');
                result = lines.Select(x =>
                    {
                        var dtoParts = x.Split('-');
                        return new LikeDto { Id = int.Parse(dtoParts[0]), TimeStamp = int.Parse(dtoParts[1]) };
                    })
                    .ToList();
            }

            if (_postTo.ContainsKey(id))
            {
                result.AddRange(_postTo[id]);
            }

            return result.Count > 0 ? result : null;
        }

        public bool ContainsLikesTo(int id)
        {
            return _toFilePositions[id, 1] > 0 || _postTo.ContainsKey(id);
        }

        public bool ContainsLikesFrom(int id)
        {
            return _fromFilePositions[id, 1] > 0 || _postFrom.ContainsKey(id);
        }
    }
}
