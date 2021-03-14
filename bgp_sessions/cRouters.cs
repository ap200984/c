using System;
using System.IO;
using System.Collections.Generic;
using System.Net;

namespace BGP_sessions2
{        
        public static class str
        {
            public static string spaces(int n, string s)
            {
                if (s==null)
                    s="";            
                string sp="";
                for (int i=0; i<n-s.Length; i++)
                    sp+=" ";
                return sp;
            }

            public static string[] parse_words(string line)
            {
                for (int i=0; i<line.Length; i++)
                {
                    
                }
                return null;
            }
        }

        public class cProgress
        {
            public int processed_sessions;
            public int total_sessions;

            public bool p30;
            public bool p60;
            public bool p80;

            public cProgress()
            {
                processed_sessions=0;
                total_sessions=0;
                p30=false;
                p60=false;
                p80=false;
            }
        }

        public class cProduct
        {
            public string Class;
            public string Payment_period;
            public string Product;
            public string Type;
            public string Status;            
        }

        public class cBGP_neighbor
        {
            public string IP;
            public string Customer;
            public string Manager;
            public string DownTime;
            public string ASN;
            public string InPkt;
            public string OutPkt;
            public string OutQ;
            public string Flaps;
            public string State;
            public string ID;
            public string Status;

            public List<cProduct> Products;

            public cBGP_neighbor(){ Products = new List<cProduct>();}

            public cBGP_neighbor(string line)
            {
                Products = new List<cProduct>();

                string[] words_all = line.Split(" ");
                                       
                List<string> words = new List<string>();
                foreach (string s in words_all)
                    if (s!="")
                        words.Add(s);  

                if (line!="")
                {
                    IPAddress ip;
                    if (IPAddress.TryParse(words[0], out ip))
                    {
                        IP=words[0];
                        ASN=words[1];
                        InPkt=words[2];
                        OutPkt=words[3];
                        OutQ=words[4];
                        Flaps=words[5];
                        for (int i=6; i<words.Count-1; i++)                                            
                            DownTime+=words[i]+" ";                                            
                        DownTime=DownTime.Substring(0,DownTime.Length-1);
                        State=words[words.Count-1];
                    }
                }
            }

            public void GetCustomerAndManagerByASN(string cookie_identity)
            {
                CookieContainer cookieContainer = new CookieContainer();
                cookieContainer.Add( new Cookie("_identity", cookie_identity, "/", "admin.ddos-guard.net") );
                string url = "https://admin.ddos-guard.net/user/company?UserSearch%5Bid%5D=&UserSearch%5Bform_organization%5D=&UserSearch%5Basn%5D="+ASN+"&UserSearch%5Bstatus%5D=Active&UserSearch%5Bmanager_id%5D=";

                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
                req.CookieContainer = cookieContainer;           
                HttpWebResponse resp = (HttpWebResponse)req.GetResponse();

                try
                {
                    StreamReader stream = new StreamReader(resp.GetResponseStream());
                    string Text = stream.ReadToEnd();
                    int start_data_key = Text.IndexOf("<tr data-key=");
                    if (start_data_key>-1)
                    {
                        string temp=Text.Substring(start_data_key);                            
                        int end_data_key = temp.IndexOf("</td><td><a href=");                                
                        temp = temp.Substring(0,end_data_key);
                                    
                        //Check if in returned table 
                        string real_asn_string=temp.Split("<td>")[4].Split("<")[0];
                        string[] real_asns=real_asn_string.Split(",");
                        bool exist=false;

                        for (int i=0; (i<real_asns.GetLength(0))&&(!exist); i++)
                            if (real_asns[i]==ASN)
                                exist=true;

                        if (exist)
                        {             
                            int st = temp.IndexOf(@"/user/summary");
                            temp=temp.Substring(st);                            
                            st=temp.IndexOf("id=")+3;
                            temp=temp.Substring(st);
                            ID=temp.Substring(0,temp.IndexOf(">")-1);
                            st = temp.IndexOf(">")+1;
                            temp = temp.Substring(st);
                            int en = temp.IndexOf("</a>");
                            string cust = temp.Substring(0,en);
                            string man = temp.Substring(temp.LastIndexOf("<td>")+4);

                            Customer = cust;
                            if (Customer.Contains("DDOS-GUARD"))
                                Manager="";
                            else
                                Manager = man;                        
                        }                            
                    }
                }
                catch {}
            }

