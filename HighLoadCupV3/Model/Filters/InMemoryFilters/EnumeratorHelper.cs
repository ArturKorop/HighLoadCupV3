using System.Collections.Generic;
using System.Linq;

namespace HighLoadCupV3.Model.Filters.InMemoryFilters
{
    public static class EnumeratorHelper
    {
        public static IEnumerable<int> EnumerateUnique(IEnumerable<List<int>> input)
        {
            var inputList = input.ToList();
            var indexes = new List<int>();
            for (int i = 0; i < inputList.Count; i++)
            {
                indexes.Add(0);
            }

            while (inputList.Count > 0)
            {
                var max = inputList[0][indexes[0]];
                var maxIndex = 0;

                for (int i = 1; i < inputList.Count; i++)
                {
                    var curInd = indexes[i];
                    if (inputList[i][curInd] > max)
                    {
                        max = inputList[i][curInd];
                        maxIndex = i;
                    }
                }

                indexes[maxIndex]++;
                if (indexes[maxIndex] == inputList[maxIndex].Count)
                {
                    inputList.RemoveAt(maxIndex);
                    indexes.RemoveAt(maxIndex);
                }

                yield return max;
            }
        }

        public static IEnumerable<int> EnumerateDuplicates(IEnumerable<List<int>> input)
        {
            var inputList = input.ToList();
            var indexes = new List<int>();
            for (int i = 0; i < inputList.Count; i++)
            {
                indexes.Add(0);
            }

            var prev = -1;
            while (inputList.Count > 0)
            {
                var max = inputList[0][indexes[0]];
                var maxIndex = 0;

                for (int i = 1; i < inputList.Count; i++)
                {
                    var curInd = indexes[i];
                    if (inputList[i][curInd] > max)
                    {
                        max = inputList[i][curInd];
                        maxIndex = i;
                    }
                }

                indexes[maxIndex]++;
                if (indexes[maxIndex] == inputList[maxIndex].Count)
                {
                    inputList.RemoveAt(maxIndex);
                    indexes.RemoveAt(maxIndex);
                }

                if (prev != max)
                {
                    prev = max;

                    yield return max;
                }
            }
        }
    }
}
