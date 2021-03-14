using System;
using System.Collections.Generic;

namespace get_commits
{
    class Program
    {
        static void Main(string[] args)
        {
            DateTime dt_start = DateTime.Now;
            

            string cookie_name="_identity";
            string cookie="4cac4cad483ac4a3d3b4242c11c988963ba5791eee421a614f09db65ad0bb88ca%3A2%3A%7Bi%3A0%3Bs%3A9%3A%22_identity%22%3Bi%3A1%3Bs%3A16%3A%22%5B105%2C%22%22%2C2592000%5D%22%3B%7D";
            string path="/";
            string domain="admin.ddos-guard.net";
            string url = "https://admin.ddos-guard.net/snmp-collector/index";


            cUpLinks UpLinks = new cUpLinks(cookie_name,cookie,path,domain,url);
            UpLinks.GetNamesTimesAndURLs();
            UpLinks.GetCommits();
            UpLinks.GetPercentiles();

            if (args.Length!=0)
            {
                if (args[0]=="-a")
                {
                    Console.WriteLine("\nВсе значения из админки:");
                    UpLinks.Print();
                    Console.WriteLine();
                }
    /*
                Console.WriteLine("\n\n* * * * * * * * * *\nНенулевой оверкоммит:");
                UpLinks.PrintNotNull();
    */
            }
            else
            {
                List<string> Exceptions = new List<string>();

                Exceptions.Add("TELIA (LA, NY, AMS, SPB, MSK)");
                Exceptions.Add("GOOGLE (MSK, SPB)");
                Exceptions.Add("RT_MSK");
                Exceptions.Add("NTT_AMS");
                Exceptions.Add("TELIA_AMS");
                Exceptions.Add("MAIL");
                Exceptions.Add("RT_SPB");
                Exceptions.Add("INETCOM_MSK");
                Exceptions.Add("CITYTELECOM");
                Exceptions.Add("Google_SPB");
                Exceptions.Add("EURASIA-IX");
                Exceptions.Add("NTT_LA");
                Exceptions.Add("TELIA_MSK");
                Exceptions.Add("TELIA_LA");
                Exceptions.Add("TELIA_SPB");
                Exceptions.Add("GLBIX_SPB");
                    
                Console.WriteLine("\nСписок аплинков, превысивших 70% от допустимых значений");
                UpLinks.PrintExceptOnlyDanger(Exceptions);
            }
            
            double tmp=0;
            tmp+=UpLinks.GetOvecommitByName("TELIA_MSK");
            tmp+=UpLinks.GetOvecommitByName("TELIA_SPB");
            tmp+=UpLinks.GetOvecommitByName("TELIA_NY");
            tmp+=UpLinks.GetOvecommitByName("TELIA_LA");
            tmp+=UpLinks.GetOvecommitByName("TELIA_AMS");
            tmp=Math.Round(tmp,2);
            
            
            Console.WriteLine("Telia MSK+SPB+LA+NY+AMS:                          "+tmp);
            
            DateTime dt_end = DateTime.Now;
            Console.WriteLine("\nВремя выполнения "+(dt_end-dt_start).Minutes+" мин "+(dt_end-dt_start).Seconds+" с.");
        }
    }
}
