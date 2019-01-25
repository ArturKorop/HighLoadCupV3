using HighLoadCupV3.Model.InMemory;

namespace HighLoadCupV3.Model.Filters.InMemoryFilters.Abstract
{
    public abstract class NullFilterBase : IMFilterBase<int>
    {
        protected NullFilterBase(InMemoryRepository repo, int order, string value) : base(repo, order, value)
        {
        }

        protected override void ValidateAndParseValue(string value)
        {
            if (!int.TryParse(value, out _value) || (_value != 0 && _value != 1))
            {
                _isValid = false;
            }
        }

        public override bool IsExcluded()
        {
            return _value == 1;
        }
    }
}