using System;
using System.Collections.Generic;
using Renci.SshNet;
using System.IO;
using System.Text.RegularExpressions;

namespace EditPrefixes
{

    class cASSET
    {
        public string Name;
        public List<string> FoundPrefixes; //Prefixes that are on the router right now
        public List<string> bgpq3Prefixes; //Prefixes that are in RADB
        public List<string> AddPrefixes;
        public List<string> DeletePrefixes;


        public cASSET()
        {
            FoundPrefixes = new List<string>();
            bgpq3Prefixes = new List<string>();
            AddPrefixes = new List<string>();
            DeletePrefixes = new List<string>();
        }

        public void GetDiffs()
        {            
            //Delete prefixes from prefix-list
            foreach (string found_prefix in FoundPrefixes)
                if (!bgpq3Prefixes.Contains(found_prefix))
                    DeletePrefixes.Add(found_prefix);

            //Add new prefixes from RADB
            foreach (string radb_prefix in bgpq3Prefixes)
                if (!FoundPrefixes.Contains(radb_prefix))
                    AddPrefixes.Add(radb_prefix);
        }

        public void PrintConfig()
        {
            foreach (string prefix in AddPrefixes)
                if (prefix!="")
                    Console.WriteLine("set policy-options prefix-list CUSTOMER:"+Name+" "+prefix);
            
            foreach (string prefix in DeletePrefixes)
                if (prefix!="")
                    Console.WriteLine("delete policy-options prefix-list CUSTOMER:"+Name+" "+prefix);
        }

        public void PrintConfigToFile(ref StreamWriter sw)
        {

            foreach (string prefix in AddPrefixes)
                if (prefix!="")
                    sw.WriteLine("set policy-options prefix-list CUSTOMER:"+Name+" "+prefix);
            
            foreach (string prefix in DeletePrefixes)
                if (prefix!="")
                    sw.WriteLine("delete policy-options prefix-list CUSTOMER:"+Name+" "+prefix);
        }

        public void InserConfigInRouter(ref StreamWriter sw, ref ShellStream shell, Regex ConfigMode)
        {

            foreach (string prefix in AddPrefixes)
                if (prefix!="")
                {
                    sw.WriteLine("set policy-options prefix-list CUSTOMER:"+Name+" "+prefix);
                    //System.Threading.Thread.Sleep(10);
                    string rep = shell.Expect(ConfigMode, new TimeSpan(0,0,0,0,10));
                    Console.WriteLine(prefix);
                }
            
            foreach (string prefix in DeletePrefixes)
                if (prefix!="")
                {
                    sw.WriteLine("delete policy-options prefix-list CUSTOMER:"+Name+" "+prefix);
                    string rep = shell.Expect(ConfigMode, new TimeSpan(0,0,0,0,10));
                    //System.Threading.Thread.Sleep(10);
                }
        }


    }

    class cRouter
    {        
        public string Name;
        protected int Port;
        protected string Username;
        protected string Password;

        private Regex OperationalModePrompt;
        private Regex ConfigurationModePrompt;        
        
        public List<cASSET> ASSETs;

        public cRouter(string name, int port, string login, string password)
        {
            ASSETs = new List<cASSET>();
            Name=name;
            Port = port;
            Username = login;
            Password = password;  
            
            string pattern = @"^"+Username+"@"+".*"+">$";  
            OperationalModePrompt = new Regex(pattern);

            pattern = @"^"+Username+"@"+".*"+"#$";  
            ConfigurationModePrompt = new Regex(pattern);

        } 

        public string GetAllConfig()
        {
            SshClient sc = new SshClient(Name,Port,Username,Password);
            sc.Connect();
            string command = "show configuration";
            string scds = sc.CreateCommand(command).Execute();
            sc.Disconnect();
            return scds;
        }

        public string RunCommand(string cmd)
        {
            SshClient sc = new SshClient(Name,Port,Username,Password);
            sc.Connect();
            string command = cmd;
            string scds = sc.CreateCommand(command).Execute();
            sc.Disconnect();
            return scds;
        }

