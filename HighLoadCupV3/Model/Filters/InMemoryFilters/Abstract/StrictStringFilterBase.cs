using HighLoadCupV3.Model.InMemory;

namespace HighLoadCupV3.Model.Filters.InMemoryFilters.Abstract
{
    public abstract class StrictStringFilterBase : IMFilterBase<string>
    {
        protected StrictStringFilterBase(InMemoryRepository repo, int order, string value) : base(repo, order, value)
        {
        }

        protected override void ValidateAndParseValue(string value)
        {
            _value = value;

            if (!IsExisted())
            {
                _isValid = false;
            }
        }

        protected abstract bool IsExisted();
    }
}