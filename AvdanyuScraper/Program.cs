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
                foreach(var dir in directories)
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
            var allMovies = Directory.GetFiles(rootDirectory, "*.*", System.IO.SearchOption.AllDirectories)
                        .Where(f => MovieExtensions.Any(f.ToLower().EndsWith)).ToList();

            foreach(var movie in allMovies)
            {
                var name = Path.GetFileNameWithoutExtension(movie);
                try
                {
                    var currDir = Path.GetDirectoryName(movie);
                    var nfo = Directory.GetFiles(currDir, $"{name}.nfo").FirstOrDefault();
                    var title = "";
                    if (!string.IsNullOrEmpty(nfo))
                    {
                        var doc = new XmlDocument();
                        doc.Load(nfo);
                        title = doc.GetElementsByTagName("title")[0]?.InnerText;
                        if (!string.IsNullOrEmpty(title))
                        {
                            //var fanart = Directory.GetFiles(currDir, $"{name}-fanart.jpg").FirstOrDefault();
                            //var poster = Directory.GetFiles(currDir, $"{name}-poster.jpg").FirstOrDefault();
                            //var thumb = Directory.GetFiles(currDir, $"{name}-thumb.jpg").FirstOrDefault();
                            var ext = Path.GetExtension(movie);
                            //FileSystem.RenameFile(fanart, $"{title}-fanart.jpg");
                            //FileSystem.RenameFile(poster, $"{title}-poster.jpg");
                            //FileSystem.RenameFile(thumb, $"{title}-thumb.jpg");
                            FileSystem.RenameFile(nfo, $"{title}.nfo");
                            FileSystem.RenameFile(movie, $"{title}{ext}");
                        }
                        else
                        {
                            Log.Warning($"{name} nfo title not found");
                        }
                    }
                    else
                    {
                        Log.Warning($"{name} nfo not found");
                    }
                }
                catch(Exception ex)
                {
                    Log.Error($"Error when renaming {name} \n\r");
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
