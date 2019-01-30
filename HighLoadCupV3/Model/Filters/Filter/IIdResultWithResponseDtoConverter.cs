using System.Collections.Generic;
using HighLoadCupV3.Model.InMemory;

namespace HighLoadCupV3.Model.Filters.Filter
{
    public interface IIdsToResponseConverter
    {
        object Convert(IEnumerable<AccountData> accounts, HashSet<string> requiredFields);
    }
}