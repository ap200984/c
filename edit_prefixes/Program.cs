using System;

namespace EditPrefixes
{
    class Program
    {
        static void Main(string[] args)
        {
            DateTime dt_start = DateTime.Now;
            cArgs Args = new cArgs(args);
            if (args.Length==0)
            {
                Console.WriteLine("Программа предназначена для получения конфигурации, которую можно будет применить на роутере для актуализации префикс-листов.");
                Console.WriteLine("Для запуска нужно после параметра -r указать имя роутера: напр. ./edit_prefixes -r bravo92");
            }
            else if ((Args.Contains("-r"))&&(Args.GetNextArgAfter("-r")!=""))
            {
                //cAuth auth = new cAuth();
                //string login = auth.login;
                //string pass = auth.password;            

                string login="script_ro";
                string pass="g]^vZ~t7Gb\"Nth[M";

                cRouter R1 = new cRouter(Args.GetNextArgAfter("-r"),22,login,pass);
                //cRouter R1 = new cRouter("86.110.170.70",32757,"a.popov","iQWyYj2w");
                
                R1.GetASSETs();

                DateTime dt_end = DateTime.Now;
                Console.WriteLine("Время выполнения "+(dt_end-dt_start).Minutes.ToString()+"m "+(dt_end-dt_start).Seconds.ToString()+"c. Начинаю сравнивать...");
                R1.GetDiffs();
                R1.PrintConfig();
                R1.PrintASSETsToFile();

                //dt_end = DateTime.Now;
                //Console.WriteLine("Время выполнения "+(dt_end-dt_start).Minutes.ToString()+"m "+(dt_end-dt_start).Seconds.ToString()+"c. Заливаю на роутер...");
                //R1.CorrectASSETs();

                if (!R1.NeedToMakeChanges())
                    Console.WriteLine("\nПрефиксы актуальны. Изменений не требуется");

                dt_end = DateTime.Now;
                Console.WriteLine("\nГотово, ёпта!");                
                Console.WriteLine("Общее время выполнения "+(dt_end-dt_start).Minutes+" мин "+(dt_end-dt_start).Seconds+" с.");
            }
        }
    }
}
