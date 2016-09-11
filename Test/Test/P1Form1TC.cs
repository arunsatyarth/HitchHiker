using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HitchHiker;
using System.Windows.Forms;
using System.Diagnostics;
namespace Test
{
    [TargetForm("Proc1Form1")]
    public class TestCases : HTestCaseBase
    {
        [HTestCase]
        public void Test1()
        {
            Form f = CurrentForm;
            foreach (Control item in f.Controls)
            {
                if(item.GetType()==typeof(Button))
                {
                    Button b = item as Button;
                    b.BackColor = System.Drawing.Color.Aquamarine;
                }
            }
            Assert(true, "The test Case has Passed");

        }
        [HTestCase]
        public void Test2()
        {
            Form f = CurrentForm;
            foreach (Control item in f.Controls)
            {
                if (item.GetType() == typeof(TextBox))
                {
                    TextBox b = item as TextBox;
                    b.Text = "I am Hacked";
                }
            }
            Assert(true, "All text boxes hacked");

        }


    }
}
