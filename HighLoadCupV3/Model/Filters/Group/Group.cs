using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HighLoadCupV3.Model.Filters.Group.Impl;
using HighLoadCupV3.Model.Filters.InMemoryFilters.Abstract;
using HighLoadCupV3.Model.InMemory;
using Newtonsoft.Json;

namespace HighLoadCupV3.Model.Filters.Group
{
    public class Group
    {
        private readonly InMemoryRepository _repo;
        private readonly GroupFactory _factory;
        private readonly FilterQueryCacheKeyGenerator _cacheKeyGenerator;

        private  GroupByCityStatus _cityStatus;
        private  GroupByCitySex _citySex;
        private  GroupByCountryStatus _countryStatus;
        private  GroupByCountrySex _countrySex;

        private  GroupByCity _city;
        private  GroupByCountry _country;
        private  GroupBySex _sex;
        private  GroupByStatus _status;
        private  GroupByInterests _interests;

        public Group(InMemoryRepository repo, GroupFactory factory)
        {
            _repo = repo;
            _factory = factory;
            _cacheKeyGenerator = new FilterQueryCacheKeyGenerator(repo);
        }

        public string GroupBy(GroupQuery query)
        {
            var holder = new GroupHolder();
            var groupBy = GetGroup(query.Key);
            if (groupBy == null)
            {
                return JsonConvert.SerializeObject(holder);
            }

            if (query.Filter.Count == 0)
            {
                var data = groupBy.GroupBy(query.Order).Take(query.Limit);
                holder.Groups = data;

                return JsonConvert.SerializeObject(holder,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            }

            var cacheKey = _cacheKeyGenerator.Generate(query.Filter);
            if (cacheKey == null)
            {
                var accounts = FilterBy(query.Filter);
                if (accounts == null)
                {
                    return null;
                }
                else if (!accounts.Any())
                {
                    holder.Groups = Enumerable.Empty<Dto.GroupResponseDto>();
                }
                else
                {
                    var data = groupBy.GroupBy(accounts, query.Order).Take(query.Limit);
                    holder.Groups = data;
                }
            }
            else
            {
                var data = groupBy.GroupByWithCache(query.Order, cacheKey).Take(query.Limit);
                holder.Groups = data;
            }

            return JsonConvert.SerializeObject(holder,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }

        private IGroupBy GetGroup(GroupKey key)
        {
            switch (key)
            {
                case GroupKey.CityStatus:
                    return _cityStatus;
                case GroupKey.CitySex:
                    return _citySex;
                case GroupKey.CountryStatus:
                    return _countryStatus;
                case GroupKey.CountrySex:
                    return _countrySex;
                case GroupKey.City:
                    return _city;
                case GroupKey.Country:
                    return _country;
                case GroupKey.Sex:
                    return _sex;
                case GroupKey.Status:
                    return _status;
                case GroupKey.Interests:
                    return _interests;
                default:
                    return null;
            }
        }

        private IEnumerable<AccountData> FilterBy(Dictionary<string, string> queries)
        {

            var likesFiler = _factory.TryGetLikes(queries);
            IEnumerable<AccountData> result = null;
            if (likesFiler != null)
            {
                if (!likesFiler.IsValid)
                {
                    return null;
                }

                if (likesFiler.IsEmpty)
                {
                    return Enumerable.Empty<AccountData>();
                }

                queries.Remove(Names.Likes);
                result = likesFiler.Filter();
            }

            var filters = new List<IFilter>();
            foreach (var query in queries)
            {
                var filter = _factory.GetFilter(query.Key, query.Value);
                if (filter == null)
                {
                    return null;
                }

                if (!filter.IsValid)
                {
                    return null;
                }

                if (filter.IsEmpty)
                {
                    return Enumerable.Empty<AccountData>();
                }

                filters.Add(filter);
            }

            filters.Sort((a, b) => a.Order - b.Order);
            foreach (var filter in filters)
            {
                result = filter.Filter(result);
            }

            return result ?? Enumerable.Empty<AccountData>();
        }


        public void CleanCacheAndCreateNewCache()
        {
            _cityStatus = new GroupByCityStatus(_repo.CityData.GetCount(), _repo.StatusData.GetCount(), _repo);
            _citySex = new GroupByCitySex(_repo.CityData.GetCount(), _repo.SexData.GetCount(), _repo);
            _countryStatus = new GroupByCountryStatus(_repo.CountryData.GetCount(), _repo.StatusData.GetCount(), _repo);
            _countrySex = new GroupByCountrySex(_repo.CountryData.GetCount(), _repo.SexData.GetCount(), _repo);

            _city = new GroupByCity(_repo.CityData.GetCount(), _repo);
            _country = new GroupByCountry(_repo.CountryData.GetCount(), _repo);
            _sex = new GroupBySex(_repo.SexData.GetCount(), _repo);
            _status = new GroupByStatus(_repo.StatusData.GetCount(), _repo);
            _interests = new GroupByInterests(_repo.InterestsData.GetCount(), _repo);

            // Generate Full cache
            _cityStatus.GroupBy(0);
            _citySex.GroupBy(0);
            _countryStatus.GroupBy(0);
            _countrySex.GroupBy(0);

            _city.GroupBy(0);
            _country.GroupBy(0);
            _sex.GroupBy(0);
            _status.GroupBy(0);
            _interests.GroupBy(0);

            var cacheKeys = _cacheKeyGenerator.GenerateAllPossibleCacheKeys().ToList();

            foreach (var key in cacheKeys)
            {
                var accounts = FilterBy(key).ToArray();
                var cacheKey = _cacheKeyGenerator.Generate(key);

                Parallel.Invoke(
                    () =>
                    {
                        _cityStatus.CreateCache(accounts, cacheKey);
                        _citySex.CreateCache(accounts, cacheKey);
                    },
                    () =>
                    {
                        _countryStatus.CreateCache(accounts, cacheKey);
                        _countrySex.CreateCache(accounts, cacheKey);
                    },

                    () =>
                    {
                        _city.CreateCache(accounts, cacheKey);
                        _interests.CreateCache(accounts, cacheKey);
                    },
                    () =>
                    {
                        _sex.CreateCache(accounts, cacheKey);
                        _status.CreateCache(accounts, cacheKey);
                        _country.CreateCache(accounts, cacheKey);
                    }
                );
            }
        }
    }
}
