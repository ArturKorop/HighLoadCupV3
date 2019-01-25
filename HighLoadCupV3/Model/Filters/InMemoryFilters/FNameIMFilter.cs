using System.Collections.Generic;
using System.Linq;
using HighLoadCupV3.Model.Filters.InMemoryFilters.Abstract;
using HighLoadCupV3.Model.InMemory;

namespace HighLoadCupV3.Model.Filters.InMemoryFilters
{
    public class FNameEqIMFilter : StringFilterBase
    {
        public FNameEqIMFilter(InMemoryRepository repo, int order, string value) : base(repo, order, value)
        {
        }

        public override string Field => Names.FName;

        protected override IEnumerable<AccountData> ContinueFilter(string value, IEnumerable<AccountData> input)
        {
            var fnameIndex = _repo.FNameData.GetIndex(value);

            return input.Where(x => x.FNameIndex == fnameIndex);
        }

        protected override IEnumerable<AccountData> StartFilter(string value)
        {
            return _repo.FNameData.GetSortedIds(value)
                .Select(x => _repo.Accounts[x]);
        }

        protected override bool IsExisted()
        {
            return _repo.FNameData.ContainsValue(_value);
        }
    }

    public class FNameAnyIMFilter : StringFilterBase
    {
        public FNameAnyIMFilter(InMemoryRepository repo, int order, string value) : base(repo, order, value)
        {
        }

        public override string Field => Names.FName;

        protected override IEnumerable<AccountData> ContinueFilter(string value, IEnumerable<AccountData> input)
        {
            var names = value.Split(',');
            var set = names.Select(x => _repo.FNameData.GetIndex(x)).ToHashSet();

            return input.Where(x => set.Contains(x.FNameIndex));
        }

        protected override IEnumerable<AccountData> StartFilter(string value)
        {
            var names = value.Split(',');

            return EnumeratorHelper.EnumerateUnique(_repo.FNameData.GetSortedIds(names))
                .Select(x => _repo.Accounts[x]);
        }

        protected override bool IsExisted()
        {
            return !string.IsNullOrEmpty(_value);
        }
    }

    public class FNameNullIMFilter : NullFilterBase
    {
        public FNameNullIMFilter(InMemoryRepository repo, int order,string value) : base(repo, order, value)
        {
        }

        public override string Field => Names.FName;

        protected override IEnumerable<AccountData> ContinueFilter(int value, IEnumerable<AccountData> input)
        {
            var emptyIndex = _repo.FNameData.DefaultIndex;
            if(value == 0)
            {
                return input.Where(x => x.FNameIndex != emptyIndex);
            }
            else
            {
                return input.Where(x => x.FNameIndex == emptyIndex);
            }
        }

        protected override IEnumerable<AccountData> StartFilter(int value)
        {
            if(value == 0)
            {
                return _repo.FNameData.GetSortedIdsNotDefault()
                .Select(x => _repo.Accounts[x]);
            }
            else
            {
                return _repo.FNameData.GetSortedIds(string.Empty)
                .Select(x => _repo.Accounts[x]);
            }
        }
    }
}
