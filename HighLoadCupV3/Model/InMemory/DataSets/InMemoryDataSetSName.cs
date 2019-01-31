using System;
using System.Collections.Generic;
using System.Linq;

namespace HighLoadCupV3.Model.InMemory.DataSets
{
    public class InMemoryDataSetSName : InMemoryDataSetBase
    {
        private readonly Dictionary<string, short> _valueToIndex = new Dictionary<string, short>();
        private readonly List<string> _indexToValue = new List<string>();

        private List<int> _notDefaultSorted = new List<int>();
        private HashSet<int> _notDefaultSet = new HashSet<int>();

        private string[] _sortedIndexToSortedValue;
        private short[] _indexToSortedIndex;
        private short[] _sortedIndexToIndex;

        private short _maxIndex = DefaultIndex + 1;
        public readonly string DefaultValue = string.Empty;
        public const short DefaultIndex = 0;

        public short GetIndex(string value)
        {
            return _valueToIndex[value];
        }

        public InMemoryDataSetSName()
        {
            _sorted.Add(new List<int>());
            _indexToValue.Add(DefaultValue);
            _valueToIndex[DefaultValue] = 0;
        }

        public short Add(string value, int id, bool afterPost)
        {
            if (afterPost)
            {
                if (!_valueToIndex.TryGetValue(value, out var index))
                {
                    _valueToIndex[value] = _maxIndex;
                    _indexToValue.Add(value);
                    _set.Add(new HashSet<int>());
                    index = _maxIndex;
                    _maxIndex++;
                }

                _set[index].Add(id);
                if (value != DefaultValue)
                {
                    _notDefaultSet.Add(id);
                }

                return index;

            }
            else
            {
                if (!_valueToIndex.TryGetValue(value, out var index))
                {
                    _valueToIndex[value] = _maxIndex;
                    _indexToValue.Add(value);
                    _sorted.Add(new List<int>());
                    index = _maxIndex;
                    _maxIndex++;
                }

                _sorted[index].Add(id);
                if (value != DefaultValue)
                {
                    _notDefaultSorted.Add(id);
                }

                return index;
            }
        }

        public string GetStatistics(bool full)
        {
            if (full)
            {
                return _maxIndex + " with " + string.Join(",", GetCountOfEachEntry());
            }

            return _maxIndex.ToString();
        }

        public IEnumerable<int> GetCountOfEachEntry()
        {
            return _sorted.Where(x=>x!=null).Select(x => x.Count);
        }

        public string GetValue(short index)
        {
            return _indexToValue[index];
        }

        public short GetSortedIndexByIndex(short index)
        {
            return _indexToSortedIndex[index];
        }

        public override void Sort()
        {
            base.Sort();
            _notDefaultSorted.Sort(_comparer);

            var sortedValueToIndexPairs = _valueToIndex.OrderBy(x => x.Key, StringComparer.Ordinal).ToArray();
            var length = sortedValueToIndexPairs.Length;
            _sortedIndexToSortedValue = new string[length];
            _indexToSortedIndex = new short[length];
            _sortedIndexToIndex = new short[length];

            for(short i = 0; i < length; i++)
            {
                _sortedIndexToSortedValue[i] = sortedValueToIndexPairs[i].Key;
                _sortedIndexToIndex[i] = sortedValueToIndexPairs[i].Value;
                _indexToSortedIndex[_sortedIndexToIndex[i]] = i;
            }
        }

        public override void PrepareForSort()
        {
            base.PrepareForSort();
            _notDefaultSorted = _notDefaultSet.ToList();
            _notDefaultSet = null;
        }

        public override void PrepareForUpdates()
        {
            base.PrepareForUpdates();
            _notDefaultSet = _notDefaultSorted.ToHashSet();
            _notDefaultSorted = null;
        }

        public Tuple<short, short> GetLowHigh(string value)
        {
            var low = Array.BinarySearch(_sortedIndexToSortedValue, value);
            if (low < 0)
            {
                low = ~low;
            }

            var high = _sortedIndexToSortedValue.Length - 1;
            for (var i = low; i < _sortedIndexToSortedValue.Length; i++)
            {
                if (!_sortedIndexToSortedValue[i].StartsWith(value, StringComparison.Ordinal))
                {
                    high = i - 1;
                    break;
                }
            }

            return Tuple.Create((short)low, (short)high);
        }

        public IEnumerable<List<int>> GetSortedIdsStartsWith(string prefix)
        {
            var lowHigh = GetLowHigh(prefix);
            if (lowHigh.Item1 > lowHigh.Item2)
            {
                yield break;
            }

            for (int i = lowHigh.Item1; i <= lowHigh.Item2; i++)
            {
                yield return _sorted[_sortedIndexToIndex[i]];
            }
        }

        public short UpdateOrAdd(string value, int id, short previousIndex)
        {
            _set[previousIndex].Remove(id);
            if(previousIndex != DefaultIndex)
            {
                _notDefaultSet.Remove(id);
            }

            return Add(value, id, true);
        }

        public bool ContainsValue(string key)
        {
            return _valueToIndex.ContainsKey(key);
        }

        public List<int> GetSortedIds(string value)
        {
            return _sorted[_valueToIndex[value]];
        }

        public List<int> GetSortedIdsNotDefault()
        {
            return _notDefaultSorted;
        }
    }
}

