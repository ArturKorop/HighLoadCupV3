using HighLoadCupV3.Model.InMemory;

namespace HighLoadCupV3.Model.Filters.Suggest
{
    public class SuggestFilterFactory
    {
        private readonly InMemoryRepository _repo;

        public SuggestFilterFactory(InMemoryRepository repo)
        {
            _repo = repo;
        }

        public ISuggestFilter CreateFilter(string key, string value)
        {
            if (key == Names.Country)
            {
                if (!_repo.CountryData.ContainsValue(value))
                {
                    return null;
                }

                return new SuggestFilterByCountry(_repo.CountryData.GetIndex(value), _repo);
            }

            if (key == Names.City)
            {
                if (!_repo.CityData.ContainsValue(value))
                {
                    return null;
                }

                return new SuggestFilterByCity(_repo.CityData.GetIndex(value), _repo);
            }

            return null;
        }
    }
}