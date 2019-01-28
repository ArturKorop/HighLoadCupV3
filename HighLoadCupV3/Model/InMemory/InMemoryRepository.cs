﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime;
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

        public InMemoryDataSetShortWithNotExisted CityData { get; } = new InMemoryDataSetShortWithNotExisted(string.Empty);
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
            _desiredPostCount = accountsCount < 100000 ? 10000 : 90000;
        }

        private readonly int _desiredPostCount;

        private int _postCount = 0;
        private static Timer _swMemory;

        public void NotifyAboutPost()
        {
            lock (this)
            {
                _postCount++;
            }

            if (_postCount == _desiredPostCount)
            {
                Task.Run(() =>
                {
                    Console.WriteLine( $"{DateTime.Now.ToLongTimeString()} Started updates after all POST [{_postCount}]");
                    _swMemory = new Timer(1000);
                    _swMemory.Elapsed += (sender, args) => { TotalMemoryHelper.Show(); };
                    _swMemory.Start();

                    var sw = new Stopwatch();
                    sw.Start();
                    CreateMainIndexes(true);


                    sw.Stop();
                    Console.WriteLine(
                        $"{DateTime.Now.ToLongTimeString()} Create indexes after all POST [{_postCount}] in {sw.ElapsedMilliseconds} ms.");

                    GC.Collect();

                    Emails = null;
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

        public void CreateMainIndexes(bool afterPost)
        {
            EmailsStorage.SortAndPropagate();

            if (afterPost)
            {
                Task.Run(() =>
                {
                    LikesBuffer.FillLikes();
                    LikesBuffer = null;
                });
            }

            Parallel.Invoke( () => EmailsStorage.SortAndPropagate(),
                () => FNameData.Sort(), ()=>SNameData.Sort(),()=> SexData.Sort(),
                ()=> StatusData.Sort(),()=>  CityData.Sort(),()=> CountryData.Sort(),
                ()=> CodeData.Sort(),()=> DomainData.Sort(),()=>  BirthYearData.Sort(),
                ()=> InterestsData.Sort(),()=> JoinedYearData.Sort(),()=> PremiumData.Sort());

            Holder.Instance.Group.CleanCacheAndCreateNewCache();

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

    public interface IResetable
    {
        void Reset();
    }

    public class ObjectPool<T> where T : IResetable, new()
    {
        public Queue<T> _pool;

        public ObjectPool(int initialCount)
        {
            _pool = new Queue<T>();
            for (int i = 0; i < initialCount; i++)
            {
                _pool.Enqueue(new T());
            }
        }

        public T Rent()
        {
            lock (_pool)
            {
                if(_pool.Count > 0)
                {
                    return _pool.Dequeue();
                }
                else
                {
                    return new T();
                }
            }
        }

        public void Return(T obj)
        {
            lock (_pool)
            {
                obj.Reset();
                _pool.Enqueue(obj);
            }
        }

        public void Return(IEnumerable<T> objects)
        {
            lock (_pool)
            {
                foreach (var obj in objects)
                {
                    obj.Reset();
                    _pool.Enqueue(obj);
                }
            }
        }
    }

    public class DictionaryPool<TKey, TValue> 
    {
        public readonly Queue<Dictionary<TKey, TValue>> _pool;
        private readonly int _capacity;

        public DictionaryPool(int initialCount, int capacity)
        {
            _capacity = capacity;
            _pool = new Queue<Dictionary<TKey, TValue>>();
            for (int i = 0; i < initialCount; i++)
            {
                _pool.Enqueue(new Dictionary<TKey, TValue>(capacity));
            }
        }

        public Dictionary<TKey, TValue> Rent()
        {
            lock (_pool)
            {
                if (_pool.Count > 0)
                {
                    return _pool.Dequeue();
                }
                else
                {
                    return new Dictionary<TKey, TValue>(_capacity);
                }
            }
        }

        public void Return(Dictionary<TKey, TValue> dictionary)
        {
            lock (_pool)
            {
                dictionary.Clear();
                _pool.Enqueue(dictionary);
            }
        }
    }
}

