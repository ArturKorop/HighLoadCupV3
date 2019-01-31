using System.Collections.Generic;
using System.Linq;

namespace HighLoadCupV3.Model.InMemory.DataSets
{
    // For Status
    public class InMemoryDataSetStatus : InMemoryDataSetBase
    {
        private const int Count = 3;

        private const string Free = "свободны";
        private const string Busy = "заняты";
        private const string Hard = "всё сложно";

        private readonly string[] _values = {Busy, Hard, Free};
        private readonly byte[] _sortedIndexes = {1, 0, 2};
        private readonly string[] _sortedValues = {Hard, Busy, Free};

        public InMemoryDataSetStatus()
        {
            for (int i = 0; i < Count; i++)
            {
                _sorted.Add(new List<int>());
            }
        }

        public void Add(byte value, int id, bool afterPost)
        {
            if (afterPost)
            {
                _set[value].Add(id);
            }
            else
            {
                _sorted[value].Add(id);
            }
        }

        public void Update(byte value, int id, byte previousValue)
        {
            _set[previousValue].Remove(id);
            _set[value].Add(id);
        }

        public string GetStatistics(bool full)
        {
            if (full)
            {
                return Count + " with " + string.Join(",", GetCountOfEachEntry());
            }

            return GetCount().ToString();
        }

        public int GetCount()
        {
            return Count;
        }

        public IEnumerable<int> GetCountOfEachEntry()
        {
            return _sorted.Select(x => x.Count);
        }

        public bool ContainsValue(string key)
        {
            return key == Free || key == Busy || key == Hard;
        }

        public List<int> GetSortedIds(byte value)
        {
            return _sorted[value];
        }

        public byte GetIndex(string value)
        {
            switch (value)
            {
                case Free:
                    return 2;
                case Hard:
                    return 1;
                default:
                    return 0;
            }
        }

        public string GetValue(byte value)
        {
            return _values[value];
        }

        public byte GetSortedIndexByIndex(byte index)
        {
            return _sortedIndexes[index];
        }

        public string GetValueBySortedIndex(byte sortedIndex)
        {
            return _sortedValues[sortedIndex];
        }

        public List<int> GetSortedIdsBySortedIndex(byte sortedIndex)
        {
            return _sorted[_sortedIndexes[sortedIndex]];
        }
    }
}

