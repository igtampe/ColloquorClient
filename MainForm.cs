using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ColloquorClient {
    public partial class MainForm:Form {

        private Colloquor.ColloquorClient Mainclient;

        public MainForm(ref Colloquor.ColloquorClient Client) {
            InitializeComponent();
            Mainclient = Client;
        }

        private void leaveToolStripMenuItem_Click(object sender,EventArgs e) {Close();}
        private void closeToolStripMenuItem_Click(object sender,EventArgs e) { Mainclient.Close(); }
    }
}
