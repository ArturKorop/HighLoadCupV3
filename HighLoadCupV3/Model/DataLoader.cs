using System;
using System.Diagnostics;
using HighLoadCupV3.Model.InMemory;

namespace HighLoadCupV3.Model
{
    public class DataLoader
    {
        public void Load(string path, string extractPath, string optionsPath, InMemoryRepository repo, RepositoryUpdater updater, FileReader retriever)
        {
            var notificationCount = 100000;
            int count = 0;
            int iteration = 1;

            Holder.Instance.CurrentTimeStamp = retriever.ReadTimeStamp(optionsPath);

            var sw = new Stopwatch();
            sw.Start();

            count = 0;
            iteration = 0;

            Console.WriteLine("Fill main data");
            foreach (var dto in retriever.ReadDto(path, extractPath, true))
            {
                updater.AddNewAccount(dto, false);
                count++;
                if (count % notificationCount == 0)
                {
                    Console.WriteLine($"Saved {notificationCount * iteration} in {sw.ElapsedMilliseconds / 1000} seconds.");
                    iteration++;
                }
            }

            Console.WriteLine($"Fill main data in {sw.ElapsedMilliseconds / 1000} seconds");

            foreach (var dto in retriever.ReadDto(path, extractPath, false))
            {
                updater.AddLikesToNewAccounts(dto);
                count++;
                if (count % notificationCount == 0)
                {
                    Console.WriteLine($"Likes added {notificationCount * iteration} in {sw.ElapsedMilliseconds / 1000} seconds.");
                    iteration++;
                }
            }

            Console.WriteLine($"Fill likes in {sw.ElapsedMilliseconds / 1000} seconds");

        }
    }
}
