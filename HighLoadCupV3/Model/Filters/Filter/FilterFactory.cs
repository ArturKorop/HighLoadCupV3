using System.Collections.Generic;
using HighLoadCupV3.Model.Filters.InMemoryFilters;
using HighLoadCupV3.Model.Filters.InMemoryFilters.Abstract;
using HighLoadCupV3.Model.InMemory;

namespace HighLoadCupV3.Model.Filters.Filter
{
    public class FilterFactory
    {
        private readonly InMemoryRepository _repo;

        public FilterFactory(InMemoryRepository repo)
        {
            _repo = repo;
        }

        public LikesContainsIMFilter TryGetLikes(Dictionary<string, string> queries)
        {
            if (queries.TryGetValue("likes_contains", out var value))
            {
                return new LikesContainsIMFilter(_repo, value);
            }

            return null;
        }

        public IFilter GetFilter(string filterKey, string filterValue)
        {
            switch (filterKey)
            {
                case "sname_eq":
                    return new SNameEqIMFilter(_repo, 1, filterValue);
                case "fname_eq":
                    return new FNameEqIMFilter(_repo, 2, filterValue);
                case "fname_any":
                    return new FNameAnyIMFilter(_repo, 3, filterValue);
                case "interests_contains":
                    return new InterestsContainsIMFilter(_repo, 4, filterValue);
                case "city_eq":
                    return new CityEqIMFilter(_repo, 5, filterValue);
                case "country_eq":
                    return new CountryEqIMFilter(_repo, 6, filterValue);
                case "phone_code":
                    return new PhoneCodeIMFilter(_repo, 7, filterValue);
                case "city_any":
                    return new CityAnyIMFilter(_repo, 8, filterValue);
                case "interests_any":
                    return new InterestsAnyIMFilter(_repo, 9, filterValue);
                case "email_domain":
                    return new EmailDomainIMFilter(_repo, 10, filterValue);
                case "birth_year":
                    return new BirthYearIMFilter(_repo, 11, filterValue);
                case "premium_now":
                    return new PremiumNowIMFilter(_repo, 12, filterValue);
                case "status_eq":
                    return new StatusEqIMFilter(_repo, 13, filterValue);
                case "sex_eq":
                    return new SexEqIMFilter(_repo, 14, filterValue);
                case "status_neq":
                    return new StatusNeqIMFilter(_repo, 15, filterValue);

                case "city_null":
                    return new CityNullIMFilter(_repo, 16, filterValue);
                case "country_null":
                    return new CountryNullIMFilter(_repo, 21, filterValue);
                case "fname_null":
                    return new FNameNullIMFilter(_repo, 22, filterValue);
                case "sname_null":
                    return new SNameNullIMFilter(_repo, 23, filterValue);

                case "phone_null":
                    return new PhoneNullIMFilter(_repo, 24, filterValue);
                case "premium_null":
                    return new PremiumNullIMFilter(_repo, 25, filterValue);
                case "sname_starts":
                    return new SNameStartsIMFilter(_repo, 26, filterValue);
                case "birth_lt":
                    return new BirthLtIMFilter(_repo, 27, filterValue);
                case "birth_gt":
                    return new BirthGtIMFilter(_repo, 28, filterValue);
                case "email_gt":
                    return new EmailGtIMFilter(_repo, 29, filterValue);
                case "email_lt":
                    return new EmailLtIMFilter(_repo, 30, filterValue);

                default: return null;
            }
        }
    }
}
