using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows.Forms;

namespace HitchHiker
{
    public class TargetForm : Attribute
    {
        public string m_FormName;
        public TargetForm(string str)
        {
            m_FormName = str;
        }
        public string FormName{
            get { return m_FormName; }
        }
    }
    public class HTestCase : Attribute
    {
    }
    public class HTestCaseBase
    {
        public Form m_Form;
        protected ITestChannel m_TestChannel;
        public Form CurrentForm
        {
            get { return m_Form; }
        }
        public void Assert(bool b, string message)
        {
            StackFrame frame=new StackFrame(1);
            string functionaname = frame.GetMethod().ReflectedType.Name +"."+ frame.GetMethod().Name;
            m_TestChannel.Assert(b,functionaname+":: "+ message);
        }
    }
}
