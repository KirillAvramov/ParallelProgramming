using System;
using System.Diagnostics;
using System.Security.Cryptography;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace ParallelMd5
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Stopwatch clock = Stopwatch.StartNew();

            GetMd5("/home/jwhite/Desktop/Boolshit/");

            clock.Stop();

            Console.WriteLine("Потрачено времени: {0}", clock.Elapsed);
        }

        private static string GetMd5(string path)
        {
            
            var files = Directory.GetFiles(path);
            var subDirs = Directory.GetDirectories(path);
            
            int filesQuantity = files.Length;
            int dirQuantity = subDirs.Length;
            
            var hashes = new StringBuilder();
            
            var fileTasks = new Task[filesQuantity];
            var dirTasks = new Task[dirQuantity];
            
            for (var i = 0; i < filesQuantity; i++)
            {
                var i1 = i;
                fileTasks[i] = Task.Run((() => Md5FromFile(files[i1])));
            }
            
            for (var i = 0; i < dirQuantity; i++)
            {
                var i1 = i;
                dirTasks[i] = Task.Run((() => GetMd5(subDirs[i1])));
            }

            using (var md5 = MD5.Create())
            {
                byte[] byteHash = md5.ComputeHash(Encoding.UTF8.GetBytes(path));
                var pathBuilder = new StringBuilder();
                foreach (byte t in byteHash)
                {
                    pathBuilder.Append(t.ToString("X2"));
                }
                hashes.Append(pathBuilder);
            }
            
            for (int i = 0; i < filesQuantity; i++)
            {
                hashes.Append(fileTasks[i]);
            }
            
            
            Task.WaitAll(dirTasks);
            Task.WaitAll(fileTasks);
            
            
            for (int i = 0; i < dirQuantity; i++)
            {
                hashes.Append(dirTasks[i]);
            }
            return hashes.ToString();
        }
        
        
        
        private static string Md5FromFile(string path) //this algorithm partly from msdn
        {
            var hashs = new StringBuilder();
            using (MD5 md5 = MD5.Create())
            {
                byte[] byteHash = md5.ComputeHash(Encoding.UTF8.GetBytes(path));
                foreach (byte t in byteHash)
                {
                    hashs.Append(t.ToString("X2"));
                }
                         
                using (var stream = new BufferedStream(File.OpenRead(path)))
                {
                    byte[] data = md5.ComputeHash(stream);
                    foreach (byte t in data)
                    {
                        hashs.Append(t.ToString("X2"));
                    }
                }
            }
            return hashs.ToString();
        }
    }
}