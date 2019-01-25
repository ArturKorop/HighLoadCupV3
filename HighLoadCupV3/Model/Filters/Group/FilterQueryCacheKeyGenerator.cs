using System.Collections.Generic;
using System.Linq;
using HighLoadCupV3.Model.InMemory;

namespace HighLoadCupV3.Model.Filters.Group
{
    public class FilterQueryCacheKeyGenerator
    {
        private readonly HashSet<string> _allowedFilters = new HashSet<string> {Names.Status, Names.Sex, Names.Joined, Names.Email, Names.Birth
            , Names.Interests, Names.Country, Names.Phone
        };
        private readonly HashSet<string> _allowedPairFilters = new HashSet<string> { Names.Status, Names.Sex, Names.Joined };
        private readonly InMemoryRepository _repo;

        public FilterQueryCacheKeyGenerator(InMemoryRepository repo)
        {
            _repo = repo;
        }

        public string Generate(Dictionary<string, string> filters)
        {
            if (filters.Count == 1)
            {
                var filterPair = filters.First();
                if (_allowedFilters.Contains(filterPair.Key))
                {
                    return filterPair.Key + filterPair.Value;
                }
            }

            if(filters.Count == 2)
            {
                if (filters.All(x => _allowedPairFilters.Contains(x.Key)))
                {
                    return string.Join(',',filters.OrderBy(x => x.Key).Select(x=> $"{x.Key}{x.Value}"));
                }
            }


            return null;
        }

        public IEnumerable<Dictionary<string, string>> GenerateAllPossibleCacheKeys()
        {
            // 1 Pairs
            for (byte i = 0; i < _repo.StatusData.GetCount(); i++)
            {
                var result = new Dictionary<string, string>();
                result[Names.Status] = _repo.StatusData.GetValue(i);

                yield return result;
            }

            for (byte i = 0; i < _repo.SexData.GetCount(); i++)
            {
                var result = new Dictionary<string, string>();
                result[Names.Sex] = _repo.SexData.GetValue(i);

                yield return result;
            }

            for (byte i = 0; i < _repo.JoinedYearData.GetCount(); i++)
            {
                var result = new Dictionary<string, string>();
                result[Names.Joined] = _repo.JoinedYearData.GetValue(i).ToString();

                yield return result;
            }

            for (byte i = 0; i < _repo.DomainData.GetCount(); i++)
            {
                var result = new Dictionary<string, string>();
                result[Names.Email] = _repo.DomainData.GetValue(i);

                yield return result;
            }

            for (byte i = 0; i < _repo.BirthYearData.GetCount(); i++)
            {
                var result = new Dictionary<string, string>();
                result[Names.Birth] = _repo.BirthYearData.GetValue(i).ToString();

                yield return result;
            }

            for (byte i = 0; i < _repo.InterestsData.GetCount(); i++)
            {
                var result = new Dictionary<string, string>();
                result[Names.Interests] = _repo.InterestsData.GetValue(i);

                yield return result;
            }

            for (byte i = 1; i < _repo.CountryData.GetCount(); i++)
            {
                var result = new Dictionary<string, string>();
                result[Names.Country] = _repo.CountryData.GetValue(i);

                yield return result;
            }

            for (byte i = 1; i < _repo.CodeData.GetCount(); i++)
            {
                var result = new Dictionary<string, string>();
                result[Names.Phone] = _repo.CodeData.GetValue(i).ToString();

                yield return result;
            }

            // 2 Pairs
            // Joined

            for (byte i = 0; i < _repo.JoinedYearData.GetCount(); i++)
            {
                for (byte j = 0; j < _repo.SexData.GetCount(); j++)
                {
                    var result = new Dictionary<string, string>();
                    result[Names.Joined] = _repo.JoinedYearData.GetValue(i).ToString();
                    result[Names.Sex] = _repo.SexData.GetValue(j);

                    yield return result;
                }
            }

            for (byte i = 0; i < _repo.JoinedYearData.GetCount(); i++)
            {
                for (byte j = 0; j < _repo.StatusData.GetCount(); j++)
                {
                    var result = new Dictionary<string, string>();
                    result[Names.Joined] = _repo.JoinedYearData.GetValue(i).ToString();
                    result[Names.Status] = _repo.StatusData.GetValue(j);

                    yield return result;
                }
            }

            // Sex
            for (byte i = 0; i < _repo.SexData.GetCount(); i++)
            {
                for (byte j = 0; j < _repo.StatusData.GetCount(); j++)
                {
                    var result = new Dictionary<string, string>();
                    result[Names.Sex] = _repo.SexData.GetValue(i);
                    result[Names.Status] = _repo.StatusData.GetValue(j);

                    yield return result;
                }
            }
        }
    }
}