using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;

namespace DAL
{
    public class Post
    {
        public static string PostCn(string url, CnJson postDataDic)
        {
            string postDataStr = JsonConvert.SerializeObject(postDataDic);
            //Console.WriteLine(postDataStr);
            byte[] postData = Encoding.UTF8.GetBytes(postDataStr);
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/json";
            request.ContentLength = postData.Length;
            using (var requestStream = request.GetRequestStream())
            {
                requestStream.Write(postData, 0, postData.Length);
                requestStream.Flush();
            }
            var response = (HttpWebResponse)request.GetResponse();
            string responseText = null;
            using (var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
            {
                responseText = reader.ReadToEnd();
            }
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return responseText;
            }
            return "";
        }
        public static string HttpClientDoPost(string url, Dictionary<string, string>  postDataDic)
        {
            string postDataStr = JsonConvert.SerializeObject(postDataDic);
            //Console.WriteLine(postDataStr);
            byte[] postData = Encoding.UTF8.GetBytes(postDataStr);
            
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.Timeout = 800;
            request.ContentType = "application/json;charset=UTF-8";
            request.ContentLength = postData.Length;
            //Console.WriteLine(postData.Length);
            using (var requestStream = request.GetRequestStream())
            {
                requestStream.Write(postData, 0, postData.Length);
                requestStream.Flush();
            }
            var response = (HttpWebResponse)request.GetResponse();
            string responseText = null;
            using (var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
            {
                responseText = reader.ReadToEnd();
            }
            if (response.StatusCode == HttpStatusCode.OK)
            {
                return responseText;
            }
            return "";
        }
    }

    public class CnJson//格式化json
    {
        public string CategoryType { get; set; }
        public int ParentCategoryId { get; set; }
        public int CategoryId { get; set; }
        public int PageIndex { get; set; }
        public int TotalPostCount { get; set; }
        public string ItemListActionName { get; set; }
    }
    /*
        postDic.Add("CategoryId", "108697");
        postDic.Add("CategoryType", "HomeCandidate");
        postDic.Add("ParentCategoryId", "0");
        postDic.Add("PageIndex", pageIndex.ToString());
        postDic.Add("TotalPostCount", "4000");
        postDic.Add("ItemListActionName", "AggSitePostList");
     */
}
