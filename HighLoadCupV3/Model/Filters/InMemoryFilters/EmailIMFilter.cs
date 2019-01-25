using System.Collections.Generic;
using System.Linq;
using HighLoadCupV3.Model.Filters.InMemoryFilters.Abstract;
using HighLoadCupV3.Model.InMemory;

namespace HighLoadCupV3.Model.Filters.InMemoryFilters
{
    public class EmailDomainIMFilter : StringFilterBase
    {
        public EmailDomainIMFilter(InMemoryRepository repo, int order, string value) : base(repo, order, value)
        {
        }

        public override string Field => Names.Email;

        protected override IEnumerable<AccountData> ContinueFilter(string value, IEnumerable<AccountData> input)
        {
            var domainIndex = _repo.DomainData.GetIndex(value);

            return input.Where(x => x.DomainIndex == domainIndex);
        }

        protected override IEnumerable<AccountData> StartFilter(string value)
        {
            return _repo.DomainData.GetSortedIds(value)
                .Select(x => _repo.Accounts[x]);
        }

        protected override bool IsExisted()
        {
            return _repo.DomainData.ContainsValue(_value);
        }
    }

    // TODO: sort and binary search for gt, lt
    public class EmailGtIMFilter : StringFilterBase
    {
        public EmailGtIMFilter(InMemoryRepository repo, int order, string value) : base(repo, order, value)
        {
        }

        public override string Field => Names.Email;

        protected override IEnumerable<AccountData> ContinueFilter(string value, IEnumerable<AccountData> input)
        {
            var low = _repo.EmailsStorage.GetPossiblePosition(value);
            return input.Where(x => x.EmailSortedIndex >= low);
        }

        protected override IEnumerable<AccountData> StartFilter(string value)
        {
            var low = _repo.EmailsStorage.GetPossiblePosition(value);
            return _repo.GetSortedAccounts().Where(x => x.EmailSortedIndex >= low);
        }

        protected override bool IsExisted()
        {
            return !string.IsNullOrEmpty(_value);
        }
    }

    public class EmailLtIMFilter : StringFilterBase 
    {
        public EmailLtIMFilter(InMemoryRepository repo, int order, string value) : base(repo, order, value)
        {
        }

        public override string Field => Names.Email;

        protected override IEnumerable<AccountData> ContinueFilter(string value, IEnumerable<AccountData> input)
        {
            var low = _repo.EmailsStorage.GetPossiblePosition(value);

            return input.Where(x => x.EmailSortedIndex < low);
        }

        protected override IEnumerable<AccountData> StartFilter(string value)
        {
            var low = _repo.EmailsStorage.GetPossiblePosition(value);
            return _repo.GetSortedAccounts().Where(x => x.EmailSortedIndex < low);
        }

        protected override bool IsExisted()
        {
            return !string.IsNullOrEmpty(_value);
        }
    }
}
