using HighLoadCupV3.Model.Dto;
using HighLoadCupV3.Model.InMemory;

namespace HighLoadCupV3.Model.Filters.Group.Impl
{
    public class GroupByCityStatus : GroupByTwoParametersBase
    {
        public GroupByCityStatus(int cityCount, int statusCount, InMemoryRepository repo) : base(cityCount, statusCount, repo)
        {
        }

        protected override void FillBuckets(int[,] buckets)
        {
            for (short i = 0; i < _mainCount; i++)
            {
                foreach (var id in _repo.CityData.GetSortedIdsBySortedIndex(i))
                {
                    var acc = _repo.Accounts[id];
                    buckets[i, acc.Status]++;
                }
            }
        }

        protected override GroupResponseDto Convert(int orderedKey, int count)
        {
            var sortedStatusIndex = orderedKey % _minorCount;
            var sortedCityIndex = (orderedKey - sortedStatusIndex) / _minorCount;

            var status = _repo.StatusData.GetValueBySortedIndex((byte)sortedStatusIndex);
            var city = _repo.CityData.GetValueBySortedIndex((short)sortedCityIndex);

            var dto = new GroupResponseDto
            {
                Count = count,
                Status = status,
                City = city == string.Empty ? null : city
            };

            return dto;
        }

        protected override int GenerateUniqueKey(AccountData acc)
        {
            return acc.CityIndex * _minorCount + acc.Status;
        }

        protected override int GenerateOrderedKey(int cityOrderedIndex, int statusIndex)
        {
            var statusOrderedIndex = _repo.StatusData.GetSortedIndexByIndex((byte)statusIndex);
            var key = cityOrderedIndex * _minorCount + statusOrderedIndex;

            return key;
        }

        protected override int GenerateOrderedKey(int uniqueKey)
        {
            var status = uniqueKey % _minorCount;
            var cityIndex = (uniqueKey - status) / _minorCount;
            var cityOrderedIndex = _repo.CityData.GetSortedIndexByIndex((short)cityIndex);
            var statusOrderedIndex = _repo.StatusData.GetSortedIndexByIndex((byte)status);

            var key = cityOrderedIndex * _minorCount + statusOrderedIndex;

            return key;
        }
    }
}