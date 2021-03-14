using System;
using System.IO;
using System.Collections.Generic;
using Renci.SshNet;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ap_discovery
{

    public class cParameterNoValue
    {
        public string PARAMETERNAME;
        public string TYPE;
    }



    public class cParameter
    {
        public string PARAMETERNAME;
        public string TYPE;
        public string VALUE;

        public void GetJSON()
        {
            string JSON = JsonConvert.SerializeObject(this, Formatting.Indented);
            Console.WriteLine(JSON);
        }

        public void GetParameterType()
        {
            if (VALUE == "") TYPE = "Unknown";
            else if ((PARAMETERNAME.IndexOf("Bytes") > -1) || (PARAMETERNAME.IndexOf("Packets") > -1) || (PARAMETERNAME.IndexOf("bytes") > -1))
                TYPE = "Speed";
            else
            {
                try
                {
                    double d = Convert.ToDouble(VALUE.Replace(".", ","));
                    if ((PARAMETERNAME == "ccq") && (d > 100))
                        VALUE = (d / 10).ToString().Replace(",", ".");
                    TYPE = "Float";
                }
                catch
                {
                    TYPE = "String";
                }
                if (TYPE == "Float")
                    VALUE = VALUE.Replace(",", ".");
            }

        }

        public string InOneLine()
        {
            try
            {
                if ((VALUE.IndexOf("{") == -1) && (VALUE.IndexOf("[") == -1))
                    return "\"" + PARAMETERNAME + "\": \"" + VALUE + "\"";
                else
                    return null;
            }
            catch
            {
                Console.WriteLine("Can't read value for parameter: \"" + PARAMETERNAME + "\"");
                return null;


            }

        }
    }

    public class cStationNoParameters
    {
        public string NAME;
    }

    public class cStation
    {
        public string NAME;
        public List<cParameter> PARAMS;

        public cStation()
        {
            PARAMS = new List<cParameter>();
        }

        public string GetJSON()
        {
            string s = JsonConvert.SerializeObject(this, Formatting.Indented);
            return s;
        }

        public string GetJSONParamsDiscover()
        {
            List<cParameterNoValue> LP = new List<cParameterNoValue>();
            foreach (cParameter P in PARAMS)
            {
                cParameterNoValue p = new cParameterNoValue();
                p.PARAMETERNAME = P.PARAMETERNAME;//.Replace("\"PARAMETERNAME\": \"","\"{#PARAMETERNAME}\": \"");
                p.TYPE = P.TYPE;//.Replace("\"TYPE\": \"","\"{#TYPE}\": \"");
                LP.Add(p);
            }

            string s = JsonConvert.SerializeObject(LP, Formatting.Indented);
            s = s.Replace("\"PARAMETERNAME\": \"", "\"{#PARAMETERNAME}\": \"");
            s = s.Replace("\"TYPE\": \"", "\"{#TYPE}\": \"");
            return s;
        }

        public string GetJSONParamsValues()
        {
            string s = "{\n";
            foreach (cParameter P in PARAMS)
                if (P.InOneLine() != null)
                    s += "\t" + P.InOneLine() + ",\n";

            s = s.Substring(0, s.Length - 2); //remove last comma
            s += "\n}";
            return s;
        }

    }

    public class cAP
    {
        public List<cParameter> PARAMETERS;
        public List<cStation> STATIONS;
        private string AP_Name;
        private string JSON;
        private string JSONFile;
        private bool SuccessConnect;

        public cAP()
        {
            PARAMETERS = new List<cParameter>();
            STATIONS = new List<cStation>();
        }

        public void DeleteJSONFile()
        {
            try
            {
                File.Delete(JSONFile);
            }
            catch
            {
            }
        }

        public cAP(string filename)
        {
            JSONFile = filename;
            string AllAPInfo = "";
            try
            {
                StreamReader sr = new StreamReader(filename);
                AllAPInfo = sr.ReadToEnd();
                sr.Close();
            }
            catch
            {
                Console.WriteLine("Can't read file " + filename);
                DeleteJSONFile();
                return;
            }

            try
            {
                cAP AP = JsonConvert.DeserializeObject<cAP>(AllAPInfo);
                PARAMETERS = AP.PARAMETERS;
                STATIONS = AP.STATIONS;
                JSON = AP.GetJSON();
                AP_Name = JSON.Substring(JSON.IndexOf("\"VALUE\": \""), JSON.IndexOf("}")).Split("\"")[3];
            }
            catch
            {
                Console.WriteLine("Can't deserialize JSON for file " + JSONFile);
                //DeleteJSONFile();
            }
        }

        public void GetDataFromDevice(string host, int port, string username, string passwd)
        {
            string ap_info = "";
            string st_info = "";
            try
            {
                cSSHClient APssh = new cSSHClient(host, port, username, passwd);
                int c = APssh.Connect();

                if (c != 1)
                {
                    DeleteJSONFile();
                    Console.WriteLine("Can't connect to "+host);
                    return;
                }


                //Getting AP parameters
                try
                {
                    ap_info = APssh.GetCommandResult("mca-status");
                }
                catch
                {
                    Console.WriteLine("Can't connect to host " + host + " by ssh\n");
                    DeleteJSONFile();
                }

                List<string> lines = new List<string>();

                string[] tmp = ap_info.Split(new[] { '\r', '\n', ',' });
                foreach (string line in tmp)
                    lines.Add(line);

                for (int i = 0; i < lines.Count; i++)
                {
                    if (lines[i] != "")
                    {
                        cParameter P = new cParameter();
                        P.PARAMETERNAME = lines[i].Split("=")[0];
                        P.VALUE = lines[i].Split("=")[1];
                        P.GetParameterType();
                        PARAMETERS.Add(P);
                        if (P.PARAMETERNAME == "deviceName")
                            AP_Name = P.VALUE;
                    }
                }

                //Getting all stations parameters
                st_info = APssh.GetCommandResult("wstalist");
                APssh.Disconnect();

                var Sts = JsonConvert.DeserializeObject(st_info);
                JArray JStations = (JArray)Sts;
                foreach (JToken JT in JStations)
                {
                    cStation S = new cStation();
                    try
                    {
                        S.NAME = JT.SelectToken("remote").SelectToken("hostname").ToString();
                    }
                    catch (System.Exception)
                    {
                        S.NAME = JT.SelectToken("name").ToString();
                    }


                    STATIONS.Add(S);

                    foreach (JProperty child in JT.Children())
                    {

                        if ((child.Name != "signals") && (child.Name != "airmax") && (child.Name != "stats") && (child.Name != "rates"))
                        {
                            cParameter P = new cParameter();
                            P.PARAMETERNAME = child.Name;
                            P.VALUE = child.Value.ToString();
                            P.GetParameterType();
                            S.PARAMS.Add(P);
                        }
                        else if (child.Name == "signals")
                        {
                            int i = 0;
                            JArray JA = (JArray)child.Value;
                            foreach (JToken jtkn in JA)
                            {
                                cParameter P = new cParameter();
                                P.PARAMETERNAME = "MSC" + i.ToString();
                                P.VALUE = jtkn.ToString();
                                P.GetParameterType();
                                S.PARAMS.Add(P);
                                i++;
                            }
                        }
                        else if (child.Name == "airmax")
                        {
                            foreach (JProperty ch in child.Value)
                            {
                                cParameter P = new cParameter();
                                P.PARAMETERNAME = "AirMAX_" + ch.Name;
                                P.VALUE = ch.Value.ToString();
                                P.GetParameterType();
                                S.PARAMS.Add(P);
                            }
                        }
                        else if (child.Name == "stats")
                        {
                            foreach (JProperty ch in child.Value)
                            {
                                cParameter P = new cParameter();
                                P.PARAMETERNAME = ch.Name;
                                P.VALUE = ch.Value.ToString();
                                P.GetParameterType();
                                S.PARAMS.Add(P);
                            }
                        }
                    }
                }
            }
            catch
            {
                Console.WriteLine("Can't connect to " + host + " or can't get command result\n");
                DeleteJSONFile();
            }
        }

        public string GetJSON()
        {
            JSON = JsonConvert.SerializeObject(this, Formatting.Indented);
            return JSON;
        }

        public string GetJSONReplaceNames()
        {
            foreach (cStation S in STATIONS)
                S.NAME = AP_Name + "." + S.NAME;

            return this.ReplaceNames(this.GetJSON().ToString());
        }


        public string GetJSONStationsDiscover()
        {
            List<cStationNoParameters> LSNP = new List<cStationNoParameters>();
            foreach (cStation cS in STATIONS)
            {
                cStationNoParameters SNP = new cStationNoParameters();
                SNP.NAME = cS.NAME;
                LSNP.Add(SNP);
            }

            string s = JsonConvert.SerializeObject(LSNP, Formatting.Indented);
            s = s.Replace("\"PARAMETERNAME\": \"", "\"{#PARAMETERNAME}\": \"");
            return s;
        }

        public string GetJSONOwnParamsDiscover()
        {
            List<cParameterNoValue> LPNV = new List<cParameterNoValue>();
            foreach (cParameter P in PARAMETERS)
            {
                cParameterNoValue PNV = new cParameterNoValue();
                PNV.PARAMETERNAME = P.PARAMETERNAME;
                PNV.TYPE = P.TYPE;
                LPNV.Add(PNV);
            }

            string s = JsonConvert.SerializeObject(LPNV, Formatting.Indented);
            s = s.Replace("\"PARAMETERNAME\": \"", "\"{#PARAMETERNAME}\": \"");
            s = s.Replace("\"TYPE\": \"", "\"{#TYPE}\": \"");
            return s;
        }

        public string GetJSONOwnParamsValues()
        {
            string s = "{\n";
            foreach (cParameter P in PARAMETERS)
                s += "\t\"" + P.PARAMETERNAME + "\": \"" + P.VALUE + "\",\n";
            s = s.Substring(0, s.Length - 2);
            s += "\n}";
            return s;
        }

        public string GetJSONStationParametersDiscover(string station_name)
        {
            string s = "";
            foreach (cStation S in STATIONS)
                if (S.NAME == station_name)
                    s = S.GetJSONParamsDiscover();
            return s;
        }

        public string GetJSONStationParametersValues(string station_name)
        {
            string s = "";
            bool is_exist = false;
            foreach (cStation S in STATIONS)
                if (S.NAME == station_name)
                {
                    s = S.GetJSONParamsValues();
                    is_exist = true;
                }
            if (!is_exist)
                Console.WriteLine("No station found with name \"" + station_name + "\" in AP \"" + AP_Name + "\"");
            return s;
        }

        public string ReplaceNames(string str)
        {
            string s = str;
            s = s.Replace("\"NAME\": \"", "\"{#NAME}\": \"");
            s = s.Replace("\"PARAMETERNAME\": \"", "\"{#PARAMETERNAME}\": \"");
            s = s.Replace("\"TYPE\": \"", "\"{#TYPE}\": \"");
            s = s.Replace("\"VALUE\": \"", "\"{#VALUE}\": \"");
            return s;
        }

        public void PrintJSONToFile(string filename)
        {
            this.GetJSON();
            bool write_success = false;
            try
            {
                StreamWriter sr = new StreamWriter(filename);
                sr.WriteLine(JSON);
                sr.Close();
                write_success = true;
            }
            catch
            {
                Console.WriteLine("Can't write to file " + filename);
                DeleteJSONFile();
            }
        }
    }

}