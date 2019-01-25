using System.Collections.Generic;

namespace HighLoadCupV3.Model.Filters.Group
{
    public class GroupQueryParser
    {
        private static readonly HashSet<string> _validGroupFilters = new HashSet<string>
        {
            Names.City,
            Names.Country,

            Names.Sex,
            Names.Status,
            Names.SName,
            Names.FName,

            Names.Interests,
            Names.Likes,

            Names.Joined,
            Names.Birth,
            Names.Premium,
            Names.Phone,
            Names.Email
        };

        public bool TryParse(Dictionary<string, string> input, out GroupQuery query)
        {
            query = null;
            input.Remove(Names.QueryId);

            if(!input.TryGetValue(Names.Limit, out var limitString) || !int.TryParse(limitString, out int limit) || limit < 1)
            {
                return false;
            }

            input.Remove(Names.Limit);

            if (!input.TryGetValue(Names.Order, out var orderString) || !int.TryParse(orderString, out int order) || (order != -1 && order != 1))
            {
                return false;
            }

            input.Remove(Names.Order);

            if (!input.TryGetValue(Names.Keys, out var keys) || !TryParseKey(keys, out var key))
            {
                return false;
            }

            input.Remove(Names.Keys);

            foreach(var pair in input)
            {
                if(!_validGroupFilters.Contains(pair.Key))
                {
                    return false;
                }
            }

            query = new GroupQuery
            {
                Key = key,
                Limit = limit,
                Order = order,
                Filter = input
            };

            return true;
        }

        private static bool TryParseKey(string keys, out GroupKey key)
        {
            key = GroupKey.City;
            switch (keys)
            {
                case Names.Sex:
                    key = GroupKey.Sex;
                    return true;
                case Names.Status:
                    key = GroupKey.Status;
                    return true;
                case Names.City:
                    key = GroupKey.City;
                    return true;
                case Names.Country:
                    key = GroupKey.Country;
                    return true;
                case Names.Interests:
                    key = GroupKey.Interests;
                    return true;
                case Names.CitySex:
                    key = GroupKey.CitySex;
                    return true;
                case Names.CityStatus:
                    key = GroupKey.CityStatus;
                    return true;
                case Names.CountrySex:
                    key = GroupKey.CountrySex;
                    return true;
                case Names.CountryStatus:
                    key = GroupKey.CountryStatus;
                    return true;
                default:
                    return false;
            }
        }
    }
}