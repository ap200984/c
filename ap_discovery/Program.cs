using System;
using System.IO;
using System.Collections.Generic;

namespace ap_discovery
{
    class Program
    {
        public static void ShowInfo()
        {
            Console.WriteLine("To get all data from AP use 5 parameters: ap_discovery [ap_name]||[IP] [port] [login] [paswd] get_all");
            Console.WriteLine("To discover stations:                     ap_discovery [ap_namne]|[IP] ap_stations_discover");
            Console.WriteLine("To discover own parameters:               ap_discovery [ap_namne]|[IP] ap_parameters_discover");
            Console.WriteLine("To get own parameters values:             ap_discovery [ap_namne]|[IP] ap_get_parameters");
            Console.WriteLine("To discover station parameters:           ap_discovery [ap_namne]|[IP] [station_name] parameters_discover]");
            Console.WriteLine("To get station parameters values:         ap_discovery [ap_namne]|[IP] [station_name] get_parameters");
            Console.WriteLine("You can use sybbol ! as first sybmol of a first parameter if You want it to be ignored");

        }

        static void Main(string[] args)
        {
            try
            {

                if ((args.Length == 0) || (args.Length == 1) || (args.Length > 5))
                {
                    ShowInfo();
                    return;
                }

                List<string> ARGS = new List<string>();
                foreach (string s in args)
                    ARGS.Add(s);
                if (ARGS[0].StartsWith("!"))
                    ARGS.RemoveAt(0);


                string filename = "/tmp/" + ARGS[0] + ".json";
                if (ARGS[1].IndexOf(".") > -1)
                    ARGS[1] = ARGS[1].Split(".")[1];

                if ((ARGS.Count != 2) && (ARGS.Count != 3) && (ARGS.Count != 5))
                    ShowInfo();
                else if (ARGS.Count == 2)
                {
                    cAP AP = new cAP(filename);

                    if (ARGS[1] == "ap_parameters_discover")
                        Console.WriteLine(AP.GetJSONOwnParamsDiscover());

                    else if (ARGS[1] == "ap_get_parameters")
                        Console.WriteLine(AP.GetJSONOwnParamsValues());

                    else if (ARGS[1] == "ap_stations_discover")
                        Console.WriteLine(AP.GetJSONStationsDiscover());

                    else
                        ShowInfo();

                }
                else if (ARGS.Count == 3)
                {
                    cAP AP = new cAP(filename);

                    if (ARGS[2] == "parameters_discover")
                        Console.WriteLine(AP.GetJSONStationParametersDiscover(ARGS[1]));
                    else if (ARGS[2] == "get_parameters")
                        Console.WriteLine(AP.GetJSONStationParametersValues(ARGS[1]));
                    else
                    {
                        ShowInfo();
                    }
                }
                else if (ARGS[4] == "get_all")
                {
                    cAP AP = new cAP();
                    AP.GetDataFromDevice(ARGS[0], Convert.ToInt32(ARGS[1]), ARGS[2], ARGS[3]);
                    if (AP.PARAMETERS.Count > 0)
                    {
                        AP.PrintJSONToFile(filename);
                        Console.WriteLine(AP.GetJSONReplaceNames());
                    }
                    else
                    {
                        Console.WriteLine("Can't get data for "+args[0]);
                        File.Delete(filename);
                    }
                }
            }
            catch
            {
                Console.WriteLine("Something went wrong:\nHere's the parameters:");
                foreach (string s in args)
                    Console.WriteLine(s);
                string filename = "/tmp/" + args[0] + ".json";
                File.Delete(filename);
            }
        }

    }
}
