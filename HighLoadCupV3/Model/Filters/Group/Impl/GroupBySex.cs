using System.Collections.Generic;
using HighLoadCupV3.Model.Dto;
using HighLoadCupV3.Model.InMemory;

namespace HighLoadCupV3.Model.Filters.Group.Impl
{
    public class GroupBySex : GroupByOneParameterBase
    {
        public GroupBySex(int sexCount, InMemoryRepository repo) : base(sexCount, repo)
        {
        }

        protected override void FillBuckets(IEnumerable<AccountData> accounts, int[] buckets)
        {
            foreach (var acc in accounts)
            {
                buckets[acc.Sex]++;
            }
        }

        protected override int[] ConvertBucketToData(int count, int index)
        {
            return new[] {count, index};
        }

        protected override void FillBuckets(int[][] data)
        {
            var ds = _repo.SexData;

            for (byte i = 0; i < _count; i++)
            {
                data[i] = new[] { ds.GetSortedIds(i).Count, i };
            }
        }

        protected override GroupResponseDto Convert(int key, int count)
        {
            var sex = _repo.SexData.GetValue((byte)key);

            var dto = new GroupResponseDto
            {
                Count = count,
                Sex = sex
            };

            return dto;
        }
    }
}