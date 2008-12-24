using System;
using System.Collections.Generic;
using System.Text;
using NTLM.Account;

namespace SvnAccount.PwdGenDen
{
    class Program
    {
        static void Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);
            
            if (args.Length < 2)
            {
                showUsage();
                Promote();
            }
            else
            { 
                Process(args[0] + " " + args[1]);
                DoAgain();
            }

            Console.Read();
        }

        static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine("Error input data...");
            if (e.IsTerminating)
            {
                Console.WriteLine("Terminating...");
            }
        }

        static void showUsage()
        {
            Console.WriteLine("usage:");
            Console.WriteLine(new string('-', 50));
            Console.WriteLine("g originalString [Encrypto]");
            Console.WriteLine("d decryptedString [Decrypto]");
        }

        static void Process(string strToDo)
        {
            if (strToDo.Length < 2)
            {
                showUsage();
            }
            else
            {
                
                if (strToDo.StartsWith("g "))
                {
                    Console.WriteLine("Result:" + new SymmetricMethod().Encrypto(strToDo.Substring(2)));
                }
                else if (strToDo.StartsWith("d "))
                {
                    Console.WriteLine("Result:" + new SymmetricMethod().Decrypto(strToDo.Substring(2)));
                }
                else
                {
                    showUsage();
                }
            }
        }

        static void Promote()
        {
            string strToDo = Console.ReadLine();
            if (strToDo.Length < 2)
            {
                showUsage();
            }
            else
            {
                Process(strToDo);
            }
            DoAgain();
        }

        static void DoAgain()
        {
            Console.WriteLine();
            Console.WriteLine("Redo, type 'y'/'n' ?"); 
            string strAgain = Console.ReadLine();
            if (string.Compare(strAgain, "y", true) == 0)
            {
                Promote();
            }
        }
    }
}
