using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Reflection;
using System.IO;
using System.Globalization;
using System.Windows.Forms;
using System.Runtime.Remoting;
using System.Security.AccessControl;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Remoting.Channels;
using System.Collections;

namespace HitchHiker
{
    public class Listener 
    {
        static bool s_AlreadyHooked = false;
        ILogger s_Logger = Logger.Instance();
        private int m_Port = 60000;//default
        private string m_CallerDirectory;
        private string m_CallerExe;
        private string m_TestDllPath;
        private Hashtable m_Table;

        IComChannel m_remoteObject = null;
        public void Listen()
        {
            try
            {
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
                if (s_AlreadyHooked == true)
                    return;
                s_AlreadyHooked = true;


                //Connect to server at port 60200 and get the port number at which to open service
                TcpClientChannel clientChannel = new TcpClientChannel("HitchHikerClient", null);
                ChannelServices.RegisterChannel(clientChannel, true);
                m_remoteObject = (IComChannel)Activator.GetObject(
                    typeof(IComChannel), "tcp://localhost:60200/HitchHiker.rem");
                if (m_remoteObject == null)
                    s_Logger.LogError("Connection to HitchHiker server could not be established");

                SyncDataFromServer(m_remoteObject);

                s_Logger.LogInfo("The port sent from server is " + m_Port.ToString(CultureInfo.CurrentCulture));
                
                //now start thread to listen to client
                object param = (object)m_Port;
                Thread clientListenerThread = new Thread(new ParameterizedThreadStart(StartClientServer));
                clientListenerThread.Start(param);

                s_Logger.LogInfo("Server has been started at client side");

            }
            catch (Exception e)
            {
            }

        }

        Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            string hitchikerDll = m_CallerDirectory + "\\HitchHiker.dll";
            Assembly asm = Assembly.LoadFile(hitchikerDll);
            return asm;
        }
        private IntPtr GatHandle(string windowName, out uint procId)
        {

            WindowFinder windowFinder = new WindowFinder();
            IntPtr hwnd = windowFinder.FindWindowAsynch(windowName, 10000, out procId);
            return hwnd;
        }
        void RunTests(List<Type> testTobeRun)
        {
            foreach (Type type in testTobeRun)
            {
                object[] formNames = type.GetCustomAttributes(typeof(TargetForm), true);
                if(formNames.Length==0)
                    continue;
                TargetForm targetform=formNames[0] as TargetForm;
                string formName=targetform.FormName;
                uint procId;
                IntPtr handle = GatHandle(formName, out procId);
                object formObj = Control.FromHandle(handle);
                if (formObj == null)
                {
                    m_remoteObject.TestFixtureError(formName+" Window Could not be Found");
                    continue;
                }
                object obj = Activator.CreateInstance(type);

                FieldInfo formInstance = type.GetField("m_Form");
                formInstance.SetValue(obj, formObj);

                FieldInfo channel = type.GetField("m_TestChannel",BindingFlags.NonPublic|BindingFlags.Instance);
                channel.SetValue(obj, m_remoteObject);

                MethodInfo[] testCases = type.GetMethods(BindingFlags.Instance | BindingFlags.Public);
                foreach (MethodInfo function in testCases)
                {
                    object[] attributes = function.GetCustomAttributes(typeof(HTestCase), true);
                    if (attributes.Length > 0)
                    {
                        try
                        {
                            function.Invoke(obj, null);
                        }
                        catch (Exception e)
                        {
                            m_remoteObject.Assert(false, type.Name+"."+function.Name+"::"+e.Message);
                        }
                    }

                }
            }
            m_remoteObject.TestFinished();
        }
        private void StartClientServer(object obj)
        {
            uint currentProcId = Win32APIs.GetCurrentProcessId();
            if (m_Table == null || m_TestDllPath==null)
                return;
            List<string> testCases = (List<string>)m_Table[currentProcId];
            List<Type> testTobeRun = new List<Type>();
            if (testCases == null || testCases.Count == 0)
                return;

            Assembly asm = Assembly.LoadFile(m_TestDllPath);
            foreach (string item in testCases)
            {
                Type t = asm.GetType(item);
                testTobeRun.Add(t);
            }
            RunTests(testTobeRun);
        }
        private void  SyncDataFromServer(IComChannel remoteObject)
        {
            try
            {
                m_CallerDirectory = remoteObject.GetClientDirectory();
                m_CallerExe = remoteObject.GetCallingExe();
                m_Table = remoteObject.GetTable();
                m_TestDllPath = remoteObject.GetTestDllPath();
                Trace.WriteLine("data was queried successfully from server");
            }
            catch (Exception ex)
            {
                //we can recover from this error
                s_Logger.LogInfo("Could not retriev data from Sniper server" + ex.ToString());
            }
        }
    }
}
