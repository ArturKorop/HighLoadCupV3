using System.Collections.Generic;
using System.Linq;

namespace HighLoadCupV3.Model.InMemory.DataSets
{
    // For FNameIndex
    // For CodeIndex
    public class InMemoryDataSetByteWithNotExisted<T> : InMemoryDataSetBase
    {
        protected Dictionary<T, byte> _valueToIndex = new Dictionary<T, byte>();
        protected List<T> _indexToValue = new List<T>();
        protected HashSet<int> _notDefaultSet = new HashSet<int>();

        private List<int> _notDefaultSorted = new List<int>();

        private byte _maxIndex = 1;
        public readonly byte DefaultIndex = 0;
        public readonly T DefaultValue;

        public InMemoryDataSetByteWithNotExisted(T defaultValue)
        {
            DefaultValue = defaultValue;
            _sorted = new List<List<int>>();
            _indexToValue = new List<T>();

            _sorted.Add(new List<int>());
            _indexToValue.Add(defaultValue);
            _valueToIndex[defaultValue] = 0;
        }

        public byte GetIndex(T value)
        {
            return _valueToIndex[value];
        }

        public byte Add(T value, int id, bool afterPost)
        {
            byte index;
            if (afterPost)
            {
                if (!_valueToIndex.TryGetValue(value, out index))
                {
                    _valueToIndex[value] = _maxIndex;
                    _indexToValue.Add(value);
                    _set.Add(new HashSet<int>());
                    index = _maxIndex;
                    _maxIndex++;
                }

                _set[index].Add(id);
                if (!value.Equals(DefaultValue))
                {
                    _notDefaultSet.Add(id);
                }
            }
            else
            {
                if (!_valueToIndex.TryGetValue(value, out index))
                {
                    _valueToIndex[value] = _maxIndex;
                    _indexToValue.Add(value);
                    _sorted.Add(new List<int>());
                    index = _maxIndex;
                    _maxIndex++;
                }

                _sorted[index].Add(id);
                if (!value.Equals(DefaultValue))
                {
                    _notDefaultSorted.Add(id);
                }
            }

            return index;
        }

        public string GetStatistics(bool full)
        {
            if (full)
            {
                return _maxIndex + " with " + string.Join(",", GetCountOfEachEntry());
            }

            return _maxIndex.ToString();
        }

        public int GetCount()
        {
            return _maxIndex;
        }

        public IEnumerable<int> GetCountOfEachEntry()
        {
            return _sorted.Where(x=>x!=null).Select(x => x.Count);
        }

        public T GetValue(byte index)
        {
            return _indexToValue[index];
        }

        public override void Sort()
        {
            base.Sort();
            _notDefaultSorted.Sort(_comparer);
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
            _notDefaultSet = new HashSet<int>(_notDefaultSorted);
            _notDefaultSorted = null;
        }

        public byte UpdateOrAdd(T value, int id, byte previousIndex)
        {
            _set[previousIndex].Remove(id);
            if (!previousIndex.Equals(DefaultIndex))
            {
                _notDefaultSet.Remove(id);
            }

            return Add(value, id, true);
        }

        public bool ContainsValue(T key)
        {
            return _valueToIndex.ContainsKey(key);
        }

        public IEnumerable<List<int>> GetSortedIds(IEnumerable<T> values)
        {
            foreach (var value in values)
            {
                if (_valueToIndex.TryGetValue(value, out var index))
                {
                    yield return _sorted[index];
                }
            }
        }

        public List<int> GetSortedIds(T value)
        {
            return _sorted[_valueToIndex[value]];
        }

        public List<int> GetSortedIdsNotDefault()
        {
            return _notDefaultSorted;
        }
    }
}

