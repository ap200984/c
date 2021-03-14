using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System;

namespace frr_bgp_check
{

    public class cNbr
    {
        public string NAME;
    }

    public class cBGP_Neighbor
    {
        public string Description;
        public string IP;
        public string ASN;
        public string state;
        public int state_bool;
        public string time;

        public string GetJSON()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }
    }

    public class cBGP_Neighbors
    {
        public List<cBGP_Neighbor> Neighbors;

        public cBGP_Neighbors() { Neighbors = new List<cBGP_Neighbor>(); }

        public string Discover_Neighbors(string sh_ip_bgp_sum)
        {
            try
            {
                List<cNbr> LN = new List<cNbr>();

                string[] lines = sh_ip_bgp_sum.Split(new[] { '\n', '\r' });
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Contains("Neighbor        V         AS   MsgRcvd   MsgSent   TblVer  InQ OutQ  Up/Down State/PfxRcd   PfxSnt Desc"))
                        while (lines[i] != "")
                        {
                            i++;
                            cNbr N = new cNbr();
                            N.NAME = lines[i].Split(" ")[0];
                            if (N.NAME != "")
                                LN.Add(N);
                        }
                }
                return JsonConvert.SerializeObject(LN, Formatting.Indented).Replace("\"NAME\"", "\"{#NAME}\"");
            }
            catch
            {
                return "Can't parse "+sh_ip_bgp_sum;
            }
        }

        public string GetNeighborINFO(string sh_ip_bgp_neigh, string ip)
        {
            string res = "";
            //Console.WriteLine(sh_ip_bgp_neigh+"\n"+ip);

            string[] lines = sh_ip_bgp_neigh.Split(new[] { '\r', '\n' });
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Contains("BGP neighbor is " + ip))
                {
                    cBGP_Neighbor N = new cBGP_Neighbor();
                    N.IP = ip;
                    N.ASN = lines[i].Split("remote AS ")[1].Split(",")[0];
                    N.Description = lines[i + 1].Split(": ")[1];
                    N.state = lines[i + 3].Split(" = ")[1].Split(",")[0];
                    if ((N.state == "Established"))
                        N.state_bool = 1;
                    try
                    {
                        N.time = lines[i + 3].Split("up for ")[1];
                    }
                    catch
                    {
                        N.time = lines[i + 4].Split("Last read ")[1].Split(",")[0];

                    }
                    res = N.GetJSON();
                    break;
                }
            }
            if (res == "")
            {
                cBGP_Neighbor N = new cBGP_Neighbor();
                N.IP = ip;
                N.state = "No such peer";
                res = N.GetJSON();
            }
            return res;
        }

    }





}