            public void GetStatus(string cookie_identity)
            {
                if (Customer!=null)
                    if (Customer.Contains("DDOS-GUARD"))
                        return;
                
                
                if (ID!=null)                
                try
                {
                    CookieContainer cookieContainer = new CookieContainer();
                    cookieContainer.Add( new Cookie("_identity", cookie_identity, "/", "admin.ddos-guard.net") );
                    
                    string url = "https://admin.ddos-guard.net/user/products?id="+ID;

                    HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url);
                    req.CookieContainer = cookieContainer;           
                    HttpWebResponse resp = (HttpWebResponse)req.GetResponse();

                    StreamReader stream = new StreamReader(resp.GetResponseStream());
                    string Text = stream.ReadToEnd();

                    string[] lines = Text.Split(new char[] {'\r','\n'});
                    List <cProduct> AllProductsTable = new List<cProduct>();
                    foreach (string line in lines)
                        if (line.Contains("data-key"))
                        {
                            cProduct prd = new cProduct();
                            prd.Class = line.Split("data-key")[0].Split("class=")[1].Replace("\"","").Trim();
                            prd.Payment_period = line.Split("<td>")[2].Split("</td>")[0];
                            prd.Product = line.Split("<td>")[3].Split("</td>")[0];
                            prd.Type = line.Split("<td>")[4].Split("</td>")[0];
                            prd.Status = line.Split("<td>")[5].Split("</td>")[0];                            
                            AllProductsTable.Add(prd);
                        }
                    
                    Status="Х.з.";
                    //Find non free first:
                    foreach (cProduct P in AllProductsTable)
                    {
                        if (P.Class=="success")
                        {
                            if (P.Product.Length>20)
                                P.Product = P.Product.Substring(0,20);

                            if (P.Payment_period=="Semi-Annually")
                            {
                                Status="| "+P.Payment_period+str.spaces(8,P.Payment_period)+"| "+P.Product+str.spaces(22,P.Product)+": "+P.Status;
                                break;
                            }
                            else if (P.Payment_period=="Monthly")
                            {
                                Status="| "+P.Payment_period+str.spaces(8,P.Payment_period)+"| "+P.Product+str.spaces(22,P.Product)+": "+P.Status;
                                break;
                            }
                            else if (P.Payment_period=="One Time")
                            {
                                Status="| "+P.Payment_period+str.spaces(8,P.Payment_period)+"| "+P.Product+str.spaces(22,P.Product)+": "+P.Status;
                                break;
                            }
                            else if (P.Payment_period=="Free")
                            {
                                Status="| "+P.Payment_period+str.spaces(8,P.Payment_period)+"| "+P.Product+str.spaces(22,P.Product)+": "+P.Status;
                            }                        
                        }
                    }
                }
                catch {}               
            }
        }  

        public class cRouter
        {
            public string Name;
            public int Port;
            public string login;
            public string passwd;

            public List<cBGP_neighbor> BNs;

            public cRouter()
            {
                BNs = new List<cBGP_neighbor>();
            }

            public void GetBGPSessionsFromSSH()
            {
                cRouterSSH R = new cRouterSSH(Name,Port,login,passwd);
                R.Connect();
                List<string> lines = R.GetBGPSessions();
                foreach (string s in lines)
                {
                    cBGP_neighbor bn = new cBGP_neighbor(s);                    
                    if (bn.IP!=null)
                        BNs.Add(bn);
                }
                R.Disconnect();
            }

            public void GetDataFromWEBAdmin(string cookie_identity, ref cProgress Progress)
            {
                foreach (cBGP_neighbor bn in BNs) 
                {   
                    Progress.processed_sessions++;
                    Console.Write(".");
                    double progress=(Progress.processed_sessions*100/Progress.total_sessions);

                    if ((!Progress.p30)&&(progress>20)) {Console.WriteLine("\n30%... Ща...");Progress.p30=true;}
                    if ((!Progress.p60)&&(progress>50)) {Console.WriteLine("\n60%... Ща как заебашу!");Progress.p60=true;}
                    if ((!Progress.p80)&&(progress>80)) {Console.WriteLine("\n80%... Почти готово, ёпта!");Progress.p80=true;}

                    bn.GetCustomerAndManagerByASN(cookie_identity);             
                    bn.GetStatus(cookie_identity);
                }
            }

            public void Print()
            {
                foreach (cBGP_neighbor bn in BNs)
                        {
                            string cust = bn.Customer;
                            if (cust!=null)
                                if (cust.Length>46)
                                    cust=cust.Substring(0,43)+"...";

                            Console.WriteLine("   "+bn.IP+str.spaces(22,bn.IP)+
                                //bn.Flaps+spaces(7,bn.Flaps)+
                                bn.DownTime+str.spaces(20,bn.DownTime)+
                                bn.State+str.spaces(10,bn.State)+
                                cust+str.spaces(50,cust)+
                                bn.Manager+str.spaces(25,bn.Manager)+
                                bn.Status);
                        }
            }

            public void PrintToString(ref string res)
            {
                foreach (cBGP_neighbor bn in BNs)
                        {
                            string cust = bn.Customer;
                            if (cust!=null)
                                if (cust.Length>46)
                                    cust=cust.Substring(0,43)+"...";

                            res+="   "+bn.IP+str.spaces(22,bn.IP)+
                                //bn.Flaps+spaces(7,bn.Flaps)+
                                bn.DownTime+str.spaces(20,bn.DownTime)+
                                bn.State+str.spaces(10,bn.State)+
                                cust+str.spaces(50,cust)+
                                bn.Manager+str.spaces(25,bn.Manager)+
                                bn.Status+"\n";
                        }
            }

            public void PrintToStream(ref StreamWriter sw)
            {
                foreach (cBGP_neighbor bn in BNs)
                        {
                            string cust = bn.Customer;
                            if (cust!=null)
                                if (cust.Length>46)
                                    cust=cust.Substring(0,43)+"...";

                            sw.WriteLine("   "+bn.IP+str.spaces(22,bn.IP)+
                                //bn.Flaps+spaces(7,bn.Flaps)+
                                bn.DownTime+str.spaces(20,bn.DownTime)+
                                bn.State+str.spaces(10,bn.State)+
                                cust+str.spaces(50,cust)+
                                bn.Manager+str.spaces(25,bn.Manager)+
                                bn.Status);
                        }
            }

            public void TryGetInfoFromDescription()
            {
                cRouterSSH R = new cRouterSSH(Name, Port, login, passwd);
                R.Connect();
                foreach (cBGP_neighbor nbr in BNs)
                    if ((nbr.Customer==null)||(nbr.Customer=="")||nbr.Customer.Contains("DDOS-GUARD"))
                    {
                        if (nbr.Customer!=null)
                        {
                            if (nbr.Customer.Contains("DDOS-GUARD"))
                                nbr.Manager="";
                        }
                        else if (nbr.IP.Substring(0,3)=="10.")
                            nbr.Manager="";
                        else
                            nbr.Manager="ASN "+nbr.ASN+" не указан в профиле ни у одного клиента";

                        nbr.Customer = R.GetBGPNeighborDescription(nbr.IP);
                    }
                R.Disconnect();
            }

            public void TryGetInfoFromDescriptionUsingAllConfig()
            {
                cRouterSSH R = new cRouterSSH(Name, Port, login, passwd);
                R.Connect();
                R.GetAllConfigDisplaySet();

                foreach (cBGP_neighbor nbr in BNs)
                    if ((nbr.Customer==null)||(nbr.Customer=="")||nbr.Customer.Contains("DDOS-GUARD"))
                    {
                        if (nbr.Customer!=null)
                        {
                            if (nbr.Customer.Contains("DDOS-GUARD"))
                                nbr.Manager="";
                        }
                        else if (nbr.IP.Substring(0,3)=="10.")
                            nbr.Manager="";
                        else
                            nbr.Manager="ASN "+nbr.ASN+" не указан в профиле ни у одного клиента";

                        nbr.Customer = R.GetBGPNeighborDescriptionUsingAllConfig(nbr.IP);
                    }
                R.Disconnect();
            }
        }

        public class cRouters
        {
            public cProgress Progress;            
            public List<cRouter> ID;
            protected string cookie_identity;
            
            public cRouters(string admin_cookie_file)         
            {
                ID = new List<cRouter>();
                Progress=new cProgress(); 
                try 
                {
                    StreamReader csr = new StreamReader(admin_cookie_file);
                    cookie_identity = csr.ReadLine();           
                    csr.Close();
                }
                catch 
                {
                    Console.WriteLine("Can't read admin_cookie.txt\nUsing existing cookie...\n");
                    cookie_identity="4cac4cad483ac4a3d3b4242c11c988963ba5791eee421a614f09db65ad0bb88ca%3A2%3A%7Bi%3A0%3Bs%3A9%3A%22_identity%22%3Bi%3A1%3Bs%3A16%3A%22%5B105%2C%22%22%2C2592000%5D%22%3B%7D";
                }
            }

            public void InitFromListManually(List<cRouter> rtrs)
            {
                foreach (cRouter r in rtrs)
                    ID.Add(r);
            }

            public void InitFromList(string router_list, int _port, string _login, string _passwd)
            {
                string[] routers = router_list.Split(",");
                foreach (string r in routers)
                {
                    Console.Write("Подключаюсь к "+r+"...");
                    cRouter R = new cRouter();
                    R.Name = r;
                    R.Port = _port;
                    R.login = _login;
                    R.passwd = _passwd;
                    R.GetBGPSessionsFromSSH();
                    ID.Add(R);
                    Console.WriteLine(" Данные о нерабочих сессиях получены.");
                }
            }

            public void GetBGPSessionsFromSSH()
            {
                foreach (cRouter R in ID)
                {
                    R.GetBGPSessionsFromSSH();
                }
            }

            public void GetBGPSessionsFromFile(string filename)
            {
                ID = new List<cRouter>();
                try
                {
                    StreamReader sr = new StreamReader(filename);
                    String line=""; int total_lines=0;
                    while (sr.Peek() >= 0)
                    {
                        line = sr.ReadLine();
                        //if ( (line.Contains("Active")) || (line.Contains("Idle")) || (line.Contains("Open")) )
                        //    Progress.total_sessions++;
                        total_lines++;
                    }
                    sr.Close();
                    //Progress.total_sessions = total_lines;

                    sr = new StreamReader(filename);
                    for (int k=0; k<total_lines; k++)
                    {   
                        if (line.IndexOf("bravo")!=0)
                            line = sr.ReadLine();
                        if (line.IndexOf("bravo")==0)
                        {   
                            cRouter R = new cRouter();
                            R.Name = line;
                            R.Port = 22;
                            ID.Add(R);
                            do
                            {    
                                line=sr.ReadLine();
                                if ((line!=null)&&(line!=""))
                                {
                                    cBGP_neighbor bn=new cBGP_neighbor(line);                        
                                    if (bn.IP!=null)
                                        R.BNs.Add(bn);
                                }
                            }
                            while ((line!=null)&&(line.IndexOf("bravo")==-1));                                                                
                        }
                    }
                    sr.Close();                 
                } 
                catch
                {

                }
                Console.WriteLine();

            }

            public void GetDataFromWEBAdmin()
            {                
                foreach (cRouter R in ID)                
                    Progress.total_sessions+=R.BNs.Count;

                foreach (cRouter R in ID)
                    R.GetDataFromWEBAdmin(cookie_identity,ref Progress);
            }

            public void Print()
            {
                foreach (cRouter r in ID)
                    {
                        if (r.BNs.Count>0)
                        {
                            Console.WriteLine();
                            Console.WriteLine(r.Name);
                            r.Print();
                        }
                    }
            }

            public string PrintToString()
            {
                string res ="";
                foreach (cRouter r in ID)
                    {
                        if (r.BNs.Count>0)
                        {
                            res+="\n";
                            res+=r.Name+"\n";
                            r.PrintToString(ref res);
                        }
                    }
                return res;
            }

            public void PrintToFile(string filename)
            {
                StreamWriter sw = new StreamWriter(filename);
                foreach (cRouter r in ID)
                    {
                        if (r.BNs.Count>0)
                        {
                            sw.WriteLine();
                            sw.WriteLine(r.Name);
                            r.PrintToStream(ref sw);
                        }
                    }
                sw.Close();
            }            
        
            public void TryGetInfoFromDescriptions()
            {
                
                foreach (cRouter R in ID)
                {
                    try
                    {
                        Console.Write("Подключюсь "+R.Name+" для получения описаний сессий...");
                        R.TryGetInfoFromDescription();            
                        Console.WriteLine(" Готово!");
                    }
                    catch
                    {
                        Console.WriteLine("Не удалось подключиться к хосту "+R.Name);
                    }
                }
            }

            public void TryGetInfoFromDescriptionsUsingAllConfig()
            {
                
                foreach (cRouter R in ID)
                {
                    try
                    {
                        Console.Write("Подключюсь "+R.Name+" для получения описаний сессий...");
                        R.TryGetInfoFromDescriptionUsingAllConfig();            
                        Console.WriteLine(" Готово!");
                    }
                    catch
                    {
                        Console.WriteLine("Не удалось подключиться к хосту "+R.Name);
                    }
                }
            }


        }

}