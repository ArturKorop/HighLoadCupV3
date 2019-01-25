using HighLoadCupV3.Model.Dto;
using HighLoadCupV3.Model.InMemory;

namespace HighLoadCupV3.Model.Filters.Recommend
{
    public class RecommendResponseDtoCovnerter
    {
        private readonly InMemoryRepository _repo;
        private readonly AccountData[] _accounts;

        public RecommendResponseDtoCovnerter(InMemoryRepository repo)
        {
            _repo = repo;
            _accounts = _repo.Accounts;
        }


        public RecommendResponseDto Convert(int id)
        {
            var acc = _accounts[id];

            var res = new RecommendResponseDto
            {
                Id = id,
                Email = acc.Email,
                SName = acc.SNameIndex != 0 ? _repo.SNameData.GetValue(acc.SNameIndex) : null,
                Birth = acc.Birth,
                Status = _repo.StatusData.GetValue(acc.Status)
            };

            if (acc.PremiumStart != 0)
            {
                res.Premium = new PremiumDto { Start = acc.PremiumStart, Finish = acc.PremiumFinish };
            }

            if (acc.FNameIndex != 0)
            {
                res.FName = _repo.FNameData.GetValue(acc.FNameIndex);
            }

            return res;
        }
    }
}
