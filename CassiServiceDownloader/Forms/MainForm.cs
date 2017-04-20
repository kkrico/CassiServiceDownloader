using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CassiServiceDownloader.Forms
{

    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            txtLocalSalvamento.Text = string.Format("{0}\\{1}\\", Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "Serviço de exemplo");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var result = folderBrowserDialog1.ShowDialog();
            if (result != DialogResult.OK) return;


            txtLocalSalvamento.Text = folderBrowserDialog1.SelectedPath;
        }
    }
}
