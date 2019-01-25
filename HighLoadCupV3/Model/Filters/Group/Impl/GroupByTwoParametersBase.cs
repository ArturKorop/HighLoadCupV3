using System;
using System.Collections.Generic;
using System.Linq;
using HighLoadCupV3.Model.Dto;
using HighLoadCupV3.Model.InMemory;

namespace HighLoadCupV3.Model.Filters.Group.Impl
{
    public abstract class GroupByTwoParametersBase : IGroupBy
    {
        protected readonly int _mainCount;
        protected readonly int _minorCount;
        protected readonly IComparer<int[]> _comparer = new GroupByComparer();

        protected readonly InMemoryRepository _repo;
        protected readonly List<GroupResponseDto>[] _fullCache = new List<GroupResponseDto>[2];
        protected Dictionary<string, List<GroupResponseDto>[]> _cache = new Dictionary<string, List<GroupResponseDto>[]>();

        protected GroupByTwoParametersBase(int mainCount, int minorCount, InMemoryRepository repo)
        {
            _mainCount = mainCount;
            _minorCount = minorCount;
            _repo = repo;
        }

        public IEnumerable<GroupResponseDto> GroupBy(IEnumerable<AccountData> accounts, int order)
        {
            var buckets = new int[_mainCount * _minorCount];
            foreach (var acc in accounts)
            {
                var key = GenerateUniqueKey(acc);
                buckets[key]++;
            }

            var data = new List<int[]>();
            for (int i = 0; i < buckets.Length; i++)
            {
                if (buckets[i] > 0)
                {
                    data.Add(new[] { buckets[i], GenerateOrderedKey(i) });
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

        public IEnumerable<GroupResponseDto> GroupByWithCache(int order, string cacheKey)
        {
            if (!_cache.ContainsKey(cacheKey))
            {
                return Enumerable.Empty<GroupResponseDto>();
            }

            var cacheIndex = order == -1 ? 0 : 1;

            return _cache[cacheKey][cacheIndex];
        }

        public void CreateCache(IEnumerable<AccountData> accounts, string cacheKey)
        {
            var buckets = new int[_mainCount * _minorCount];

            foreach (var acc in accounts)
            {
                var key = GenerateUniqueKey(acc);
                buckets[key]++;
            }

            var data = new List<int[]>();
            for (int i = 0; i < buckets.Length; i++)
            {
                if (buckets[i] > 0)
                {
                    data.Add(new[] { buckets[i], GenerateOrderedKey(i) });
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
            var buckets = new int[_mainCount, _minorCount];
            FillBuckets(buckets);

            var data = new List<int[]>();
            for (int i = 0; i < _mainCount; i++)
            {
                for (int j = 0; j < _minorCount; j++)
                {
                    if (buckets[i, j] > 0)
                    {
                        data.Add(new[] { buckets[i, j], GenerateOrderedKey(i, j) });
                    }
                }
            }

            data.Sort(_comparer);

            _fullCache[0] = new List<GroupResponseDto>();
            _fullCache[1] = new List<GroupResponseDto>();

            var to = Math.Min(data.Count, Names.GroupByCacheCount);
            for (int i = 0; i < to; i++)
            {
                _fullCache[1].Add(Convert(data[i][1], data[i][0]));
            }

            to = Math.Max(0, data.Count - Names.GroupByCacheCount);
            for (int i = data.Count-1; i >= to; i--)
            {
                _fullCache[0].Add(Convert(data[i][1], data[i][0]));
            }
        }

        protected abstract void FillBuckets(int[,] buckets);

        protected abstract GroupResponseDto Convert(int orderedKey, int count);

        protected abstract int GenerateOrderedKey(int uniqueKey);

        protected abstract int GenerateOrderedKey(int mainOrderedIndex, int minorOrderedIndex);

        protected abstract int GenerateUniqueKey(AccountData acc);
    }
}