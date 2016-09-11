using HitchHiker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HConsole
{
    class TestDiagnostics
    {
        static object m_Singletonsynch = new object();
        int m_PassedTestCount;
        int m_FailedTestCount;
        volatile int m_FinishedFromProcessCount;
        volatile int m_TotalProcessHooked;
        public int PassedTC
        {
            get { return m_PassedTestCount; }
        }
        public int FailedTC
        {
            get { return m_FailedTestCount; }
        }
        public int TotalProcess
        {
            get { return m_TotalProcessHooked; }
            set { m_TotalProcessHooked = value; }
        }

        object m_LogSynch = new object();
        private static TestDiagnostics s_TestTracker = null;
        ILogger m_Logger = Logger.Instance();
        public void TestPass(string message)
        {
            m_PassedTestCount++;
            m_Logger.LogTestCaseStatus(true, message);
        }
        public void TestFail(string message)
        {
            m_FailedTestCount++;
            m_Logger.LogTestCaseStatus(false, message);

        }
        public void TestFixtureError(string message)
        {
            m_Logger.LogError( message);

        }
        public bool TestsRunning
        {
            get 
            {
                if (m_FinishedFromProcessCount< m_TotalProcessHooked )
                    return true;//Tests are still running
                return false;
            }
        }
        /// <summary>
        /// Every time a process runjng our tests is done runnin, it will cal this
        /// When all process have called this, m_FinishedFromProcessCount will be equal to number of process hooked 
        /// </summary>
        /// <returns></returns>
        public int IncrementTestFinished()
        {
             ++m_FinishedFromProcessCount;
             m_Logger.LogInfo(m_FinishedFromProcessCount.ToString() + "of " + m_TotalProcessHooked .ToString()+ "Process have finished runnig tests");
             return m_FinishedFromProcessCount;
        }
        public static TestDiagnostics Instance()
        {
            if (s_TestTracker != null)
                return s_TestTracker;
            lock (m_Singletonsynch)
            {
                if (s_TestTracker == null)
                {
                    s_TestTracker = new TestDiagnostics();
                }
            }
            return s_TestTracker;
        }
    }
}
