using System.Collections.Generic;
using System.Linq;
using HighLoadCupV3.Model.Filters.InMemoryFilters.Abstract;
using HighLoadCupV3.Model.InMemory;

namespace HighLoadCupV3.Model.Filters.InMemoryFilters
{
    public class SexEqIMFilter : StrictStringFilterBase
    {
        public SexEqIMFilter(InMemoryRepository repo, int order, string value) : base(repo, order, value)
        {
        }

        public override string Field => Names.Sex;

        protected override IEnumerable<AccountData> ContinueFilter(string value, IEnumerable<AccountData> input)
        {
            var sexIndex = _repo.SexData.GetIndex(value);

            return input.Where(x => x.Sex == sexIndex);
        }

        protected override IEnumerable<AccountData> StartFilter(string value)
        {
            var sexIndex = _repo.SexData.GetIndex(value);

            return _repo.SexData.GetSortedIds(sexIndex).Select(x => _repo.Accounts[x]);
        }

        protected override bool IsExisted()
        {
            return _repo.SexData.ContainsValue(_value);
        }
    }
}
