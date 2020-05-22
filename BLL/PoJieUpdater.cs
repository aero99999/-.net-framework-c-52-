using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using HtmlAgilityPack;
using Model;


namespace 爬虫
{
    public class PoJieUpdater 
    {

        Baseinfo baseinfo=new Baseinfo();
        public  int BaseRun(string menuNumber = "")
        {
            int returnNumber = 0;
            int pageIndex = 1;

            var isContinue = true;
            while (isContinue && !baseinfo.IsLastPage)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("正在解析【" + baseinfo.Name + "】第" + pageIndex + "页, 请等待……");
                Console.WriteLine("----------------------------------------------------------------------------------------------------------------------");
                pageIndex = UpdateData(pageIndex, menuNumber);
                Console.WriteLine("----------------------------------------------------------------------------------------------------------------------");
                if (baseinfo.IsLastPage)
                {
                    Console.WriteLine("已经是最后一页,按任意键返回菜单");
                    var input = Console.ReadKey();
                    if (input.Key == ConsoleKey.Enter)
                    {
                        isContinue = false;
                        returnNumber = 1;
                    }
                }
                else
                {
                    Console.WriteLine("第" + (pageIndex - 1) + "页解析完毕,按Enter键继续解析，按其它键返回主菜单");
                    var input = Console.ReadKey();
                    if (input.Key != ConsoleKey.Enter)
                    {
                        isContinue = false;
                        returnNumber = 1;
                    }
                }
            }
            return returnNumber;
        }

        /// <summary>
        /// 吾爱破解
        /// </summary>
        public string PoJieGet(string url)
        {
            string result = string.Empty;
            HttpWebRequest wbRequest = (HttpWebRequest)WebRequest.Create(url);
            wbRequest.Method = "GET";
            wbRequest.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/65.0.3325.146 Safari/537.36";
            HttpWebResponse wbResponse = (HttpWebResponse)wbRequest.GetResponse();
            using (var reader = new StreamReader(wbResponse.GetResponseStream(), Encoding.GetEncoding("GBK")))
            {
                result = reader.ReadToEnd();
            }
            return result;
        }

        /// <summary>
        /// 返回字符串中的中文字符个数
        /// </summary>
        public int GetChineseCharCount(string s)
        {
            int count = 0;
            foreach (char c in s)
            {
                if (Convert.ToInt32(c) > 255)
                {
                    count++;
                }
            }
            return count;
        }

        private static HttpClient GetHttpClient(int? timeOutSeconds = null)
        {
            var client = new HttpClient();
            if (!timeOutSeconds.HasValue)
            {
                timeOutSeconds = 3;
            }

            client.Timeout = new TimeSpan(0, 0, timeOutSeconds.Value);
            return client;
        }
        public  int Run(string menuNumber = "")
        {
            var resultNumber = 0;
            PrintMenu();
            menuNumber = Console.ReadLine();
            switch (menuNumber)
            {
                case "1":
                    baseinfo.Name = "吾爱破解-人气热门";
                    resultNumber = BaseRun(menuNumber);
                    break;
                case "2":
                    baseinfo.Name = "吾爱破解-新鲜出炉";
                    resultNumber = BaseRun(menuNumber);
                    break;
                case "3":
                    baseinfo.Name = "吾爱破解-技术分享";
                    resultNumber = BaseRun(menuNumber);
                    
                    break;
                case "4":
                    baseinfo.Name = "吾爱破解-精华采撷";
                    BaseRun(menuNumber); 
                    resultNumber = 1;                   
                    break;
                default:
                    baseinfo.Name = "吾爱破解";
                    Console.WriteLine("输入错误,已退出吾爱破解");
                    break;
            }
            return 0;
        }

        public int UpdateData(int pageIndex, string menuNumber = "")
        {
            var postDic = new Dictionary<string, string>();
            postDic.Add("ItemListActionName", "PostList");
            postDic.Add("PageIndex", pageIndex.ToString());
            switch (menuNumber)
            {
                case "1":
                    baseinfo.Url = "https://www.52pojie.cn/forum.php?mod=guide&view=hot&page=" + pageIndex;
                    UpdatePoJieData();
                    break;
                case "2":
                    baseinfo.Url = "https://www.52pojie.cn/forum.php?mod=guide&view=newthread&page=" + pageIndex;
                    UpdatePoJieData();
                    break;
                case "3":
                    baseinfo.Url = "https://www.52pojie.cn/forum.php?mod=guide&view=tech&page=" + pageIndex;
                    UpdatePoJieData();
                    break;
                case "4":
                    baseinfo.Url = "https://www.52pojie.cn/forum.php?mod=guide&view=digest";
                    UpdatePoJieData();
                    break;
            }
            return pageIndex + 1;
        }

        private void UpdatePoJieData()
        {
            var result = PoJieGet(baseinfo.Url);
            if (!string.IsNullOrEmpty(result))
            {
                if (result.IndexOf("暂时还没有帖子") > -1)
                {
                    Console.WriteLine("已经是最后一页了");
                    baseinfo.IsLastPage = true;
                }
                else
                {
                    //Console.WriteLine(result);
                    var doc = new HtmlDocument();
                    doc.LoadHtml(result);
                    HtmlNode node = doc.GetElementbyId("threadlist");
                    var nodes = node.SelectNodes("//table/tbody");
                    Console.WriteLine(baseinfo.Url);
                    Console.WriteLine(node.InnerText);
                    if (nodes.Count()==0)
                    {

                        Console.WriteLine("未获取数据,可能已经是最后一页了");
                        baseinfo.IsLastPage = true;
                        return;
                    }
                    foreach (var item in nodes)
                    {
                        try
                        {
                            var dateTimeStr = item.SelectSingleNode(".//td[@class='by']/em/span").InnerText;
                            var dateTimeArr = dateTimeStr.Split(' ');
                            var dateArr = dateTimeArr[0].Split('-');
                            var month = dateArr[1].Length == 1 ? (0 + dateArr[1]) : dateArr[1];
                            var day = dateArr[2].Length == 1 ? (0 + dateArr[2]) : dateArr[2];
                            dateTimeStr = dateArr[0] + "-" + month + "-" + day + " " + dateTimeArr[1];
                            var a = item.SelectSingleNode(".//th[@class='common']/a");
                            var title = a.InnerText;
                            var url = a.GetAttributeValue("href", "未获取链接");
                            Console.WriteLine(dateTimeStr + " | https://www.52pojie.cn/" + url + " | " + title);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("解析出错：" + ex);
                        }
                    }
                }
            }
            else
            {
                Console.WriteLine("未获取数据,可能已经是最后一页了");
                baseinfo.IsLastPage = true;
            }
        }

        private void PrintMenu()
        {
            Console.Clear();
            Console.WriteLine("");
            Console.WriteLine("                                                           ╔======吾爱破解======╗");
            Console.WriteLine("                                                           ║                    ║");
            Console.WriteLine("                                                           ║     1.人气热门     ║");
            Console.WriteLine("                                                           ║     2.新鲜出炉     ║");
            Console.WriteLine("                                                           ║     3.技术分享     ║");
            Console.WriteLine("                                                           ║     4.精华采撷     ║");
            Console.WriteLine("                                                           ║ 请输入对应编号选择 ║");
            Console.WriteLine("                                                           ║                    ║");
            Console.WriteLine("                                                           ╚====================╝");
        }
    }
}
