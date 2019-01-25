namespace HighLoadCupV3.Model.Filters.Recommend
{
    public class InterestsIntersectCalculator
    {
        public int Calculate(byte[] a, byte[] b)
        {
            var count = 0;
            var i = 0;
            var j = 0;

            while (i < a.Length && j < b.Length)
            {
                if (a[i] == b[j])
                {
                    count++;
                    i++;
                    j++;
                }
                else if (a[i] < b[j])
                {
                    i++;
                }
                else
                {
                    j++;
                }
            }

            return count;
        }
    }
}
