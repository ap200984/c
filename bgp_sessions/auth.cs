using System;
using System.IO;
using System.Collections.Generic;
using System.Net;

namespace BGP_sessions2
{
    public class cAuth
    {
        public string login;
        public string password;
       

        public cAuth()
        {
            Console.WriteLine("Enter login:");
            login = Console.ReadLine();

            string pass = "";                    
            ConsoleKeyInfo key;
                        
            Console.WriteLine("Enter password:");
            do
            {
                key = Console.ReadKey(true);

                // Backspace Should Not Work
                if (key.Key != ConsoleKey.Backspace)
                {
                    pass += key.KeyChar;
                    Console.Write("*");
                }
                else
                {
                    Console.WriteLine();
                }
            }
            // Stops Receving Keys Once Enter is Pressed
            while (key.Key != ConsoleKey.Enter);                 
                        
            pass=pass.Substring(0,pass.Length-1);
            password = pass;
            Console.WriteLine();
        }
    }

}