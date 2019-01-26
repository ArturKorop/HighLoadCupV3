using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime;
using System.Threading.Tasks;
using HighLoadCupV3.Model.InMemory.DataSets;

namespace HighLoadCupV3.Model.InMemory
{
    public class InMemoryRepository
    {
        public int MaxAccountId { get; set; }
        //TODO: remove SortedId and IdSet - use only account and MaxAccount 
        //public List<int> SortedId { get; private set; } = new List<int>();
        public HashSet<int> IdSet { get; private set; } = new HashSet<int>();
        public HashSet<string> Emails { get; set; } = new HashSet<string>();

        public InMemoryDataSetSex SexData { get; } = new InMemoryDataSetSex();
        public InMemoryDataSetStatus StatusData { get; } = new InMemoryDataSetStatus();
        public InMemoryDataSetPremium PremiumData { get; } = new InMemoryDataSetPremium();

        public InMemoryDataSetSName SNameData { get; } = new InMemoryDataSetSName();

        public InMemoryDataSetShortWithNotExisted CityData { get; } = new InMemoryDataSetShortWithNotExisted(string.Empty);
        public InMemoryDataSetCountry CountryData { get; } = new InMemoryDataSetCountry(string.Empty);

        public InMemoryDataSetByteWithNotExisted<string> FNameData { get; } = new InMemoryDataSetByteWithNotExisted<string>(string.Empty);
        public InMemoryDataSetByteWithNotExisted<int> CodeData { get; } = new InMemoryDataSetByteWithNotExisted<int>(0);

        public InMemoryDataSetByteWithAlwaysExisted<int> BirthYearData { get; } = new InMemoryDataSetByteWithAlwaysExisted<int>();
        public InMemoryDataSetByteWithAlwaysExisted<int> JoinedYearData { get; } = new InMemoryDataSetByteWithAlwaysExisted<int>();
        public InMemoryDataSetByteWithAlwaysExisted<string> DomainData { get; } = new InMemoryDataSetByteWithAlwaysExisted<string>();

        public InMemoryDataSetInterests InterestsData { get; } = new InMemoryDataSetInterests();

        public AccountData[] Accounts { get; }
        //public ILikesStorage LikesStorage { get; }
        public EmailsStorage EmailsStorage { get; set; }
        public  LikesBuffer LikesBuffer { get; set; }

        public InMemoryRepository(int accountsCount)
        {
            Accounts = new AccountData[accountsCount];
            //LikesStorage = new CombinedLikesStorage(accountsCount);
            EmailsStorage = new EmailsStorage(this);
            LikesBuffer = new LikesBuffer(accountsCount, this);
            _desiredPostCount = accountsCount < 100000 ? 10000 : 90000;
        }

        private readonly int _desiredPostCount;

        private int _postCount = 0;

        public void NotifyAboutPost()
        {
            lock (this)
            {
                _postCount++;
            }

            if (_postCount == _desiredPostCount)
            {
                Console.WriteLine($"{DateTime.Now.ToLongTimeString()} Started updates after all POST [{_postCount}]");
                Task.Run(() =>
                {
                    var sw = new Stopwatch();
                    sw.Start();
                    CreateMainIndexes(true);

                    sw.Stop();
                    Console.WriteLine(
                        $"{DateTime.Now.ToLongTimeString()} Create indexes after all POST [{_postCount}] in {sw.ElapsedMilliseconds} ms.");

                    Emails = null;
                    IdSet = null;
                });
            }
        }

        public IEnumerable<AccountData> GetSortedAccounts()
        {
            for (int i = MaxAccountId; i >= 0; i--)
            {
                if (Accounts[i] != null)
                {
                    yield return Accounts[i];
                }
            }
        }

        public void CreateMainIndexes(bool afterPost)
        {
            //SortedId = IdSet.ToList();
            //SortedId.Sort(new DescComparer());
            EmailsStorage.SortAndPropagate();

            if (afterPost)
            {
                LikesBuffer.FillLikes();
                LikesBuffer = null;
            }

            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect();

            Parallel.Invoke(
                () => FNameData.Sort(), ()=>SNameData.Sort(),()=> SexData.Sort(),
                ()=> StatusData.Sort(),()=>  CityData.Sort(),()=> CountryData.Sort(),
                ()=> CodeData.Sort(),()=> DomainData.Sort(),()=>  BirthYearData.Sort(),
                ()=> InterestsData.Sort(),()=> JoinedYearData.Sort(),()=> PremiumData.Sort());

            Holder.Instance.Group.CleanCacheAndCreateNewCache();

            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect();

            Statistics(false);
        }

        private void Statistics(bool full)
        {
            Console.WriteLine($"Current TimeStamp: {Holder.Instance.CurrentTimeStamp}");

            Console.WriteLine($"Sex {SexData.GetStatistics(full)}");
            Console.WriteLine($"Status {StatusData.GetStatistics(full)}");
            Console.WriteLine($"Premium {PremiumData.GetStatistics(full)}");

            Console.WriteLine($"City {CityData.GetStatistics(full)}");
            Console.WriteLine($"Country {CountryData.GetStatistics(full)}");

            Console.WriteLine(value: $"Code {CodeData.GetStatistics(full)}");
            Console.WriteLine($"Domain {DomainData.GetStatistics(full)}");
            Console.WriteLine($"FName {FNameData.GetStatistics(full)}");
            Console.WriteLine($"SName {SNameData.GetStatistics(full)}");

            Console.WriteLine($"Interests {InterestsData.GetStatistics(full)}");

            Console.WriteLine($"BirthYear {BirthYearData.GetStatistics(full)}");
            Console.WriteLine($"JoinedYear {JoinedYearData.GetStatistics(full)}");

            //Console.WriteLine($"Likes Storage type: {LikesStorage.GetType()}");
        }
    }

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

