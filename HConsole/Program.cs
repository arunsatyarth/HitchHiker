using HitchHiker;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace HConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            string testDllPath = "";
            bool filePresent = false;
            bool firstTry = true;
            Assembly asm=null;
            Logger.Instance().LogInfo("Starting Test Server");
#if DEBUG
            args = new string[1];
            args[0]=@"K:\Apps\HitchHiker\Bin\Test.dll";
#endif
            while (!filePresent)
            {
                if (args.Length == 0 || !firstTry)
                {
                    Console.WriteLine("Enter the path of Test dll");
                    testDllPath = Console.ReadLine();
                }
                else if (args.Length == 1)
                {
                    testDllPath = args[0];
                }
                else
                {
                    firstTry = false;
                    continue;
                }
                firstTry = false;
                FileInfo file=new FileInfo(testDllPath);
                if(file.Exists)
                {
                    try 
	                {	        
                        asm=Assembly.LoadFile(testDllPath);
                        filePresent = true;
	                }
	                catch (Exception e)
	                {
                        continue;
	                }
                }
            }
            Executor executor = new Executor(asm);
            executor.Execute();
            Console.ReadKey();
        }
    }
}
