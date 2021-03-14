using System;
using System.IO;
using System.Collections.Generic;
using System.Text;


namespace csv2vcf
{
    public class cContact
    {
        public string Name;
        public string Number;

        public string tryparsename(string s)
        {
            string res = "";
            try
            {
                res = s.Split(": ")[1];
            }
            catch
            {
            }
            return res;
        }
        
        public string DecodeToUtf8(string unicodeString)
        {
            string res="";
            Byte[] encodedBytes = Encoding.UTF8.GetBytes(unicodeString);
            for (int ctr = 0; ctr < encodedBytes.Length; ctr++)
                res+=String.Format("{0:X2} ", encodedBytes[ctr]);

            res=res.TrimEnd();
            res=res.Replace(" ","=");
            res="="+res;
            
            return res;
        }
    }



    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            StreamReader sr = new StreamReader("papa-unicode.txt");
            string text = sr.ReadToEnd();
            sr.Close();

            List<string> lines = new List<string>();

            string[] tmp = text.Split(new[] { '\r', '\n', ',' });
            foreach (string line in tmp)
                if (line != "")
                    lines.Add(line);

            List<cContact> Contacts = new List<cContact>();


            for (int i = 0; i < lines.Count; i++)
            {
                if ((lines[i].IndexOf("Фамилия:") > -1) || (lines[i].IndexOf("мя:") > -1) || (lines[i].IndexOf("Компания:") > -1))
                {
                    cContact C = new cContact();


                    while (lines[i].IndexOf("телефон:") == -1)
                    {
                        C.Name += C.tryparsename(lines[i]) + " ";
                        i++;
                    }
                    if (C.Name.Substring(C.Name.Length - 1) == " ")
                        C.Name = C.Name.Substring(0, C.Name.Length - 1);
                    C.Name = C.DecodeToUtf8(C.Name);
                    C.Number = lines[i].Split(" ")[2];
                    Contacts.Add(C);

                }
            }

            string pre_name = "BEGIN:VCARD\r\nVERSION:2.1\r\nN;CHARSET=UTF-8;ENCODING=QUOTED-PRINTABLE:";
            string past_name = ";;;";
            string pre_tel = "TEL;CELL:";
            string str_end = "END:VCARD";


            StreamWriter sw = new StreamWriter("papa.vcf");
            foreach (cContact C in Contacts)
            {
                sw.WriteLine(pre_name + C.Name + past_name + "\r\n" + pre_tel + C.Number + "\r\n" + str_end);
            }
            sw.Close();









        }
    }
}
