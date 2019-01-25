using System.Collections.Generic;
using HighLoadCupV3.Model.Dto;
using HighLoadCupV3.Model.InMemory;

namespace HighLoadCupV3.Model.Filters.Group.Impl
{
    public class GroupByStatus : GroupByOneParameterBase
    {
        public GroupByStatus(int statusCount, InMemoryRepository repo) : base(statusCount, repo)
        {
        }

        protected override void FillBuckets(IEnumerable<AccountData> accounts, int[] buckets)
        {
            foreach (var acc in accounts)
            {
                buckets[acc.Status]++;
            }
        }

        protected override int[] ConvertBucketToData(int count, int index)
        {
            var ds = _repo.StatusData;
            return new[] {count, ds.GetSortedIndexByIndex((byte)index)};
        }

        protected override void FillBuckets(int[][] data)
        {
            var ds = _repo.StatusData;

            for (byte i = 0; i < _count; i++)
            {
                data[i] = new[] { ds.GetSortedIdsBySortedIndex(i).Count, i };
            }
        }

        protected override GroupResponseDto Convert(int key, int count)
        {
            var sortedStatusIndex = key;
            var status = _repo.StatusData.GetValueBySortedIndex((byte)sortedStatusIndex);

            var dto = new GroupResponseDto
            {
                Count = count,
                Status = status
            };

            return dto;
        }
    }
}