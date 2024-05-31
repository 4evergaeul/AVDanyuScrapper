# AV男优刮削器
## 简介
目前我所了解的jav影片刮削器仅支持女优名刮削，所以这个程序可以从[Avdanyu](http://avdanyuwiki.com/)上刮削男演员名并添加在**现有的**nfo文件里。

如果你还不知道av刮削器，请参考这两个repo。建立管理良好的影片库是优雅撸管的第一步。：）
1. https://github.com/JustMachiavelli/javsdt
2. https://github.com/yoshiko2/AV_Data_Capture

## 开发环境：
.NET Core 3.1

## 项目
1. AvdanyuScraper: 从Avdanyu上刮削男演员名称并存在现有的nfo文件里。
2. AvdanyuScraper.ClassLibrary: 存放数据models
3. AvdanyuScraper.PlayerListBuild: 支持输入演员名来生成potplayer的播放列表，感兴趣的小伙伴可以查看源码了解下如何使用。

## 使用前提
1. 你已经完成了影片刮削。
2. 你的影片文件夹按照**女演员名**建立。

## 使用
1. [这里](https://github.com/aiceom/AVDanyuScrapper/releases/tag/0.1)下载。
2. 请阅读使用条件，确保你的影片文件结构符合条件。
3. 选定演员的根目录，即可开始刮削。