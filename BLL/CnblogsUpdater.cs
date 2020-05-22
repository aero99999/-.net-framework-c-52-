using HtmlAgilityPack;
using System;
using Model;
using DAL;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace 爬虫
{
    /// <summary>
    /// 博客园
    /// </summary>
    public class CnblogsUpdater
    {
        Baseinfo baseinfo = new Baseinfo();

        public int CnRun()
        {
            var resultNumber = 0;
            PrintMenu();//显示博客园的菜单
            string menuNumber = Console.ReadLine();//捕获用户输入的选项
            switch (menuNumber)
            {
                case "1":
                    baseinfo.Name = "博客园-最新新闻";
                    baseinfo.Url = "https://news.cnblogs.com/n/page/";
                    resultNumber = BaseRun(menuNumber);
                    break;
                case "2":
                    baseinfo.Name = "博客园-48小时阅读排行";
                    resultNumber = BaseRun(menuNumber);
                    break;
                case "3":
                    baseinfo.Name = "博客园-10天推荐排行";
                    resultNumber = BaseRun(menuNumber);
                    break;
                case "4":
                    baseinfo.Name = "博客园-候选页面";
                    resultNumber = BaseRun(menuNumber);
                    break;
                default:
                    baseinfo.Name = "博客园";
                    Console.WriteLine("输入错误,已退出博客园");
                    break;
            }
            return resultNumber;
        }
        public string Get(string url)
        {
            using (var client = GetHttpClient())
            {
                var request = new HttpRequestMessage
                {
                    RequestUri = new Uri(url),
                    Method = HttpMethod.Get,
                };

                var sendTask = client.SendAsync(request);
                sendTask.Wait();

                var response = sendTask.Result.EnsureSuccessStatusCode();
                var responseContent = response.Content.ReadAsStringAsync().Result;

                return responseContent;
            }
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

        public int BaseRun(string menuNumber = "")
        {
            int returnNumber = 0;
            int pageIndex = 1;

            var isContinue = true;//判断用户是否想继续
            while (isContinue && !baseinfo.IsLastPage)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("正在解析【" + baseinfo.Name + "】第" + pageIndex + "页, 请等待……");
                Console.WriteLine("----------------------------------------------------------------------------------------------------------------------");
                UpdateData(pageIndex, menuNumber);//获取数据的核心代码
                Console.ForegroundColor = ConsoleColor.White;

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
                    Console.WriteLine("第" + pageIndex  + "页解析完毕,按Enter键继续解析，按其它键返回主菜单");
                    var input = Console.ReadKey();
                    if (input.Key != ConsoleKey.Enter)
                    {
                        isContinue = false;
                        returnNumber = 1;
                        
                    }
                }
                pageIndex = pageIndex + 1;

            }
            return returnNumber;
        }


        public void UpdateData(int pageIndex, string menuNumber = "")
        {
            var postDic = new Dictionary<string, string>();
            switch (menuNumber)
            {
                case "1":
                    baseinfo.Url = "https://news.cnblogs.com/n/page/" + pageIndex;
                    UpdateNewsData();
                    break;
                case "2":
                case "3":
                case "4"://抓取候选页面
                    baseinfo.Url = "https://www.cnblogs.com/AggSite/AggSitePostList";
                    UpdateCnblogsData(pageIndex,Convert.ToInt32(menuNumber));

                    /*如果都是字符串可以用下面的方法
                    postDic.Add("CategoryType", "HomeCandidate");
                    postDic.Add("ParentCategoryId", "0");
                    postDic.Add("CategoryId", "108697");
                    postDic.Add("PageIndex", pageIndex.ToString());
                    postDic.Add("TotalPostCount", "4000");
                    postDic.Add("ItemListActionName", "AggSitePostList");
                    UpdateCnblogsData(postDic);
                    */
                    break;
            }
        }

        private void PrintMenu()
        {
            Console.Clear();
            Console.WriteLine("");
            Console.WriteLine("                                                           ╔=======博客园=======╗");
            Console.WriteLine("                                                           ║                    ║");
            Console.WriteLine("                                                           ║   1.最新新闻       ║");
            Console.WriteLine("                                                           ║   2.48小时阅读排行 ║");
            Console.WriteLine("                                                           ║   3.10天推荐排行   ║");
            Console.WriteLine("                                                           ║   4.DotNet新手区   ║");
            Console.WriteLine("                                                           ║ 请输入对应编号选择 ║");
            Console.WriteLine("                                                           ║                    ║");
            Console.WriteLine("                                                           ╚====================╝");
        }
        /// <summary>
        /// 获取字符串中的数字
        /// </summary>
        public static int GetNum(string str)
        {
            int result = 0;
            if (str != null && str != string.Empty)
            {
                // 正则表达式剔除非数字字符（不包含小数点.）
                str = Regex.Replace(str, @"[^\d.\d]", "");
                // 如果是数字，则转换为decimal类型
                if (Regex.IsMatch(str, @"^[+-]?\d*[.]?\d*$"))
                {
                    result = int.Parse(str);
                }
            }
            return result;
        }
        //博客园--最新新闻
        private void UpdateNewsData()
        {
            var result = Get(baseinfo.Url);
            if (!string.IsNullOrEmpty(result))
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(result);
                //打印一下网页源代码
                //Console.WriteLine(doc.Text);
                var node = doc.DocumentNode.SelectSingleNode("//div[@class='news_block']");
                if (node.InnerText == null)
                {
                    return;
                }
                var nodes = doc.DocumentNode.SelectNodes("//div[@class='news_block']");
                //Console.WriteLine(doc.Text);
                foreach (var item in nodes)
                {
                    try
                    {
                        //获取这篇文章的推荐数，如果大于等于10，就加红显示
                        var hotNumber = item.SelectSingleNode(".//div[@class='action']/div[@class='diggit']/span").InnerText;
                        if (Convert.ToInt32(hotNumber) >= 10)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                        hotNumber = hotNumber.PadRight(4);
                        var date = item.SelectSingleNode(".//div[@class='entry_footer']/span[@class='gray']").InnerText;
                        var title = item.SelectSingleNode(".//div[@class='content']/h2/a").InnerText;
                        var content = item.SelectSingleNode(".//div[@class='entry_summary']").InnerText;
                        var url = item.SelectSingleNode(".//div[@class='content']/h2/a").GetAttributeValue("href", "未获取链接");
                        Console.WriteLine(date + " | 推荐：" + hotNumber + "| https://news.cnblogs.com" + url + " | " + title);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("解析出错：" + ex);
                    }
                }
            }
        }

        //博客园--知识库get方法buxing
        private void UpdateKnowledge()
        {
            var result = Get(baseinfo.Url);
            if (!string.IsNullOrEmpty(result))
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(result);
                //打印一下网页源代码
                //Console.WriteLine(doc.Text);
                var node = doc.DocumentNode.SelectSingleNode("//div[@class='list_block']");
                if (node == null)
                {
                    return;
                }
                var nodes = doc.DocumentNode.SelectNodes("//div[@class='news_block']");
                //Console.WriteLine(doc.Text);
                foreach (var item in nodes)
                {
                    try
                    {
                        //获取这篇文章的推荐数，如果大于等于10，就加红显示
                        var hotNumber = item.SelectSingleNode(".//div[@class='msg_tag']/span[@class='recommend']").InnerText;
                        if (Convert.ToInt32(hotNumber) >= 10)
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                        }
                        else
                        {
                            Console.ForegroundColor = ConsoleColor.White;
                        }
                        hotNumber = hotNumber.PadRight(4);
                        var date = item.SelectSingleNode(".//div[@class='entry_footer']/span[@class='gray']").InnerText;
                        var title = item.SelectSingleNode(".//div[@class='content']/h2/a").InnerText;
                        var content = item.SelectSingleNode(".//div[@class='entry_summary']").InnerText;
                        var url = item.SelectSingleNode(".//div[@class='content']/h2/a").GetAttributeValue("href", "未获取链接");
                        Console.WriteLine(date + " | 推荐：" + hotNumber + "| https://news.cnblogs.com" + url + " | " + title);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("解析出错：" + ex);
                    }
                }
            }
        }

        //博客园--候选
        private void UpdateCnblogsData(int pageIndex, int sel)
        {
            var cnjson = new CnJson();
            int rednum = 0;//阅读数达到多少加红显示
            if (sel == 2)//阅读数大于等于10就加红显示
            {
                rednum = 1200;
                cnjson.CategoryType = "TopViews";
                cnjson.ParentCategoryId = 0;
                cnjson.CategoryId = 0;
                cnjson.PageIndex = pageIndex;
                cnjson.TotalPostCount = 0;
                cnjson.ItemListActionName = "AggSitePostList";
            }
            else if(sel == 3)//阅读数大于等于10就加红显示
            {
                rednum = 1200;
                cnjson.CategoryType = "TopDiggs";
                cnjson.ParentCategoryId = 0;
                cnjson.CategoryId = 0;
                cnjson.PageIndex = pageIndex;
                cnjson.TotalPostCount = 0;
                cnjson.ItemListActionName = "AggSitePostList";
            }
            else if (sel == 4)//阅读数大于等于10就加红显示
            {
                rednum = 10;
                cnjson.CategoryType = "HomeCandidate";
                cnjson.ParentCategoryId = 0;
                cnjson.CategoryId = 108697;
                cnjson.PageIndex = pageIndex;
                cnjson.TotalPostCount = 4000;
                cnjson.ItemListActionName = "AggSitePostList";
            }

            var result = Post.PostCn(baseinfo.Url, cnjson);
            if (!string.IsNullOrEmpty(result) && !string.IsNullOrWhiteSpace(result))
            {
                var doc = new HtmlDocument();
                doc.LoadHtml(result);
                //Console.WriteLine(result);
                var nodes = doc.DocumentNode.SelectNodes("//div[@class='post_item_body']");
                if (nodes == null)
                {
                    Console.WriteLine("没获取到数据，对不起");
                    baseinfo.IsLastPage = true;
                    return;
                }
                Console.WriteLine("本页文章数：" + nodes.Count);
                foreach (var item in nodes)
                {
                    try
                    {
                        var hotNumber = item.SelectSingleNode(".//div[@class='post_item_foot']/span[@class='article_view']/a[@class='gray']").InnerText;
                        int hotnum = GetNum(hotNumber);
                        Console.ForegroundColor = ConsoleColor.White;
                        if (Convert.ToInt32(hotnum) >= rednum)//阅读数大于等于 多少 就加红显示
                        {
                            Console.ForegroundColor = ConsoleColor.Red;
                        }
                        hotNumber = hotnum.ToString().PadRight(4);
                        var title = item.SelectSingleNode(".//h3/a").InnerText;
                        var other = item.SelectSingleNode(".//div[@class='post_item_foot']/a").InnerText;
                        var number = 18 - GetChineseCharCount(other);

                        if (!(number <= 0))
                        {
                            other = other.PadRight(number);
                        }
                        var createdTime = item.SelectSingleNode(".//div[@class='post_item_foot']/a").NextSibling.InnerText.Replace("\r\n", "").Replace("发布于", "").Replace(" ", "");
                        Console.WriteLine(createdTime + " | 推荐：" + hotNumber + "| " + other + " | " + title);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("解析出错：" + ex);
                    }
                }
                if (nodes.Count < 20)
                {
                    Console.WriteLine("这是最后一页啦");
                    baseinfo.IsLastPage = true;
                }
            }
            else
            {
                Console.WriteLine("未获取数据,可能是最后一页了");
                baseinfo.IsLastPage = true;
            }
        }

    }
}
