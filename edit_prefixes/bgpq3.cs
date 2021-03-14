using System;
using System.Text;
using System.Net.Sockets;
using System.IO;
using System.Collections.Generic;
using System.Linq;

//Утилита bgpq3 использует стандарнтый TCP склиент для подключения к серверу whois.radb.net
//Вместо нее можно использовать два вызова whois: 
//1. Определить, какие ASN входят в соств AS-SET: whois -h whois.radb.net '!iAS-REGRU,1' (Параметр ",1" означает получить все вложенные ASN рекурсивно)
//2. Сформировав общий запрос, получить одним разом все префиксы во всех ASN: 
//
//whois -h whois.radb.net '!gas1213
//...[тут все ASN с добавлением !g впереди]...
//!gas1215
//!q'
//В программе вместо нажатия ENTER используется символ \n.
//Но для ручного запроса можно использовать только клавишу только ENTER после каждого ASN и !q перед закрытием команды апострофом


namespace EditPrefixes
{
    public static class bgpq3
    {
        private static string GetWhoisInformation(string whoisServer, string url)
        {
            StringBuilder stringBuilderResult = new StringBuilder();
            TcpClient tcpClinetWhois = new TcpClient(whoisServer, 43);
            NetworkStream networkStreamWhois = tcpClinetWhois.GetStream();
            BufferedStream bufferedStreamWhois = new BufferedStream(networkStreamWhois);
            StreamWriter streamWriter = new StreamWriter(bufferedStreamWhois);

            streamWriter.WriteLine(url);
            streamWriter.Flush();

            StreamReader streamReaderReceive = new StreamReader(bufferedStreamWhois);
            while (!streamReaderReceive.EndOfStream)
                stringBuilderResult.AppendLine(streamReaderReceive.ReadLine());

            tcpClinetWhois.Close();
            return stringBuilderResult.ToString();
        }

        private static List<string> GetAllASNsFromRADB(string as_set)
        {
            string raw_output= GetWhoisInformation("whois.radb.net","!i"+as_set+",1");
            string[] lines = raw_output.Split(new [] { '\r', '\n' });
            
            List<string> LS = new List<string>();
                        
            string[] ASNs = lines[1].Split(" ");
            foreach (string ASN in ASNs)
                if (!LS.Contains(ASN))
                    LS.Add(ASN);
            
            return LS;
        }

        public static void PrintList(List<string> LS)
        {
            foreach (string s in LS)
                Console.WriteLine(s);
            Console.WriteLine(LS.Count);
        }       

        public static void PrintToFile(List<string> LS, string filename)
        {
            StreamWriter sr = new StreamWriter(Path.Combine(filename));
            foreach (string s in LS)
                sr.WriteLine(s);
            sr.Close();
        }

        public static List<string> GetPrefixes(string s)
        {
            string RADBRequest="!!\n";

            List<string> LS = new List<string>();
            
            
            if (s.IndexOf("AS-")==-1) //Then it is just an ASN
            {
                RADBRequest+="!g"+s+"\n";         
            

            }
            else //It is an AS-SET
            {
                List<string> ASNs = GetAllASNsFromRADB(s); 

                /* Removing Pravate ASNs */
                if (ASNs.Contains("AS23456"))
                    ASNs.Remove("AS23456");

                for (int j=0; j<ASNs.Count; j++)
                    if ( (Convert.ToInt32(ASNs[j].Substring(2))>=64496) &&
                        (Convert.ToInt32(ASNs[j].Substring(2))<=131071) )
                    {                    
                        //Console.WriteLine(ASNs[j]);
                        ASNs.RemoveAt(j);
                        j--;
                    }

                foreach(string ASN in ASNs)
                    RADBRequest+="!g"+ASN+"\n";

            }

            
            RADBRequest+="!q\n";
            
            string RADBResponce=GetWhoisInformation("whois.radb.net",RADBRequest);
            StreamWriter sr = new StreamWriter(Path.Combine("RADBResponce.txt"));
            sr.WriteLine(RADBResponce);
            sr.Close();

            string[] lines = RADBResponce.Split(new char[] { '\r', '\n' });

            List<string> lines_with_prefixes = new List<string>();

            foreach (string line in lines)
                if ( (!line.StartsWith("A")) && 
                     (!line.StartsWith("C")) &&
                     (!line.StartsWith("D")) &&
                     (line!="") )
                    lines_with_prefixes.Add(line);
                        
            foreach (string line in lines_with_prefixes)
                {
                    string[] prefixes = line.Split(" ");
                    foreach (string prefix in prefixes)
                        LS.Add(prefix);
                }
            
            LS.Sort();
            LS = LS.Distinct().ToList();

            return LS;
        }
    }
}
