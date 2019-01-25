using System.Collections.Generic;
using System.Linq;
using HighLoadCupV3.Model.Filters.InMemoryFilters.Abstract;
using HighLoadCupV3.Model.InMemory;

namespace HighLoadCupV3.Model.Filters.InMemoryFilters
{
    public class PhoneCodeIMFilter : IntFilterBase
    {
        public PhoneCodeIMFilter(InMemoryRepository repo, int order, string value) : base(repo, order, value)
        {
        }

        public override string Field => Names.Phone;

        protected override IEnumerable<AccountData> ContinueFilter(int value, IEnumerable<AccountData> input)
        {
            var codeIndex = _repo.CodeData.GetIndex(value);

            return input.Where(x => x.CodeIndex == codeIndex);
        }

        protected override IEnumerable<AccountData> StartFilter(int value)
        {
            return _repo.CodeData.GetSortedIds(value).Select(x=>_repo.Accounts[x]);
        }

        protected override bool IsExisted()
        {
            return _repo.CodeData.ContainsValue(_value);
        }
    }

    public class PhoneNullIMFilter : NullFilterBase
    {
        public PhoneNullIMFilter(InMemoryRepository repo, int order, string value) : base(repo, order, value)
        {
        }

        public override string Field => Names.Phone;

        protected override IEnumerable<AccountData> ContinueFilter(int value, IEnumerable<AccountData> input)
        {
            if (value == 0)
            {
                return input.Where(x => x.Phone != 0);
            }
            else
            {
                return input.Where(x => x.Phone == 0);
            }
        }

        protected override IEnumerable<AccountData> StartFilter(int value)
        {
            if (value == 0)
            {
                return _repo.CodeData.GetSortedIdsNotDefault()
                    .Select(x => _repo.Accounts[x]);
            }
            else
            {
                return _repo.CodeData.GetSortedIds(_repo.CodeData.DefaultValue)
                    .Select(x => _repo.Accounts[x]);
            }
        }
    }
}
