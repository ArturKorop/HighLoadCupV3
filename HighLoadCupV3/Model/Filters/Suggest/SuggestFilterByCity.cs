using HighLoadCupV3.Model.InMemory;

namespace HighLoadCupV3.Model.Filters.Suggest
{
    public class SuggestFilterByCity : ISuggestFilter
    {
        private readonly short _city;
        private readonly AccountData[] _accounts;

        public SuggestFilterByCity(short city, InMemoryRepository repo)
        {
            _city = city;
            _accounts = repo.Accounts;
        }

        public bool IsOk(int id)
        {
            return _accounts[id].CityIndex == _city;
        }
    }
}