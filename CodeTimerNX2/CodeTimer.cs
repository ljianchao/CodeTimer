using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Threading;
using System.Diagnostics;

namespace CodeTimerNX2
{
    public static class CodeTimer
    {
        public delegate void Action();

        static CodeTimer()
        {
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
            Thread.CurrentThread.Priority = ThreadPriority.Highest;
        }

        [DllImport("kernel32.dll")]
        static extern IntPtr GetCurrentThread();

        /// <summary>
        /// GetThreadTimes 给出了线程在内核态和用户态占用的时间，单位是 100 ns，两个时间的总和就是线程占用的CPU时间
        /// GetThreadTimes 这个API函数的进度没有 QueryThreadCycleTime 高
        /// </summary>
        /// <param name="hThread"></param>
        /// <param name="lpCreationTime"></param>
        /// <param name="lpExitTime"></param>
        /// <param name="lpKernelTime"></param>
        /// <param name="lpUserTime"></param>
        /// <returns></returns>
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool GetThreadTimes(IntPtr hThread, out long lpCreationTime,
            out long lpExitTime, out long lpKernelTime, out long lpUserTime);

        private static long GetCurrentThreadTimes()
        {
            long l;
            long kernelTime, userTime;
            GetThreadTimes(GetCurrentThread(), out l, out l, out kernelTime, out userTime);
            return kernelTime + userTime;
        }

        public static void Time(string name, int iteration,Action action)
        { 
            if(string.IsNullOrEmpty(name))
                return;

            if(action == null)
                return;

            //1.保留当前控制台前景色，并使用黄色输出名称参数
            ConsoleColor currentForeColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(name);

            //2.强制GC进行收集，并记录目前各代已经收集的次数
            GC.Collect(GC.MaxGeneration);
            int[] gcCounts = new int[GC.MaxGeneration + 1];
            for (int i = 0; i <= GC.MaxGeneration; i++)
			{
                gcCounts[i] = GC.CollectionCount(i);
			}

            //3.执行代码，记录下消耗的时间及CPU占用时间
            Stopwatch watch = new Stopwatch();
            watch.Start();
            
            long ticksFst = GetCurrentThreadTimes();
            for (int i = 0; i < iteration; i++)
			{
                action();
			}
            long ticks = GetCurrentThreadTimes() - ticksFst;
            watch.Stop();

            //4.恢复控制台默认前景色，并打印出消耗时间及CPU时钟周期
            Console.ForegroundColor = currentForeColor;
            Console.WriteLine("\tTime Elapesd:\t{0} ms", watch.ElapsedMilliseconds.ToString("N0"));
            Console.WriteLine("\tTime Elapesd(one time):\t{0}ms", (watch.ElapsedMilliseconds / iteration).ToString("N0"));
            Console.WriteLine("\tCPU time:\t{0}ns", (ticks * 100).ToString("N0"));
            Console.WriteLine("\tCpu time(one time):\t{0}ns", (ticks * 100 / iteration).ToString("N0"));

            //5.打印执行过程中各代垃圾收集回收次数
            for (int i = 0; i <= GC.MaxGeneration; i++)
            {
                int count = GC.CollectionCount(i) - gcCounts[i];
                Console.WriteLine("\tGen {0}:\t\t{1}", i, count);
            }
        }
    }
}
