using System.Collections.Generic;
using System.Linq;
using HighLoadCupV3.Model.InMemory;

namespace HighLoadCupV3.Model.Filters.InMemoryFilters
{
    public class LikesContainsIMFilter
    {
        private readonly IComparer<int> _comparer = new DescComparer();
        private readonly DescLikeDtoComparer _likeDtoComparer = new DescLikeDtoComparer();
        private int[] _value;
        private readonly InMemoryRepository _repo;
        private bool _isValid = true;
        private bool _isContainsData = true;

        public LikesContainsIMFilter(InMemoryRepository repo, string value)
        {
            _repo = repo;
            ValidateAndParseValue(value);
        }

        private void ValidateAndParseValue(string value)
        {
            var likes = value.Split(',');
            _value = new int[likes.Length];

            for (int i = 0; i < likes.Length; i++)
            {
                if(!int.TryParse(likes[i], out int id) || id < 0 || id >= _repo.Accounts.Length || _repo.Accounts[id] == null)
                {
                    _isValid = false;
                    return;
                }

                if (!_repo.Accounts[id].AnyLikesTo())
                {
                    _isContainsData = false;
                    return;
                }

                _value[i] = id;
            }
        }

        public string Field => Names.Likes;

        // TODO: get rid of soring for groups
        public  IEnumerable<AccountData> Filter()
        {
            if (_value.Length == 1)
            {
                var likes = _repo.Accounts[_value[0]].GetLikesTo().ToList();
                likes.Sort(_comparer);

                var prev = -1;
                foreach(var likeId in likes)
                {
                    if(likeId != prev)
                    {
                        prev = likeId;
                        yield return _repo.Accounts[likeId];
                    }

                }

            }
            else
            {
                var likes = _value
                    .Select(v => _repo.Accounts[v].GetLikesTo().ToHashSet()).ToArray();
                var first = likes[0].OrderByDescending(x => x);

                var prev = -1;
                foreach (var liker in first)
                {
                    bool ok = true;
                    for (int i = 1; i < likes.Length; i++)
                    {
                        if (!likes[i].Contains(liker))
                        {
                            ok = false;
                            break;
                        }

                    }

                    if (ok && liker != prev)
                    {
                        prev = liker;
                        yield return _repo.Accounts[liker];
                    }
                }
            }
        }

        public bool IsValid => _isValid;

        public bool IsEmpty => !_isContainsData;
    }
}
