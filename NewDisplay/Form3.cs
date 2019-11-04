using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NewDisplay
{
    public partial class Form3 : Form
    {
        public ArrayList busNum = new ArrayList();
        Boolean checkAll = true;
        public Form3()
        {
            InitializeComponent();
        }

        private void Form3_Load(object sender, EventArgs e)
        {
            for(int i = 0; i < busNum.Count; i++)
            {
                checkedListBox1.Items.Add(busNum[i]);
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (checkAll)
            {
                for (int i = 0; i < checkedListBox1.Items.Count; i++)
                    checkedListBox1.SetItemChecked(i, true);
                checkAll = false;
            }
            else
            {
                for (int i = 0; i < checkedListBox1.Items.Count; i++)
                    checkedListBox1.SetItemChecked(i, false);
                checkAll = true;
            }

        }
    }
}
