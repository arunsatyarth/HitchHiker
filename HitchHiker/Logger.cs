using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace HitchHiker
{
    public class Logger:ILogger
    {
        static object m_Singletonsynch=new object();
        object m_LogSynch=new object();
        private static  ILogger s_LoggerObject = null;
        public void LogError(string error)
        {
            lock (m_LogSynch)
            {
                ConsoleColor previousColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.Magenta;
                Console.WriteLine(error);
                Console.ForegroundColor = previousColor;

            }
        }
        public void LogInfo(string error)
        {
            lock (m_LogSynch)
            {
                ConsoleColor previousColor = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine(error);
                Console.ForegroundColor = previousColor;

            }
        }
        public void LogTestCaseStatus(bool passed,string message)
        {
            lock (m_LogSynch)
            {
                ConsoleColor previousColor = Console.ForegroundColor;
                if (passed)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGreen;
                    Console.WriteLine(message);
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.DarkRed;
                    Console.WriteLine(message);
                }
                Console.ForegroundColor = previousColor;

            }
        }
        private Logger()
        {
        }
        //Singleton object
        public static ILogger Instance()
        {
            if(s_LoggerObject!=null)
                return s_LoggerObject;
            lock (m_Singletonsynch)
            {
                if(s_LoggerObject==null)
                {
                    s_LoggerObject = new Logger();
                }
            }
            return s_LoggerObject;
        }
    }
}
