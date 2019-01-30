using System.Collections.Generic;
using System.Linq;
using HighLoadCupV3.Model.Dto;
using HighLoadCupV3.Model.InMemory;
using Newtonsoft.Json;

namespace HighLoadCupV3.Model.Filters.Recommend
{
    public class Recommend
    {
        private readonly InMemoryRepository _repo;
        private readonly RecommendImplBase _all;
        private readonly RecommendImplBase _specific;

        public Recommend(InMemoryRepository repo)
        {
            _repo = repo;
            _all = new RecommendAll(repo, new InterestsIntersectCalculator(), new RecommendResponseDtoCovnerter(_repo));
            _specific = new RecommendSpecificUsers(repo, new InterestsIntersectCalculator(), new RecommendResponseDtoCovnerter(_repo));
        }

        public string GetRecommendations(int id, string key, string value, int limit)
        {
            var acc = _repo.Accounts[id];
            var interestsSet = acc.Interests;
            var holder = new RecommendAccountsResponseDto
            {
                Accounts = new RecommendResponseDto[0]
            };

            if (interestsSet == null)
            {
                return JsonConvert.SerializeObject(holder);
            }

            IEnumerable<RecommendResponseDto> recommendations;
            if (key != null)
            {
                var ids = Filter(key, value);
                recommendations = _specific.Recommend(id, limit, ids);
            }
            else
            {
                recommendations = _all.Recommend(id, limit, null);
            }


            holder.Accounts = recommendations;

            var serializedResult = JsonConvert.SerializeObject(holder,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });

            return serializedResult;
        }

        private IEnumerable<int> Filter(string key, string value)
        {
            if (key == Names.City)
            {
                if (!_repo.CityData.ContainsValue(value))
                {
                    return Enumerable.Empty<int>();
                }

                return _repo.CityData.GetSortedIds(value);
            }
            else
            {
                if (!_repo.CountryData.ContainsValue(value))
                {
                    return Enumerable.Empty<int>();
                }

                return _repo.CountryData.GetSortedIds(value);
            }
        }
    }
}
