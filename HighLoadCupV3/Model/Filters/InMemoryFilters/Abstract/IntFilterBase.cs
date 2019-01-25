using HighLoadCupV3.Model.InMemory;

namespace HighLoadCupV3.Model.Filters.InMemoryFilters.Abstract
{
    public abstract class IntFilterBase : IMFilterBase<int>
    {
        protected IntFilterBase(InMemoryRepository repo, int order, string value) : base(repo, order, value)
        {
        }

        protected override void ValidateAndParseValue(string value)
        {
            if (!int.TryParse(value, out _value))
            {
                _isValid = false;
                return;
            }

            if (!IsExisted())
            {
                _isContainsData = false; 
            }
        }

        protected abstract bool IsExisted();
    }
}