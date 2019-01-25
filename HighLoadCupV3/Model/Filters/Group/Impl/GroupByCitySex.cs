using HighLoadCupV3.Model.Dto;
using HighLoadCupV3.Model.InMemory;

namespace HighLoadCupV3.Model.Filters.Group.Impl
{
    public class GroupByCitySex : GroupByTwoParametersBase
    {
        public GroupByCitySex(int cityCount, int sexCount, InMemoryRepository repo) : base(cityCount, sexCount, repo)
        {
        }

        protected override void FillBuckets(int[,] buckets)
        {
            for (short i = 0; i < _mainCount; i++)
            {
                foreach (var id in _repo.CityData.GetSortedIdsBySortedIndex(i))
                {
                    var acc = _repo.Accounts[id];
                    buckets[i, acc.Sex]++;
                }
            }
        }

        protected override GroupResponseDto Convert(int orderedKey, int count)
        {
            var sortedSexIndex = orderedKey % _minorCount;
            var sortedCityIndex = (orderedKey - sortedSexIndex) / _minorCount;

            var sex = _repo.SexData.GetValue((byte)sortedSexIndex);
            var city = _repo.CityData.GetValueBySortedIndex((short)sortedCityIndex);

            var dto = new GroupResponseDto
            {
                Count = count,
                Sex = sex,
                City = city == string.Empty ? null : city
            };

            return dto;
        }

        protected override int GenerateUniqueKey(AccountData acc)
        {
            return acc.CityIndex * _minorCount + acc.Sex;
        }

        protected override int GenerateOrderedKey(int cityOrderedIndex, int sexOrderedIndex)
        {
            var key = cityOrderedIndex * _minorCount + sexOrderedIndex;

            return key;
        }

        protected override int GenerateOrderedKey(int uniqueKey)
        {
            var sex = uniqueKey % _minorCount;
            var cityIndex = (uniqueKey - sex) / _minorCount;
            var cityOrderedIndex = _repo.CityData.GetSortedIndexByIndex((short)cityIndex);

            var key = cityOrderedIndex * _minorCount + sex;

            return key;
        }
    }
}