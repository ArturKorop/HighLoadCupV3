using System;
using System.Runtime;
using HighLoadCupV3.Model;
using HighLoadCupV3.Model.Filters.Filter;
using HighLoadCupV3.Model.Filters.Group;
using HighLoadCupV3.Model.Filters.Recommend;
using HighLoadCupV3.Model.Filters.Suggest;
using HighLoadCupV3.Model.InMemory;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace HighLoadCupV3
{
    public class Program
    {
        public static void Main(string[] args)
        {
            GCSettings.LatencyMode = GCLatencyMode.LowLatency;
            Console.WriteLine("Is server GC: " + GCSettings.IsServerGC);
            Console.WriteLine("Latency Mode: " + GCSettings.LatencyMode);

            var dataFilePath = "/tmp/data/data.zip";
            var optionsPath = "/tmp/data/options.txt";
            var extractPath = "zip";
            var retriever = new FileReader();

            var dataLoader = new DataLoader();
            var accountsFileCount = retriever.ReadAccountFilesCount(dataFilePath);
            var accountsCount = (int)(accountsFileCount * 10000) + 22000;

            var inMemory = new InMemoryRepository(accountsCount);

            Holder.Instance.InMemory = inMemory;

            var updater = new RepositoryUpdater(inMemory);
            Holder.Instance.Updater = updater;
            dataLoader.Load(dataFilePath, extractPath, optionsPath, inMemory, updater, retriever);

            Holder.Instance.Filter = new Filter(inMemory);
            Holder.Instance.Group = new Group(inMemory, new GroupFactory(inMemory));
            Holder.Instance.GroupQueryParser = new GroupQueryParser();
            Holder.Instance.Recommend = new Recommend(inMemory);
            Holder.Instance.Suggest = new Suggest(inMemory);

            inMemory.CreateMainIndexes(false);


            GCSettings.LargeObjectHeapCompactionMode = GCLargeObjectHeapCompactionMode.CompactOnce;
            GC.Collect();
            GC.WaitForFullGCComplete();
            CreateWebHostBuilder(args).Build().Run();
        }

        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseLibuv()
                .UseStartup<Startup>();
    }
}
