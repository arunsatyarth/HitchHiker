using HitchHiker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace Test
{
    [TargetForm("SomeForm")]
    class Process2Test : HTestCaseBase
    {
        [HTestCase]
        public void Test1()
        {
            Form f = CurrentForm;
            Label label = new Label();
            label.Text = "Well I sure can bud!!. Thisis what I mean when I say I have developer Access at Runtime!!";
            f.Controls.Add(label);
            Assert(true,"New Label Has been Added");

        }
   
    }
}
