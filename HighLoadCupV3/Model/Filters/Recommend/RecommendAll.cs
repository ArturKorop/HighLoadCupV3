using System;
using System.Collections.Generic;
using System.Linq;
using HighLoadCupV3.Model.Dto;
using HighLoadCupV3.Model.InMemory;

namespace HighLoadCupV3.Model.Filters.Recommend
{
    public class RecommendAll : RecommendImplBase
    {

        public RecommendAll(InMemoryRepository repo, InterestsIntersectCalculator calculator, RecommendResponseDtoCovnerter converter) : base(repo, calculator, converter)
        {
        }

        public override IEnumerable<RecommendResponseDto> Recommend(int targetId, int limit, IEnumerable<int> ids)
        {
            var targetAcc = _accounts[targetId];
            var targetInterests = targetAcc.Interests;
            if (targetInterests == null)
            {
                return Enumerable.Empty<RecommendResponseDto>();
            }

            var targetBirth = targetAcc.Birth;

            var recommendations = new List<RecommendResponseDto>();
            var desiredSex = targetAcc.Sex == 0 ? (byte) 1 : (byte) 0;

            var from = 5 + desiredSex * 6;
            var to = desiredSex * 6;


            for (int i = from; i >= to; i--)
            {
                var current = GetBuckets(i, targetInterests, targetBirth);
                for (int j = current.Length - 1; j >= 0; j--)
                {
                    current[j].Sort(_comparer);
                    int prevId = -1;
                    for (int t = 0; t < current[j].Count; t++)
                    {
                        if (current[j][t].Item1 != prevId)
                        {
                            prevId = current[j][t].Item1;
                            recommendations.Add(_converter.Convert(prevId));

                            if (recommendations.Count == limit)
                            {
                                break;
                            }
                        }
                    }

                    if (recommendations.Count == limit)
                    {
                        break;
                    }
                }

                if (recommendations.Count == limit)
                {
                    break;
                }
            }

            return recommendations;
        }

        protected List<Tuple<int, int>>[] GetBuckets(int premiumStatusSexKey, byte[] targetInterests, int targetBirth)
        {
            var priorityBucketsForInterests = new List<Tuple<int, int>>[targetInterests.Length];
            for (int i = 0; i < targetInterests.Length; i++)
            {
                priorityBucketsForInterests[i] = new List<Tuple<int, int>>();
            }

            foreach (var targetInterest in targetInterests)
            {
                var ids = _repo.InterestsData.GetDataForRecommend(targetInterest, premiumStatusSexKey);
                foreach (var id in ids)
                {
                    var acc = _accounts[id];
                    var intersectedInterests = _calculator.Calculate(acc.Interests, targetInterests);
                    if (intersectedInterests == 0)
                    {
                        continue;
                    }

                    priorityBucketsForInterests[intersectedInterests-1].Add(Tuple.Create(id, Math.Abs(acc.Birth - targetBirth)));
                }
            }

            return priorityBucketsForInterests;
        }
    }
}
