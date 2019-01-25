using System;
using System.Collections.Generic;
using System.Linq;
using HighLoadCupV3.Model.Dto;
using HighLoadCupV3.Model.InMemory;

namespace HighLoadCupV3.Model.Filters.Group.Impl
{
    public abstract class GroupByOneParameterBase : IGroupBy
    {
        protected readonly int _count;
        protected readonly IComparer<int[]> _comparer = new GroupByComparer();

        protected readonly InMemoryRepository _repo;
        protected readonly List<GroupResponseDto>[] _fullCache = new List<GroupResponseDto>[2];
        protected Dictionary<string, List<GroupResponseDto>[]> _cache = new Dictionary<string, List<GroupResponseDto>[]>();

        protected GroupByOneParameterBase(int count, InMemoryRepository repo)
        {
            _count = count;
            _repo = repo;
        }

        public IEnumerable<GroupResponseDto> GroupBy(IEnumerable<AccountData> accounts, int order)
        {
            var buckets = new int[_count];

            FillBuckets(accounts, buckets);

            var data = new List<int[]>();

            for (short i = 0; i < buckets.Length; i++)
            {
                if (buckets[i] > 0)
                {
                    data.Add(ConvertBucketToData(buckets[i], i));
                }
            }

            data.Sort(_comparer);

            if (order == 1)
            {
                for (int i = 0; i < data.Count; i++)
                {
                    yield return Convert(data[i][1], data[i][0]);
                }
            }
            else
            {
                for (int i = data.Count - 1; i >= 0; i--)
                {
                    yield return Convert(data[i][1], data[i][0]);
                }
            }
        }

        public void CreateCache(IEnumerable<AccountData> accounts, string cacheKey)
        {
            var buckets = new int[_count];

            FillBuckets(accounts, buckets);

            var data = new List<int[]>();

            for (short i = 0; i < buckets.Length; i++)
            {
                if (buckets[i] > 0)
                {
                    data.Add(ConvertBucketToData(buckets[i], i));
                }
            }

            data.Sort(_comparer);

            var cacheEntry = new List<GroupResponseDto>[2];
            cacheEntry[0] = new List<GroupResponseDto>();
            cacheEntry[1] = new List<GroupResponseDto>();

            var to = Math.Min(data.Count, Names.GroupByCacheCount);
            for (int i = 0; i < to; i++)
            {
                cacheEntry[1].Add(Convert(data[i][1], data[i][0]));
            }

            to = Math.Max(0, data.Count - Names.GroupByCacheCount);
            for (int i = data.Count - 1; i >= to; i--)
            {
                cacheEntry[0].Add(Convert(data[i][1], data[i][0]));
            }

            _cache[cacheKey] = cacheEntry;
        }

        public IEnumerable<GroupResponseDto> GroupByWithCache(int order, string cacheKey)
        {
            if (!_cache.ContainsKey(cacheKey))
            {
                return Enumerable.Empty<GroupResponseDto>();
            }

            var cacheIndex = order == -1 ? 0 : 1;

            return _cache[cacheKey][cacheIndex];
        }

        public IEnumerable<GroupResponseDto> GroupBy(int order)
        {
            var cacheIndex = order == -1 ? 0 : 1;
            if (_fullCache[cacheIndex] != null)
            {
                return _fullCache[cacheIndex];
            }

            FillFullCache();

            return _fullCache[cacheIndex];
        }

        private void FillFullCache()
        {
            var data = new int[_count][];
            FillBuckets(data);

            Array.Sort(data, _comparer);

            _fullCache[0] = new List<GroupResponseDto>();
            _fullCache[1] = new List<GroupResponseDto>();

            var count = 0;
            for (int i = 0; i < data.Length; i++)
            {
                if (data[i][0] > 0)
                {
                    count++;
                    _fullCache[1].Add(Convert(data[i][1], data[i][0]));
                    if (count == Names.GroupByCacheCount)
                    {
                        break;
                    }
                }
            }

            count = 0;
            for (int i = data.Length - 1; i >= 0; i--)
            {
                if (data[i][0] > 0)
                {
                    count++;
                    _fullCache[0].Add(Convert(data[i][1], data[i][0]));
                    if (count == Names.GroupByCacheCount)
                    {
                        break;
                    }
                }
            }
        }

        protected abstract void FillBuckets(IEnumerable<AccountData> accounts, int[] buckets);

        protected abstract int[] ConvertBucketToData(int count, int index);

        protected abstract void FillBuckets(int[][] data);

        protected abstract GroupResponseDto Convert(int key, int count);
    }
}