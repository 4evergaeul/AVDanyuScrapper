using AvdanyuScraper.ClassLibrary;
using HtmlAgilityPack;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;

namespace AvdanyuScraper.Services
{
    public class MovieInformationService
    {
        private Regex danyuPattern;
        private Regex codeFilterPattern;
        private Regex codePattern;
        private Regex numberPattern;

        public MovieInformationService()
        {
            danyuPattern = new Regex("出演AV男優 ：  (.*?)\\t");
            codeFilterPattern = new Regex("品番：(.*?)(<br>|</p>)");
            codePattern = new Regex("[a-zA-Z]{2,}.+[0-9]");
            numberPattern = new Regex("([1-9]+[0-9]+)|[1-9]");
        }

        public List<MovieInformation> GetMovieInformation(string searchString)
        {
            var movieInformation = new List<MovieInformation>();
            try
            {
                var html = $"https://avdanyuwiki.com/?s={searchString}";

                HtmlWeb web = new HtmlWeb();
                var htmlDoc = web.Load(html);

                var totalPage = 1;
                var paginator = htmlDoc.DocumentNode.Descendants(0).Where(x => x.HasClass("pages")).FirstOrDefault();
                if (paginator != null)
                {
                    totalPage = int.Parse(htmlDoc.DocumentNode.Descendants(0)
                        .Where(x => x.HasClass("pages")).FirstOrDefault()?.InnerText.Split("/")[1]);
                }
                for (int i = 1; i <= totalPage; ++i)
                {
                    if (i > 1)
                    {
                        html = $"https://avdanyuwiki.com/page/{i}/?s={searchString}";
                        htmlDoc = web.Load(html);
                    }
                    var articles = htmlDoc.DocumentNode.SelectNodes("//article");
                    if (articles != null)
                    {
                        for (int j = 0; j < articles.Count(); ++j)
                        {
                            var article = articles[j];
                            movieInformation.Add(ProcessArticleElement(article, j));
                        }
                    }
                    else
                    {
                        Log.Warning($"Thread {Thread.CurrentThread.ManagedThreadId}: 未能找到 {searchString} 的影片！");
                    }
                }
                Log.Debug($"Thread {Thread.CurrentThread.ManagedThreadId}: {searchString} 的元数据已刮削完毕，共找到 {movieInformation.Count()} 影片数据！");
            }
            catch (Exception ex)
            {
                Log.Error($"Thread {Thread.CurrentThread.ManagedThreadId}: 刮取 {searchString} 时发生错误，错误信息: \n {ex.Message}");
            }
            return movieInformation;
        }

        private MovieInformation ProcessArticleElement(HtmlNode article, int index)
        {
            var codeResultRaw = codeFilterPattern.Match(article.InnerHtml)?.ToString();
            if (codeResultRaw.Length != 0)
            {
                codeResultRaw = codeResultRaw.Substring(4, codeResultRaw.Length - 5);
                var codeResult = codePattern.Match(codeResultRaw)?.ToString();
                if (codeResult.Length >= 6)
                {
                    var sb = new StringBuilder();
                    int firstDigitIndex = 0;
                    while (firstDigitIndex < codeResult.Length)
                    {
                        if (Char.IsDigit(codeResult[firstDigitIndex]))
                        {
                            break;
                        }
                        firstDigitIndex++;
                    }
                    sb.Append($"{codeResult.Substring(0, firstDigitIndex)}-");
                    var matchedNumber = numberPattern.Match(codeResult.Substring(firstDigitIndex, codeResult.Length - firstDigitIndex)).ToString();
                    var number = "";
                    if (matchedNumber.Length == 1)
                    {
                        number = $"00{matchedNumber}";
                    }
                    else if (matchedNumber.Length == 2)
                    {
                        number = $"0{matchedNumber}";
                    }
                    else
                    {
                        number = matchedNumber;
                    }
                    sb.Append(number);

                    var title = sb.ToString().ToUpper();

                    var danyuResult = danyuPattern.Match(article.InnerText)?.ToString();
                    if (!string.IsNullOrEmpty(danyuResult))
                    {
                        danyuResult = danyuResult.Substring(10, danyuResult.Length - 11);
                    }
                    else
                    {
                        Log.Warning($"Thread {Thread.CurrentThread.ManagedThreadId}: {title} 找不到对应的男演员！");
                    }
                    var danyus = new List<string>();
                    if (!string.IsNullOrEmpty(danyuResult))
                    {
                        danyus = danyuResult.Split(",").ToList();
                    }
                    return new MovieInformation()
                    {
                        Code = title,
                        Title = article.SelectNodes("//h2")[index].InnerText,
                        Danyus = danyus
                    };
                }
                else
                {
                    Log.Warning($"Thread {Thread.CurrentThread.ManagedThreadId}: {codeResultRaw} 的影片代码无法被处理！详细信息如下: ");
                    Log.Warning(article.InnerHtml);
                }
            }
            else
            {
                Log.Warning($"Thread {Thread.CurrentThread.ManagedThreadId}: 未能找到该影片代码！详细信息如下: ");
                Log.Warning(article.InnerHtml);
            }
            return new MovieInformation()
            {
                Code = "INVALID CODE"
            };
        }
    }
}
