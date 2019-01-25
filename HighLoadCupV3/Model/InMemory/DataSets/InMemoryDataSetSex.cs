using System.Collections.Generic;
using System.Linq;

namespace HighLoadCupV3.Model.InMemory.DataSets
{
    // For Sex
    public class InMemoryDataSetSex
    {
        private const int Count = 2;
        protected List<List<int>> _sorted = new List<List<int>>(Count);

        private readonly IComparer<int> _comparer = new DescComparer();

        public InMemoryDataSetSex()
        {
            for(int i = 0; i < Count; i++)
            {
                _sorted.Add(new List<int>());
            }
        }

        public void Add(byte value, int id)
        {
            _sorted[value].Add(id);
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
            return 2;
        }

        public IEnumerable<int> GetCountOfEachEntry()
        {
            return _sorted.Select(x => x.Count);
        }

        public void Sort()
        {
            _sorted.ForEach(x => x.Sort(_comparer));
        }

        public void Update(byte value, int id, byte previousValue)
        {
            _sorted[previousValue].Remove(id);
            Add(value, id);
        }

        public bool ContainsValue(string key)
        {
            return key == "m" || key == "f";
        }

        public List<int> GetSortedIds(byte value)
        {
            return _sorted[value];
        }

        public byte GetIndex(string value)
        {
            return value == "f" ? (byte)0 : (byte)1;
        }

        public string GetValue(byte value)
        {
            return value == 0 ? "f" : "m";
        }
    }
}

