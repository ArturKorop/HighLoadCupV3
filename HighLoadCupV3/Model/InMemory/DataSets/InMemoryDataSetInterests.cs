using System;
using System.Collections.Generic;
using System.Linq;

namespace HighLoadCupV3.Model.InMemory.DataSets
{
    public class InMemoryDataSetInterests
    {
        private readonly List<List<int>> _sorted = new List<List<int>>();
        private readonly List<List<int>[]> _dataForRecommend = new List<List<int>[]>();
        private readonly Dictionary<string, byte> _valueToIndex = new Dictionary<string, byte>();
        private readonly List<string> _indexToValue = new List<string>();

        private short[] _indexToSortedIndex;
        private short[] _sortedIndexToIndex;
        private string[] _sortedIndexToValue;

        private byte _maxIndex;

        private readonly IComparer<int> _comparer = new DescComparer();

        public IEnumerable<byte> Add(IEnumerable<string> values, int id, byte premium, byte status, byte sex)
        {
            foreach (var value in values)
            {
                if (!_valueToIndex.TryGetValue(value, out var index))
                {
                    _valueToIndex[value] = _maxIndex;
                    _indexToValue.Add(value);
                    _sorted.Add(new List<int>());

                    _dataForRecommend.Add(new List<int>[12]);
                    index = _maxIndex;

                    for (int i = 0; i < 12; i++)
                    {
                        _dataForRecommend[index][i] = new List<int>();
                    }

                    _maxIndex++;
                }

                _sorted[index].Add(id);

                var key = GenerateBucketKey(premium, status, sex);
                _dataForRecommend[index][key].Add(id);

                yield return index;
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

        public string GetValue(byte index)
        {
            return _indexToValue[index];
        }

        public short GetSortedIndexByIndex(int index)
        {
            return _indexToSortedIndex[index];
        }

        public string GetValueBySortedIndex(int sortedIndex)
        {
            return _sortedIndexToValue[sortedIndex];
        }

        public void Sort()
        {
            foreach (var sorted in _sorted)
            {
                sorted.Sort(_comparer);
            }

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

        public IEnumerable<byte> UpdateOrAdd(IEnumerable<string> values, int id, IEnumerable<byte> previousIndexes, byte prevPremium, byte prevStatus, byte prevSex, byte premium, byte status, byte sex)
        {
            var key = GenerateBucketKey(prevPremium, prevStatus, prevSex);
            foreach (var index in previousIndexes)
            {
                _sorted[index].Remove(id);
                _dataForRecommend[index][key].Remove(id);
            }

            return Add(values, id, premium, status, sex);
        }

        public void UpdateRecommendationsData(int id, byte[] indexes, byte prevPremium, byte prevStatus, byte prevSex, byte premium, byte status, byte sex)
        {
            var prevKey = GenerateBucketKey(prevPremium, prevStatus, prevSex);
            foreach (var index in indexes)
            {
                _dataForRecommend[index][prevKey].Remove(id);
            }

            var newKey = GenerateBucketKey(premium, status, sex);
            foreach (var index in indexes)
            {
                _dataForRecommend[index][newKey].Add(id);
            }
        }

        public bool ContainsValue(string value)
        {
            return _valueToIndex.ContainsKey(value);
        }

        public byte GetIndex(string value)
        {
            return _valueToIndex[value];
        }

        public List<int> GetDataForRecommend(int index, int key)
        {
            return _dataForRecommend[index][key];
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

        public IEnumerable<List<int>> GetSortedIds(IEnumerable<byte> indexes)
        {
            foreach (var index in indexes)
            {
                yield return _sorted[index];
            }
        }

        public List<int> GetSortedIds(string value)
        {
            return _sorted[_valueToIndex[value]];
        }

        public List<int> GetSortedIds(byte index)
        {
            return _sorted[index];
        }

        public List<int> GetSortedIdsBySortedIndex(byte sortedIndex)
        {
            return _sorted[_sortedIndexToIndex[sortedIndex]];
        }

        public int GetCount()
        {
            return _maxIndex;
        }

        private int GenerateBucketKey(byte premium, byte status, byte sex)
        {
            var key = 0;
            if (premium == 1)
            {
                key = 3;
            }

            key += status;

            key += sex * 6;

            return key;
        }
    }
}

