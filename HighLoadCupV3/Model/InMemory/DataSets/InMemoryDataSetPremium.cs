using System.Collections.Generic;
using System.Linq;

namespace HighLoadCupV3.Model.InMemory.DataSets
{
    // For Status
    public class InMemoryDataSetPremium
    {
        private const int Count = 3;
        private readonly List<List<int>> _sorted = new List<List<int>>();

        private readonly IComparer<int> _comparer = new DescComparer();

        public InMemoryDataSetPremium()
        {
            for (int i = 0; i < Count; i++)
            {
                _sorted.Add(new List<int>());
            }
        }

        public void Add(byte value, int id, bool expired)
        {
            if (value == 1)
            {
                _sorted[1].Add(id);
            }
            else
            {
                if (!expired)
                {
                    _sorted[0].Add(id);
                }
                else
                {
                    _sorted[2].Add(id);
                }
            }
        }

        public string GetStatistics(bool full)
        {
            if (full)
            {
                return Count + " with " + string.Join(",", GetCountOfEachEntry());
            }

            return Count.ToString();
        }

        public int GetCount()
        {
            return Count;
        }

        public IEnumerable<int> GetCountOfEachEntry()
        {
            return _sorted.Select(x => x.Count);
        }

        public void Sort()
        {
            _sorted.ForEach(x => x.Sort(_comparer));
        }

        public void Update(byte value, int id, byte previousValue, bool previousExpired, bool currentExpired)
        {
            if (previousValue == 1)
            {
                _sorted[1].Remove(id);
            }
            else
            {
                if (!previousExpired)
                {
                    _sorted[0].Remove(id);
                }
                else
                {
                    _sorted[2].Remove(id);
                }
            }

            Add(value, id, currentExpired);
        }

        public List<int> GetSortedIds(byte value, bool expired)
        {
            if (value == 1)
            {
                return _sorted[1];
            }
            else
            {
                if (!expired)
                {
                    return _sorted[0];
                }

                return _sorted[2];
            }
        }
    }
}

