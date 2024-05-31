using AvdanyuScraper.ClassLibrary;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;

namespace AvdanyuScraper.Services
{
    public class NfoService
    {
        public void UpdaeteNfoData(List<MovieInformation> movieInformation, string nfo)
        {
            var doc = new XmlDocument();
            try
            {
                MovieInformation movieInfo = movieInformation.Where(x => nfo.Contains(x.Code))?.FirstOrDefault();
                if (movieInfo != null)
                {
                    Log.Debug($"Thread {Thread.CurrentThread.ManagedThreadId}: 开始添加 {movieInfo.Code} {movieInfo.Title} 元数据...");
                    doc.Load(nfo);
                    XmlElement parent = (XmlElement)doc.DocumentElement.SelectSingleNode("/movie");
                    foreach (var danyu in movieInfo.Danyus)
                    {
                        XmlElement danyuElement = doc.CreateElement("actor");
                        danyuElement.InnerXml = $"<name>{danyu}</name><type>Actor</type>";
                        parent.AppendChild(danyuElement);
                    }
                    doc.Save(nfo);
                    Log.Debug($"Thread {Thread.CurrentThread.ManagedThreadId}: {movieInfo.Code} {movieInfo.Title} 添加完毕！");
                }
                else
                {
                    Log.Warning($"Thread {Thread.CurrentThread.ManagedThreadId}: 未能找到与 {nfo} 匹配的元数据");
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Thread {Thread.CurrentThread.ManagedThreadId}: 更新 {nfo} 时发生错误，错误信息: \n {ex.Message}");
            }
        }

        public void UpdateNfoDataInFolder(List<MovieInformation> movieInformation, string path)
        {
            var doc = new XmlDocument();
            var allNfos = Directory.GetFiles(path, "*.nfo");
            foreach (var filename in allNfos)
            {
                try
                {
                    MovieInformation movieInfo = movieInformation.Where(x => filename.Contains(x.Code))?.FirstOrDefault();
                    if (movieInfo != null)
                    {
                        Log.Debug($"Thread {Thread.CurrentThread.ManagedThreadId}: 开始添加 {movieInfo.Code} {movieInfo.Title} 元数据...");
                        doc.Load(filename);
                        XmlElement parent = (XmlElement)doc.DocumentElement.SelectSingleNode("/movie");
                        foreach (var danyu in movieInfo.Danyus)
                        {
                            XmlElement danyuElement = doc.CreateElement("actor");
                            danyuElement.InnerXml = $"<name>{danyu}</name><type>Actor</type>";
                            parent.AppendChild(danyuElement);
                        }
                        doc.Save(filename);
                        Log.Debug($"Thread {Thread.CurrentThread.ManagedThreadId}: {movieInfo.Code} {movieInfo.Title} 添加完毕！");
                    }
                    else
                    {
                        Log.Warning($"Thread {Thread.CurrentThread.ManagedThreadId}: 未能找到与 {filename} 匹配的元数据");
                    }
                }
                catch (Exception ex)
                {
                    Log.Error($"Thread {Thread.CurrentThread.ManagedThreadId}: 更新 {filename} 时发生错误，错误信息: \n {ex.Message}");
                }

            }
        }
    }
}
