using HitchHiker;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Text;
using System.Threading;

namespace HConsole
{
    class TcpServer
    {
        AutoResetEvent m_ServerStarted = new AutoResetEvent(false);
        Thread m_ServerThread = null;
        volatile Hashtable  m_Table = null;
        volatile string m_TestDllPath = "";
        static ILogger s_Logger;
        int m_Timeout;
        volatile static TcpServerChannel channel = new TcpServerChannel("HitchHikerService", 60200);
        static TcpServer()
        {
            ChannelServices.RegisterChannel(channel, true);
            s_Logger = Logger.Instance();

        }
        public TcpServer(int timeout,Hashtable table,string testDllPath)
        {
            m_Timeout = timeout;
            m_Table = table;
            m_TestDllPath = testDllPath;
        }
        public void  Listen()
        {
            //Start a thread and wait for hooked clients to connect so that we can tell them which port to use
            m_ServerThread = new Thread(Threadproc);
            m_ServerThread.Start();
            m_ServerStarted.WaitOne(m_Timeout);
        }


        public void Threadproc()
        {
            TestRunnerInfo data = null;
            try
            {
                data = new TestRunnerInfo(m_Table, m_TestDllPath);

                RemotingServices.Marshal(data, "HitchHiker.rem");
                m_ServerStarted.Set();
                s_Logger.LogInfo("TcpServerInfo is listening  ");

                while (true)
                {
                    Thread.Sleep(2000);
                }
            }
            catch (ThreadAbortException e)
            {
                s_Logger.LogInfo("TcpServerInfo:Aborting the thread ");
            }
            catch (Exception e)
            {
                s_Logger.LogError("TcpServerInfo:ThreadProc error  " + e.Message);

            }
            finally
            {
                m_ServerStarted.Set();
                RemotingServices.Disconnect(data);
            }
        }
        public void StopListening()
        {
            try
            {
                m_ServerThread.Abort();
            }
            catch (Exception e)
            {
                s_Logger.LogError("TcpServerInfo:StopListening error while aborting thread");

            }
        }

        #region IDisposable Members

        private bool m_DisposeCalled = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        public void Dispose(bool disposing)
        {
            if (!m_DisposeCalled)
            {
                if (disposing)
                {
                    m_ServerStarted.Close();
                }

            }
            m_DisposeCalled = true;
        }


        ~TcpServer()
        {
            Dispose(false);
        }

        #endregion

    }
    class TestRunnerInfo : MarshalByRefObject, IComChannel
    {
        private string m_ClientDirectory = "";
        private string m_CallingExe = "";
        private string m_testDll = "";

        Hashtable m_procIdToForms=new Hashtable();

        public Hashtable HTable
        {
            get{return m_procIdToForms;}
        }
        public TestRunnerInfo(Hashtable table,string testDll)
	    {
            m_procIdToForms=table;
            m_testDll = testDll;
	    }
        

        public string GetClientDirectory()
        {
            if (m_ClientDirectory == "")
                m_ClientDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            return m_ClientDirectory;
        }

        public string GetCallingExe()
        {
            if (m_CallingExe == "")
                m_CallingExe = Assembly.GetExecutingAssembly().Location;
            return m_CallingExe;
        }
        public Hashtable GetTable()
        {
            return m_procIdToForms;
        }
        public string GetTestDllPath()
        {
            return m_testDll;
        }
        public void  Assert(bool passed, string message)
        {
            if (passed)
                TestDiagnostics.Instance().TestPass(message);
            else
                TestDiagnostics.Instance().TestFail(message);

        }
        public void TestFinished()
        {
            TestDiagnostics.Instance().IncrementTestFinished();
        }
        public void TestFixtureError(string message)
        {
            TestDiagnostics.Instance().TestFixtureError(message);
        }

    }
}
