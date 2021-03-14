using System.Text.RegularExpressions;
using Renci.SshNet;
using System.Collections.Generic;
using System;
using System.IO;


namespace BGP_sessions2
{
    class cRouterSSH
    {        
        public string Name;
        protected int Port;
        protected string Username;
        protected string Password;
        protected string ConfigDisplaySet;

        protected SshClient SSH;

        private Regex OperationalModePrompt;
        private Regex ConfigurationModePrompt;        

        public cRouterSSH(string name, int port, string login, string password)
        {
            Name=name;
            Port = port;
            Username = login;
            Password = password;  
            
            string pattern = @"^"+Username+"@"+".*"+">$";  
            OperationalModePrompt = new Regex(pattern);

            pattern = @"^"+Username+"@"+".*"+"#$";  
            ConfigurationModePrompt = new Regex(pattern);

            SSH = new SshClient(Name,Port,Username,Password);
        }

        public void Connect()
        {
            try
            {
                SSH.Connect();
            }
            catch
            {
                Console.WriteLine("Не могу подключиться к "+Name);
            }
        }

        public string GetCommandResult(string command)
        {
            return SSH.CreateCommand(command).Execute();
        }

        public List<string> GetBGPSessions()
        {
            string s = GetCommandResult("show bgp summary | grep \"Active|Idle|Open|Connect|OpenSent|OpenConfirm\" | except \"Peer\"");
            string[] lines = s.Split(new [] { '\r', '\n' });

            List<string> sessions = new List<string>();
            foreach (string line in lines)
                try
                {                    
                    if (line!="")
                    {
                        sessions.Add(line);
                    }
                }
                catch
                {                    
                }
            return sessions;
        }
        
        public void GetAllConfigDisplaySet()
        {
            ConfigDisplaySet = GetCommandResult("show configuration | display set");
            //StreamWriter sw = new StreamWriter(Name+" "+DateTime.Now.Date+".txt");
            //sw.WriteLine(ConfigDisplaySet);
            //sw.Close();

        }


        public string GetBGPNeighborDescription(string IP)
        {
            string s = GetCommandResult("show configuration | display set | grep "+IP);
            string[] lines = s.Split(new [] { '\r', '\n' });
            string temp ="";
            foreach (string line in lines)
                try
                {
                    if (line!="")
                    {
                        if (line.IndexOf(IP)+IP.Length<line.Length)
                        {
                            if (line.Substring(line.IndexOf(IP)+IP.Length,1)==" ")
                                if (line.Contains("description"))
                                    {
                                        temp = line.Split("description")[1];
                                        break;
                                    }
                        }
                    }
                }
                catch
                {
                    temp="Не удалось найти описание для "+IP;                    
                }

            temp = temp.Replace("\"","");
            temp = temp.Trim();
            if (!temp.Contains("##"))
                temp="## "+temp+" ##";
            return temp;
        }

        public string GetBGPNeighborDescriptionUsingAllConfig(string IP)
        {
            string[] all_lines = ConfigDisplaySet.Split(new [] { '\r', '\n' });
            List<string> lines = new List<string>();
            foreach (string s in all_lines)
                if (s.Contains(IP))
                    lines.Add(s);
            
            string temp ="";
            foreach (string line in lines)
                try
                {
                    if (line!="")
                    {
                        if (line.IndexOf(IP)+IP.Length<line.Length)
                        {
                            if (line.Substring(line.IndexOf(IP)+IP.Length,1)==" ")
                                if (line.Contains("description"))
                                    {
                                        temp = line.Split("description")[1];
                                        break;
                                    }
                        }
                    }
                }
                catch
                {
                    temp="Не удалось найти описание для "+IP;                    
                }

            temp = temp.Replace("\"","");
            temp = temp.Trim();
            if (!temp.Contains("##"))
                temp="## "+temp+" ##";
            return temp;
        }


        public void Disconnect()
        {
            SSH.Disconnect();
        }

        
    }
}