using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace HitchHiker
{
    public interface ITestChannel
    {
        void Assert(bool passed, string message);
        void TestFinished();
        void TestFixtureError(string message);
    }
    public interface IComChannel:ITestChannel
    {
        string GetClientDirectory();
        string GetCallingExe();
        string GetTestDllPath();
        Hashtable GetTable();
    }

   
    public interface ILogger
    {
        void LogError(string error);
        void LogInfo(string message);
        void LogTestCaseStatus(bool passed, string message);
    }

}
