using System;
using System.Collections.Generic;

namespace HighLoadCupV3.Model.InMemory
{
    public class AccountData
    {
        private static readonly long _mask = (long)Math.Pow(2, 24) - 1;

        public int Id { get; set; }

        public string Email { get; set; }
        public int EmailSortedIndex { get; set; }

        // Can be null
        public short SNameIndex { get; set; }
        public short CityIndex { get; set; }
        public byte CountryIndex { get; set; }

        // Can be null
        public byte FNameIndex { get; set; }
        public byte CodeIndex { get; set; }

        // Always existed
        public byte BirthYearIndex { get; set; }
        public byte DomainIndex { get; set; }
        public byte JoinedYearIndex { get; set; }

        // Always existed and predefined
        public byte Sex { get; set; }
        public byte Status { get; set; }
        public byte PremiumIndex { get; set; }

        // Always existed
        public byte[] Interests { get; set; }

        public int Phone { get; set; }
        public int Birth { get; set; }
        public int PremiumStart { get; set; }
        public int PremiumFinish { get; set; }

        private List<long> _likesFromId = new List<long>();
        private byte _likesFromCount = 0;

        private List<long> _likesToId = new List<long>();
        private List<int> _likesToTs  = new List<int>();

        public void AddLikesFromToNewAccount(IEnumerable<int> likesFrom)
        {
            long idStorage = 0;
            var count = 0;
            foreach (var like in likesFrom)
            {
                idStorage = AddIntToLong(idStorage, like, count);
                count++;
                if(count == 3)
                {
                    count = 0;
                    _likesFromId.Add(idStorage);
                    idStorage = 0;
                }

                _likesFromCount++;
            }

            if (count > 0)
            {
                _likesFromId.Add(idStorage);
            }
        }

        public void AddLikeFrom(int id)
        {
            var count = _likesFromCount % 3;
            long idStorage = 0;
            if (count == 0)
            {
                idStorage = AddIntToLong(idStorage, id, 0);
                _likesFromId.Add(idStorage);
            }
            else
            {
                idStorage = AddIntToLong(_likesFromId[_likesFromId.Count - 1], id, count);
                _likesFromId[_likesFromId.Count - 1] = idStorage;
            }

            _likesFromCount++;
        }

        public bool AnyLikesFrom()
        {
            return _likesFromCount > 0;
        }

        public void AddLikeTo(int id, int ts)
        {
            var count = _likesToTs.Count % 3;
            long idStorage = 0;
            if(count == 0)
            {
                idStorage = AddIntToLong(idStorage, id, 0);
                _likesToId.Add(idStorage);
            }
            else
            {
                idStorage = AddIntToLong(_likesToId[_likesToId.Count - 1], id, count);
                _likesToId[_likesToId.Count - 1] = idStorage;
            }

            _likesToTs.Add(ts);
        }

        public IEnumerable<int> GetLikesFrom()
        {
            for (int i = 0; i < _likesFromId.Count - 1; i++)
            {
                foreach (var likeId in GetIntsFromLong(_likesFromId[i]))
                {
                    yield return likeId;
                }
            }

            foreach (var likeId in GetIntsFromLong(_likesFromId[_likesFromId.Count-1]))
            {
                if (likeId > 0)
                {
                    yield return likeId;
                }
            }
        }

        public IEnumerable<int> GetLikesTo()
        {
            for (int i = 0; i < _likesToId.Count - 1; i++)
            {
                foreach (var likeId in GetIntsFromLong(_likesToId[i]))
                {
                    yield return likeId;
                }
            }

            foreach (var likeId in GetIntsFromLong(_likesToId[_likesToId.Count - 1]))
            {
                if (likeId > 0)
                {
                    yield return likeId;
                }
            }
        }

        public IEnumerable<Tuple<int, int>> GetLikesToWithTs()
        {
            var tsIndex = 0;
            for (int i = 0; i < _likesToId.Count - 1; i++)
            {
                foreach (var likeId in GetIntsFromLong(_likesToId[i]))
                {
                    yield return Tuple.Create(likeId, _likesToTs[tsIndex]);
                    tsIndex++;
                }
            }

            foreach (var likeId in GetIntsFromLong(_likesToId[_likesToId.Count - 1]))
            {
                if (likeId > 0)
                {
                    yield return Tuple.Create(likeId, _likesToTs[tsIndex]);
                    tsIndex++;
                }
            }
        }

        public bool AnyLikesTo()
        {
            return _likesToTs.Count > 0;
        }

        private const int _firstShift = 21;
        private const int _secondShift = 42;

        private const long _zeroMask = 2097151;
        private const long _firstMask = 4398044413952;

        private static long AddIntToLong(long target, int input, int shift)
        {
            if(shift == 0)
            {
                return target | (long)input;
            }
            else if(shift == 1)
            {
                return target | ((long)input << _firstShift);
            }
            else 
            {
                return target | ((long)input << _secondShift);
            }
        }

        private static IEnumerable<int> GetIntsFromLong(long input)
        {
            yield return (int)(input & _zeroMask);

            yield return (int)((input & _firstMask) >> _firstShift);

            yield return (int)(input >> _secondShift);
        }
    }
}

