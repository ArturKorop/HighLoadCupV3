using System.Collections.Generic;
using HighLoadCupV3.Model.InMemory;

namespace HighLoadCupV3.Model.Filters.Filter
{
    public interface IIdsToResponseConverter
    {
        string Convert(IEnumerable<AccountData> accounts, HashSet<string> requiredFields);
    }
}