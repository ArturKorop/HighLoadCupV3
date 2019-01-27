using System;
using System.Buffers;
using System.Collections;
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

        private static readonly ArrayPool<KeyValuePair<int, SimilarityCounter>> Pool =
            ArrayPool<KeyValuePair<int, SimilarityCounter>>.Create();

        private static readonly IComparer<KeyValuePair<int, SimilarityCounter>> DescSimilarityComparer =
            new SimilarityComparer();

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
                var holder2 = new SuggestAccountsResponseDto {Accounts = Enumerable.Empty<SuggestResponseDto>()};

                return JsonConvert.SerializeObject(holder2,
                    new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore});
            }

            var suggestions = SuggestForV2(id, limit, key, value);
            var holder = new SuggestAccountsResponseDto {Accounts = suggestions};

            return JsonConvert.SerializeObject(holder,
                new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore});
        }

        public IEnumerable<SuggestResponseDto> SuggestForV2(int id, int limit, string key, string value)
        {
            var accounts = _repo.Accounts;

            var uniqueLikesOfTarget = accounts[id].GetLikesFrom().ToHashSet();

            ISuggestFilter filter = null;
            if (key != null)
            {
                filter = _factory.CreateFilter(key, value);
                if (filter == null)
                {
                    return Enumerable.Empty<SuggestResponseDto>();
                }
            }

            var likersData = new Dictionary<int, SimilarityCounter>();

            foreach (var likeeId in uniqueLikesOfTarget)
            {
                long targetTsSum = 0;
                long targetTsCount = 0;

                var likersOfTargetLike = accounts[likeeId].GetLikesToWithTs();
                var l = likersOfTargetLike.GetLength(0);
                for (int i = 0; i < l; i++)
                {
                    var likerId = likersOfTargetLike[i, 0];
                    var ts = likersOfTargetLike[i, 1];

                    if (likerId == id)
                    {
                        targetTsCount++;
                        targetTsSum += ts;
                    }
                    else if (filter == null || filter.IsOk(likerId))
                    {
                        if (!likersData.ContainsKey(likerId))
                        {
                            likersData[likerId] = new SimilarityCounter();
                        }

                        likersData[likerId].AddTs(ts);
                    }
                }

                var targetTs = (double) (targetTsSum / targetTsCount);
                foreach (var counter in likersData.Values)
                {
                    counter.Calculate(targetTs);
                }
            }


            if (likersData.Count == 0)
            {
                return Enumerable.Empty<SuggestResponseDto>();
            }

            var count = likersData.Count;
            var sortedIdsBySimilarity = Pool.Rent(count);

            var index = 0;
            foreach (var pair in likersData)
            {
                sortedIdsBySimilarity[index] = pair;
                index++;
            }

            Array.Sort(sortedIdsBySimilarity, 0, count, DescSimilarityComparer);

            var allExceptLikes = new List<int>();
            for (int i = 0; i < count; i++)
            {
                var likesFromCurrent = accounts[sortedIdsBySimilarity[i].Key].GetLikesFrom();
                Array.Sort(likesFromCurrent, _descComparer);

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

            Pool.Return(sortedIdsBySimilarity);

            return allExceptLikes.Take(limit).Select(x => _converter.Convert(x));
        }
    }

    public class SimilarityComparer : IComparer<KeyValuePair<int, SimilarityCounter>>
    {
        public int Compare(KeyValuePair<int, SimilarityCounter> x, KeyValuePair<int, SimilarityCounter> y)
        {
            if (x.Value.Similarity != y.Value.Similarity)
            {
                return y.Value.Similarity - x.Value.Similarity > 0 ? 1 : -1;
            }

            return y.Key - x.Key;
        }
    }

    public class SimilarityCounter
    {
        private long _tsSum;
        private byte _tsCount;

        public double Similarity { get; private set; }

        public void AddTs(int ts)
        {
            _tsCount++;
            _tsSum += ts;
        }

        public void Calculate(double targetTs)
        {
            if (_tsCount == 0) return;

            double avg = (double) _tsSum / _tsCount;
            if (avg == targetTs)
            {
                Similarity++;
            }
            else
            {
                Similarity += 1 / Math.Abs(avg - targetTs);
            }

            _tsCount = 0;
            _tsSum = 0;
        }
    }
}
