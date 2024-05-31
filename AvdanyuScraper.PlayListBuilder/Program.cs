using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace AvdanyuScraper.PlayListBuilder
{
    public class Program
    {
        [STAThread]
        public static void Main(string[] args)
        {
            ConfigureLogging();

            var movieSearchDir = new FolderBrowserDialog();
            movieSearchDir.Description = "请设置影片库路径";
            movieSearchDir.UseDescriptionForTitle = true;
            movieSearchDir.SelectedPath = @"E:\MyFile\New\有码\演员\";

            var playListDir = new FolderBrowserDialog();
            playListDir.Description = "请设置playlist路径";
            movieSearchDir.UseDescriptionForTitle = true;
            playListDir.SelectedPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.UserProfile)}\\AppData\\Roaming\\PotPlayerMini64\\Playlist\\";

            while (movieSearchDir.ShowDialog() == DialogResult.OK &&  playListDir.ShowDialog() == DialogResult.OK)
            {
                Console.Write("请输入演员名，多名演员请用逗号分隔： ");
                var inputActorNames = Console.ReadLine();
                if (!String.IsNullOrEmpty(inputActorNames))
                {                  
                    CreatePlayList(movieSearchDir, playListDir, inputActorNames);
                }
                else
                {
                    Log.Error("没有输入演员名，请输入演员名！");
                }
            }
            Console.WriteLine("按任意键退出...");
            Console.ReadKey();
        }

        private static void CreatePlayList(FolderBrowserDialog movieSearchDir, FolderBrowserDialog playListDir, string actors)
        {
            var actorsList = actors.Split(",").ToList();
            var output = new List<string>();
            var folders = Directory.GetDirectories(movieSearchDir.SelectedPath);
            var outputPath = playListDir.SelectedPath;
            foreach (var folder in folders)
            {
                var allNfos = Directory.GetFiles(folder, "*.nfo");
                var allMovies = Directory.GetFiles(folder, "*.*").ToList().Where(x => new FileInfo(x).Length > 100000000).ToList();

                foreach (var nfo in allNfos)
                {
                    try
                    {
                        var doc = new XmlDocument();
                        doc.Load(nfo);
                        var title = nfo.Substring(0, nfo.Length - 4);
                        var nfoActors = doc.DocumentElement.SelectNodes("/movie/actor/name");
                        var currentActors = new List<string>();
                        foreach (XmlElement nfoActor in nfoActors)
                        {
                            currentActors.Add(nfoActor.InnerText.Trim());
                        }
                        var isContainsAll = true;
                        foreach (var actor in actorsList)
                        {
                            if (!currentActors.Contains(actor.Trim()))
                            {
                                isContainsAll = false;
                            }
                        }
                        if (isContainsAll)
                        {
                            var foundMovies = allMovies.Where(x => x.Contains(title)).ToList();
                            foreach (var foundMovie in foundMovies)
                            {
                                output.Add(foundMovie);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error($"处理 {nfo} 发生错误！ 错误信息： {ex.Message} \n\r");
                    }

                }

                if (output.Count > 0)
                {

                    FileStream fs = new FileStream($"{outputPath}\\{actors.Replace(',', ' ')}.dpl", FileMode.Create);
                    using (StreamWriter writer = new StreamWriter(fs))
                    {
                        var defaultInput = "DAUMPLAYLIST\nplaytime=0\ntopindex=0\nfoldertype=2\nsaveplaypos=0\n";
                        writer.Write(defaultInput);
                        for (int i = 0; i < output.Count; ++i)
                        {
                            writer.WriteLine($"{i + 1}*file*{output[i]}");
                        }
                    }
                    fs.Close();
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
