using System.Collections.Generic;
using HighLoadCupV3.Model.Filters.InMemoryFilters;
using HighLoadCupV3.Model.Filters.InMemoryFilters.Abstract;
using HighLoadCupV3.Model.InMemory;

namespace HighLoadCupV3.Model.Filters.Group
{
    public class GroupFactory
    {
        private readonly InMemoryRepository _repo;

        public GroupFactory(InMemoryRepository repo)
        {
            _repo = repo;
        }

        public LikesContainsIMFilter TryGetLikes(Dictionary<string, string> queries)
        {
            if (queries.TryGetValue(Names.Likes, out var value))
            {
                return new LikesContainsIMFilter(_repo, value);
            }

            return null;
        }

        public IFilter GetFilter(string filterKey, string filterValue)
        {
            switch (filterKey)
            {
                case "sname": return new SNameEqIMFilter(_repo, 1, filterValue);
                case "fname": return new FNameEqIMFilter(_repo, 2, filterValue);
                case "interests": return new InterestsAnyIMFilter(_repo, 3, filterValue);
                case "city": return new CityEqIMFilter(_repo, 4, filterValue);
                case "country": return new CountryEqIMFilter(_repo, 5, filterValue);
                case "email": return new EmailDomainIMFilter(_repo, 6, filterValue);
                case "phone": return new PhoneCodeIMFilter(_repo, 7, filterValue);
                case "birth": return new BirthYearIMFilter(_repo, 8, filterValue);
                case "joined": return new JoinedYearIMFilter(_repo, 9, filterValue);
                case "premium": return new PremiumNowIMFilter(_repo, 10, filterValue);
                case "status": return new StatusEqIMFilter(_repo, 11, filterValue);
                case "sex": return new SexEqIMFilter(_repo, 12, filterValue);
                default: return null;
            }
        }
    }
}