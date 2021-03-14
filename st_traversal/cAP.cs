using System;
using System.Collections.Generic;

namespace st_traversal
{
    public class cStation
    {
        public string Name;
        public string hostname;
        public int port;
        public string login;
        public string passwd;

        public string brctl_show;
        public List<int> vlans;

        private cSSHClient ssh;

        public cStation(string name, int _port, string _login, string _passwd)
        {

            vlans = new List<int>();
            Name = name;
            port = _port;
            login = _login;
            passwd = _passwd;
        }

        public void GetInfo()
        {
            List<string> passwords = new List<string>();
            passwords.Add(passwd);
            passwords.Add("qqq12654");
            passwords.Add("RadioGfhjkm");
            passwords.Add("ubnt");

            bool connected = false;
            foreach (string pass in passwords)
            {
                ssh = new cSSHClient(Name, port, login, pass);
                int r = ssh.Connect();
                if (r == 1)
                {
                    connected = true;
                    passwd = pass;
                    break;
                }
            }

            if (connected)
            {
                brctl_show = ssh.GetCommandResult("brctl show");
                hostname = ssh.GetCommandResult("mca-status | grep deviceName | cut -f2 -d '=' | cut -f1 -d','");
                hostname = hostname.Split("\n")[0];
                string s = ssh.GetCommandResult("brctl show | grep ath0 | grep -v ath0.100");
                string[] lines = s.Split(new[] { '\r', '\n' });
                foreach (string vlan in lines)
                    if (vlan != "")
                    {
                        string v = vlan.Substring(vlan.IndexOf("ath0.")+5);
                        vlans.Add(Convert.ToInt32(v));
                    }

                ssh.Disconnect();
            }
        }
        public void Print()
        {
            Console.WriteLine(Name);
            Console.WriteLine(brctl_show);
            foreach (int i in vlans)
                Console.WriteLine(i.ToString());

        }
    }

    public class cAP
    {
        public string Name;
        public int port;
        public string login;
        public string passwd;

        public int total_stations;
        public int connected_stations;

        public List<cStation> Stations;

        private cSSHClient ssh;

        public cAP(string name, int _port, string _login, string _passwd)
        {
            Stations = new List<cStation>();
            Name = name;
            port = _port;
            login = _login;
            passwd = _passwd;
        }

        public void GetStations()
        {
            ssh = new cSSHClient(Name, port, login, passwd);
            ssh.Connect();
            string wst = ssh.GetCommandResult("wstalist | grep lastip | cut -f4 -d\"\\\"\"");
            total_stations = Convert.ToInt32(ssh.GetCommandResult("wstalist | grep mac | wc -l"));
            ssh.Disconnect();

            string[] lines = wst.Split(new[] { '\r', '\n' });
            foreach (string line in lines)
                if (line != "")
                {
                    cStation st = new cStation(line, 22, login, passwd);
                    Stations.Add(st);
                }

            foreach (cStation S in Stations)
                S.GetInfo();

            connected_stations = Stations.Count;

        }



        public void Print()
        {
            foreach (cStation S in Stations)
                S.Print();


            Console.WriteLine("\n\n********\nTotal stations:" + total_stations + "\nConnected stations: " + connected_stations + "\nVlans summary:");
            List<int> VLANs = new List<int>();

            foreach (cStation S in Stations)
                foreach (int v in S.vlans)
                    VLANs.Add(v);

            VLANs.Sort();
            int k = 0;
            foreach (int v in VLANs)
            {
                foreach (cStation S in Stations)
                    if (S.vlans.Contains(v))
                    {
                        k++;
                        Console.WriteLine(k.ToString() + ". " + v.ToString() + " " + S.hostname + " (Пароль: "+passwd+")");
                    }
            }


        }




    }




}