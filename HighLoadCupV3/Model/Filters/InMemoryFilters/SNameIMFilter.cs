using System.Collections.Generic;
using System.Linq;
using HighLoadCupV3.Model.Filters.InMemoryFilters.Abstract;
using HighLoadCupV3.Model.InMemory;

namespace HighLoadCupV3.Model.Filters.InMemoryFilters
{
    public class SNameEqIMFilter : StringFilterBase
    {
        public SNameEqIMFilter(InMemoryRepository repo, int order, string value) : base(repo, order, value)
        {
        }

        public override string Field => Names.SName;

        protected override IEnumerable<AccountData> ContinueFilter(string value, IEnumerable<AccountData> input)
        {
            var fnameIndex = _repo.SNameData.GetIndex(value);

            return input.Where(x => x.SNameIndex == fnameIndex);
        }

        protected override IEnumerable<AccountData> StartFilter(string value)
        {
            return _repo.SNameData.GetSortedIds(value).Select(x => _repo.Accounts[x]);
;
        }

        protected override bool IsExisted()
        {
            return _repo.SNameData.ContainsValue(_value);
        }
    }

    public class SNameStartsIMFilter : StringFilterBase
    {
        private int _low;
        private int _high;

        public SNameStartsIMFilter(InMemoryRepository repo, int order, string value) : base(repo, order, value)
        {
        }

        public override string Field => Names.SName;

        protected override IEnumerable<AccountData> ContinueFilter(string value, IEnumerable<AccountData> input)
        {
            return input.Where(x =>
            {
                var index = x.SNameIndex;
                var sortedIndex = _repo.SNameData.GetSortedIndexByIndex(index);
                return sortedIndex >= _low && sortedIndex <= _high;
            });
        }

        protected override IEnumerable<AccountData> StartFilter(string value)
        {
            var names = _repo.SNameData.GetSortedIdsStartsWith(value);

            return EnumeratorHelper.EnumerateUnique(names).Select(x => _repo.Accounts[x]);
;
        }

        protected override bool IsExisted()
        {
            var lowHigh = _repo.SNameData.GetLowHigh(_value);
            if (lowHigh.Item1 > lowHigh.Item2)
            {
                return false;
            }

            _low = lowHigh.Item1;
            _high = lowHigh.Item2;

            return true;
        }
    }

    public class SNameNullIMFilter : NullFilterBase
    {
        private const short EmptyValueIndex = 0;

        public SNameNullIMFilter(InMemoryRepository repo, int order, string value) : base(repo, order, value)
        {
        }

        public override string Field => Names.SName;

        protected override IEnumerable<AccountData> ContinueFilter(int value, IEnumerable<AccountData> input)
        {
            if (value == 0)
            {
                return input.Where(x => x.SNameIndex != EmptyValueIndex);
            }
            else
            {
                return input.Where(x => x.SNameIndex == -EmptyValueIndex);
            }
        }

        protected override IEnumerable<AccountData> StartFilter(int value)
        {
            if (value == 0)
            {
                return _repo.SNameData.GetSortedIdsNotDefault().Select(x => _repo.Accounts[x]);
            }
            else
            {
                return _repo.SNameData.GetSortedIds(_repo.SNameData.DefaultValue).Select(x => _repo.Accounts[x]);
;
            }
        }
    }
}
