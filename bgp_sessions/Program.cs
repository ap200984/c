using System;
using System.IO;
using System.Net;
using System.Collections.Generic;
//using System.Net.Http;
//using System.Diagnostics;
//using System.Xml.Serialization;

namespace BGP_sessions2
{
    class Program
    {
        static void Main(string[] args)
        { 
            cArgs Args = new cArgs(args);
            cRouters  Routers = new cRouters("admin_cookies.txt");

            string login="script_ro";
            string pass="g]^vZ~t7Gb\"Nth[M";

            /*
            if ( (Args.Contains("-r")) || (Args.Contains("-d")) )
            {
                cAuth auth = new cAuth();
                login = auth.login;
                pass = auth.password;
                //login = "a.popov";
                //pass = "***";

                if (Args.Contains("-d"))
                {
                    //string RouterList = "bravo8,bravo91,bravo92,bravo15,bravo14,bravo81,bravo82,bravo11,bravo12";                    
                    string RouterList = "bravo91";
                    Routers.InitFromList(RouterList,22,login,pass);                    
                }
            }

            if (args.Length==0)
            {
                Console.WriteLine("Программа предназначена для получения доп. информации о bgp сессиях из профилей клиентов и из описания сессий на роутерах.");
                Console.WriteLine("При отсутствии параметров список нерабочих сессий будет считан из файла sessions.txt,");
                Console.WriteLine("результат будет выведен в консоль и в файл bgp_sessions.txt, описания будут получены только из профилей клиентов.");                
                Console.WriteLine("Доступные параметры:");                
                Console.WriteLine("-f: после флага нужно указать имя файла со списком нерабочих сессий (вместо sessions.txt)");
                Console.WriteLine("-r: При отсутсвии информации о сессиях в профилях клиентов будет произведен поиск описаний сессий на роутерах");
                Console.WriteLine("-d: информация о нерабочих сессиях будет получена по SSH непосредственно с роутеров. Файл со списокм сессий использоваться не будет");
                Console.WriteLine("-w: вывод результата быдет произведен в указанный за флагом файл");
                Console.WriteLine("-m: Отправить результат по почте на указанные за этим ключом ящик.");
                Console.WriteLine("Советую запускать так: -d -m mpls@ddos-guard.net");
            }

            string sessions_filename="sessions.txt";

            if (args.Length==0)
                Routers.GetBGPSessionsFromFile(sessions_filename);

            if (Args.Contains("-f")&&(!Args.Contains("-d")))
                Routers.GetBGPSessionsFromFile(Args.GetNextArgAfter("-f"));

            if ((!Args.Contains("-d"))&&(!Args.Contains("-r")))
                Routers.GetBGPSessionsFromFile(sessions_filename);
/*
            cRouter MyEVERoter = new cRouter();
            MyEVERoter.Name = "86.110.170.70";
            MyEVERoter.Port = 32757;
            MyEVERoter.login = "a.popov";
            MyEVERoter.passwd ="iQWyYj2w";

            List<cRouter> LR = new List<cRouter>();
            LR.Add(MyEVERoter);

            Routers.InitFromListManually(LR);
            Routers.GetBGPSessionsFromSSH();
*/
            string RouterList = "bravo8,bravo91,bravo92,bravo16,bravo14,bravo81,bravo82,bravo11,bravo12";                    
            //string RouterList = "bravo91";
            Routers.InitFromList(RouterList,22,login,pass);

            Console.WriteLine("\nПолучаю данные из профилей клиентов на сайте admin.ddos-guard.net...");
            Routers.GetDataFromWEBAdmin();
            Console.WriteLine();
            Console.WriteLine();

            //if ((Args.Contains("-r"))||(Args.Contains("-d")))
                Routers.TryGetInfoFromDescriptionsUsingAllConfig();                    

            Routers.Print();

            if (Args.Contains("-w"))
            {
                string filename = (Args.GetNextArgAfter("-w"));
                if (filename!="no args after")
                    try
                    {
                        Routers.PrintToFile(filename);
                    }
                    catch
                    {
                        Console.WriteLine("Не могу создать файл "+filename);
                    }
                else
                {
                    Console.WriteLine("Не указано имя файла после аргумента -w. Пишу в bgp_sessions.txt");
                    Routers.PrintToFile("bgp_sessions.txt");
                }
            }
            else
                Routers.PrintToFile("bgp_sessions.txt");

            if (Args.Contains("-m"))
            {                
                string Result = Routers.PrintToString();
                if (login=="")
                {
                    cAuth auth = new cAuth();
                    login = auth.login;
                    pass = auth.password;
                }
                string _to=Args.GetNextArgAfter("-m");
                if (_to=="no args after")
                    _to=login+"@ddos-guard.net";
                cSendMail Mail = new cSendMail(login+"@ddos-guard.net",_to, login, pass,"BGP sessions extensive",Result);
            }
        }
    }
}
