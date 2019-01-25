using System.Collections.Generic;

namespace HighLoadCupV3.Model.InMemory
{
    public class DescComparer : IComparer<int>
    {
        public int Compare(int x, int y)
        {
            return y - x;
        }
    }
}

