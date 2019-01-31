using System.Collections.Generic;
using System.IO;
using HighLoadCupV3.Model.Dto;
using HighLoadCupV3.Model.InMemory;

namespace HighLoadCupV3
{
    public class LikesUpdateDeserializer
    {
        private readonly InMemoryRepository _repo;

        public LikesUpdateDeserializer(InMemoryRepository repo)
        {
            _repo = repo;
        }

        public List<LikeUpdateDto> Deserialize(Stream stream)
        {
            var result = new List<LikeUpdateDto>();
            using (var reader = new StreamReader(stream))
            {
                var c = reader.Read();
                while (c != -1)
                {
                    if (c == '[')
                    {
                        break;
                    }

                    c = reader.Read();
                }

                var buffer = new List<char>();

                while (c >= 0)
                {
                    c = reader.Read();
                    if (c == '{')
                    {
                        var dto = new LikeUpdateDto();
                        c = reader.Read();

                        while(c != '"')
                        {
                            c = reader.Read();
                        }

                        buffer.Clear();
                        c =  reader.Read();
                        while(c != '"')
                        {
                            buffer.Add((char)c);
                            c = reader.Read();
                        }

                        var type = new string(buffer.ToArray());

                        while (c != ':')
                        {
                            c = reader.Read();
                        }

                        c = reader.Read();
                        buffer.Clear();
                        while (c != ',')
                        {
                            buffer.Add((char)c);
                            c = reader.Read();
                        }

                        if (!int.TryParse(buffer.ToArray(), out int value))
                        {
                            return null;
                        }

                        dto = SetValue(type, value, dto);
                        if(dto == null)
                        {
                            return null;
                        }

                        // Second line

                        while (c != '"')
                        {
                            c = reader.Read();
                        }

                        buffer.Clear();
                        c = reader.Read();
                        while (c != '"')
                        {
                            buffer.Add((char)c);
                            c = reader.Read();
                        }

                        type = new string(buffer.ToArray());

                        while (c != ':')
                        {
                            c = reader.Read();
                        }

                        buffer.Clear();
                        c = reader.Read();
                        while (c != ',')
                        {
                            buffer.Add((char)c);
                            c = reader.Read();
                        }

                        if (!int.TryParse(buffer.ToArray(), out int value2))
                        {
                            return null;
                        }

                        dto = SetValue(type, value2, dto);
                        if(dto == null)
                        {
                            return null;
                        }

                        // Third line

                        while (c != '"')
                        {
                            c = reader.Read();
                        }

                        buffer.Clear();
                        c = reader.Read();
                        while (c != '"')
                        {
                            buffer.Add((char)c);
                            c = reader.Read();
                        }

                        type = new string(buffer.ToArray());

                        while (c != ':')
                        {
                            c = reader.Read();
                        }

                        buffer.Clear();
                        c = reader.Read();
                        while (c != '}')
                        {
                            buffer.Add((char)c);
                            c = reader.Read();
                        }


                        if (!int.TryParse(buffer.ToArray(), out int value3))
                        {
                            return null;
                        }

                        dto = SetValue(type, value3, dto);
                        if (dto == null)
                        {
                            return null;
                        }

                        result.Add(dto);
                    }
                }
            }

            return result;

        }

        private LikeUpdateDto SetValue(string type, int value, LikeUpdateDto dto)
        {
            switch (type)
            {
                case "ts":
                {
                    dto.TimeStamp = value;
                    return dto;
                }
                case "liker":
                {
                    if (!_repo.IsExistedAccountId(value))
                    {
                        return null;
                    }

                    dto.Liker = value;
                    return dto;
                }
                case "likee":
                {
                    if (!_repo.IsExistedAccountId(value))
                    {
                        return null;
                    }

                    dto.Likee = value;
                    return dto;
                }
                default:
                    return null;
            }
        }
    }
}