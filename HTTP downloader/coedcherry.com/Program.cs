using System;
using System.IO;

namespace coedcherry.com
{
    class Program
    {
        static void Main(string[] args)
        {
            string filename = Directory.GetFiles(".","*.html")[0];
            cSite site = new cSite(filename);
            site.GetHRefs();
            site.DownLoadAllRefsImages();

        }
    }
}
