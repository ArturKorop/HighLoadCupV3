using System;
using System.Collections.Generic;
using System.Linq;
using HighLoadCupV3.Model.Dto;
using HighLoadCupV3.Model.InMemory;
using Newtonsoft.Json;

namespace HighLoadCupV3.Model.Filters.Suggest
{
    public class Suggest
    {
        private readonly InMemoryRepository _repo;
        private readonly DescComparer _descComparer = new DescComparer();
        private readonly SuggestResponseDtoConverter _converter;
        private readonly SuggestFilterFactory _factory;

        public Suggest(InMemoryRepository repo)
        {
            _repo = repo;
            _converter = new SuggestResponseDtoConverter(repo);
            _factory = new SuggestFilterFactory(repo);
        }

        public string GetSuggestions(int id, int limit, string key, string value)
        {
            if (!_repo.Accounts[id].AnyLikesFrom())
            {
                var holder2 = new SuggestAccountsResponseDto { Accounts = Enumerable.Empty<SuggestResponseDto>() };

                return JsonConvert.SerializeObject(holder2,
                    new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
            }

            var suggestions = SuggestForV2(id, limit, key, value);
            var holder = new SuggestAccountsResponseDto { Accounts = suggestions };

            return JsonConvert.SerializeObject(holder,
                new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        }

        public IEnumerable<SuggestResponseDto> SuggestForV2(int id, int limit, string key, string value)
        {
            var accounts = _repo.Accounts;

            var uniqueLikesOfTarget = accounts[id].GetLikesFrom().ToHashSet();
            var uniqueLikesOfTargetArray = uniqueLikesOfTarget.ToArray();

            ISuggestFilter filter = null;
            if (key != null)
            {
                filter = _factory.CreateFilter(key, value);
                if (filter == null)
                {
                    return Enumerable.Empty<SuggestResponseDto>();
                }
            }

            var likersData = new Dictionary<int, List<int[]>>();
            foreach (var likeeId in uniqueLikesOfTargetArray)
            {
                var likersOfTargetLike = accounts[likeeId].GetLikesToWithTs();
                foreach (var idTsPair in likersOfTargetLike)
                {
                    if (filter == null || filter.IsOk(idTsPair.Item1) || idTsPair.Item1 == id)
                    {
                        if (!likersData.ContainsKey(idTsPair.Item1))
                        {
                            likersData[idTsPair.Item1] = new List<int[]> { new int[] { likeeId, idTsPair.Item2 } };
                        }
                        else
                        {
                            likersData[idTsPair.Item1].Add(new int[] { likeeId, idTsPair.Item2 });
                        }
                    }
                }
            }

            var targetData = likersData[id];
            likersData.Remove(id);

            var targetPrecalcualteData = Precalculate(targetData);

            var similarityData = new List<Tuple<int, double>>();
            foreach (var likers in likersData)
            {
                var precalculate = Precalculate(likers.Value);
                var similarity = CalculateSimilarity(targetPrecalcualteData, precalculate);

                similarityData.Add(Tuple.Create(likers.Key, similarity));
            }

            similarityData.Sort((x, y) =>
            {
                if (x.Item2 == y.Item2)
                {
                    return y.Item1 - x.Item1;
                }

                var diff = y.Item2 - x.Item2;
                return diff < 0 ? -1 : diff > 0 ? 1 : 0;
            });

            var allExceptLikes = new List<int>();

            foreach (var tuple in similarityData)
            {
                var likesFromCurrent = accounts[tuple.Item1].GetLikesFrom().ToList();

                likesFromCurrent.Sort(_descComparer);
                foreach (var like in likesFromCurrent)
                {
                    if (!uniqueLikesOfTarget.Contains(like))
                    {
                        allExceptLikes.Add(like);
                        uniqueLikesOfTarget.Add(like);
                    }
                }

                if (allExceptLikes.Count >= limit)
                {
                    break;
                }
            }


            return allExceptLikes.Take(limit).Select(x => _converter.Convert(x));
        }

        private Dictionary<int, double> Precalculate(Tuple<int, int>[] input)
        {
            var data = new Dictionary<int, List<int>>();
            foreach (var tuple in input)
            {
                if (!data.ContainsKey(tuple.Item1))
                {
                    data[tuple.Item1] = new List<int> { tuple.Item2 };
                }
                else
                {
                    data[tuple.Item1].Add(tuple.Item2);
                }
            }

            var result = new Dictionary<int, double>();
            foreach (var pair in data)
            {
                double sum = 0;
                var count = 0;
                foreach (var sc in pair.Value)
                {
                    sum += sc;
                    count++;
                }

                result[pair.Key] = sum / count;
            }

            return result;
        }

        private List<double[]> Precalculate(List<int[]> input)
        {
            input.Sort((x, y) => x[0] - y[0]);
            var result = new List<double[]>();

            var prev = input[0][0];
            var curSum = input[0][1];
            var curCount = 1;

            for (int i = 1; i < input.Count; i++)
            {
                var cur = input[i];
                if (cur[0] != prev)
                {
                    result.Add(new double[] { prev, (double)curSum / curCount });
                    prev = cur[0];
                    curSum = cur[1];
                    curCount = 1;
                }
                else
                {
                    curSum += cur[1];
                    curCount++;
                }
            }

            result.Add(new double[] { prev, (double)curSum / curCount });

            return result;
        }

        private double CalculateSimilarity(Dictionary<int, double> target, Dictionary<int, double> current)
        {
            double result = 0;

            foreach (var pair in current)
            {
                var targetTs = target[pair.Key];
                if (targetTs == pair.Value)
                {
                    result++;
                }
                else
                {
                    result += 1 / Math.Abs(targetTs - pair.Value);
                }
            }

            return result;
        }

        private double CalculateSimilarity(List<double[]> target, List<double[]> current)
        {
            double result = 0;

            int i = 0;
            int j = 0;

            while (j < current.Count)
            {
                if (target[i][0] == current[j][0])
                {
                    if (target[i][1] == current[j][1])
                    {
                        result++;
                    }
                    else
                    {
                        result += 1 / Math.Abs(target[i][1] - current[j][1]);
                    }

                    i++;
                    j++;
                }
                else
                {
                    i++;
                }
            }

            return result;
        }
    }
}
