using System.IO;
using System.Net;

namespace get_commits
{
    public class cWGET
    {
        protected CookieContainer Cookies;
        
        public cWGET(string cookie_name, string cookies, string path, string domain)
        {
            Cookies = new CookieContainer();
            Cookies.Add( new Cookie(cookie_name, cookies, path, domain) );
        }

        public string GetUrl(string url)
        {
            HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
            req.CookieContainer = Cookies;           
            HttpWebResponse resp = (HttpWebResponse)req.GetResponse();

            try
            {
                StreamReader stream = new StreamReader(resp.GetResponseStream());
                string Text = stream.ReadToEnd();
                stream.Close();
                return Text;
            }
            catch
            {
                return "Cannot get url "+url;
            }

        }
    }



}