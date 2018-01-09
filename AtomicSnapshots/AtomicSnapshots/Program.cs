using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AtomicSnapshots
{
    internal static class Program
    {
        private static void Main()
        {
            const int taskCount = 10;
            const int registerAmount = 2;
            
            var registers = new Register[registerAmount];

            for (var i = 0; i < registerAmount; i++)
            {
                registers[i] = new Register(0, i, registerAmount);
            }

            var tasks = new List<Task>();
            var regSnap = new RegSnap(registers);

            Task.Run(() =>
            {
                for (var i = 2; i < 12; i += 2)
                {
                    Console.WriteLine("Task {0} Update() register {1} ; value = {2}...", Task.CurrentId, 0, i);
                    regSnap.Update(0, i);
                    Thread.Sleep(100);
                }
            });

            Task.Run(() =>
            {
                for (var i = 1; i < 12; i += 2)
                {
                    Console.WriteLine("Task {0} Update() register {1} ; value = {2}...", Task.CurrentId, 1, i);
                    regSnap.Update(1, i);
                    Thread.Sleep(100);
                }
            });


            for (var i = 0; i < taskCount; i++)
            {
                tasks.Add(Task.Run(() =>
                {
                    Console.WriteLine("Task {0} Scan()...", Task.CurrentId);
                    var array = regSnap.Scan();
                    Console.WriteLine("Task {0} Scaned :>> {{ {1} , {2} }}\n", Task.CurrentId, array[0], array[1]);
                }));
                Thread.Sleep(100);
            }
            
            Task.WaitAll(tasks.ToArray());
            
        }
    }
}