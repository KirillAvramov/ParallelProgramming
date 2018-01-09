using System;
using System.Net;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace PageDownloader
{
    internal static class Program
    {
        public static void Main(string[] args)
        {
            var time = new Stopwatch();
            var downloader = new PageDownloader();

            Console.WriteLine("Enter URI:");
            
            input:
            var input = new Uri(Console.ReadLine());
            Console.WriteLine("Characters: ");
            try
            {
                time.Start();
                downloader.Download(input);
                time.Stop();
                Console.WriteLine("Time: {0}", time.Elapsed);
            }
            catch (ArgumentNullException)
            {
                Console.WriteLine("Wrong input. Try again:");
                goto input;
            }
            
            catch(UriFormatException)
            {
                Console.WriteLine("Wrong input. Try again:");
                goto input;
            }

            
        }
    }

    internal class PageDownloader
    {
        public void Download(Uri linkUri)
        {
            const string form = @"href=""(?<url>http(s)?[\w\.:?&-_=#/]*)""+?";
            
            var links = new List<string>();
            var client = new WebClient();
            var expression = new Regex(form, RegexOptions.IgnoreCase);

            var parentString = client.DownloadString(linkUri);
            
            Console.WriteLine("{0} => {1}", linkUri, parentString.Length);

            var matches = expression.Matches(parentString);

            foreach (Match match in matches)
            {
                links.Add(match.ToString().Split('"')[1]);
            }

            try
            {
                Task.WaitAll(links.Select(DownloadSublinks).ToArray());
            }
            catch
            {
                // ignored
            }
        }

        private static async Task DownloadSublinks(string link)
        {
            using (var client = new WebClient())
            {
                var uri = new Uri(link);
                client.DownloadDataCompleted += Result;
                client.Headers["address"] = link;
                await client.DownloadDataTaskAsync(uri);
            }
        }

        private static void Result(object obj, DownloadDataCompletedEventArgs e)
        {
            if (obj is WebClient client && !e.Cancelled && e.Error == null)
            {
                Console.WriteLine("{0} => {1}", client.Headers["address"], client.Encoding.GetString(e.Result).Length);
            }
        }
    }
}