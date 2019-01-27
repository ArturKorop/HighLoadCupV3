using System;

namespace HighLoadCupV3
{
    public static class TotalMemoryHelper
    {
        private const int BytesInMb = 1024 * 1024;
        private static long _prev = 0;

        public static void Show()
        {
            //Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] Total memory - {GC.GetTotalMemory(false)/BytesInMb}");
            var current = GC.GetTotalMemory(false);
            var diff = (current - _prev) / 1024;
            Console.WriteLine($"[{DateTime.Now.ToLongTimeString()}] Total memory - {current/1024} Kb, Diff - {diff} Kb");
            _prev = current;
        }
    }
}