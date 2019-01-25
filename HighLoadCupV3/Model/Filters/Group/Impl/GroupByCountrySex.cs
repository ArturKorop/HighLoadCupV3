using HighLoadCupV3.Model.Dto;
using HighLoadCupV3.Model.InMemory;

namespace HighLoadCupV3.Model.Filters.Group.Impl
{
    public class GroupByCountrySex : GroupByTwoParametersBase
    {
        public GroupByCountrySex(int countryCount, int sexCount, InMemoryRepository repo) : base(countryCount, sexCount, repo)
        {
        }

        protected override void FillBuckets(int[,] buckets)
        {
            for (byte i = 0; i < _mainCount; i++)
            {
                foreach (var id in _repo.CountryData.GetSortedIdsBySortedIndex(i))
                {
                    var acc = _repo.Accounts[id];
                    buckets[i, acc.Sex]++;
                }
            }
        }

        protected override GroupResponseDto Convert(int orderedKey, int count)
        {
            var sortedSexIndex = orderedKey % _minorCount;
            var sortedCountryIndex = (orderedKey - sortedSexIndex) / _minorCount;

            var sex = _repo.SexData.GetValue((byte)sortedSexIndex);
            var country = _repo.CountryData.GetValueBySortedIndex((byte)sortedCountryIndex);

            var dto = new GroupResponseDto
            {
                Count = count,
                Sex = sex,
                Country = country == string.Empty ? null : country
            };

            return dto;
        }

        protected override int GenerateUniqueKey(AccountData acc)
        {
            return acc.CountryIndex * _minorCount + acc.Sex;
        }

        protected override int GenerateOrderedKey(int countryOrderedIndex, int sexOrderedIndex)
        {
            var key = countryOrderedIndex * _minorCount + sexOrderedIndex;

            return key;
        }

        protected override int GenerateOrderedKey(int uniqueKey)
        {
            var sex = uniqueKey % _minorCount;
            var countryIndex = (uniqueKey - sex) / _minorCount;
            var countryOrderedIndex = _repo.CountryData.GetSortedIndexByIndex((byte)countryIndex);

            var key = countryOrderedIndex * _minorCount + sex;

            return key;
        }
    }
}