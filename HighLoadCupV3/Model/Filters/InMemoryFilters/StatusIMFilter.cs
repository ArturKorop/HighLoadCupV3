using System.Collections.Generic;
using System.Linq;
using HighLoadCupV3.Model.Filters.InMemoryFilters.Abstract;
using HighLoadCupV3.Model.InMemory;

namespace HighLoadCupV3.Model.Filters.InMemoryFilters
{
    public class StatusEqIMFilter : StrictStringFilterBase
    {
        public StatusEqIMFilter(InMemoryRepository repo, int order, string value) : base(repo, order, value)
        {
        }

        public override string Field => Names.Status;

        protected override IEnumerable<AccountData> ContinueFilter(string value, IEnumerable<AccountData> input)
        {
            var statusIndex = _repo.StatusData.GetIndex(value);

            return input.Where(x => x.Status == statusIndex);
        }

        protected override IEnumerable<AccountData> StartFilter(string value)
        {
            var statusIndex = _repo.StatusData.GetIndex(value);

            return _repo.StatusData.GetSortedIds(statusIndex).Select(x => _repo.Accounts[x]);
;
        }

        protected override bool IsExisted()
        {
            return _repo.StatusData.ContainsValue(_value);
        }
    }

    public class StatusNeqIMFilter : StrictStringFilterBase
    {
        public StatusNeqIMFilter(InMemoryRepository repo, int order, string value) : base(repo, order, value)
        {
        }

        public override string Field => Names.Status;

        protected override IEnumerable<AccountData> ContinueFilter(string value, IEnumerable<AccountData> input)
        {
            var statusIndex = _repo.StatusData.GetIndex(value);

            return input.Where(x => x.Status != statusIndex);
        }

        protected override IEnumerable<AccountData> StartFilter(string value)
        {
            var statusIndex = _repo.StatusData.GetIndex(value);

            return _repo.GetSortedAccounts().Where(x => x.Status != statusIndex);
;
        }

        protected override bool IsExisted()
        {
            return _repo.StatusData.ContainsValue(_value);
        }
    }
}
