//using System;
//using System.Collections.Generic;
//using HighLoadCupWithMongo.Model.InMemory;

//namespace HighLoadCupWithMongo.Model
//{
//    public class GroupByCodeManager
//    {
//        private const int StatusShift = 1;
//        private const int CountryShift = 3;
//        private const int CityShift = 11;
//        private const int InterestShift = 22;

//        private readonly int _statusSize = (int)Math.Pow(2, 2) - 1;
//        private readonly int _citySize = (int)Math.Pow(2, 11) - 1;
//        private readonly int _countrySize = (int)Math.Pow(2, 8) - 1;
//        private readonly int _interestSize = (int)Math.Pow(2, 9) - 1;

//        private int _statusMask;
//        private int _cityMask;
//        private int _countryMask;
//        private int _interestMask;

//        private readonly InMemoryRepository _repo;

//        public GroupByCodeManager(InMemoryRepository repo)
//        {
//            _repo = repo;
//            _statusMask = _statusSize << StatusShift;
//            _cityMask = _citySize << CityShift;
//            _countryMask = _countrySize << CountryShift;
//            _interestMask = _interestSize << InterestShift;
//        }

//        public int Create(string sex, string status, string city, string country)
//        {
//            var res = 0;
//            var sexIndex = _repo.SexData.GetIndexByValue(sex);
//            res |= sexIndex;

//            var statusIndex = _repo.StatusData.GetIndexByValue(status);
//            res |= statusIndex << StatusShift;

//            var countryIndex = _repo.CountryData.GetIndexByValue(country ?? string.Empty);
//            res |= countryIndex << CountryShift;

//            var cityIndex = _repo.CityData.GetIndexByValue(city ?? string.Empty);
//            res |= cityIndex << CityShift;

//            return res;
//        }

//        public int UpdateCode(int code, string sex, string status, string city, string country)
//        {
//            if (sex == null && status == null && city == null && country == null)
//            {
//                return code;
//            }

//            var res = 0;
//            if (sex == null)
//            {
//                var sexIndex = code & 1;
//                res |= sexIndex;
//            }
//            else
//            {
//                var sexIndex = _repo.SexData.GetIndexByValue(sex);
//                res |= sexIndex;
//            }

//            if (status == null)
//            {
//                var statusIndex = code & _statusMask;
//                res |= statusIndex;
//            }
//            else
//            {
//                var statusIndex = _repo.StatusData.GetIndexByValue(status);
//                res |= statusIndex << StatusShift;
//            }

//            if (city == null)
//            {
//                var cityIndex = code & _cityMask;
//                res |= cityIndex;
//            }
//            else
//            {
//                var cityIndex = _repo.CityData.GetIndexByValue(city ?? string.Empty);
//                res |= cityIndex << CityShift;
//            }

//            if (country == null)
//            {
//                var countryIndex = code & _countryMask;
//                res |= countryIndex;
//            }
//            else
//            {
//                var countryIndex = _repo.CountryData.GetIndexByValue(country ?? string.Empty);
//                res |= countryIndex << CountryShift;
//            }

//            return res;
//        }

//        public string GetSex(int code)
//        {
//            var sexIndex = code & 1;

//            return _repo.SexData.GetValueByIndex(sexIndex);
//        }

//        public string GetStatus(int code)
//        {
//            var statusIndex = (code >> StatusShift) & _statusSize;

//            return _repo.StatusData.GetValueByIndex(statusIndex);
//        }

//        public string GetCity(int code)
//        {
//            var cityIndex = (code >> CityShift) & _citySize;

//            return _repo.CityData.GetValueByIndex(cityIndex);
//        }

//        public string GetCountry(int code)
//        {
//            var countryIndex = (code >> CountryShift) & _countrySize;

//            return _repo.CountryData.GetValueByIndex(countryIndex);
//        }

//        public string GetInterest(int code)
//        {
//            var interestIndex = (code >> InterestShift) & _interestSize;

//            return _repo.InterestsData.GetValueByIndex(interestIndex);
//        }


//        public int CreateSpecificCode(int code, HashSet<string> keys)
//        {
//            var res = 0;
//            if (keys.Contains(Names.Sex))
//            {
//                var sexIndex = code & 1;
//                res |= sexIndex;
//            }

//            if (keys.Contains(Names.Status))
//            {
//                var statusIndex = code & _statusMask;
//                res |= statusIndex;
//            }

//            if (keys.Contains(Names.City))
//            {
//                var cityIndex = code & _cityMask;
//                res |= cityIndex;
//            }

//            if (keys.Contains(Names.Country))
//            {
//                var countryIndex = code & _countryMask;
//                res |= countryIndex;
//            }

//            return res;
//        }

//        public IEnumerable<int> CreateSpecificCodeWithInterest(InMemoryAccountData acc, HashSet<string> keys)
//        {
//            var code = acc.GroupByCode;
//            var res = CreateSpecificCode(code, keys);
//            if (acc.Interests != null)
//            {
//                foreach (var interest in acc.Interests)
//                {
//                    var interestIndex = interest << InterestShift;
//                    var current = res | interestIndex;

//                    yield return current;
//                }
//            }
//            //else
//            //{
//                //var emptyInterestId = _repo.InterestsData.GetIndexByValue(string.Empty);
//                //var interestIndex = emptyInterestId << InterestShift;
//                //var current = res | interestIndex;

//                //yield return current;
//            //}
//        }

//        public int CreateOrderedKey(int code, string[] orderedKeys)
//        {
//            var sexIndex = _repo.SexData.GetSortedIndexByIndex(code & 1);
//            var statusIndex = _repo.StatusData.GetSortedIndexByIndex((code & _statusMask) >> StatusShift);
//            var cityIndex = _repo.CityData.GetSortedIndexByIndex((code & _cityMask) >> CityShift);
//            var countryIndex = _repo.CountryData.GetSortedIndexByIndex((code & _countryMask) >> CountryShift);
//            var interestIndex = _repo.InterestsData.GetSortedIndexByIndex((code & _interestMask) >> InterestShift);

//            var res = 0;
//            foreach (var key in orderedKeys)
//            {
//                res |= GetOrderedKeyPart(key, cityIndex, countryIndex, sexIndex, statusIndex, interestIndex);
//            }

//            return res;
//        }

//        private static int GetOrderedKeyPart(string key, int city, int country, int sex, int status, int interest)
//        {
//            switch (key)
//            {
//                case Names.City: return city;
//                case Names.Country: return country;
//                case Names.Sex: return sex;
//                case Names.Status: return status;
//                case Names.Interests: return interest;
//                default: throw new ArgumentException(key);
//            }
//        }

//    }
//}