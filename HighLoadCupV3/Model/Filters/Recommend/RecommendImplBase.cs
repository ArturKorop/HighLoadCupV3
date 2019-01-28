using System;
using System.Collections.Generic;
using HighLoadCupV3.Model.Dto;
using HighLoadCupV3.Model.InMemory;

namespace HighLoadCupV3.Model.Filters.Recommend
{
    public abstract class RecommendImplBase
    {
        protected readonly AccountData[] _accounts;
        protected readonly InMemoryRepository _repo;
        protected readonly InterestsIntersectCalculator _calculator;
        protected readonly BucketComparer _comparer = new BucketComparer();
        protected readonly RecommendResponseDtoCovnerter _converter;

        public static ListPool<Tuple<int,int>> ListTuplePool = new ListPool<Tuple<int, int>>(50, 500);
        public static ListPool<RecommendResponseDto> ResponseListPool = new ListPool<RecommendResponseDto>(10, 20);

        protected RecommendImplBase(InMemoryRepository repo, InterestsIntersectCalculator calculator, RecommendResponseDtoCovnerter converter)
        {
            _repo = repo;
            _accounts = _repo.Accounts;
            _calculator = calculator;
            _converter = converter;
        }

        public abstract List<RecommendResponseDto> Recommend(int targetId, int limit, IEnumerable<int> ids);
    }
}
