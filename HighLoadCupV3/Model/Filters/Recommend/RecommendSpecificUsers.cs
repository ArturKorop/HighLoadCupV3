using System;
using System.Collections.Generic;
using HighLoadCupV3.Model.Dto;
using HighLoadCupV3.Model.InMemory;

namespace HighLoadCupV3.Model.Filters.Recommend
{
    public class RecommendSpecificUsers : RecommendImplBase
    {

        public RecommendSpecificUsers(InMemoryRepository repo, InterestsIntersectCalculator calculator, RecommendResponseDtoCovnerter converter) : base(repo, calculator, converter)
        {
        }

        public override IEnumerable<RecommendResponseDto> Recommend(int targetId, int limit, IEnumerable<int> ids)
        {
            var targetAcc = _accounts[targetId];
            var targetInterests = targetAcc.Interests;
            if (targetInterests == null)
            {
                yield break;
            }

            var interestCount = targetInterests.Length;
            var targetBirth = targetAcc.Birth;

            var buckets = new List<Tuple<int, int>>[6 * targetInterests.Length];
            for (int i = 0; i < buckets.Length; i++)
            {
                buckets[i] = new List<Tuple<int, int>>();
            }

            FillBuckets(buckets, ids, targetInterests, interestCount, targetBirth, targetAcc.Sex == 0 ? (byte)1 : (byte)0);

            var count = 0;
            for (int i = buckets.Length - 1; i >= 0; i--)
            {
                var current = buckets[i];
                current.Sort(_comparer);
                int prevId = -1;
                for (int t = 0; t < current.Count; t++)
                {
                    if (current[t].Item1 != prevId)
                    {
                        prevId = current[t].Item1;

                        yield return _converter.Convert(prevId);

                        count++;
                        if (count == limit)
                        {
                            yield break;
                        }
                    }
                }
            }
        }

        protected void FillBuckets(List<Tuple<int, int>>[] buckets, IEnumerable<int> ids, byte[] targetInterests, int interestCount, int targetBirth, byte desiredSex)
        {
            foreach (var id in ids)
            {
                var acc = _accounts[id];
                if (acc.Sex == desiredSex && acc.Interests != null)
                {
                    var bucketKey = GenerateBucketKey(acc, interestCount, targetInterests);
                    if (bucketKey >= 0)
                    {
                        buckets[bucketKey].Add(Tuple.Create(id, Math.Abs(acc.Birth - targetBirth)));
                    }
                }
            }
        }

        protected int GenerateBucketKey(AccountData acc, int interestsCount, byte[] targetInterests)
        {
            var intersectedInterests = _calculator.Calculate(acc.Interests, targetInterests);
            if (intersectedInterests == 0)
            {
                return -1;
            }

            var key = 0;
            if (acc.PremiumIndex == 1)
            {
                key = 3;
            }

            key += acc.Status;
            key *= interestsCount;

            key += intersectedInterests;

            return key - 1;
        }
    }
}
