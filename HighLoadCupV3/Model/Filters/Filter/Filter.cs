using System.Collections.Generic;
using System.Linq;
using HighLoadCupV3.Model.Filters.InMemoryFilters.Abstract;
using HighLoadCupV3.Model.InMemory;

namespace HighLoadCupV3.Model.Filters.Filter
{
    public class Filter
    {
        private readonly InMemoryRepository _repo;
        private readonly FilterFactory _factory;
        private readonly IIdsToResponseConverter _converter;

        public Filter(InMemoryRepository repo)
        {
            _repo = repo;
            _factory = new FilterFactory(repo);
            _converter = new IdResultWithResponseDtoConverter(repo);
        }

        public object FilterBy(Dictionary<string, string> queries)
        {
            if (!Validate(queries, out var limit))
            {
                return null;
            }

            queries.Remove("query_id");
            var requiredFields = new HashSet<string> { Names.Id, Names.Email };

            var likeFilter = _factory.TryGetLikes(queries);
            var empty = false;
            if (likeFilter != null)
            {
                if (!likeFilter.IsValid)
                {
                    return null;
                }

                if (likeFilter.IsEmpty)
                {
                    empty = true;
                }

                queries.Remove("likes_contains");
            }

            var filters = new List<IFilter>();
            foreach (var query in queries)
            {
                var filter = _factory.GetFilter(query.Key, query.Value);
                if (filter == null || !filter.IsValid)
                {
                    return null;
                }

                if (filter.IsEmpty)
                {
                    empty = true;
                }

                filters.Add(filter);

                if (!filter.IsExcluded())
                {
                    requiredFields.Add(filter.Field);
                }
            }

            if (empty)
            {
                return _converter.Convert(new AccountData[0], null);
            }

            IEnumerable<AccountData> result = null;
            if (likeFilter != null)
            {
                result = likeFilter.Filter();
            }

            filters.Sort((a, b) => a.Order - b.Order);
            foreach (var filter in filters)
            {
                result = filter.Filter(result);
            }

            var accounts = (result ?? _repo.GetSortedAccounts()).Take(limit);

            return _converter.Convert(accounts, requiredFields);
        }


        private bool Validate(Dictionary<string, string> queries, out int limit)
        {
            limit = 0;
            if (!queries.ContainsKey(Names.Limit) || !int.TryParse(queries[Names.Limit], out limit))
            {
                return false;
            }

            queries.Remove(Names.Limit);
            return true;
        }


    }
}
