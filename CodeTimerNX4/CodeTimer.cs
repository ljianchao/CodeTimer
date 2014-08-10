using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace CodeTimerNX4
{
    public static class CodeTimer
    {
        //在测试开始前调用
        //当前进程及当前线程的优先级设为最高，这样便可以相对减少操作系统在调度上造成的干扰
        //调用一次Time方法进行“预热”，让JIT将IL编译成本地代码，让Time方法尽快“进入状态”
        public static void Initialize()
        {
            //当前进程的优先级设为最高
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
            //当前线程的优先级设为最高
            Thread.CurrentThread.Priority = ThreadPriority.Highest;
            Time("", 1, () => { });
        }

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool QueryThreadCycleTime(IntPtr threadHandler, ref ulong cycleTime);

        [DllImport("kernel32.dll")]
        static extern IntPtr GetCurrentThread();

        private static ulong GetCycleCount()
        {
            ulong cycleCount = 0;
            QueryThreadCycleTime(GetCurrentThread(), ref cycleCount);
            return cycleCount;
        }

        /// <summary>
        /// 测试方法
        /// </summary>
        /// <param name="name">测试名称</param>
        /// <param name="iteration">迭代计数</param>
        /// <param name="action">要测试方法委托</param>
        public static void Time(string name, int iteration, Action action)
        {
            Time(name, iteration, action, Console.WriteLine);
        }

        /// <summary>
        /// 测试方法
        /// </summary>
        /// <param name="name">测试名称</param>
        /// <param name="iteration">迭代计数</param>
        /// <param name="action">要测试方法委托</param>
        /// <param name="output">输出平台，如web,winform,控制台</param>
        public static void Time(string name, int iteration, Action action, Action<string> output)
        {
            if (String.IsNullOrEmpty(name))
                return;

            //1.保留当前控制台前景色，并使用黄色输出名称参数
            ConsoleColor currentForeColor = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            output(name);

            //2.强制GC进行收集，并记录目前各代已经收集的次数
            GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced);
            int[] gcCounts = new int[GC.MaxGeneration + 1];
            for (int i = 0; i <= GC.MaxGeneration; i++)
            {
                gcCounts[i] = GC.CollectionCount(i);
            }

            //3.执行代码，记录下消耗的时间及CPU时钟周期1
            //CPU时钟周期是性能计数中的辅助参考
            //说明CPU分配了多少时间片给这段方法来执行，它和消耗时间并没有必然联系
            //例如Thread.Sleep方法会让CPU暂时停止对当前线程的“供给”，这样虽然消耗了时间，但是节省了CPU时钟周期
            Stopwatch watch = new Stopwatch();
            watch.Start();
            ulong cycleCount = GetCycleCount();
            for (int i = 0; i < iteration; i++)
            {
                action();
            }
            ulong cpuCycles = GetCycleCount() - cycleCount;
            watch.Stop();

            //4.恢复控制台默认前景色，并打印出消耗时间及CPU时钟周期
            Console.ForegroundColor = currentForeColor;
            output(string.Format("\tTime Elapesd:\t{0} ms", watch.ElapsedMilliseconds.ToString("N0")));
            output(string.Format("\tCPU cycles:\t{0}", cpuCycles.ToString("N0")));

            //5.打印执行过程中各代垃圾收集回收次数
            for (int i = 0; i <= GC.MaxGeneration; i++)
            {
                int count = GC.CollectionCount(i) - gcCounts[i];
                output(string.Format("\tGen {0}:\t\t{1}", i, count));
            }

            output(string.Empty);
        }
    }
}
