using System.Net;
using System.IO;
using System.Collections.Generic;
using System;

namespace coedcherry.com
{
    public class cSite
    {
        public List<string> AllRefs;
        public string url;

        public cSite(string html_page)
        {
            AllRefs = new List<string>();
            url = html_page;

        }
        public void GetHRefs()
        {
            StreamReader sr = new StreamReader(url);
            string txt = sr.ReadToEnd();
            string[] all_lines = txt.Split(new[] { '\r', '\n' });

            foreach (string s in all_lines)
                if (s.Contains("ng-href"))
                {
                    try
                    {
                        string h = "https://www.coedcherry.com" + s.Split(" ")[2].Split("=")[1].Split("\"")[1];
                        AllRefs.Add(h);
                        //Console.WriteLine(h);
                    }
                    catch
                    {

                    }

                }
        }

        public void DownLoadAllRefsImages()
        {
            foreach (string url in AllRefs)
            {
                try
                {
                    string text = GetHTML(url);
                    GetImegesRefs(text);
                }
                catch
                {
                    Console.WriteLine(url);
                }
            }

        }

        public string GetHTML(string url)
        {
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();

            StreamReader stream = new StreamReader(resp.GetResponseStream());
            string Text = stream.ReadToEnd();

            return Text;
        }

        public string GetImegesRefs(string html_page)
        {
            string[] all_lines = html_page.Split(new[] { '\r', '\n' });
            List<string> ImagesUrls = new List<string>();

            foreach (string s in all_lines)
                if (s.Contains("<a href=\"https://content"))
                {
                    try
                    {
                        string h = s.Split("\"")[1];
                        ImagesUrls.Add(h);
                        Console.Write(h + "   ");
                        //string filename = h.Split("://")[1];
                        //string path=filename.Substring(0,filename.LastIndexOf("/"));

                        string path = h.Split("/")[4];
                        string filename = "./" + path + "/" + h.Split("/")[5];
                        Console.WriteLine(path);
                        DownloadImage(h, path, filename);

                    }
                    catch
                    {
                        Console.WriteLine(html_page);
                    }

                }

            return "";
        }


        public void DownloadImage(string image_url, string path, string filename)
        {
            DirectoryInfo di = Directory.CreateDirectory(path);

            byte[] content;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(image_url);
            WebResponse response = request.GetResponse();

            Stream stream = response.GetResponseStream();

            using (BinaryReader br = new BinaryReader(stream))
            {
                content = br.ReadBytes(500000);
                br.Close();
            }
            response.Close();

            FileStream fs = new FileStream(filename, FileMode.Create);
            BinaryWriter bw = new BinaryWriter(fs);
            try
            {
                bw.Write(content);
            }
            finally
            {
                fs.Close();
                bw.Close();
            }

        }


    }

}