using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HighLoadCupV3.Model.Dto;

namespace HighLoadCupV3.Model.InMemory
{
    public class InMemoryLikesStorage  : ILikesStorage
    {
        private readonly byte[][] _likesFrom;
        private readonly byte[][] _likesTo;
        private List<LikeDto>[] _toBuffer;

        //private readonly DescLikeDtoComparer _comparer = new DescLikeDtoComparer();

        public InMemoryLikesStorage(int accountsCount)
        {
            _likesFrom = new byte[accountsCount][];
            _likesTo = new byte[accountsCount][];
            _toBuffer = new List<LikeDto>[accountsCount];
        }

        public void AddFromFile(int id, LikeDto[] likes)
        {
            _likesFrom[id] = Compress(ConvertIntToString(likes.Select(x=>x.Id)));

            foreach (var likeDto in likes)
            {
                if (_toBuffer[likeDto.Id] == null)
                {
                    _toBuffer[likeDto.Id] = new List<LikeDto>();
                }

                _toBuffer[likeDto.Id].Add(new LikeDto { Id = id, TimeStamp = likeDto.TimeStamp });
            }
        }

        public void AddToBuffer(int id, LikeDto[] likes)
        {
            _likesFrom[id] = Compress(ConvertIntToString(likes.Select(x => x.Id)));

            foreach (var likeDto in likes)
            {
                var dto = new LikeDto { Id = id, TimeStamp = likeDto.TimeStamp };
                if(_likesTo[likeDto.Id] == null)
                {
                    _likesTo[likeDto.Id] = Compress(ConvertLikeDtoToString(new List<LikeDto> { dto }));
                }
                else
                {
                    var existedLikes = ConvertStringToLikeDto(Decompress(_likesTo[likeDto.Id]));
                    existedLikes.Add(dto);
                    _likesTo[likeDto.Id] = Compress(ConvertLikeDtoToString(existedLikes));
                }
            }
        }

        public void UpdateBuffer(LikeUpdateDto dto)
        {
            if(_likesFrom[dto.Liker] == null)
            {
                _likesFrom[dto.Liker] = Compress(ConvertIntToString(new int[] { dto.Likee }));
            }
            else
            {
                var existed = ConvertStringToIds(Decompress(_likesFrom[dto.Liker]));
                existed.Add(dto.Likee);
                _likesFrom[dto.Liker] = Compress(ConvertIntToString(existed));
            }

            var likeDto = new LikeDto { Id = dto.Liker, TimeStamp = dto.TimeStamp };
            if (_likesTo[dto.Likee] == null)
            {
                _likesTo[dto.Liker] = Compress(ConvertLikeDtoToString(new List<LikeDto> { likeDto }));
            }
            else
            {
                var existed = ConvertStringToLikeDto(Decompress(_likesTo[dto.Likee]));
                existed.Add(likeDto);
                _likesTo[dto.Likee] = Compress(ConvertLikeDtoToString(existed));
            }
        }

        public void UpdateBuffer(LikeDto dto, int id)
        {
            if (_likesFrom[id] == null)
            {
                _likesFrom[id] = Compress(ConvertIntToString(new int[] { dto.Id }));
            }
            else
            {
                var existed = ConvertStringToIds(Decompress(_likesFrom[id]));
                existed.Add(dto.Id);
                _likesFrom[id] = Compress(ConvertIntToString(existed));
            }

            var likeDto = new LikeDto { Id = id, TimeStamp = dto.TimeStamp };
            if (_likesTo[dto.Id] == null)
            {
                _likesTo[dto.Id] = Compress(ConvertLikeDtoToString(new List<LikeDto> { likeDto }));
            }
            else
            {
                var existed = ConvertStringToLikeDto(Decompress(_likesTo[dto.Id]));
                existed.Add(likeDto);
                _likesTo[dto.Id] = Compress(ConvertLikeDtoToString(existed));
            }
        }

        public void FillReverse(Stopwatch sw)
        {
            for (int i = 0; i < _toBuffer.Length; i++)
            {
                if (_toBuffer[i] != null)
                {
                    _likesTo[i] = Compress(ConvertLikeDtoToString(_toBuffer[i]));
                }
            }

            _toBuffer = null;
        }

        public List<int> GetFrom(int id)
        {
            if (_likesFrom[id] == null)
            {
                return null;
            }

            return ConvertStringToIds(Decompress(_likesFrom[id]));
        }

        public List<LikeDto> GetTo(int id)
        {
            if (_likesTo[id] == null)
            {
                return null;
            }

            return ConvertStringToLikeDto(Decompress(_likesTo[id]));
        }

        public List<List<LikeDto>> GetTo(IEnumerable<int> ids)
        {
            return ids.Select(x => GetTo(x)).ToList();
        }

        public IEnumerable<List<int>> GetFrom(IEnumerable<int> ids) 
        {
            return ids.Select(x => GetFrom(x)).ToList();
        }


        public bool ContainsLikesTo(int id)
        {
            return _likesTo[id] != null;
        }

        public bool ContainsLikesFrom(int id)
        {
            return _likesFrom[id] != null;
        }

        private static string ConvertLikeDtoToString(List<LikeDto> dtos)
        {
            return string.Join(',', dtos.Select(x => $"{x.Id}-{x.TimeStamp}"));
        }

        private static string ConvertIntToString(IEnumerable<int> ids)
        {
            return string.Join(',', ids);
        }

        private static List<int> ConvertStringToIds(string input)
        {
            var parts = input.Split(',');

            return parts.Select(int.Parse).ToList();
        }

        private static List<LikeDto> ConvertStringToLikeDto(string input)
        {
            var parts = input.Split(',');
            var dtos = new List<LikeDto>();

            for (int i = 0; i < parts.Length; i++)
            {
                var dtoParts = parts[i].Split('-');
                dtos.Add(new LikeDto {Id = int.Parse(dtoParts[0]), TimeStamp = int.Parse(dtoParts[1])});
            }

            return dtos;
        }

        private static byte[] Compress(string data)
        {
            using (MemoryStream inMemStream = new MemoryStream(Encoding.UTF8.GetBytes(data)), outMemStream = new MemoryStream())
            {
                using (var zipStream = new DeflateStream(outMemStream, CompressionMode.Compress, true))
                {
                    inMemStream.WriteTo(zipStream);
                }

                return outMemStream.ToArray();
            }
        }

        private static string Decompress(byte[] data)
        {
            using (var inMemStream = new MemoryStream(data))
            {
                using (var decompressionStream = new DeflateStream(inMemStream, CompressionMode.Decompress))
                {
                    using (var streamReader = new StreamReader(decompressionStream, Encoding.UTF8))
                    {
                        return streamReader.ReadToEnd();
                    }
                }
            }
        }

        public void Flush()
        {
        }

        public Task<List<int>> GetFromAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<List<List<int>>> GetFromAsync(IEnumerable<int> ids)
        {
            throw new NotImplementedException();
        }

        public Task<List<LikeDto>> GetToAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<List<List<LikeDto>>> GetToAsync(IEnumerable<int> ids)
        {
            throw new NotImplementedException();
        }
    }
}