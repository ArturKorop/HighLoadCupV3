using System.Collections.Generic;
using HighLoadCupV3.Model.Dto;
using HighLoadCupV3.Model.InMemory;

namespace HighLoadCupV3.Model.Filters.Group.Impl
{
    public class GroupByCity : GroupByOneParameterBase
    {
        public GroupByCity(int cityCount, InMemoryRepository repo) : base(cityCount, repo)
        {
        }

        protected override void FillBuckets(IEnumerable<AccountData> accounts, int[] buckets)
        {
            foreach (var acc in accounts)
            {
                buckets[acc.CityIndex]++;
            }
        }

        protected override int[] ConvertBucketToData(int count, int index)
        {
            return new[] {count, _repo.CityData.GetSortedIndexByIndex((short)index)};
        }

        protected override void FillBuckets(int[][] data)
        {
            var ds = _repo.CityData;
            for (short i = 0; i < _count; i++)
            {
                data[i] = new[] { ds.GetSortedIdsBySortedIndex(i).Count, i };
            }
        }

        protected override GroupResponseDto Convert(int key, int count)
        {
            var sortedCityIndex = key;
            var city = _repo.CityData.GetValueBySortedIndex((short) sortedCityIndex);

            var dto = new GroupResponseDto
            {
                Count = count,
                City = city == string.Empty ? null : city
            };

            return dto;
        }
    }
}

