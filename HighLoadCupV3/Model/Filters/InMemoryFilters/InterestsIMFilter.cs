using System;
using System.Collections.Generic;
using System.Linq;
using HighLoadCupV3.Model.Filters.InMemoryFilters.Abstract;
using HighLoadCupV3.Model.InMemory;

namespace HighLoadCupV3.Model.Filters.InMemoryFilters
{
    public class InterestsContainsIMFilter : IMFilterBase<byte[]>
    {
        public InterestsContainsIMFilter(InMemoryRepository repo, int order, string value) : base(repo, order, value)
        {
        }

        protected override void ValidateAndParseValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                _isValid = false;
                return;
            }

            var interests = value.Split(',');
            _value = new byte[interests.Length];

            for (int i = 0; i < interests.Length; i++)
            {
                if (!_repo.InterestsData.ContainsValue(interests[i]))
                {
                    _isContainsData = false;
                    return;
                }

                _value[i] = _repo.InterestsData.GetIndex(interests[i]);
            }

            Array.Sort(_value);
        }

        public override string Field => Names.Interests;

        protected override IEnumerable<AccountData> ContinueFilter(byte[] value, IEnumerable<AccountData> input)
        {
            var desiredCount = value.Length;

            return input.Where(x =>
            {
                var interests = x.Interests;
                if (interests == null) return false;
                if (interests.Length < desiredCount) return false;

                return IsContainsAllRequired(interests, value);
            });
        }

        protected override IEnumerable<AccountData> StartFilter(byte[] value)
        {
            if (value.Length == 1)
            {
                return _repo.InterestsData.GetSortedIds(value[0]).Select(x => _repo.Accounts[x]);
            }
            else
            {

                var interestsSorted = _repo.InterestsData.GetSortedIds(value).ToList();

                HashSet<int> ids = interestsSorted[0].ToHashSet();

                for (int i = 1; i < interestsSorted.Count; i++)
                {
                    ids.IntersectWith(interestsSorted[i]);
                }

                return ids.OrderByDescending(x => x).Select(x => _repo.Accounts[x]);
            }
        }

        public override bool IsExcluded()
        {
            return true;
        }

        private bool IsContainsAllRequired(byte[] target, byte[] required)
        {
            int i = 0;
            int j = 0;
            var count = 0;

            while (i < target.Length && j < required.Length)
            {
                if (target[i] == required[j])
                {
                    count++;
                    i++;
                    j++;
                }
                else if (target[i] < required[j])
                {
                    i++;
                }
                else
                {
                    return false;
                }
            }

            return j == required.Length;
        }
    }

    public class InterestsAnyIMFilter : IMFilterBase<HashSet<byte>>
    {
        public InterestsAnyIMFilter(InMemoryRepository repo, int order, string value) : base(repo, order, value)
        {
        }

        protected override void ValidateAndParseValue(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                _isValid = false;
                return;
            }

            var interests = value.Split(',');
            _value = new HashSet<byte>();

            foreach (var interest in interests)
            {
                if (_repo.InterestsData.ContainsValue(interest))
                {
                    _value.Add(_repo.InterestsData.GetIndex(interest));
                }
            }

            if (_value.Count == 0)
            {
                _isContainsData = false;
                return;
            }
        }

        public override string Field => Names.Interests;

        protected override IEnumerable<AccountData> ContinueFilter(HashSet<byte> value, IEnumerable<AccountData> input)
        {
            return input.Where(x => x.Interests != null && x.Interests.Any(i => value.Contains(i)));
        }

        protected override IEnumerable<AccountData> StartFilter(HashSet<byte> value)
        {
            if (value.Count == 1)
            {
                return _repo.InterestsData.GetSortedIds(value.First()).Select(x => _repo.Accounts[x]);
            }
            else
            {
                var interestsSorted = _repo.InterestsData.GetSortedIds(value);

                return EnumeratorHelper.EnumerateDuplicates(interestsSorted).Select(x => _repo.Accounts[x]);
            }
        }

        public override bool IsExcluded()
        {
            return true;
        }
    }
}
