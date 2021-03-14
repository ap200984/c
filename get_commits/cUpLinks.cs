using System;
using System.Collections.Generic;

namespace get_commits
{ 
    public class cUpLink
    {
        public string Name;
        public string Time;
        public double Commit;
        public double Percentile;
        
        public bool Disabled;

        public string PercentileLink;
        public string CommitLink;

        protected string cookie_name;
        protected string cookies;
        protected string path;
        protected string domain;     

        protected string RawHTMLPercentile;
        protected string RawHTMLCommit;
        
        public cUpLink(string _cookie_name, string _cookies, string _path, string _domain)
        {
            cookie_name = _cookie_name;
            cookies = _cookies;
            path = _path;
            domain = _domain;
        }

        public void Print()
        {
            cSTR str = new cSTR();
            Console.WriteLine(Name+str.spaces(30,Name)+Time+str.spaces(20,Time)+Percentile+str.spaces(10,Percentile.ToString())+Commit);
        }

        public void GetCommitByHRef()
        {
            cWGET site = new cWGET(cookie_name,cookies,path,domain);
            RawHTMLCommit = site.GetUrl("https://admin.ddos-guard.net/"+CommitLink);

            string[] lines = RawHTMLCommit.Split(new char[] {'\r','\n'});
            foreach (string line in lines)
            {
                if (line.IndexOf("[commit]")>-1)
                    Commit=Convert.ToDouble(line.Split("value=\"")[1].Split("\"")[0])/1000;
            }     
        }

        public void GetPercentileByHRef()
        {
            cWGET site = new cWGET(cookie_name,cookies,path,domain);
            RawHTMLPercentile = site.GetUrl("https://admin.ddos-guard.net/"+PercentileLink);

            string[] lines = RawHTMLPercentile.Split(new char[] {'\r','\n'});
            int i=0;
            string s="";
            foreach (string line in lines)
            {
                i++;
                if (line.IndexOf("95% Prevail")>-1)
                {
                    s=lines[i];
                    if (s.IndexOf("Mbps")>-1)                
                        Percentile=Math.Round(Convert.ToDouble(s.Split(">")[1].Split("<")[0].Split(" Mbps")[0])/1000,2);
                    else
                        Percentile=Convert.ToDouble(s.Split(">")[1].Split("<")[0].Split(" Gbps")[0]);
                }
            }            
        }
    }
    public class cUpLinks
    {
        public List<cUpLink> ID;

        protected string cookie_name;
        protected string cookies;
        protected string path;
        protected string domain;

        protected string RawHTML;

        public cUpLinks(string _cookie_name, string _cookies, string _path, string _domain, string _url)
        {
            cookie_name = _cookie_name;
            cookies = _cookies;
            path = _path;
            domain = _domain;

            ID = new List<cUpLink>();

            cWGET site = new cWGET(cookie_name,cookies,path,domain);
            RawHTML = site.GetUrl(_url);
        }

        public void GetNamesTimesAndURLs()
        {
            string[] lines = RawHTML.Split(new char[] {'\r','\n'});
            List<string> SignificantLines = new List<string>();
            
            int i=0;
            foreach (string line in lines)
            {
                i++;
                if (line.IndexOf("<strong><i ")>-1)
                    {
                        string s = line.Substring(line.LastIndexOf("color")+1);                        
                        cUpLink UL = new cUpLink(cookie_name,cookies,path,domain);
                        UL.Name = s.Split("></i>")[1].Split("<a")[0];                        
                        UL.Name = UL.Name.Substring(1);
                        UL.Name = UL.Name.Substring(0,UL.Name.IndexOf("  "));

                        for (int k=i; k<i+6; k++)
                        {
                            if (lines[k].IndexOf("overcommit")>-1)
                            {
                                UL.Time = lines[k].Substring(0,lines[k].IndexOf("overcommit"));
                                UL.Time = UL.Time.Substring(UL.Time.LastIndexOf(">")).Substring(2);
                                if (UL.Time.IndexOf("No ")>-1)
                                    UL.Time = "0";
                                else
                                    UL.Time = UL.Time.Substring(0,UL.Time.LastIndexOf("  "));
                            }
                        }
                        if ((UL.Time=="")||(UL.Time==null))
                            UL.Disabled=true;
                        else
                            UL.Disabled=false;

                        UL.CommitLink = (s.Split("href=\"")[1].Split("\"")[0]).Substring(1);
                        UL.CommitLink = UL.CommitLink.Replace("amp;","");

                        UL.PercentileLink = (s.Split("href=\"")[2].Split("\"")[0]).Substring(1);
                        UL.PercentileLink = UL.PercentileLink.Replace("amp;","");
                        ID.Add(UL);
                    }
            }            
        }
        public void GetCommits()
        {
            for (int i=0; i<ID.Count; i++)
            {
                Console.Write("  1. Получаю данные о допустимых коммитах: {0,-3}% \r", Math.Round(Convert.ToDouble(i)*100/ID.Count),2);
                if ((!ID[i].Disabled))
                    ID[i].GetCommitByHRef();
                
            }
            Console.WriteLine("  1. Получаю данные о допустимых коммитах: 100%");
        }

        public void GetPercentiles()
        {            
            for (int i=0; i<ID.Count; i++)
            {
                Console.Write("  2. Получаю данные о текущих значениях: {0,-3}% \r", Math.Round(Convert.ToDouble(i)*100/ID.Count),2);
                if ((!ID[i].Disabled))
                    ID[i].GetPercentileByHRef();
            }
            Console.WriteLine("  1. Получаю данные о текущих значениях: 100%");
        }
        public void Print()
        {
            foreach (cUpLink u in ID)
                if ((!u.Disabled))
                    u.Print();
        }
        
        public void PrintNotNull()
        {
            foreach (cUpLink u in ID)
                if ((!u.Disabled)&&(u.Time!="0"))
                    u.Print();
        }

        public void PrintExcept(List<string> LS)
        {
            foreach (cUpLink u in ID)
                if ((!LS.Contains(u.Name))&&(!u.Disabled))
                    u.Print();
        }

        public void PrintExceptOnlyDanger(List<string> LS)
        {
            foreach (cUpLink u in ID)
            {
                double rate = u.Percentile/u.Commit;                
                if ((!LS.Contains(u.Name))&&(!u.Disabled)&&(rate>.7))
                    u.Print();
            }
        }

        public double GetOvecommitByName(string name)
        {
            foreach (cUpLink u in ID)
                if (u.Name==name)
                    return u.Percentile;

            return 0;
        }

    }
}
