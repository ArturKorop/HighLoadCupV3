using System.Collections.Generic;
using System.Linq;
using HighLoadCupV3.Model.Filters.InMemoryFilters.Abstract;
using HighLoadCupV3.Model.InMemory;

namespace HighLoadCupV3.Model.Filters.InMemoryFilters
{
    public class CountryEqIMFilter : StringFilterBase
    {
        public CountryEqIMFilter(InMemoryRepository repo, int order, string value) : base(repo, order, value)
        {
        }

        protected override bool IsExisted()
        {
            return _repo.CountryData.ContainsValue(_value);
        }

        public override string Field => Names.Country;

        protected override IEnumerable<AccountData> ContinueFilter(string value, IEnumerable<AccountData> input)
        {
            var countryIndex = _repo.CountryData.GetIndex(value);

            return input.Where(x => x.CountryIndex == countryIndex);
        }

        protected override IEnumerable<AccountData> StartFilter(string value)
        {
            return _repo.CountryData.GetSortedIds(value)
                .Select(x => _repo.Accounts[x]);
        }
    }

    public class CountryNullIMFilter : NullFilterBase
    {
        public CountryNullIMFilter(InMemoryRepository repo, int order, string value) : base(repo, order, value)
        {
        }

        public override string Field => Names.Country;

        protected override IEnumerable<AccountData> ContinueFilter(int value, IEnumerable<AccountData> input)
        {
            var emptyIndex = _repo.CountryData.DefaultIndex;
            if (value == 0)
            {
                return input.Where(x => x.CountryIndex != emptyIndex);
            }
            else
            {
                return input.Where(x => x.CountryIndex == emptyIndex);
            }
        }

        protected override IEnumerable<AccountData> StartFilter(int value)
        {
            if (value == 0)
            {
                var emptyIndex = _repo.CountryData.DefaultIndex;

                return _repo.CountryData.GetSortedIdsNotDefault()
                .Select(x => _repo.Accounts[x]);
            }
            else
            {
                return _repo.CountryData.GetSortedIds(_repo.CountryData.DefaultValue)
                .Select(x => _repo.Accounts[x]);
            }
        }
    }
}
