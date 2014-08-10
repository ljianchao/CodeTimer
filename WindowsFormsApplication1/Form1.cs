using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using CodeTimerNX4;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        StringBuilder sb;
        public Form1()
        {
            InitializeComponent();
        }

        private void btnTest_Click(object sender, EventArgs e)
        {
            string s = string.Empty;

            CodeTimer.Time("String Concat", 10000, () => { s += "a"; }, Print);

            StringBuilder sb1 = new StringBuilder();
            CodeTimerNX4.CodeTimer.Time("StringBuilder", 10000, () => { sb1.Append("a"); }, Print);

            this.rtbShowResult.Text = sb.ToString();
        }

        void Print(string msg)
        {
            if (sb == null)
                sb = new StringBuilder();
            sb.AppendLine(msg);
            Console.WriteLine(msg);
        }
    }
}
