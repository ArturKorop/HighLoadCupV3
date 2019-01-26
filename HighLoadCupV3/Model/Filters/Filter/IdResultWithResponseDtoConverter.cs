using System.Collections.Generic;
using HighLoadCupV3.Model.Dto;
using HighLoadCupV3.Model.InMemory;
using Newtonsoft.Json;

namespace HighLoadCupV3.Model.Filters.Filter
{
    public class IdResultWithResponseDtoConverter : IIdsToResponseConverter
    {
        private readonly InMemoryRepository _repo;

        public IdResultWithResponseDtoConverter(InMemoryRepository inMemory)
        {
            _repo = inMemory;
        }

        public string Convert(IEnumerable<AccountData> accounts, HashSet<string> requiredFields)
        {
            var data = new List<AccountResponseDto>();
            foreach (var acc in accounts)
            {
                data.Add(Convert(acc, requiredFields));
            }

            var holder = new Holder { Accounts = data };

            return JsonConvert.SerializeObject(holder, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }

        public AccountResponseDto Convert(AccountData acc, HashSet<string> requiredFields)
        {
            var res = new AccountResponseDto
            {
                Id = acc.Id,
                Email = acc.Email
            };

            if (requiredFields.Contains(Names.FName))
            {
                res.FName = _repo.FNameData.GetValue(acc.FNameIndex);
            }

            if (requiredFields.Contains(Names.SName))
            {
                res.SName = _repo.SNameData.GetValue(acc.SNameIndex);
            }

            if (requiredFields.Contains(Names.Sex))
            {
                res.Sex = _repo.SexData.GetValue(acc.Sex);
            }

            if (requiredFields.Contains(Names.Status))
            {
                res.Status = _repo.StatusData.GetValue(acc.Status);
            }

            if (requiredFields.Contains(Names.City))
            {
                res.City = _repo.CityData.GetValue(acc.CityIndex);
            }

            if (requiredFields.Contains(Names.Country))
            {
                res.Country = _repo.CountryData.GetValue(acc.CountryIndex);
            }

            if (requiredFields.Contains(Names.Phone))
            {
                if (acc.Phone != 0)
                {
                    var phoneSub = acc.Phone.ToString();
                    if (phoneSub.Length < 7)
                    {
                        if (phoneSub.Length == 5)
                        {
                            phoneSub = "00" + phoneSub;
                        }
                        else if (phoneSub.Length == 6)
                        {
                            phoneSub = "0" + phoneSub;
                        }
                        else if (phoneSub.Length == 4)
                        {
                            phoneSub = "000" + phoneSub;
                        }
                        else if (phoneSub.Length == 3)
                        {
                            phoneSub = "0000" + phoneSub;
                        }
                    }

                    res.Phone = $"8({_repo.CodeData.GetValue(acc.CodeIndex)}){phoneSub}";
                }
            }

            if (requiredFields.Contains(Names.Birth))
            {
                res.Birth = acc.Birth;
            }

            if (requiredFields.Contains(Names.Premium))
            {
                var premium = new PremiumDto
                {
                    Start = acc.PremiumStart,
                    Finish = acc.PremiumFinish,
                };

                res.Premium = premium;
            }

            return res;
        }

        private class Holder
        {
            [JsonProperty("accounts")]
            public List<AccountResponseDto> Accounts { get; set; }
        }
    }
}
