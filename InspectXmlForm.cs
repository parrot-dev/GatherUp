using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GatherUp
{
    public partial class InspectXmlForm : Form
    {
        public InspectXmlForm(string xml)
        {
            InitializeComponent();
            textBox1.Text = xml;
            textBox1.Select(0, 0);
           
        }
    }
}
