using HighLoadCupV3.Model.InMemory;

namespace HighLoadCupV3.Model.Filters.Suggest
{
    public class SuggestFilterByCountry : ISuggestFilter
    {
        private readonly short _country;
        private readonly AccountData[] _accounts;

        public SuggestFilterByCountry(short country, InMemoryRepository repo)
        {
            _country = country;
            _accounts = repo.Accounts;
        }

        public bool IsOk(int id)
        {
            return _accounts[id].CountryIndex == _country;
        }
    }
}