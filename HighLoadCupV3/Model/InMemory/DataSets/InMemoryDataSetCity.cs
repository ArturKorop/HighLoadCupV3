using System;
using System.Collections.Generic;
using System.Linq;

namespace HighLoadCupV3.Model.InMemory.DataSets
{
    // For City
    public class InMemoryDataSetCity : InMemoryDataSetBase
    {
        private readonly Dictionary<string, short> _valueToIndex = new Dictionary<string, short>();
        private readonly List<string> _indexToValue = new List<string>();

        private List<int> _notDefaultSorted = new List<int>();
        private HashSet<int> _notDefaultSet = new HashSet<int>();

        private short[] _indexToSortedIndex;
        private string[] _sortedIndexToValue;
        private short[] _sortedIndexToIndex;

        private short _maxIndex = 1;
        public readonly string DefaultValue;
        public readonly short DefaultIndex = 0;

        public InMemoryDataSetCity(string defaultValue)
        {
            DefaultValue = defaultValue;

            _sorted.Add(new List<int>());
            _indexToValue.Add(defaultValue);
            _valueToIndex[defaultValue] = 0;
        }

        public short GetIndex(string value)
        {
            return _valueToIndex[value];
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

            return GetCount().ToString();
        }

        public int GetCount()
        {
            return _maxIndex;
        }

        public IEnumerable<int> GetCountOfEachEntry()
        {
            return _sorted.Where(x => x != null).Select(x => x.Count);
        }

        public string GetValue(short index)
        {
            return _indexToValue[index];
        }

        public short GetSortedIndexByIndex(short index)
        {
            return _indexToSortedIndex[index];
        }

        public string GetValueBySortedIndex(short sortedIndex)
        {
            return _sortedIndexToValue[sortedIndex];
        }

        public override void Sort()
        {
            base.Sort();
            _notDefaultSorted.Sort(_comparer);


            var sortedValueToIndexPairs = _valueToIndex.OrderBy(x => x.Key, StringComparer.Ordinal).ToArray();
            var length = sortedValueToIndexPairs.Length;
            _sortedIndexToValue = new string[length];
            _indexToSortedIndex = new short[length];
            _sortedIndexToIndex = new short[length];

            for (short i = 0; i < length; i++)
            {
                _sortedIndexToValue[i] = sortedValueToIndexPairs[i].Key;
                _indexToSortedIndex[sortedValueToIndexPairs[i].Value] = i;
                _sortedIndexToIndex[i] = sortedValueToIndexPairs[i].Value;
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

        public short UpdateOrAdd(string value, int id, short previousIndex)
        {
            _set[previousIndex].Remove(id);
            if (previousIndex != DefaultIndex)
            {
                _notDefaultSet.Remove(id);
            }

            return Add(value, id, true);
        }

        public bool ContainsValue(string key)
        {
            return _valueToIndex.ContainsKey(key);
        }

        public IEnumerable<List<int>> GetSortedIds(IEnumerable<string> values)
        {
            foreach (var value in values)
            {
                if (_valueToIndex.TryGetValue(value, out var index))
                {
                    yield return _sorted[index];
                }
            }
        }

        public List<int> GetSortedIds(string value)
        {
            return _sorted[_valueToIndex[value]];
        }

        public List<int> GetSortedIdsBySortedIndex(short sortedIndex)
        {
            return _sorted[_sortedIndexToIndex[sortedIndex]];
        }


        public List<int> GetSortedIdsNotDefault()
        {
            return _notDefaultSorted;
        }
    }
}

