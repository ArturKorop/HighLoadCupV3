using System.Collections.Generic;
using HighLoadCupV3.Model.Dto;
using HighLoadCupV3.Model.InMemory;

namespace HighLoadCupV3.Model.Filters.Group
{
    public interface IGroupBy
    {
        IEnumerable<GroupResponseDto> GroupBy(IEnumerable<AccountData> accounts, int order);
        IEnumerable<GroupResponseDto> GroupBy(int order);
        IEnumerable<GroupResponseDto> GroupByWithCache(int order, string cacheKey);
        void CreateCache(IEnumerable<AccountData> accounts, string cacheKey);
    }
}