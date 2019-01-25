using System.Collections.Generic;
using HighLoadCupV3.Model.Dto;
using HighLoadCupV3.Model.InMemory;

namespace HighLoadCupV3.Model.Filters.Group.Impl
{
    public class GroupByInterests : GroupByOneParameterBase
    {
        public GroupByInterests(int interestsCount, InMemoryRepository repo) : base(interestsCount, repo)
        {
        }

        protected override void FillBuckets(IEnumerable<AccountData> accounts, int[] buckets)
        {
            foreach (var acc in accounts)
            {
                if (acc.Interests == null)
                {
                    continue;
                }

                foreach (var interest in acc.Interests)
                {
                    buckets[interest]++;
                }
            }
        }

        protected override int[] ConvertBucketToData(int count, int index)
        {
            return new[] { count, _repo.InterestsData.GetSortedIndexByIndex((short)index) };
        }

        protected override void FillBuckets(int[][] data)
        {
            var ds = _repo.InterestsData;
            for (byte i = 0; i < _count; i++)
            {
                data[i] = new[] { ds.GetSortedIdsBySortedIndex(i).Count, i };
            }
        }

        protected override GroupResponseDto Convert(int key, int count)
        {
            var sortedInterestsIndex = key;
            var interest = _repo.InterestsData.GetValueBySortedIndex((short)sortedInterestsIndex);

            var dto = new GroupResponseDto
            {
                Count = count,
                Interests = interest
            };

            return dto;
        }
    }
}