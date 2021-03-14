using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace bgp_peers
{
    public class cBGPPeer
    {
        public string NAME; //IP
        public string Description;
        public string StateStr;
        public int StateInt;
        public string Time;
        public int PrefixCount;
        public int RemoteAS;
    }    

    public class cBGPPeers
    {
        public List<cBGPPeer> ID;

        public cBGPPeers(string sh_ip_bgp_neigh)
        {
            ID = new List<cBGPPeer>();
            string[] lines = sh_ip_bgp_neigh.Split(new[] { '\r', '\n'});
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].IndexOf("BGP neighbor is") > -1)
                {
                    try
                    {
                        cBGPPeer P = new cBGPPeer();
                        P.NAME = lines[i].Split(" ")[3].Split(",")[0];
                        P.RemoteAS = Convert.ToInt32(lines[i].Split(" ")[6].Split(",")[0]);
                        P.Description = lines[i + 1].Split(" ")[2];
                        P.StateStr = lines[i + 2].Split(" ")[5].Split(",")[0];
                        P.StateInt = Convert.ToInt32(P.StateStr.IndexOf("Established") > -1);                        
                        P.Time = lines[i + 2].Split(" ")[8];
                        P.PrefixCount = Convert.ToInt32(lines[i + 3].Split(" ")[2]);

                        ID.Add(P);
                    }
                    catch { }
                }
            }
        }
        public string GetJson()
        {
            string JSON = JsonConvert.SerializeObject(this.ID, Formatting.Indented);
            return JSON.Replace("\"NAME\"","\"{#NAME}\"");
        }
    }
}