        public void GetASSETs()
        {            
            SshClient sc = new SshClient(Name,Port,Username,Password);
            sc.Connect();
            string command = "show configuration | display set | grep \"set policy-options prefix-list CUSTOMER:\"";

            DateTime dt_start = DateTime.Now;
            Console.WriteLine(Name+ ": Выгружаю конфиг из роутера...");
            string scds = sc.CreateCommand(command).Execute();

            DateTime dt_end = DateTime.Now;            
            Console.WriteLine(Name + ": Конфиг выгружен за "+(dt_end-dt_start).Seconds.ToString()+"с. Пробую распарсить и загрузить данные из RADB...");
            
            dt_start = DateTime.Now;
            string[] lines = scds.Split(new [] { '\r', '\n' });
            
            List<string> KnownASSETS = new List<string>();

            string AS="";
            foreach (string line in lines)
            {    
                try 
                { 
                    string prefix="";            
                    if (line!="")
                    {    
                        string afterCustomer = line.Substring(line.IndexOf("CUSTOMER:")+9);
                        try
                        {        
                            AS = afterCustomer.Split(" ")[0];
                        }
                        catch
                        {
                            Console.WriteLine(Name+": Ошибка! Чё-то не получается распарсить AS-SET или ASN в строке "+line);
                        }
                        try
                        {
                            if (afterCustomer.Split(" ").Length>1)
                                //prefix=line.Split(":")[1].Split(" ")[1];
                                prefix=line.Substring(line.LastIndexOf(" ")+1);                                
                            else
                                if (afterCustomer!="")
                                    Console.WriteLine(Name+": О, бля! Походу, тут пустой префикс-лист CUSTOMER:"+afterCustomer);
                                else 
                                    Console.WriteLine(Name+": Ошибка! Чё-то не получается распарсить AS-SET или ASN в строке  "+line);
                        }
                        catch
                        {
                            Console.WriteLine(Name+": Ошибка! Чё-то вообще ни хера не понятно с этой строкой +\""+line+"\".");
                        }
                    }
                    
                    if (AS!="")
                    {
                        if (!KnownASSETS.Contains(AS))
                        {        
                            KnownASSETS.Add(AS);
                            cASSET tempASSET = new cASSET();
                            tempASSET.Name=AS;
                            try
                            {
                                tempASSET.bgpq3Prefixes = bgpq3.GetPrefixes(AS);
                            }
                            catch
                            {
                                Console.WriteLine(Name + ": Ёптеть! В RADB такого нет "+AS);
                            }

                            if (prefix!="")
                               tempASSET.FoundPrefixes.Add(prefix);
                            ASSETs.Add(tempASSET);
                        }
                        else
                        {
                            if (prefix!="")
                                ASSETs.Find(x=>x.Name.Equals(AS)).FoundPrefixes.Add(prefix);
                        }
                    }
                }                
                catch
                {
                    Console.WriteLine(Name+": Ошибка!: Ни хера не пойму "+line);
                }            
            }    
            dt_end = DateTime.Now;            
            Console.WriteLine(Name + ": Распарсил конфиг и загрузил данные за "+(dt_end-dt_start).Seconds.ToString()+"с");         

            sc.Disconnect();
        }

        public bool NeedToMakeChanges()
        {
            bool need_to_makes_changes=false;

            foreach (cASSET asset in ASSETs)
                if ( (asset.AddPrefixes.Count!=0) || (asset.DeletePrefixes.Count!=0) )
                {
                    need_to_makes_changes=true;
                    break;
                }
            return need_to_makes_changes;
        }

        public void CorrectASSETs()
        {
            bool need_to_makes_changes = NeedToMakeChanges();
            
            if (need_to_makes_changes)
            {
                SshClient cl = new SshClient(Name, Port,Username,Password);
                cl.Connect();            
                ShellStream shell = cl.CreateShellStream("", 0, 0, 0, 0, 10240000);
                StreamWriter wr = new StreamWriter(shell);
                StreamReader rd = new StreamReader(shell);
                wr.AutoFlush = true;

                wr.WriteLine("edit");
                string rep;

                rep = shell.Expect(ConfigurationModePrompt, new TimeSpan(0,0,3));

                foreach (cASSET asset in ASSETs)
                {
                    if ( (asset.AddPrefixes.Count!=0) || (asset.DeletePrefixes.Count!=0) )
                    {
                        asset.InserConfigInRouter(ref wr, ref shell, ConfigurationModePrompt);
                        rep = shell.Expect(ConfigurationModePrompt, new TimeSpan(0,0,3));
                    }
                }
                
                //rep = shell.Expect(ConfigurationModePrompt, new TimeSpan(0,0,3));
                //wr.WriteLine("show | comapre");
                //rep = shell.Expect(ConfigurationModePrompt, new TimeSpan(0,0,3));
                //if (rep!=null)
                //{   
                //    wr.WriteLine("commit and-quit");
                    //rep = shell.Expect(OperationalModePrompt, new TimeSpan(0,0,30));
                //}

                shell.Close();
                cl.Disconnect();
                
                cl.Connect();
                shell = cl.CreateShellStream("", 0, 0, 0, 0, 10240000);
                wr = new StreamWriter(shell);
                rd = new StreamReader(shell);
                wr.AutoFlush = true;
                
                wr.WriteLine("edit");
                rep = shell.Expect(ConfigurationModePrompt, new TimeSpan(0,0,10));

                wr.WriteLine("commit");
                System.Threading.Thread.Sleep(3000);
                wr.WriteLine("commit and-quit");
                shell.Close();
                cl.Disconnect();
            }
            else
            {
                Console.WriteLine("Тут все остается без изменений");
            }

        }
        public void GetDiffs()
        {
            foreach (cASSET asset in ASSETs)
                asset.GetDiffs();           
        }

        public void PrintConfig()
        {
            foreach (cASSET asset in ASSETs)
                asset.PrintConfig();
        }

        public void PrintASSETsToFile()
        {
            StreamWriter sw = new StreamWriter(Name+" fix prefixes.txt");
            foreach (cASSET asset in ASSETs)
                asset.PrintConfigToFile(ref sw);
            sw.Close();
        }

        public void PrintToFile(string s)
        {
            StreamWriter sw = new StreamWriter(Name+" "+Port.ToString()+" cfg.txt");
            sw.WriteLine(s);
            sw.Close();
        }
    }    

}