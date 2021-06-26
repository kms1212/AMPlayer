using System;
using System.Windows.Forms;

namespace AMPlayer
{
    public partial class DebugForm : Form
    {
        public DebugForm()
        {
            InitializeComponent();
        }

        public void textBox1_append(string str)
        {
            textBox1.AppendText(str + Environment.NewLine);
        }

        private void DebugForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
        }
    }
}
