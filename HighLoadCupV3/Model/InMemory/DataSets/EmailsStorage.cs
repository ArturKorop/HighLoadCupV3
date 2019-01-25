using System;
using System.Collections.Generic;

namespace HighLoadCupV3.Model.InMemory.DataSets
{
    public class EmailsStorage
    {
        private readonly InMemoryRepository _repo;
        private List<string> _emails;

        public EmailsStorage(InMemoryRepository repo)
        {
            _repo = repo;
        }

        public void SortAndPropagate()
        {
            var accounts = _repo.Accounts;
            var emails = new List<Tuple<int, string>>();
            for (int i = 0; i < accounts.Length; i++)
            {
                if (accounts[i] != null)
                {
                    emails.Add(Tuple.Create(i, accounts[i].Email));
                }
            }

            emails.Sort((x,y) => string.CompareOrdinal(x.Item2, y.Item2));

            for (int i = 0; i < emails.Count; i++)
            {
                accounts[emails[i].Item1].EmailSortedIndex = i;
            }

            _emails = new List<string>();
            foreach (var email in emails)
            {
                _emails.Add(email.Item2);
            }
        }

        public int GetPossiblePosition(string prefix)
        {
            var low = _emails.BinarySearch(prefix, StringComparer.Ordinal);
            if (low < 0)
            {
                low = ~low;
            }

            return low;
        }
    }
}
