using System;
using System.Collections.Generic;

namespace HighLoadCupV3.Model.InMemory
{
    public class LikesBuffer
    {
        private readonly List<int>[] _likesFrom;
        private readonly List<Tuple<int, int>>[] _likesTo;
        private readonly InMemoryRepository _repo;

        public LikesBuffer(int count, InMemoryRepository repo)
        {
            _repo = repo;
            _likesFrom = new List<int>[count];
            _likesTo = new List<Tuple<int, int>>[count];
        }

        public void AddLikes(int liker, int likee, int ts)
        {
            if (_likesFrom[liker] == null)
            {
                _likesFrom[liker] = new List<int>();
            }

            _likesFrom[liker].Add(likee);

            if (_likesTo[likee] == null)
            {
                _likesTo[likee] = new List<Tuple<int, int>>();
            }

            _likesTo[likee].Add(Tuple.Create(liker, ts));
        }

        public void FillLikes()
        {
            var accounts = _repo.Accounts;
            for (int i = 0; i < _likesFrom.Length; i++)
            {
                if (_likesFrom[i] != null)
                {
                    accounts[i].AddLikesFrom(_likesFrom[i]);
                }

                if (_likesTo[i] != null)
                {
                    accounts[i].AddLikesTo(_likesTo[i]);
                }
            }
        }
    }
}