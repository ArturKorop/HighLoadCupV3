using System.Collections.Generic;
using System.Linq;
using HighLoadCupV3.Model.Filters.InMemoryFilters.Abstract;
using HighLoadCupV3.Model.InMemory;

namespace HighLoadCupV3.Model.Filters.InMemoryFilters
{
    public class CityEqIMFilter : StringFilterBase
    {
        public CityEqIMFilter(InMemoryRepository repo, int order, string value) : base(repo, order, value)
        {
        }

        protected override bool IsExisted()
        {
            return _repo.CityData.ContainsValue(_value);
        }

        public override string Field => Names.City;

        protected override IEnumerable<AccountData> ContinueFilter(string value, IEnumerable<AccountData> input)
        {
            var cityIndex = _repo.CityData.GetIndex(value);

            return input.Where(x => x.CityIndex == cityIndex);
        }

        protected override IEnumerable<AccountData> StartFilter(string value)
        {
            return _repo.CityData.GetSortedIds(value).Select(x => _repo.Accounts[x]);
        }
    }

    public class CityAnyIMFilter : StringFilterBase
    {
        public CityAnyIMFilter(InMemoryRepository repo, int order,string value) : base(repo, order, value)
        {
        }

        protected override bool IsExisted()
        {
            return true;
        }

        public override string Field => Names.City;

        protected override IEnumerable<AccountData> ContinueFilter(string value, IEnumerable<AccountData> input)
        {
            var cityNames = value.Split(',').ToHashSet();
            var set = cityNames.Where(x => _repo.CityData.ContainsValue(x)).Select(x => _repo.CityData.GetIndex(x)).ToHashSet();

            return input.Where(x => set.Contains(x.CityIndex));
        }

        protected override IEnumerable<AccountData> StartFilter(string value)
        {
            var cityNames = value.Split(',').ToHashSet();
            if (cityNames.Count == 0)
            {
                if (!_repo.CityData.ContainsValue(cityNames.First()))
                {
                    return Enumerable.Empty<AccountData>();
                }

                return _repo.CityData.GetSortedIds(cityNames.First()).Select(x => _repo.Accounts[x]);
            }
            else
            {
                var ids = _repo.CityData.GetSortedIds(cityNames);

                return EnumeratorHelper.EnumerateUnique(ids).Select(x => _repo.Accounts[x]);
            }
        }
    }

    public class CityNullIMFilter : NullFilterBase
    {
        public CityNullIMFilter(InMemoryRepository repo, int order, string value) : base(repo, order, value)
        {
        }

        public override string Field => Names.City;

        protected override IEnumerable<AccountData> ContinueFilter(int value, IEnumerable<AccountData> input)
        {
            var emptyIndex = _repo.CityData.DefaultIndex;
            if (value == 0)
            {
                return input.Where(x => x.CityIndex != emptyIndex);
            }
            else
            {
                return input.Where(x => x.CityIndex == emptyIndex);
            }
        }

        protected override IEnumerable<AccountData> StartFilter(int value)
        {
            if (value == 0)
            {
                return _repo.CityData.GetSortedIdsNotDefault().Select(x => _repo.Accounts[x]);
            }
            else
            {
                return _repo.CityData.GetSortedIds(_repo.CityData.DefaultValue)
                    .Select(x => _repo.Accounts[x]);
            }
        }
    }
}
