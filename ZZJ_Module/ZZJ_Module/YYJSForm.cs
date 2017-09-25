using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace ZZJ_Module
{
    public partial class YYJSForm : Form
    {
        public YYJSForm()
        {
            InitializeComponent();
        }

        private void YYJSForm_Load(object sender, EventArgs e)
        {
            this.Left = 0;
            this.Top = 0;
            this.Width = 1280;
            this.Height = 1024;
            //
            webBrowser1.Left = 0;
            webBrowser1.Top = 70;
            webBrowser1.Width = 1280;
            webBrowser1.Height = 1024-70;
            //url
						//
						webBrowser1.Navigate("http://10.17.133.1:3000/h5/JG/fwjg.html");//http://192.168.40.199:806/doctor_list.asp//http://10.17.133.1:3000/h5/index.html
					//http://192.168.40.199:806/doctor_list.asp             


        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

        }
    }
}
