using System.Collections.Generic;

namespace HighLoadCupV3.Model.Filters.Group.Impl
{
    public class GroupByComparer : IComparer<int[]>
    {
        public int Compare(int[] x, int[] y)
        {
            if (x[0] != y[0])
            {
                return x[0] - y[0];
            }

            return x[1] - y[1];
        }
    }
}