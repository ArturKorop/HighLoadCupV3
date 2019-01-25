using System.Collections.Generic;
using HighLoadCupV3.Model.InMemory;

namespace HighLoadCupV3.Model.Filters.InMemoryFilters.Abstract
{
    public abstract class IMFilterBase<T> : IFilter
    {
        protected InMemoryRepository _repo;
        protected T _value;
        protected bool _isValid = true;
        protected bool _isContainsData = true;

        protected IMFilterBase(InMemoryRepository repo, int order, string value)
        {
            _repo = repo;
            Order = order;
            // ReSharper disable once VirtualMemberCallInConstructor
            ValidateAndParseValue(value);
        }

        public IEnumerable<AccountData> Filter(IEnumerable<AccountData> input)
        {
            if (input == null)
            {
                return StartFilter(_value);
            }

            return ContinueFilter(_value, input);
        }

        public virtual bool IsExcluded()
        {
            return false;
        }

        protected abstract IEnumerable<AccountData> StartFilter(T value);

        protected abstract IEnumerable<AccountData> ContinueFilter(T value, IEnumerable<AccountData> input);

        protected abstract void ValidateAndParseValue(string value);

        public abstract string Field { get; }

        public int Order { get; }

        public bool IsValid => _isValid;

        public bool IsEmpty => !_isContainsData;
    }
}
