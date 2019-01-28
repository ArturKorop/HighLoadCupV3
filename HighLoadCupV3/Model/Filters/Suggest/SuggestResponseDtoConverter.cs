using HighLoadCupV3.Model.Dto;
using HighLoadCupV3.Model.InMemory;

namespace HighLoadCupV3.Model.Filters.Suggest
{
    public class SuggestResponseDtoConverter
    {
        private readonly InMemoryRepository _repo;
        private readonly AccountData[] _accounts;

        public static readonly ObjectPool<SuggestResponseDto> Pool = new ObjectPool<SuggestResponseDto>(100);

        public SuggestResponseDtoConverter(InMemoryRepository repo)
        {
            _repo = repo;
            _accounts = _repo.Accounts;
        }

        public SuggestResponseDto Convert(int id)
        {
            var acc = _accounts[id];

            var res = Pool.Rent();

            res.Id = id;
            res.Email = acc.Email;
            res.FName = acc.FNameIndex != 0 ? _repo.FNameData.GetValue(acc.FNameIndex) : null;
            res.SName = acc.SNameIndex != 0 ? _repo.SNameData.GetValue(acc.SNameIndex) : null;
            res.Status = _repo.StatusData.GetValue(acc.Status);

            return res;
        }
    }
}