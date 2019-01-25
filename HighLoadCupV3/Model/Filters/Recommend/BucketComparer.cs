using System;
using System.Collections.Generic;

namespace HighLoadCupV3.Model.Filters.Recommend
{
    public class BucketComparer : IComparer<Tuple<int, int>>
    {
        public int Compare(Tuple<int, int> x, Tuple<int, int> y)
        {
            return x.Item2 - y.Item2;
        }
    }
}
