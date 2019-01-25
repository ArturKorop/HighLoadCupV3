using System.Collections.Generic;
using HighLoadCupV3.Model.Dto;
using HighLoadCupV3.Model.InMemory;

namespace HighLoadCupV3.Model.Filters.Group.Impl
{
    public class GroupByCountry : GroupByOneParameterBase
    {
        public GroupByCountry(int countryCount, InMemoryRepository repo) : base(countryCount, repo)
        {
        }

        protected override void FillBuckets(IEnumerable<AccountData> accounts, int[] buckets)
        {
            foreach (var acc in accounts)
            {
                buckets[acc.CountryIndex]++;
            }
        }

        protected override int[] ConvertBucketToData(int count, int index)
        {
            return new[] { count, _repo.CountryData.GetSortedIndexByIndex((byte)index) };
        }

        protected override void FillBuckets(int[][] data)
        {
            var ds = _repo.CountryData;
            for (byte i = 0; i < _count; i++)
            {
                data[i] = new[] { ds.GetSortedIdsBySortedIndex(i).Count, i };
            }
        }

        protected override GroupResponseDto Convert(int key, int count)
        {
            var sortedCountryIndex = key;
            var country = _repo.CountryData.GetValueBySortedIndex((byte)sortedCountryIndex);

            var dto = new GroupResponseDto
            {
                Count = count,
                Country = country == string.Empty ? null : country
            };

            return dto;
        }
    }
}

