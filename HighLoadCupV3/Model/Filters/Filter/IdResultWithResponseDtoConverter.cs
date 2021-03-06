﻿using System.Collections.Generic;
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

        public object Convert(IEnumerable<AccountData> accounts, HashSet<string> requiredFields)
        {
            var data = new List<AccountResponseDto>();
            foreach (var acc in accounts)
            {
                data.Add(Convert(acc, requiredFields));
            }

            var holder = new Holder { Accounts = data };

            return holder;
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
                res.Phone = acc.Phone;
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
