using HighLoadCupV3.Model.InMemory;

namespace HighLoadCupV3.Model.Filters.InMemoryFilters.Abstract
{
    public abstract class StringFilterBase : IMFilterBase<string>
    {
        protected StringFilterBase(InMemoryRepository repo, int order, string value) : base(repo, order, value)
        {
        }

        protected override void ValidateAndParseValue(string value)
        {
            _value = value;

            if (!IsExisted())
            {
                _isContainsData = false;
            }

        }

        protected abstract bool IsExisted();
    }
}