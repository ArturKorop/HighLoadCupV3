using System.Collections.Generic;
using System.Linq;
using HighLoadCupV3.Model.Filters.InMemoryFilters.Abstract;
using HighLoadCupV3.Model.InMemory;

namespace HighLoadCupV3.Model.Filters.InMemoryFilters
{
    public class BirthYearIMFilter : IntFilterBase
    {
        public BirthYearIMFilter(InMemoryRepository repo, int order, string value) : base(repo, order, value)
        {
        }

        public override string Field => Names.Birth;

        protected override IEnumerable<AccountData> ContinueFilter(int value, IEnumerable<AccountData> input)
        {
            var yearIndex = _repo.BirthYearData.GetIndex(value);

            return input.Where(x => x.BirthYearIndex == yearIndex);
        }

        protected override IEnumerable<AccountData> StartFilter(int value)
        {
            if (!IsExisted())
            {
                return Enumerable.Empty<AccountData>();
            }

            return _repo.BirthYearData.GetSortedIds(value).Select(x => _repo.Accounts[x]);
        }

        protected override bool IsExisted()
        {
            return _repo.BirthYearData.ContainsValue(_value);
        }
    }

    public class BirthGtIMFilter : IntFilterBase
    {
        public BirthGtIMFilter(InMemoryRepository repo, int order, string value) : base(repo, order, value)
        {
        }

        public override string Field => Names.Birth;

        protected override IEnumerable<AccountData> ContinueFilter(int value, IEnumerable<AccountData> input)
        {
            return input.Where(x => x.Birth > value);
        }

        protected override bool IsExisted()
        {
            return true;
        }

        protected override IEnumerable<AccountData> StartFilter(int value)
        {
            return _repo.GetSortedAccounts().Where(x => x.Birth > value);
        }
    }

    public class BirthLtIMFilter : IntFilterBase
    {
        public BirthLtIMFilter(InMemoryRepository repo, int order, string value) : base(repo, order, value)
        {
        }

        public override string Field => Names.Birth;

        protected override IEnumerable<AccountData> ContinueFilter(int value, IEnumerable<AccountData> input)
        {
            return input.Where(x => x.Birth < value);
        }

        protected override bool IsExisted()
        {
            return true;
        }

        protected override IEnumerable<AccountData> StartFilter(int value)
        {
            return _repo.GetSortedAccounts().Where(x => x.Birth < value);
        }
    }
}
