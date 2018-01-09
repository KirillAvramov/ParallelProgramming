using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace PrimeNumbers
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Введите границы промежутка:");
            Console.Write("Нижняя граница = ");
            uint a = Convert.ToUInt32(Console.ReadLine());
            Console.ReadKey();
            Console.Write("Верхняя граница = ");
            uint b = Convert.ToUInt32(Console.ReadLine());
            Console.ReadKey();
            Console.WriteLine("Все простые числа в промежутке от {0} до {1}", a, b);
            
            var mid = a + (b - a ) / 2 ;
            
            //parallel by 2 tasks
            var timer = Stopwatch.StartNew();
            var tasks = new Task[2];
            tasks[0] = Task.Factory.StartNew(() => PrimeNumbersIn(a, mid));
            tasks[1] = Task.Factory.StartNew(() => PrimeNumbersIn(mid + 1, b));
            Task.WaitAll(tasks);
            timer.Stop();
            Console.WriteLine("Времени потрачено: {0}", timer.Elapsed);
            Console.WriteLine("Нажмите любую клавишу для продолжения.");
            Console.ReadKey(); Console.WriteLine();
            
            //parallel by 2 threads
            timer.Restart();
            var threads = new Thread[2];
            threads[0] = new Thread(() => PrimeNumbersIn(a, mid)); threads[0].Start();
            threads[1] = new Thread(()  => PrimeNumbersIn(mid + 1, b)); threads[1].Start();
            threads[0].Join();
            threads[1].Join();
            timer.Stop();
            Console.WriteLine("Времени потрачено: {0}", timer.Elapsed);
            Console.WriteLine("Нажмите любую клавишу для продолжения.");
            Console.ReadKey(); Console.WriteLine();
            
            //parallel by thread in pool
            timer.Restart();
            var myEvent = new ManualResetEvent(false);
            ThreadPool.QueueUserWorkItem(delegate
            {
                PrimeNumbersIn(a, b);
                myEvent.Set();
            });
            myEvent.WaitOne();
            timer.Stop();
            Console.WriteLine("Времени потрачено: {0}", timer.Elapsed);
            Console.ReadLine();

        }

        static void PrimeNumbersIn(uint a, uint b)     // b > a
        {
            var projection = new bool[b - a + 1];
            for (uint i = 0; i < b - a + 1; i++) projection[i] = true;
            
            for (uint i = 2; i <= b / 2; i++)
            {
                var modi = a % i;
                if (modi == 0 && projection[0] && i != a) projection[0] = false;

                modi = i - modi;
                while (modi < b - a + 1)
                {
                    if (projection[modi] && i != a+modi) projection[modi] = false;
                    modi += i;
                }
                
            }
            
            for (uint i = 0; i < b - a + 1; i++) if (projection[i]) Console.WriteLine(i+a);
        }
    }
}