using System.Collections.Generic;
using System.Linq;

namespace HighLoadCupV3.Model.InMemory.DataSets
{
    public abstract class InMemoryDataSetBase
    {
        protected List<List<int>> _sorted = new List<List<int>>();
        protected List<HashSet<int>> _set;
        protected readonly IComparer<int> _comparer = new DescComparer();

        public virtual void Sort()
        {
            _sorted.ForEach(x => x.Sort(_comparer));
        }

        public virtual void PrepareForSort()
        {
            _sorted = new List<List<int>>();
            foreach (var set in _set)
            {
                var list = set.ToList();
                _sorted.Add(list);
            }

            _set = null;
        }

        public virtual void PrepareForUpdates()
        {
            _set = new List<HashSet<int>>();
            foreach (var sortedList in _sorted)
            {
                _set.Add(sortedList.ToHashSet());
            }

            _sorted = null;
        }
    }
}