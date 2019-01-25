using System.Collections.Generic;
using System.Linq;
using HighLoadCupV3.Model.Filters.InMemoryFilters.Abstract;
using HighLoadCupV3.Model.InMemory;

namespace HighLoadCupV3.Model.Filters.InMemoryFilters
{
    public class JoinedYearIMFilter : IntFilterBase
    {
        public JoinedYearIMFilter(InMemoryRepository repo, int order, string value) : base(repo, order, value)
        {
        }

        public override string Field => Names.Joined;

        protected override IEnumerable<AccountData> ContinueFilter(int value, IEnumerable<AccountData> input)
        {
            var yearIndex = _repo.JoinedYearData.GetIndex(value);

            return input.Where(x => x.JoinedYearIndex == yearIndex);
        }

        protected override IEnumerable<AccountData> StartFilter(int value)
        {
            return _repo.JoinedYearData.GetSortedIds(value).Select(x => _repo.Accounts[x]);
        }

        protected override bool IsExisted()
        {
            return _repo.JoinedYearData.ContainsValue(_value);
        }
    }
}