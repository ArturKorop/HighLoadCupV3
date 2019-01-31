using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using HighLoadCupV3.Model.InMemory.DataSets;

namespace HighLoadCupV3.Model.InMemory
{
    public class InMemoryRepository
    {
        public int MaxAccountId { get; set; }

        public HashSet<string> Emails { get; set; } = new HashSet<string>();

        public InMemoryDataSetSex SexData { get; } = new InMemoryDataSetSex();
        public InMemoryDataSetStatus StatusData { get; } = new InMemoryDataSetStatus();
        public InMemoryDataSetPremium PremiumData { get; } = new InMemoryDataSetPremium();

        public InMemoryDataSetSName SNameData { get; } = new InMemoryDataSetSName();

        public InMemoryDataSetCity CityData { get; } = new InMemoryDataSetCity(string.Empty);
        public InMemoryDataSetCountry CountryData { get; } = new InMemoryDataSetCountry(string.Empty);

        public InMemoryDataSetByteWithNotExisted<string> FNameData { get; } = new InMemoryDataSetByteWithNotExisted<string>(string.Empty);
        public InMemoryDataSetByteWithNotExisted<int> CodeData { get; } = new InMemoryDataSetByteWithNotExisted<int>(0);

        public InMemoryDataSetByteWithAlwaysExisted<int> BirthYearData { get; } = new InMemoryDataSetByteWithAlwaysExisted<int>();
        public InMemoryDataSetByteWithAlwaysExisted<int> JoinedYearData { get; } = new InMemoryDataSetByteWithAlwaysExisted<int>();
        public InMemoryDataSetByteWithAlwaysExisted<string> DomainData { get; } = new InMemoryDataSetByteWithAlwaysExisted<string>();

        public InMemoryDataSetInterests InterestsData { get; } = new InMemoryDataSetInterests();

        public AccountData[] Accounts { get; }
        public EmailsStorage EmailsStorage { get; set; }
        public  LikesBuffer LikesBuffer { get; set; }

        public InMemoryRepository(int accountsCount)
        {
            Accounts = new AccountData[accountsCount];
            EmailsStorage = new EmailsStorage(this);
            LikesBuffer = new LikesBuffer(accountsCount, this);
#if  DEBUG
            _desiredPostCount = 5;
            _desiredGetCount = 2;
#else
            _desiredPostCount = accountsCount < 100000 ? 10000 : 90000;
            _desiredGetCount = accountsCount < 100000 ? 3000 : 27000;
#endif
        }

        private readonly int _desiredPostCount;
        private readonly int _desiredGetCount;

        private int _postCount = 0;
        private int _getCount = 0;
        private static System.Timers.Timer _swMemory;

        public void NotifyAboutGet()
        {
            Interlocked.Increment(ref _getCount);

            if (_getCount == _desiredGetCount)
            {
                Task.Run(() =>
                {
                    var sw = new Stopwatch();
                    sw.Start();
                    Console.WriteLine($"{DateTime.Now.ToLongTimeString()} Started preparation for POST");

                    _swMemory = new System.Timers.Timer(1000);
                    _swMemory.Elapsed += (sender, args) => { TotalMemoryHelper.Show(); };
                    _swMemory.Start();

                    StatusData.PrepareForUpdates();
                    CityData.PrepareForUpdates();
                    CountryData.PrepareForUpdates();
                    SNameData.PrepareForUpdates();

                    FNameData.PrepareForUpdates();
                    SexData.PrepareForUpdates();
                    PremiumData.PrepareForUpdates();
                    CodeData.PrepareForUpdates();

                    DomainData.PrepareForUpdates();
                    BirthYearData.PrepareForUpdates();
                    InterestsData.PrepareForUpdates();
                    JoinedYearData.PrepareForUpdates();

                    TotalMemoryHelper.Show();
                    GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
                    GC.Collect();
                    GC.WaitForFullGCApproach();
                    TotalMemoryHelper.Show();


                    Console.WriteLine($"{DateTime.Now.ToLongTimeString()} Finished preparation for POST in {sw.ElapsedMilliseconds} ms");
                });
            }
        }

        public void NotifyAboutPost()
        {
            Interlocked.Increment(ref _postCount);

            if (_postCount == _desiredPostCount)
            {
                Task.Run(() =>
                {
                    Console.WriteLine( $"{DateTime.Now.ToLongTimeString()} Started updates after all POST [{_postCount}]");

                    var sw = new Stopwatch();
                    sw.Start();
                    CreateMainIndexes(true, sw);


                    sw.Stop();
                    Console.WriteLine(
                        $"{DateTime.Now.ToLongTimeString()} Create indexes after all POST [{_postCount}] in {sw.ElapsedMilliseconds} ms.");
                    Emails = null;

                    GC.Collect();

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

        public bool IsExistedAccountId(int id)
        {
            return id > 0 && id <= MaxAccountId && Accounts[id] != null;
        }

        public void CreateMainIndexes(bool afterPost, Stopwatch sw)
        {
            if (afterPost)
            {
                FNameData.PrepareForSort();
                SNameData.PrepareForSort();
                SexData.PrepareForSort();
                StatusData.PrepareForSort();
                CityData.PrepareForSort();
                CountryData.PrepareForSort();
                CodeData.PrepareForSort();
                DomainData.PrepareForSort();
                BirthYearData.PrepareForSort();
                InterestsData.PrepareForSort();
                JoinedYearData.PrepareForSort();
                PremiumData.PrepareForSort();
                GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
                GC.Collect();
                GC.WaitForFullGCApproach();

                Console.WriteLine($"Preparation for sort done in [{sw.ElapsedMilliseconds}] ms");

                Task.Run(() =>
                {
                    LikesBuffer.FillLikes();
                    LikesBuffer = null;
                    Console.WriteLine($"Likes filling done in [{sw.ElapsedMilliseconds}] ms");
                });
            }

            var preparationForGroup1 = Task.Run(() =>
            {
                EmailsStorage.SortAndPropagate();
                FNameData.Sort();
                SNameData.Sort();
                SexData.Sort();
            });

            var preparationForGroup2 = Task.Run(() =>
            {
                StatusData.Sort();
                CityData.Sort();
                CountryData.Sort();
                CodeData.Sort();
                DomainData.Sort();
            });

            var preparationForGroup3 = Task.Run(() =>
            {
                BirthYearData.Sort();
                InterestsData.Sort();
                JoinedYearData.Sort();
                PremiumData.Sort();
            });

            Task.WaitAll(preparationForGroup1, preparationForGroup2, preparationForGroup3);
            Console.WriteLine($"Ids sorting done in [{sw.ElapsedMilliseconds}] ms");

            Holder.Instance.Group.CleanCacheAndCreateNewCache();
            Console.WriteLine($"Cache recreating done in [{sw.ElapsedMilliseconds}] ms");

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
        }
    }
}

