using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using HighLoadCupV3.Model.Dto;

namespace HighLoadCupV3.Model.InMemory
{
    public class CombinedLikesStorage : ILikesStorage
    {
        private readonly List<int>[] _fromData;
        private readonly List<long>[] _toData;
        private static readonly long _mask = (long)Math.Pow(2, 24) - 1;

        public CombinedLikesStorage(int accountsCount)
        {
            _fromData = new List<int>[accountsCount];
            _toData = new List<long>[accountsCount];
        }

        private void WriterLikesFrom(int id, List<int> likes)
        {
            _fromData[id] = likes;
        }

        private void WriteLikesTo(int id, List<LikeDto> likes)
        {
            _toData[id] = new List<long>();
            for (int i = 0; i < likes.Count; i++)
            {
                _toData[i].Add(Transform(likes[i]));
            }
        }

        public void AddToBuffer(int id, LikeDto[] likes)
        {
            _fromData[id] = likes.Select(x => x.Id).ToList();

            foreach (var like in likes)
            {
                if (_toData[like.Id] == null)
                {
                    _toData[like.Id] = new List<long> { Transform(id, like.TimeStamp) };
                }
                else
                {
                    _toData[like.Id].Add(Transform(id, like.TimeStamp));
                }
            }
        }

        public void UpdateBuffer(LikeUpdateDto dto)
        {
            if (_fromData[dto.Liker] == null)
            {
                _fromData[dto.Liker] = new List<int> { dto.Likee };
            }
            else
            {
                _fromData[dto.Liker].Add(dto.Likee);
            }

            if (_toData[dto.Likee] == null)
            {
                _toData[dto.Likee] = new List<long> { Transform(dto.Liker, dto.TimeStamp) };
            }
            else
            {
                _toData[dto.Likee].Add(Transform(dto.Liker, dto.TimeStamp));
            }
        }

        public void UpdateBuffer(LikeDto dto, int id)
        {
            if (_fromData[id] == null)
            {
                _fromData[id] = new List<int> { dto.Id };
            }
            else
            {
                _fromData[dto.Id].Add(dto.Id);
            }

            if (_toData[dto.Id] == null)
            {
                _toData[dto.Id] = new List<long> { Transform(id, dto.TimeStamp) };
            }
            else
            {
                _toData[dto.Id].Add(Transform(id, dto.TimeStamp));
            }
        }


        public void AddFromFile(int id, LikeDto[] likes)
        {
            if (likes == null || likes.Length == 0) return;

            WriterLikesFrom(id, likes.Select(x => x.Id).ToList());

            foreach (var like in likes)
            {
                if (_toData[like.Id] == null)
                {
                    _toData[like.Id] = new List<long> { Transform(id, like.TimeStamp) };
                }
                else
                {
                    _toData[like.Id].Add(Transform(id, like.TimeStamp));
                }
            }
        }

        public void Flush()
        {
        }

        public void FillReverse(Stopwatch sw)
        {
        }

        public async Task<List<int>> GetFromAsync(int id)
        {
            return _fromData[id];
        }

        public List<int> GetFrom(int id)
        {
            return _fromData[id];
        }

        public async Task<List<LikeDto>> GetToAsync(int id)
        {
            return _toData[id].Select(x => Transform(x)).ToList();
        }

        public async Task<List<List<LikeDto>>> GetToAsync(IEnumerable<int> ids)
        {
            var result = new List<List<LikeDto>>();
            foreach (var id in ids)
            {
                result.Add(_toData[id].Select(x=> Transform(x)).ToList());
            }

            return result;
        }

        public List<List<LikeDto>> GetTo(IEnumerable<int> ids)
        {
            var result = new List<List<LikeDto>>();
            foreach (var id in ids)
            {
                result.Add(_toData[id].Select(x => Transform(x)).ToList());
            }

            return result;
        }


        public List<LikeDto> GetTo(int id)
        {
            return _toData[id].Select(x => Transform(x)).ToList();
        }

        public bool ContainsLikesTo(int id)
        {
            return _toData[id] != null;
        }

        public bool ContainsLikesFrom(int id)
        {
            return _fromData[id] != null;
        }

        private static long Transform(int id, int ts)
        {
            long res = id;
            res = res | (long)ts << 25;

            return res;
        }

        private static LikeDto Transform(long input)
        {
            var dto = new LikeDto
            {
                Id = (int)(input & _mask),
                TimeStamp = (int)(input >> 25)
            };

            return dto;
        }

        private static long Transform(LikeDto dto)
        {
            return Transform(dto.Id, dto.TimeStamp);
        }
    }
}

