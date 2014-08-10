using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace TestConsoleApplication
{
    class Program
    {
        static void Main(string[] args)
        {
            //代码“预热”
            CodeTimerNX4.CodeTimer.Initialize();

            //测试CPU时钟周期
            //CodeTimerNX4.CodeTimer.Time("Thread Sleep", 1, () => { Thread.Sleep(3000); });
            //CodeTimerNX2.CodeTimer.Time("Thread Sleep(2.0)", 1, delegate() { Thread.Sleep(3000); });

            //CodeTimerNX4.CodeTimer.Time("Empty method", 10000000, () => { });
            //CodeTimerNX2.CodeTimer.Time("Empty method", 10000000, delegate() { });

            //垃圾收集次数统计
            int iteration = 100 * 100;
            string s = "";
            CodeTimerNX4.CodeTimer.Time("String concat", iteration, () => { s += "a"; });
            StringBuilder sb = new StringBuilder();
            CodeTimerNX4.CodeTimer.Time("StringBuilder", iteration, () => { sb.Append("a"); });
            //CodeTimerNX2.CodeTimer.Time("String concat", iteration, delegate() { s += "a"; });
            //CodeTimerNX2.CodeTimer.Time("StringBuilder", iteration, delegate(){ sb.Append("a"); });

            Console.ReadKey();
        }
    }
}
