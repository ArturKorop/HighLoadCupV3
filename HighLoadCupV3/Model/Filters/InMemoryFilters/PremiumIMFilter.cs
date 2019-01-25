using System.Collections.Generic;
using System.Linq;
using HighLoadCupV3.Model.Filters.InMemoryFilters.Abstract;
using HighLoadCupV3.Model.InMemory;

namespace HighLoadCupV3.Model.Filters.InMemoryFilters
{
    public class PremiumNowIMFilter : StringFilterBase
    {
        private const byte PremiumIndex = 1;

        public PremiumNowIMFilter(InMemoryRepository repo, int order, string value) : base(repo, order, value)
        {
        }

        public override string Field => Names.Premium;

        protected override IEnumerable<AccountData> ContinueFilter(string value, IEnumerable<AccountData> input)
        {
            return input.Where(x => x.PremiumIndex == PremiumIndex);
        }

        protected override IEnumerable<AccountData> StartFilter(string value)
        {
            return _repo.PremiumData.GetSortedIds(PremiumIndex, false).Select(x => _repo.Accounts[x]);
        }

        protected override bool IsExisted()
        {
            return _value == "1";
        }
    }

    public class PremiumNullIMFilter : NullFilterBase
    {
        private const byte NonPremiumIndex = 0;
        private const byte PremiumIndex = 1;

        public PremiumNullIMFilter(InMemoryRepository repo, int order, string value) : base(repo, order, value)
        {
        }

        public override string Field => Names.Premium;

        protected override IEnumerable<AccountData> ContinueFilter(int value, IEnumerable<AccountData> input)
        {
            if(value == 0)
            {
                return input.Where(x => x.PremiumStart > 0);
            }
            else
            {
                return input.Where(x => x.PremiumStart == 0);
            }
        }

        protected override IEnumerable<AccountData> StartFilter(int value)
        {
            if (value == 0)
            {
                return EnumeratorHelper.EnumerateUnique(new[]
                {
                    _repo.PremiumData.GetSortedIds(PremiumIndex, false),
                    _repo.PremiumData.GetSortedIds(NonPremiumIndex, true)
                }).Select(x => _repo.Accounts[x]);
;
            }
            else
            {
                return _repo.PremiumData.GetSortedIds(NonPremiumIndex, false).Select(x => _repo.Accounts[x]);
;
            }
        }
    }
}
