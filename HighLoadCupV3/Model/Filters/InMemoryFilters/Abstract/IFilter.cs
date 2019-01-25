using System.Collections.Generic;
using HighLoadCupV3.Model.InMemory;

namespace HighLoadCupV3.Model.Filters.InMemoryFilters.Abstract
{
    public interface IFilter
    {
        IEnumerable<AccountData> Filter(IEnumerable<AccountData> input);
        bool IsExcluded();
        string Field { get; }
        int Order { get; }

        bool IsValid { get; }
        bool IsEmpty { get; }
    }
}