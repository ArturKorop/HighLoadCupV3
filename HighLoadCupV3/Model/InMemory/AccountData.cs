using System;
using System.Collections.Generic;

namespace HighLoadCupV3.Model.InMemory
{
    public class AccountData
    {
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

        private int[] _likesFromId;
        private int[,] _likesTo;

        public void AddLikesFromToNewAccount(int[] likesFrom)
        {
            _likesFromId = likesFrom;
        }

        public void AddLikesFrom(List<int> likesFrom)
        {
            if (_likesFromId == null)
            {
                _likesFromId = likesFrom.ToArray();
            }
            else
            {
                var l = _likesFromId.Length;
                var buffer = new int[l + likesFrom.Count];
                Array.Copy(_likesFromId, buffer, l);
                _likesFromId = buffer;
                for (int i = 0; i < likesFrom.Count; i++)
                {
                    _likesFromId[l + i] = likesFrom[i];
                }
            }
        }

        public bool AnyLikesFrom()
        {
            return _likesFromId != null;
        }

        public void AddLikesTo(List<Tuple<int, int>> likersWithTs)
        {
            if (_likesTo == null)
            {
                _likesTo = new int[likersWithTs.Count,2];
                for (int i = 0; i < likersWithTs.Count; i++)
                {
                    _likesTo[i, 0] = likersWithTs[i].Item1;
                    _likesTo[i, 1] = likersWithTs[i].Item2;
                }
            }
            else
            {
                var l = _likesTo.GetLength(0);
                var likesToBuffer = new int[l + likersWithTs.Count, 2];
                Array.Copy(_likesTo, likesToBuffer, _likesTo.Length);
                _likesTo = likesToBuffer;

                for (int i = 0; i < likersWithTs.Count; i++)
                {
                    _likesTo[l+i, 0] = likersWithTs[i].Item1;
                    _likesTo[l+i, 1] = likersWithTs[i].Item2;
                }

            }
        }

        public void AddLikeTo(int id, int ts)
        {
            if (_likesTo == null)
            {
                _likesTo = new int[1,2];
                _likesTo[0, 0] = id;
                _likesTo[0, 1] = ts;
            }
            else
            {
                var l = _likesTo.GetLength(0);
                var likesToBuffer = new int[l + 1, 2];
                Array.Copy(_likesTo, likesToBuffer, _likesTo.Length);
                _likesTo = likesToBuffer;
                _likesTo[l, 0] = id;
                _likesTo[l, 1] = ts;
            }
        }

        public IEnumerable<int> GetLikesFrom()
        {
            for (int i = 0; i < _likesFromId.Length; i++)
            {
                yield return _likesFromId[i];
            }
        }

        public IEnumerable<int> GetLikesTo()
        {
            var l = _likesTo.GetLength(0);
            for (int i = 0; i < l; i++)
            {
                yield return _likesTo[i, 0];
            }
        }

        public IEnumerable<Tuple<int, int>> GetLikesToWithTs()
        {
            var l = _likesTo.GetLength(0);
            for (int i = 0; i < l; i++)
            {
                yield return Tuple.Create(_likesTo[i, 0], _likesTo[i, 1]);
            }
        }

        public bool AnyLikesTo()
        {
            return _likesTo != null;
        }
    }
}

