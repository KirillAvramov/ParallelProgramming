using System;
using System.Diagnostics;
using System.Threading.Tasks;


namespace ParallelBst
{
    internal static class Program
    {
        public static void Main()
        {
            var parallelBst = new ParallelBst<int>();
            
            var time = new Stopwatch();


            var keysToInsert = new int[1000000];
            var keysToDelete = new int[1000000];
            var keysToFind = new int[1000000];

            var random = new Random();

            
            for (var i = 0; i < keysToInsert.Length; i++) keysToInsert[i] = random.Next(-1000000, 1000000);
            

            for (var i = 0; i < keysToDelete.Length; i++)
            {
                var j = random.Next(0, keysToInsert.Length);
                keysToDelete[i] = keysToInsert[j];
            }

            for (var i = 0; i < keysToFind.Length; i++)
            {
                var j = random.Next(0, keysToInsert.Length);
                keysToFind[i] = keysToInsert[j];
            }

            
            time.Start();
            Parallel.ForEach(keysToInsert, key => { parallelBst.ParallelInsert(key); });
            Console.WriteLine(parallelBst.Keys()+ " " + parallelBst.AreInserted);
            time.Stop();
            Console.WriteLine("parallelBst insert: {0}", time.Elapsed);
            Console.WriteLine();
            
            time.Restart();
            Parallel.ForEach(keysToDelete, key => { parallelBst.ParallelRemove(key); });
            time.Stop();
            Console.WriteLine(parallelBst.Keys() + " " + parallelBst.AreDeleted+ " " + parallelBst.Sim);
            Console.WriteLine("parallelBst delete: {0}", time.Elapsed);
            Console.WriteLine();
            
            time.Restart();
            Parallel.ForEach(keysToFind, key => { parallelBst.Find(key); });
            time.Stop();
            Console.WriteLine("parallelBst search: {0}", time.Elapsed);
            Console.WriteLine();
            
        }
    }
}