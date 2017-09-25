using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HLReadCard;
using ZZJSubModuleInterface;
namespace HLReadCardFrm
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        string msg = "";
        private void button1_Click(object sender, EventArgs e)
        {
            ReadCardDevCls devCls=new ReadCardDevCls();
          
            devCls.DevType="ACTA6";
            devCls.DevPort="COM1";
		      	devCls.KPJZ = 1;
						devCls.YYJZK_Key = new byte[] { 0XA1, 0XA3, 0XA2, 0XA4, 0XA6, 0XA5 };
            devCls.YYJZK_SJK =0;
            devCls.YYJZK_SJSQ = 1;





            int iret= LoopReadCard.InitReadCard(devCls, ReadResult, out msg);
             if (iret == 0)
            {
                MessageBox.Show("OK");
            }
            else
            {
                MessageBox.Show("FALSE"+msg);
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            LoopReadCard.ExitCard();
        }

        private void ReadResult(int step,int iRet, ReadCardResultInfoCls resultinfo)
        {
			if(iRet==0)
			{
				this.Invoke(new MethodInvoker(delegate()
				{
					if (resultinfo != null) 
					{

						textBox1.Text = resultinfo.KH;
						if (resultinfo.KPLX == 1)  //如果等于区域卡
						{
							if(resultinfo.BL!=null)
							{
							 QDQYCardInfo objinfo=( QDQYCardInfo)resultinfo.BL[0];

							 string data = objinfo.Crad_Id + "\t";
								data+= objinfo.Patient_Name + "\t";
							 data += objinfo.Patient_Sex + "\t";
							 data += objinfo.Patient_sfz + "\t";
							 data += objinfo.Patient_Tel + "\t";
							 data += objinfo.PT_CradId + "\t";
							 data += objinfo.PTLS_Id + "\t";
							 data += objinfo.QY_Date + "\t";
							 data += objinfo.FX_Date + "\t";
							 data += objinfo.YX_Date + "\t";
							 data += objinfo.Hospit_Code + "\t";
							 textBox1.Text = data;
							}
						}
						else if(resultinfo.KPLX == 2)  //如果等于社保IC
						{
							string data = resultinfo.XM + "\t";
							data += resultinfo.ZJHM + "\t";
						 
							textBox1.Text = data;
						}
					}
					return;
				}));
				
			}
			else if (iRet == 1)
			{
				this.Invoke(new MethodInvoker(delegate()
				{
					textBox1.Text = "正常识别" + "\n";

					return;
				}));
			}
			else
			{
				this.Invoke(new MethodInvoker(delegate()
				{
					textBox1.Text = "ERROR" + "\n";

					return;
				}));
			}
     	}

		private void button2_Click(object sender, EventArgs e)
		{
		    //LoopReadCard.StopAndCloseReadCard();
		    this.Close();
		}

		private void Form1_Load(object sender, EventArgs e)
		{

		}

		private void button4_Click(object sender, EventArgs e)
		{
			int iret=LoopReadCard.SetCardIn(true);
			if (iret == 0)
			{
				MessageBox.Show("OK");
			}
			else
			{
				MessageBox.Show("FALSE" + msg);
			}
		}

		private void button5_Click(object sender, EventArgs e)
		{
			int iret = LoopReadCard.SetCardIn(false);
			if (iret == 0)
			{
				MessageBox.Show("OK");
			}
			else
			{
				MessageBox.Show("FALSE" + msg);
			}
		}

		private void button6_Click(object sender, EventArgs e)
		{
			LoopReadCard.ReleaseDKQ();
		}

		private void button7_Click(object sender, EventArgs e)
		{
			LoopReadCard.ReturnDKQ();
		}
    }
}
