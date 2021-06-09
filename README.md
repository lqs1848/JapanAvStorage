# JapanAvStorage
大姐姐的收纳整理术

让大姐姐变得井然有序<br>

AvCoverDownloader<br>自动识别目录下的番号 下载大姐姐的靓照 和 本片所有出场的大姐姐姓名<br>

自动按 骑兵/步兵 分到当前目录下 并整理番号信息到nfo文件(视频软件用于识别影片信息的文件 比如:plex,emby,jellyfin等)<br>

自动在目录下创建  参演女优名称.txt 方便使用 **everything** 检索 (例: 081311-777 あいりみく.star.txt)<br>



示例图为 使用 [**Jellyfin**](https://github.com/jellyfin/jellyfin) 读取整理过后的目录<br>

![image](https://github.com/lqs1848/JapanAvStorage/blob/master/info/1.jpg)<br>
![image](https://github.com/lqs1848/JapanAvStorage/blob/master/info/2.jpg)<br>
![image](https://github.com/lqs1848/JapanAvStorage/blob/master/info/3.png)<br>

![image](https://github.com/lqs1848/JapanAvStorage/blob/master/info/3.jpg)<br>

![image](https://github.com/lqs1848/JapanAvStorage/blob/master/info/4.jpg)<br>

![image](https://github.com/lqs1848/JapanAvStorage/blob/master/info/5.jpg)<br>







![image](https://github.com/lqs1848/JapanAvStorage/blob/master/info/111.jpg)<br>

![image](https://github.com/lqs1848/JapanAvStorage/blob/master/info/222.jpg)<br>

地址过期了自己替换 address.ini 中的地址就行<br>

如果不挂代理 proxy=127.0.0.1:1080 删掉就好<br>











**其他**

AvCoverDownloader2只是换了一个网站抓取 没有2就是 抓去javbus f2cppv 识别 f2c 的

<br>
AvMatchCheck 自动迭代查找目录下所有的子目录的 所有视频文件 识别视频文件名称中的番号 并单独为这个视频生成一个文件夹
<br>
AvMosaicSpot 自动识别目录下的AV 番号 查询是有码还是无码的大姐姐 自动分类<br>
<br>
NfoDownload 为视频文件生成 .nfo文件 可以为jellyfin添加 发行日期 TAG 导演 女优信息 还有厂商
<br>
StartDownload 女优头像采集    请指定到Jellyfin的数据存储目录 下的 /metadata/People

<br>

<br>

<br>