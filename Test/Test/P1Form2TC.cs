using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HitchHiker;
using System.Windows.Forms;
using System.Diagnostics;
namespace Test
{
    [TargetForm("Proc1Form2")]
    public class P1Form2TC : HTestCaseBase
    {
        [HTestCase]
        public void Test1()
        {
            Form f = CurrentForm;
            foreach (Control item in f.Controls)
            {
                if (item.GetType() == typeof(Label))
                {
                    Label b = item as Label;
                    if (b.Name == "label1")
                        b.Text = b.Text+ " !!Yeah Right!!!";
                }
            }
            Assert(true, "The label has been changed");

        }
        [HTestCase]
        public void Test2()
        {
            Form f = CurrentForm;
            foreach (Control item in f.Controls)
            {
                if (item.GetType() == typeof(Button))
                {
                    Button b = item as Button;
                    b.Text = " Hacked";
                }
            }
            Assert(true, "The test Case has passed");

        }


    }
}
