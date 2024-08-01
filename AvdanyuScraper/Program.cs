using AvdanyuScraper.Services;
using System;
using System.IO;
using System.Linq;
using Serilog;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Xml;
using Microsoft.VisualBasic.FileIO;
using System.Windows.Input;
using System.Xml.Linq;

namespace AvdanyuScraper
{
    public class Program
    {
        private static readonly List<string> MovieExtensions = new List<string> { ".avi", ".mp4", ".wmv", ".mkv" };

        [STAThread]
        public static void Main(string[] args)
        {
            ConfigureLogging();
            var movieInfoSrv = new MovieInformationService();
            var nfoSrv = new NfoService();

            UpdateActorsInSelectedFolder(movieInfoSrv, nfoSrv);
            //var date = DateTime.Now.AddDays(-1);
            //UpdateActorInSelectedDate(date, movieInfoSrv, nfoSrv);
            //RenameMovies(@"I:\上月\JAV_output");

            Console.WriteLine("按任意键退出...");
            Console.ReadKey();
        }

        private static void UpdateActorInSelectedDate(DateTime date, MovieInformationService movieInfoService, NfoService nfoService)
        {
            var nfos = new List<string>();
            var rootDirectory = @"D:\上个月\JAV_output";

            // Find all recent nfos
            var allNfos = Directory.GetFiles(rootDirectory, "*.nfo", System.IO.SearchOption.AllDirectories);
            foreach(var nfo in allNfos)
            {
                if(File.GetCreationTime(nfo) > date)
                {
                    nfos.Add(nfo);
                }
            }
            // Add actress - nfos to dictionary
            var actressDic = new Dictionary<string, List<string>>();
            foreach (var nfo in nfos)
            {
                var splitedPath = nfo.Split("\\");
                if (splitedPath.Length > 0)
                {
                    var searchName = splitedPath[splitedPath.Length - 3];
                    if (!actressDic.ContainsKey(searchName))
                    {
                        actressDic.Add(searchName, new List<string>() { nfo });
                    }
                    else
                    {
                        actressDic[searchName].Add(nfo);
                    }
                }
            }
            // Process Nfos
            foreach (var actress in actressDic)
            {
                Log.Debug($"Thread {Thread.CurrentThread.ManagedThreadId}: 开始获取 {actress.Key} 元数据...");
                var movieInfomation = movieInfoService.GetMovieInformation(actress.Key);
                foreach (string nfo in actress.Value)
                {
                    nfoService.UpdaeteNfoData(movieInfomation, nfo);
                }
                Log.Debug($"Thread {Thread.CurrentThread.ManagedThreadId}: 完成获取 {actress.Key} 元数据！\n\r");          
            }
            RenameMovies(rootDirectory);
        }

        private static void UpdateActorsInSelectedFolder(MovieInformationService movieInfoService, NfoService nfoService)
        {
            var fbd = new FolderBrowserDialog();
            if (fbd.ShowDialog() == DialogResult.OK)
            {
                var directories = Directory.GetDirectories(fbd.SelectedPath);
                foreach (var dir in directories)
                {
                    // Find all recent nfos
                    var allNfos = Directory.GetFiles(dir, "*.nfo", System.IO.SearchOption.AllDirectories);
                    // Add actress - nfos to dictionary
                    var actressDic = new Dictionary<string, List<string>>();
                    foreach (var nfo in allNfos)
                    {
                        var splitedPath = nfo.Split("\\");
                        if (splitedPath.Length > 0)
                        {
                            var searchName = splitedPath[splitedPath.Length - 3];
                            if (!actressDic.ContainsKey(searchName))
                            {
                                actressDic.Add(searchName, new List<string>() { nfo });
                            }
                            else
                            {
                                actressDic[searchName].Add(nfo);
                            }
                        }
                    }
                    // Process Nfos
                    foreach (var actress in actressDic)
                    {
                        Log.Debug($"Thread {Thread.CurrentThread.ManagedThreadId}: 开始获取 {actress.Key} 元数据...");
                        var movieInfomation = movieInfoService.GetMovieInformation(actress.Key);
                        foreach (string nfo in actress.Value)
                        {
                            nfoService.UpdaeteNfoData(movieInfomation, nfo);
                        }
                        Log.Debug($"Thread {Thread.CurrentThread.ManagedThreadId}: 完成获取 {actress.Key} 元数据！\n\r");
                    }
                }
                RenameMovies(fbd.SelectedPath);
            }
        }

        private static void RenameMovies(string rootDirectory)
        {
            var allMovieNfos = Directory.GetFiles(rootDirectory, "*.nfo", System.IO.SearchOption.AllDirectories).ToList();

            foreach (var nfo in allMovieNfos)
            {
                try
                {
                    var doc = new XmlDocument();
                    doc.Load(nfo);
                    var title = doc.GetElementsByTagName("title")[0]?.InnerText;

                    var currDir = Path.GetDirectoryName(nfo);
                    var movies = Directory.GetFiles(currDir, "*.*", System.IO.SearchOption.AllDirectories)
                            .Where(f => MovieExtensions.Any(f.ToLower().EndsWith)).ToList();
                    if (movies.Count == 1)
                    {
                        var movie = movies.FirstOrDefault();
                        var ext = Path.GetExtension(movie);
                        FileSystem.RenameFile(movie, $"{title}{ext}");
                    }
                    else if(movies.Count > 1)
                    {
                        for (int i = 0; i < movies.Count; i++)
                        {
                            var ext = Path.GetExtension(movies[i]);
                            FileSystem.RenameFile(movies[i], $"{title}-CD{i+1}{ext}");
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"Error when renaming {Path.GetFileNameWithoutExtension(nfo)} \n\r");
                    Log.Error(ex.ToString());
                }

            }
        }

        private static void ConfigureLogging()
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File($"logs/logs-{DateTime.Now.ToString("yyyyMMddHHmmss")}.txt")
                .CreateLogger();
        }
    }
}
