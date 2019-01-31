using System.Collections.Generic;
using System.Linq;

namespace HighLoadCupV3.Model.InMemory.DataSets
{
    // For BirthYear
    // For Domain
    // For JoinedYear
    public class InMemoryDataSetByteWithAlwaysExisted<T> : InMemoryDataSetBase
    {
        protected Dictionary<T, byte> _valueToIndex = new Dictionary<T, byte>();
        protected List<T> _indexToValue;

        private byte _maxIndex;

        public InMemoryDataSetByteWithAlwaysExisted()
        {
            _indexToValue = new List<T>();
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
            }

            return index;
        }

        public string GetStatistics(bool full)
        {
            if (full)
            {
                return GetCount() + " with " + string.Join(",", GetCountOfEachEntry());
            }

            return GetCount().ToString();
        }

        public int GetCount() 
        {
            return _maxIndex;
        }

        public IEnumerable<int> GetCountOfEachEntry()
        {
            return _sorted.Where(x=>x != null).Select(x => x.Count);
        }

        public T GetValue(byte index)
        {
            return _indexToValue[index];
        }

        public byte UpdateOrAdd(T value, int id, byte previousIndex)
        {
            if (previousIndex != byte.MinValue)
            {
                _set[previousIndex].Remove(id);
            }

            return Add(value, id, true);
        }

        public bool ContainsValue(T value)
        {
            return _valueToIndex.ContainsKey(value);
        }

        public byte GetIndex(T value)
        {
            return _valueToIndex[value];
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
    }
}

