using HitchHiker;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;

namespace HConsole
{
    class Executor
    {
        List<string> m_FormsToHook = new List<string>();
        Assembly m_testDll = null;
        ILogger m_Logger = null;
        Hashtable m_procIdToForms = new Hashtable();
        Hashtable m_procIdToTypes = new Hashtable();
        HashSet<uint> m_allredyHookedProcess = new HashSet<uint>();
        AutoResetEvent m_ThreadSynchronizer = new AutoResetEvent(false);

        public Executor(Assembly asm)
        {
            m_testDll = asm;
            m_Logger = Logger.Instance();
        }
        private int  IdentifyForms()
        {
            Type[] classes = m_testDll.GetTypes();
            int processCount = 0;
            foreach (Type type in classes)
            {
                object[] attributes = type.GetCustomAttributes(typeof(TargetForm), true);
                if (attributes.Length == 0)
                    continue;
                TargetForm formAttr = attributes[0] as TargetForm;
                if (formAttr != null)
                {
                    string formName = formAttr.FormName;
                    if (formName != null && formName.Length > 0)
                    {

                        int handle;
                        uint procId = GetProcId(formName, out handle);
                        if (procId <= 0)
                            continue;
                        if (m_procIdToTypes.Contains(procId))
                        {
                            List<string> forms = (List<string>)m_procIdToTypes[procId];
                            forms.Add(type.FullName);
                            m_procIdToTypes[procId] = forms;
                        }
                        else
                        {
                            List<string> types = new List<string>();
                            types.Add(type.FullName);
                            m_procIdToTypes[procId] = types;
                            processCount++;

                        }
                        if (!m_FormsToHook.Contains(formName))
                            m_FormsToHook.Add(formName);
                        
                    }
                }

            }
            return processCount;
        }
        private uint GetProcId(string windowName, out int handle)
        {

            uint procId;
            WindowFinder windowFinder = new WindowFinder();
            IntPtr hwnd = windowFinder.FindWindowAsynch(windowName, 10000, out procId);
            handle = hwnd.ToInt32();
            return procId;
        }

        private void HookProcess()
        {
            m_Logger.LogInfo("Going to Hook Process and Load Test Dlls");
            foreach (string windowName in m_FormsToHook)
            {
                int handle;
                uint procId = GetProcId(windowName, out handle);
                if (!m_allredyHookedProcess.Contains(procId))
                {

                    m_allredyHookedProcess.Add(procId);
                    //Inject DLL on the intended process
                    ProcessStartInfo processInfo = new ProcessStartInfo();
                    processInfo.Arguments = windowName;
                    processInfo.CreateNoWindow = true;
                    processInfo.WindowStyle = ProcessWindowStyle.Hidden;

                    processInfo.FileName = "Sniper.exe";

                    Process process = Process.Start(processInfo);
  
                }

            }
        }
        private void StartServer()
        {
            TcpServer tcp = new TcpServer(1000,m_procIdToTypes,m_testDll.Location);
            tcp.Listen();

        }

        public void Execute()
        {
            int numberOfProcess=IdentifyForms();
            if(m_FormsToHook.Count==0)
            {
                m_Logger.LogError("No Test cases have targeted any forms. Did you miss TargetForm attribute on your test case classes? ");
                return;
            }
            TestDiagnostics.Instance().TotalProcess = numberOfProcess;
            StartServer();
            HookProcess();
            m_Logger.LogError("Test Server Waiting...");

            //Now the TCpServerthread will take care
            while (TestDiagnostics.Instance().TestsRunning)
            {

                Thread.Sleep(1000);
            }

            m_Logger.LogInfo("All Test Cases Completed");
            if (TestDiagnostics.Instance().PassedTC>0)
                m_Logger.LogTestCaseStatus(true,"Test Cases Passed: " + TestDiagnostics.Instance().PassedTC.ToString());
            else
                m_Logger.LogInfo( "Test Cases Passed: " + TestDiagnostics.Instance().PassedTC.ToString());

            if (TestDiagnostics.Instance().FailedTC > 0)
                m_Logger.LogTestCaseStatus(false, "Test Cases Failed: " + TestDiagnostics.Instance().FailedTC.ToString());
            else
                m_Logger.LogInfo("Test Cases Failed: " + TestDiagnostics.Instance().FailedTC.ToString());


        }
    }
}
