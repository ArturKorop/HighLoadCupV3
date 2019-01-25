using HighLoadCupV3.Model.Dto;
using HighLoadCupV3.Model.InMemory;

namespace HighLoadCupV3.Model.Filters.Group.Impl
{
    public class GroupByCountryStatus : GroupByTwoParametersBase
    {
        public GroupByCountryStatus(int countryCount, int statusCount, InMemoryRepository repo) : base(countryCount, statusCount, repo)
        {
        }

        protected override void FillBuckets(int[,] buckets)
        {
            for (byte i = 0; i < _mainCount; i++)
            {
                foreach (var id in _repo.CountryData.GetSortedIdsBySortedIndex(i))
                {
                    var acc = _repo.Accounts[id];
                    buckets[i, acc.Status]++;
                }
            }
        }

        protected override GroupResponseDto Convert(int orderedKey, int count)
        {
            var sortedStatusIndex = orderedKey % _minorCount;
            var sortedCountryIndex = (orderedKey - sortedStatusIndex) / _minorCount;

            var status = _repo.StatusData.GetValueBySortedIndex((byte)sortedStatusIndex);
            var country = _repo.CountryData.GetValueBySortedIndex((byte)sortedCountryIndex);

            var dto = new GroupResponseDto
            {
                Count = count,
                Status = status,
                Country = country == string.Empty ? null : country
            };

            return dto;
        }

        protected override int GenerateUniqueKey(AccountData acc)
        {
            return acc.CountryIndex * _minorCount + acc.Status;
        }

        protected override int GenerateOrderedKey(int countryOrderedIndex, int statusIndex)
        {
            var statusOrderedIndex = _repo.StatusData.GetSortedIndexByIndex((byte)statusIndex);
            var key = countryOrderedIndex * _minorCount + statusOrderedIndex;

            return key;
        }

        protected override int GenerateOrderedKey(int uniqueKey)
        {
            var status = uniqueKey % _minorCount;
            var countryIndex = (uniqueKey - status) / _minorCount;
            var countryOrderedIndex = _repo.CountryData.GetSortedIndexByIndex((byte)countryIndex);
            var statusOrderedIndex = _repo.StatusData.GetSortedIndexByIndex((byte)status);

            var key = countryOrderedIndex * _minorCount + statusOrderedIndex;

            return key;
        }
    }
}