using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HL.Devices.DKQ;
using System.Threading;
using ZZJSubModuleInterface;
using Mw_Public;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace HLReadCard
{
	#region 换卡操作类型
	/// <summary>
	/// 换卡操作类型
	/// </summary>
	public enum KPCZLX
	{
		EXIT_CARD = 9,
		SWITCH_YNK = 0,
		SWITCH_QYK = 1,
		SWITCH_SBK_IC = 2,
		SWITCH_SBK_OLD = 3,
		SWITCH_QDYHK = 4,
		SWITCH_YHK = 5
	}
	#endregion

	#region 读卡入参
	/// <summary>
	/// 读卡入参
	/// </summary>
	public class ReadCardDevCls
	{
		public string DevType { set; get; }
		public string DevPort { set; get; }
		public int KPJZ { set; get; }
		//院内卡参数
		public int YYJZK_SJK { set; get; }
		public int YYJZK_SJSQ { set; get; }
		public byte[] YYJZK_Key { set; get; }
		//区域卡参数
		public int QYJZK_SJK { set; get; }
		public int QYJZK_SJSQ { set; get; }
		public byte[] QYJZK_Key { set; get; }
		//社保卡激活认证(0,不需要; 需要)
		public int SBK_QYJHRZ { set; get; }

	}
	#endregion

	public class LoopReadCard
	{
		private static CancellationTokenSource cts;

		private static int cardStatus = -1;

		static ReadCardDevCls objReadcardCls = null;

		public delegate void delegateReadResult(int step, int iRet, ReadCardResultInfoCls objinfo);//定义一个委托类，没有实体，
		static delegateReadResult readResult = null;//声明一个委托类的实例。

		private static volatile bool DKQOpened = false; // 是否正确打开读卡器
		private static volatile bool boolReadOK = false; //是否读取成功

		private static volatile bool RequestExitCard = false;//是否有退卡请求
		private static volatile bool InCardAllowed = true;//是否允许进卡
		private static volatile bool RequestBorrowDKQ = false; //是否有借读卡器请求
		private static volatile bool CardExited = true;//卡片已退出
		private static volatile bool DKQBorrrowed = false;//读卡器已借出
		private static volatile bool DKQReturned = true;//读卡器已归还

		static byte[] qykeys = new byte[] { 0x7E, 0x8F, 0xE8, 0x5C, 0x71, 0x42 }; //区域卡密码

		#region 初始化读卡器
		/// <summary>
		/// 初始化读卡器
		/// </summary>
		/// <param name="objReadcardCls"></param>
		/// <returns></returns>
		public static int InitReadCard(ReadCardDevCls objCls, delegateReadResult objresult, out string msg)
		{
			int iRet = -1;
			msg = "";
			objReadcardCls = objCls;

			//如果读卡器名称或端口号任一设置为空，则默认为没有设置
			if (objCls.DevType == "" || objCls.DevPort == "")
			{
				msg = "名称型号不能为空";// 
				return -1;
			}
			if (objReadcardCls.DevPort == "")
			{
				msg = "端口号不能为空";
				return -1;
			}
			/************************************
			 * 初始化参数
			 * ***********************************/
			readResult = objresult; //赋值读卡委托
			Task.Factory.StartNew(StartLoopReadCardStub);//还始循环读取卡信息
			return 0;
		}
		#endregion

		public static void SleepEx(int sTime = 1)
		{
			int Tick = Environment.TickCount;
			do
			{
				Thread.Sleep(10);
				Application.DoEvents();
			} while ((Environment.TickCount - Tick) > sTime);
		}

		#region 循环检测读卡
		public static void StartLoopReadCardStub()
		{
			while (true)
			{
				cts = new CancellationTokenSource();
				try
				{
					StartLoopReadCard();
				}
				catch (Exception e)
				{
					Pl_Fuction.Write_Debug("读卡异常:" + e.Message + Environment.NewLine + e.StackTrace);
				}
				Thread.Sleep(500);
			}
		}

		/// <summary>
		/// 循环检测读卡
		/// </summary>
		public static void StartLoopReadCard()
		{
			int iRet = -1;
			cardStatus = -1;
			int kplx = 0;
			string msg = "";
			string dkqPort = objReadcardCls.DevPort.Substring(3, objReadcardCls.DevPort.Length - 3);
			while (!cts.IsCancellationRequested)  //循环打开或者关闭读卡器
			{
				boolReadOK = false; // 重置
				DKQOpened = false; //重置

				Thread.Sleep(200);
				if (RequestBorrowDKQ)  // 2秒检测一次释放读卡器状态,申请状态时，线程等待
				{
					continue;
				}

				DKQReturned = true;
				DKQBorrrowed = false;

				//归还状态时，重新打开
				DKQDev.CloseDevice();//先关掉端口
				Thread.Sleep(150);
				if (DKQDev.OpenDevice(objReadcardCls.DevType, dkqPort, 9600) != 0)
				{
					Thread.Sleep(1000); //1秒后重试
					continue;
				}
				Thread.Sleep(100);
				if (InCardAllowed)
				{
					DKQDev.SetInCardMode(InCardMode.AllowAll, InCardMode.AllowAll);
					//		DKQDev.SetCardIn(2, 0); //仅允许射磁卡
				}
				//else
				//{
				//  DKQDev.SetCardIn(1, 1);
				//}
				Thread.Sleep(150);
				DKQDev.ICEnableExtIO(true);
				Thread.Sleep(150);

				DKQDev.SetCardPostion(CardPosition.InternalIC);//直接进IC卡位
				boolReadOK = false; //是否读取成功
				DKQOpened = true; //成功打开读卡器
				iRet = -1;
				cardStatus = -1;
				kplx = 0;
				msg = "";
				while (true)   //循环读卡
				{
					Thread.Sleep(100);
					if (RequestBorrowDKQ)  //收到释放读卡器申请时，关闭读卡器，退出循环读卡
					{
						DKQDev.ICEnableExtIO(false); //IC 切换到卡槽内
						Thread.Sleep(150);
						DKQDev.CloseDevice(); //关闭读卡器.

						DKQBorrrowed = true;
						DKQReturned = false;
						break;
					}
					if (RequestExitCard)
					{
						Thread.Sleep(50);
						DKQDev.MoveCard(CardPosition.FrontUnhold);
						//	DKQDev.MoveCard(2);
						RequestExitCard = false;
						CardExited = true;
						break;
					}
					if (boolReadOK) //如果读卡成功，则保持线程运行
					{
						continue;
					}
					cardStatus = -1;
					iRet = DKQDev.GetStatus(ref cardStatus);
					if (iRet != 0) //执行不成功，则跳下一循环
					{
						Thread.Sleep(150);
						DKQDev.ICEnableExtIO(false); //IC 切换到卡槽内
						Thread.Sleep(150);
						DKQDev.CloseDevice(); //关闭读卡器.
						Thread.Sleep(150);
						break;
						//continue;
					}
					Thread.Sleep(50);
					if (cardStatus != 1) //卡机内无卡
					{
						if (InCardAllowed)
						{
							DKQDev.SetInCardMode(InCardMode.AllowAll, InCardMode.AllowAll);
							//	DKQDev.SetCardIn(3, 0); //仅允许射磁卡
						}
						//else
						//{
						//  DKQDev.SetCardIn(1, 1);
						//}
						continue;
					}

					CardExited = false;
					Pl_Fuction.Write_Debug("检测到卡槽内有卡...");
					//有卡，开始读卡 
					if (readResult != null)//有卡，开始读卡委托给应用层
					{
						readResult(0, 0, null);
					}
					Thread.Sleep(150);
					DKQDev.ICEnableExtIO(false); //IC 切换到卡槽内
					Pl_Fuction.Write_Debug("IC读头切换到卡槽内...");
					Thread.Sleep(200);
					if (readResult != null)//检测卡类型委托给应用层
					{
						readResult(1, 0, null);
					}
					//
					Thread.Sleep(200);
					//	DKQDev.MoveCard(CardPosition.FrontHold);
					//		DKQDev.MoveCard(4);
					Thread.Sleep(200);
					iRet = DKQDev.DetectICType(out kplx, out msg); //检测卡类型 
					Thread.Sleep(200);

					if (iRet != 0) //检测卡类型失败
					{
						Pl_Fuction.Write_Debug("检测卡类型失败");
						if (readResult != null)//检测卡类型委托给应用层
						{
							readResult(1, -1, null);
						}
						DKQDev.MoveCard(CardPosition.FrontHold);
						//	DKQDev.MoveCard(1); //将卡重新走位到前端位置，不持卡 //退卡
						continue;//执行不成功，则跳下一循环
					}
					Pl_Fuction.Write_Debug("检测卡类型成功：" + kplx.ToString());
					if (readResult != null)//正在识别就诊卡
					{
						readResult(2, 1, null);
					}
					//检测卡类型成功
					ReadCardResultInfoCls objCardinfo = new ReadCardResultInfoCls();
					switch (kplx) //根据卡片类型执行不同的读卡操作
					{

						case 0x10: //S50卡
							{
								
							Pl_Fuction.Write_Debug("M1 S50射频卡...");
								byte[] s50bytes = new byte[512];
								///***************************************************
								////    先读取医院院内卡
								// * ***********************************************/
								Pl_Fuction.Write_Debug("尝试读取医院院内卡...");
							//先读取医院院内卡
							iRet = DKQDev.M1_ReadCard(objReadcardCls.YYJZK_SJSQ, objReadcardCls.YYJZK_SJK, objReadcardCls.YYJZK_Key, ref s50bytes, ref msg);
							if (iRet != 0) //读取失败  
							{
								if (readResult != null)//把读取的数据委托给应用层
								{
									readResult(2, iRet, null);
								}
								DKQDev.MoveCard(CardPosition.FrontHold);//将卡重新走位到前端位置
							//	DKQDev.MoveCard(1); //将卡重新走位到前端位置， 持卡 //退卡
								continue; //退卡后，继续寻卡
							}
							//读取卡号成功
							boolReadOK = true; //读取成功标志
							//读取成功,分析卡号
							objCardinfo.KPLX = 0; //卡片类型(0 院内卡，1 区域卡，2 社保卡(IC)，3.社保卡(旧）   4.青岛银行卡）
							objCardinfo.KH = Encoding.ASCII.GetString(s50bytes).TrimEnd('\0');
							if (readResult != null)//把读取的数据委托给应用层
							{
								readResult(2, iRet, objCardinfo);
							}
							continue;
						}


								//Pl_Fuction.Write_Debug("M1 S50射频卡...");
								//byte[] s50bytes = new byte[512];
								///***************************************************
								////    先读取医院院内卡
								// * ***********************************************/
								//Pl_Fuction.Write_Debug("尝试读取医院院内卡...");
								//iRet = DKQDev.M1_ReadCard(objReadcardCls.YYJZK_SJSQ, objReadcardCls.YYJZK_SJK, objReadcardCls.YYJZK_Key, ref s50bytes, ref msg);
								//if (iRet == 0) //读取成功 
								//{
								//  //读取卡号成功
								//  boolReadOK = true; //读取成功标志
								//  //读取成功,分析卡号
								//  objCardinfo.KPLX = 0; //卡片类型(0 院内卡，1 区域卡，2 社保卡(IC)，3.社保卡(旧）  4.青岛银行卡）
								//  objCardinfo.KH = Encoding.ASCII.GetString(s50bytes).TrimEnd('\0');
								//  Pl_Fuction.Write_Debug("读取医院院内卡成功：" + objCardinfo.KH);
								//  if (readResult != null)//把读取的数据委托给应用层
								//  {
								//    readResult(2, iRet, objCardinfo);
								//  }
								//  continue;
								//}
								//else
								//{
								//  Pl_Fuction.Write_Debug("医院院内卡读取失败，尝试读取区域卡...");
								//  /*************************************************
								//  //      医院卡读取失败，再读取区域卡
								//  ***********************************************/
								//  QDQYCardInfo qyCardinfo = new QDQYCardInfo();
								//  iRet = ReadQYCarddInfo(out qyCardinfo, out msg);
								//  if (iRet == 0)
								//  {

								//    //读取区域卡号成功
								//    boolReadOK = true; //读取成功标志
								//    //读取成功,分析卡号
								//    objCardinfo.KPLX = 1; //卡片类型(0 院内卡，1 区域卡，2 社保卡(IC)，3.社保卡(旧）   4.青岛银行卡）
								//    objCardinfo.KH = qyCardinfo != null ? qyCardinfo.Crad_Id : "";
								//    objCardinfo.BL = new object[] { qyCardinfo }; //把区域卡数据委托给应用层
								//    Pl_Fuction.Write_Debug("读取区域卡成功：" + objCardinfo.KH);
								//    if (readResult != null)//把读取的数据委托给应用层
								//    {
								//      readResult(2, iRet, objCardinfo);
								//    }
								//    continue;
								//  }
								//  else //读取区域卡失败
								//  {
								//    Pl_Fuction.Write_Debug("读取区域卡失败，不支持的M1卡.");
								//    if (readResult != null)//把读取的数据委托给应用层
								//    {
								//      readResult(2, iRet, null);
								//    }
								//    DKQDev.MoveCard(CardPosition.FrontHold);
								//    //	DKQDev.MoveCard(1); //将卡重新走位到前端位置， 持卡 //退卡
								//    continue; //退卡后，继续寻卡
								//  }
								//}
							//}
						case 0x20:  //  ICCTYPE_T0_CPU
							{
								//(9为所支持的全部卡类，卡片类型(0 院内卡，1 区域卡，2 社保卡(IC)，3.社保卡(旧）   4.青岛银行卡）

								/*************************
								 *   用医保DLL 尝试读卡
								* *********************/
								Pl_Fuction.Write_Debug("ICCTYPE_T0_CPU 卡...");
                                #region 银联卡
                                HL.Devices.Card.ApduOp.SetCardReaderName("ACTA6");
                                DKQDev.MoveCard(CardPosition.InternalIC);
                                DKQDev.ICPowerOn();//给银行卡上电
                                string no = "";
                                bool flag=HL.Devices.Card.ApduOp.GetCardNo(ref no);
                                if (!flag || string.IsNullOrEmpty(no))
                                {
                                    Pl_Fuction.Write_Debug("读银行卡失败或读取的银联卡为空...");
                                    if (readResult != null)//把读卡失败信息委托给应用层
                                    {
                                        readResult(2, -1, null);
                                    }
                                    DKQDev.MoveCard(CardPosition.FrontHold);
                                    //	DKQDev.MoveCard(1); //将卡重新走位到前端位置， 持卡 //退卡
                                    continue; //退卡后，继续寻卡
                                }
                                else  //读银联卡成功
                                {
                                    //读卡
                                    Pl_Fuction.Write_Debug("读银联卡成功！卡号为:" + no);
                                    objCardinfo.KPLX = 4;
                                    objCardinfo.KH = no;
                                    Pl_Fuction.Write_Debug("给应用层赋值");

                                    if (readResult != null)//委托给应用层
                                    {
                                        readResult(2, 0, objCardinfo);
                                    }
                                    boolReadOK = true; //读取成功标志
                                    continue;
                                }
                                #endregion 


                                #region  调用医保DLL读医保卡
                                QDYBCard.YYDM = ZZJCore.SuanFa.Proc.ReadPublicINI("YBYYDM", "");
								QDYBCard.SFZD = ZZJCore.SuanFa.Proc.ReadPublicINI("YBSFZD", "");
								QDYBCard.SFRY = ZZJCore.SuanFa.Proc.ReadPublicINI("YBSFRY", "");
								Pl_Fuction.Write_Debug("切换到外部IO读IC...");
								DKQDev.ICEnableExtIO(true); //切换到外部IO读IC
								Thread.Sleep(50);
                                QDYBCard.SFXX ybcardinfo = QDYBCard.ReadCard();//调用医保DLL读卡
								//读取失败
								if (ybcardinfo.XM == null || ybcardinfo.SFZ == null)
								{
									Pl_Fuction.Write_Debug("读青岛医保卡失败...");
									if (readResult != null)//把读卡失败信息委托给应用层
									{
										readResult(2, -1, null);
									}
									DKQDev.MoveCard(CardPosition.FrontHold);
									//	DKQDev.MoveCard(1); //将卡重新走位到前端位置， 持卡 //退卡
									continue; //退卡后，继续寻卡
								}
								else //读青岛医保卡成功，则解析数据，并委托给应用层
								{
									Pl_Fuction.Write_Debug("读青岛医保卡成功...");
									Thread.Sleep(10);
									//送到医保中心验证
									QDYBCard.Mode = 1;
									if (objReadcardCls.SBK_QYJHRZ == 1)  //青岛医保卡需要激活认证
									{
										Pl_Fuction.Write_Debug("青岛医保卡需要激活认证.");
										if (QDYBCard.YBKHGetHZInfo("") != 0)
										{
											Pl_Fuction.Write_Debug("青岛医保卡认证失败...");
											QDYBCard.Mode = 0;
											DKQDev.ICEnableExtIO(false); //IC 切换到卡槽内
											//Msg = "与中心交易失败!";
											if (readResult != null)//把失败信息委托给应用层
											{
												readResult(3, -1, null);
											}
											DKQDev.MoveCard(CardPosition.FrontHold);
											//		DKQDev.MoveCard(1); //将卡重新走位到前端位置， 持卡 //退卡
											continue; //退卡后，继续寻卡
										}
										Pl_Fuction.Write_Debug("青岛医保卡激活认证成功...");
									}
									else
									{
										Pl_Fuction.Write_Debug("青岛医保卡不需要激活认证.");
									}
									//认证成功
									QDYBCard.Mode = 0;
                                    //DKQDev.ICEnableExtIO(false); //IC 切换到卡槽内
									objCardinfo.XM = ybcardinfo.XM.ToString(); //姓名
									objCardinfo.ZJHM = ybcardinfo.SFZ.ToString(); //证件号码
									Pl_Fuction.Write_Debug("医保姓名：" + objCardinfo.XM);
									Pl_Fuction.Write_Debug("证件号码：" + objCardinfo.ZJHM);
									Pl_Fuction.Write_Debug("IC 切换到卡槽内...");
									objCardinfo.KPLX = 2;         //卡片类型(0 院内卡，1 区域卡，2 社保卡(IC)，3.社保卡(旧）   4.青岛银行卡）
									objCardinfo.ZJLX = 0; //证件类型(0,身份证）
									if (readResult != null)//委托给应用层
									{
										readResult(2, 0, objCardinfo);
									}
									boolReadOK = true; //读取成功标志
									continue;
                                }
                                #endregion
                            }
						default://其余的卡----港内卡处理
							{

								/*******************************************
																		 * 先读取磁条信息
																		 * ******************************************/
								string[] data = new string[3];

								//Thread.Sleep(50);
								//DKQDev.MoveCard(CardPosition.FrontHold);
								//Thread.Sleep(50);
								//DKQDev.MoveCard(CardPosition.InternalRF);
								//Thread.Sleep(10);

								iRet = DKQDev.ReadTracksASCII(TrackNum.TAll, ref data);
								//	iRet = DKQDev.ReadTracksACSII(4, ref data);
								if (iRet == 0) //读取成功 
								{
									if (data == null || string.IsNullOrEmpty(data[1]))
									{
										Pl_Fuction.Write_Debug("读其余卡（或磁条）成功，但数据为空.");
										if (readResult != null)//把读取的数据委托给应用层
										{
											readResult(2, -1, null);
										}
										DKQDev.MoveCard(CardPosition.FrontHold);
										//	DKQDev.MoveCard(1); //将卡重新走位到前端位置， 持卡 //退卡
										continue; //退卡后，继续寻卡
									}

									//读取卡号成功
									boolReadOK = true; //读取成功标志
									//读取成功,分析卡号
									objCardinfo.KPLX = 5; //卡片类型(0 院内卡，1 区域卡，2 社保卡(IC)，3.社保卡(旧）  4.青岛银行卡）
									objCardinfo.KH = data[1].TrimEnd('\0');
									Pl_Fuction.Write_Debug("读取医院院内卡成功：" + objCardinfo.KH);
									if (readResult != null)//把读取的数据委托给应用层
									{
										readResult(2, iRet, objCardinfo);
                                    }


                                    continue;
								}
								else  //读取失败
								{
									
									//Pl_Fuction.Write_Debug("读其余卡（或磁条）失败.");
									//if (readResult != null)//把读取的数据委托给应用层
									//{
									//  readResult(2, iRet, null);
									//}
									//DKQDev.MoveCard(CardPosition.FrontHold);
									////	DKQDev.MoveCard(1); //将卡重新走位到前端位置， 持卡 //退卡
									//continue; //退卡后，继续寻卡

									Pl_Fuction.Write_Debug("检测到未知的卡类型，退卡");
									boolReadOK = false; // 
									if (readResult != null)//把读卡失败信息委托给应用层
									{
										readResult(9, -1, null);
									}
									DKQDev.MoveCard(CardPosition.FrontHold);
									//	DKQDev.MoveCard(1); //将卡重新走位到前端位置，不持卡 //退卡
									continue; //退卡后，继续寻卡

								}
							

								//Pl_Fuction.Write_Debug("检测到未知的卡类型，退卡");
								//boolReadOK = false; // 
								//if (readResult != null)//把读卡失败信息委托给应用层
								//{
								//  readResult(9, -1, null);
								//}
								//DKQDev.MoveCard(1); //将卡重新走位到前端位置，不持卡 //退卡
								//continue; //退卡后，继续寻卡
							}
					}
				}
			}

			Thread.Sleep(10);
			DKQDev.CloseDevice();
		}
		#endregion

		#region 退卡
		/// <summary>
		/// 退卡
		/// </summary>
		/// <returns></returns>
		public static int ExitCard()
		{
			RequestExitCard = true;
			while (true)
			{
				if (CardExited)
				{
					break;
				}
			}
			return 0;
		}

		/// <summary>
		/// 退卡
		/// </summary>
		/// <returns></returns>
		public static int ExitCard(bool waitCardExit = true)
		{
			RequestExitCard = true;
			while (waitCardExit)
			{
				if (CardExited)
				{
					break;
				}
			}
			return 0;
		}
		#endregion

		#region 检测读卡器中是否有卡
		/// <summary>
		/// 检测读卡器中是否有卡
		/// </summary>
		/// <param name="cardState">!= 1卡机内无卡</param>
		/// <returns></returns>
		public static int GetCardStatus(out int cardState)
		{
			cardState = cardStatus;
			return DKQOpened ? 0 : -1;
		}
		#endregion

		#region 设置是否允许进卡
		/// <summary>
		/// 设置是否允许进卡
		/// </summary>
		/// <param name="boolcardin"></param>
		/// <returns></returns>
		public static int SetCardIn(bool boolcardin = false)
		{
			int iRet = -1;
			if (DKQOpened == false)
			{
				return iRet;
			}
			InCardAllowed = boolcardin;
			return 0;
		}
		#endregion

		#region 申请读卡器(需要释放)
		/// <summary>
		/// 申请读卡器(需要释放)
		/// </summary>
		public static void ReleaseDKQ()
		{
			RequestBorrowDKQ = true;
			while (true)
			{
				Thread.Sleep(20);
				if (DKQBorrrowed)
				{
					break;
				}
			}
		}
		#endregion

		#region 归还读卡器(需重新打开)
		/// <summary>
		/// 归还读卡器(需重新打开)
		/// </summary>
		public static void ReturnDKQ()
		{
			RequestBorrowDKQ = false;
			while (true)
			{
				Thread.Sleep(20);
				if (DKQReturned)
				{
					break;
				}
			}
		}
		#endregion

		#region 关闭读卡器
		/// <summary>
		/// 关闭读卡器
		/// </summary>
		public static void CloseDKQ()
		{
			try
			{
				if (DKQOpened)
				{
					DKQDev.MoveCard(CardPosition.FrontUnhold);
				//	DKQDev.MoveCard(1);
					DKQDev.SetInCardMode(InCardMode.DisAllow, InCardMode.DisAllow);
				}
				if (cts != null)
				{
					cts.Cancel();
				}
			}
			catch { }

		}
		#endregion

		#region 读取区域卡信息
		/// <summary>
		/// 读取区域卡信息
		/// </summary>
		/// <param name="sinfo"></param>
		/// <returns></returns>
		public static int ReadQYCarddInfo(out QDQYCardInfo sinfo, out string msg)
		{
			msg = "";
			sinfo = null;
			byte[] CardId = new byte[19];

			string RetString = "";
			byte[] DataOut = new byte[16];
			int iRet = DKQDev.M1_ReadCard(0, 0, qykeys, ref DataOut, ref RetString);
			/**************************************
			 * 读卡号失败
			 * **************************************/
			if (iRet != 0)  //读卡号失败
			{
				msg = "读区域卡号失败";
				Pl_Fuction.Write_Debug(msg);
				return -1;
			}
			sinfo = new QDQYCardInfo();
			//int Number = 0;
			// Number = BitConverter.ToInt32(DataOut, 0);
			byte[] item = new byte[4];
			item[0] = DataOut[3];
			item[1] = DataOut[2];
			item[2] = DataOut[1];
			item[3] = DataOut[0];
			//卡的ID
			sinfo.Crad_Id = ByteToHexString(item, 4);
			//sinfo.Crad_Id = BitConverter.ToInt32(item,0).ToString()

			sinfo.Crad_Id = Convert.ToInt64(sinfo.Crad_Id, 16).ToString();
			/**************************************************
			//     读取数据
			 * **************************************/
			iRet = DKQDev.M1_ReadCard(5, 0, qykeys, ref DataOut, ref RetString);
			if (iRet != 0) //读区域数据不成功，为兼容妇儿医院的老卡没有数据,没有数据也返回成功
			{
				msg = "读取5扇区0块失败";
				Pl_Fuction.Write_Debug(msg);
				return 0;
			}
			if (!CheckJiaoyan(DataOut))
			{
				msg = "5扇区0块校验错";
				Pl_Fuction.Write_Debug(msg);
				return 0; //读取5扇区0块失败
			}
			//患者名字
			sinfo.Patient_Name = Encoding.Default.GetString(DataOut);
			//患者性别
			sinfo.Patient_Sex = (DataOut[14] == 1 ? "男" : "女");


			//读取块1
			iRet = DKQDev.M1_ReadCard(5, 1, qykeys, ref DataOut, ref RetString);
			if (iRet != 0) //读区域数据不成功，为兼容妇儿医院的老卡没有数据,没有数据也返回成功
			{
				msg = "读取5扇区1块失败";
				Pl_Fuction.Write_Debug(msg);
				return 0;
			}
			int SUM = CheckSum(DataOut, 16);
			//患者证件类型
			if (DataOut[1] == 1) sinfo.Patient_CardType = "身份证";
			if (DataOut[1] == 2) sinfo.Patient_CardType = "监护人身份证";
			if (DataOut[1] == 0) sinfo.Patient_CardType = "未知";

			//拷贝块1中的14字节
			Array.Copy(DataOut, 2, CardId, 0, 14);
			//读取块2
			iRet = DKQDev.M1_ReadCard(5, 2, qykeys, ref DataOut, ref RetString);
			if (iRet != 0) //读区域数据不成功，为兼容妇儿医院的老卡没有数据,没有数据也返回成功
			{
				msg = "读取5扇区2块失败";
				Pl_Fuction.Write_Debug(msg);
				return 0;
			}
			SUM += CheckSum(DataOut, 15);
			byte[] BCD = IntStrToBcd(SUM.ToString());

			if (BCD[BCD.Length - 1] != DataOut[15])
			{
				msg = "5扇区2块校验错";
				Pl_Fuction.Write_Debug(msg);
				return 0;
			}

			// if (!CheckJiaoyan(DataOut)) return -1;
			//拷贝块2
			Array.Copy(DataOut, 0, CardId, 14, 4);
			byte[] tel = new byte[12];
			tel[11] = 0;
			Array.Copy(DataOut, 4, tel, 0, 11);

			//患者证件号
			sinfo.Patient_sfz = Encoding.ASCII.GetString(CardId);
			//患者联系方式
			sinfo.Patient_Tel = Encoding.Default.GetString(tel);

			//平台卡号
			iRet = DKQDev.M1_ReadCard(4, 0, qykeys, ref DataOut, ref RetString);
			if (iRet != 0) //读区域数据不成功，为兼容妇儿医院的老卡没有数据,没有数据也返回成功
			{
				msg = "读取4扇区0块失败";
				Pl_Fuction.Write_Debug(msg);
				return 0;
			}
			if (!CheckJiaoyan(DataOut))
			{
				msg = "4扇区0块校验错";
				Pl_Fuction.Write_Debug(msg);
				return 0;
			}
			byte[] Ptkh = new byte[9];
			Array.Copy(DataOut, 0, Ptkh, 0, 9);
			sinfo.PT_CradId = BcdToIntString(Ptkh);

			//院区代码
			Ptkh = new byte[5];
			Array.Copy(DataOut, 9, Ptkh, 0, 5);
			sinfo.Hospit_Code = BcdToIntString(Ptkh);//12


			//平台流水
			iRet = DKQDev.M1_ReadCard(4, 2, qykeys, ref DataOut, ref RetString);
			if (iRet != 0) //读区域数据不成功，为兼容妇儿医院的老卡没有数据,没有数据也返回成功
			{
				msg = "读取4扇区2块失败";
				Pl_Fuction.Write_Debug(msg);
				return 0;
			}
			if (!CheckJiaoyan(DataOut))
			{
				msg = "4扇区2块校验错";
				Pl_Fuction.Write_Debug(msg);
				return 0;
			}
			Ptkh = new byte[15];
			Array.Copy(DataOut, 0, Ptkh, 0, 15);
			sinfo.PTLS_Id = BcdToIntString(Ptkh);//12
			//病人ID
			iRet = DKQDev.M1_ReadCard(3, 0, qykeys, ref DataOut, ref RetString);
			if (iRet != 0) //读区域数据不成功，为兼容妇儿医院的老卡没有数据,没有数据也返回成功
			{
				msg = "读取3扇区0块失败";
				Pl_Fuction.Write_Debug(msg);
				return 0;
			}
			CardId = new byte[11];
			Array.Copy(DataOut, 0, CardId, 0, 10);
			sinfo.Patient_Id = Encoding.Default.GetString(CardId);


			sinfo.Patient_Name = TrimString(sinfo.Patient_Name);
			sinfo.Patient_sfz = TrimString(sinfo.Patient_sfz);
			sinfo.Patient_Tel = TrimString(sinfo.Patient_Tel);
			byte[] TimeByte = new byte[4];

			iRet = DKQDev.M1_ReadCard(4, 1, qykeys, ref DataOut, ref RetString);
			if (iRet != 0) //读区域数据不成功，为兼容妇儿医院的老卡没有数据,没有数据也返回成功
			{
				msg = "读取4扇区1块失败";
				Pl_Fuction.Write_Debug(msg);
				return 0;
			}
			if (!CheckJiaoyan(DataOut))
			{
				msg = "4扇区1块校验错";
				Pl_Fuction.Write_Debug(msg);
				return 0;
			}
			//发行日期
			Array.Copy(DataOut, 0, TimeByte, 0, 4);
			sinfo.FX_Date = BCDToDateTime(TimeByte, 4).ToString("yyyy-MM-dd");

			//有效日期
			Array.Copy(DataOut, 4, TimeByte, 0, 4);
			sinfo.YX_Date = BCDToDateTime(TimeByte, 4).ToString("yyyy-MM-dd");

			//启用日期
			Array.Copy(DataOut, 8, TimeByte, 0, 4);
			sinfo.QY_Date = BCDToDateTime(TimeByte, 4).ToString("yyyy-MM-dd");


			sinfo.PTLS_Id = Int64.Parse(TrimString(sinfo.PTLS_Id)).ToString();
			sinfo.PT_CradId = Int64.Parse(TrimString(sinfo.PT_CradId)).ToString();

			sinfo.Patient_Id = TrimString(sinfo.Patient_Id);
			sinfo.Hospit_Code = TrimString(sinfo.Hospit_Code);
			msg = "成功读取完整区域卡信息！";
			Pl_Fuction.Write_Debug(msg);
			return 0;
		}
		#endregion

		//验证密码
		public static int VerifyPassword(string sPassWord)
		{
			qykeys = HexConvert(sPassWord);//7E8FE85C7142
			return DKQDev.VerifyPassword(0, qykeys);
		}

		//初始化卡密码
		public static int InitPassWord(string sOldPassWord, string sPassWord)
		{
			byte[] Oldcode = HexConvert(sOldPassWord);//7E8FE85C7142
			qykeys = HexConvert(sPassWord);//7E8FE85C7142
			if (DKQDev.VerifyPassword(0, Oldcode) == 0) DKQDev.UpdatePassword(0, qykeys);
			if (DKQDev.VerifyPassword(1, Oldcode) == 0) DKQDev.UpdatePassword(1, qykeys);
			if (DKQDev.VerifyPassword(2, Oldcode) == 0) DKQDev.UpdatePassword(2, qykeys);
			if (DKQDev.VerifyPassword(3, Oldcode) == 0) DKQDev.UpdatePassword(3, qykeys);
			if (DKQDev.VerifyPassword(4, Oldcode) == 0) DKQDev.UpdatePassword(4, qykeys);
			if (DKQDev.VerifyPassword(5, Oldcode) == 0) DKQDev.UpdatePassword(5, qykeys);
			return 0;
		}

		public static int GetCardLocation()
		{
			int nstate = 0;
			return DKQDev.GetStatus(ref nstate);
		}

		//public static int Write_CardId(string CardId)
		//{

		//    ulong cid = ulong.Parse(CardId);
		//    byte[] blist = BitConverter.GetBytes(cid);
		//    byte[] Tmp = new byte[1];
		//    BcdToByte(blist, ref Tmp, 8);


		//    byte[] Data1 = new byte[16];
		//    Array.Copy(Tmp, 0, Data1, 0, Tmp.Length < 16 ? Tmp.Length : 16);
		//    //DKQDev.M1_WriteCard(0,0,qykeys,
		//      int n = DKQDev.M1WriteBlock(0, 0, Data1);
		//      return n;
		//}

		//写入性别
		public static int WriteSex(string sPatientName)
		{
			byte sex = (byte)(sPatientName == "男" ? 1 : 2);
			string Err = "";
			byte[] Data1 = new byte[16];
			int d = DKQDev.M1_ReadCard(5, 0, qykeys, ref Data1, ref Err);
			//把性别填进去
			Data1[14] = sex;
			return DKQDev.M1WriteBlock(5, 0, Data1);
		}

		//写入名字
		public static int WriteName(string sPatientName)
		{
			string Err = "";
			byte[] tmp = new byte[16];
			byte[] name = Encoding.GetEncoding("GB2312").GetBytes(sPatientName);
			tmp = new byte[14];
			if (name.Length > 0) Array.Copy(name, 0, tmp, 0, name.Length > 14 ? 14 : name.Length);
			byte[] Data1 = new byte[16];
			int d = DKQDev.M1_ReadCard(5, 0, qykeys, ref Data1, ref Err);
			//把名字填进去
			Array.Copy(tmp, 0, Data1, 0, tmp.Length);
			return DKQDev.M1WriteBlock(5, 0, Data1);
		}

		//写入身份证号
		public static int WriteSfzId(string sSfzId)
		{
			string Err = "";
			byte[] Data1 = new byte[16];
			byte[] Data2 = new byte[16];
			//if (sSfzId.Length != 15 && sSfzId.Length != 18) return -1;
			byte[] sfzId = Encoding.ASCII.GetBytes(sSfzId);
			//1表示身份证
			Data1[0] = 0;
			Data1[1] = 1;
			//15位身份证
			if (sfzId.Length > 0) Array.Copy(sfzId, 0, Data1, 2, 14);
			//写入第一块数据
			DKQDev.M1WriteBlock(5, 1, Data1);
			//后3位身份证
			//读取所有数据.后面有手机号
			int d = DKQDev.M1_ReadCard(5, 2, qykeys, ref Data2, ref Err);
			Data2[0] = 0;
			Data2[1] = 0;
			Data2[2] = 0;
			Data2[3] = 0;
			//后三位身份证号
			if (sfzId.Length == 18) Array.Copy(sfzId, 14, Data2, 0, 4);
			//写入第二块数据
			DKQDev.M1WriteBlock(5, 2, Data2);
			return 0;
		}

		//写入手机号码
		public static int WritePhoneId(string sPatientName)
		{
			string Err = "";
			byte[] Data1 = new byte[16];
			byte[] tmp;
			//if (sPatientName.Length > 11 || sPatientName.Length ==0) return -1;
			//转编码
			byte[] PhoneId = Encoding.ASCII.GetBytes(sPatientName);
			tmp = new byte[11];
			if (PhoneId.Length > 0) Array.Copy(PhoneId, 0, tmp, 0, PhoneId.Length > 11 ? 11 : PhoneId.Length);
			//读取第二块数据
			DKQDev.M1_ReadCard(5, 2, qykeys, ref Data1, ref Err);
			//填入联系方式
			Array.Copy(tmp, 0, Data1, 4, tmp.Length);
			//校验码,先填0
			Data1[15] = 0;
			DKQDev.M1WriteBlock(5, 2, Data1);
			return 0;
		}

		//写入平台ID
		public static int WritePT_CradId(string sPT_CradId)
		{
			string Err = "";
			byte[] DD;
			if (sPT_CradId.Length == 0)
				DD = new byte[5];
			else
				DD = IntStrToBcd(sPT_CradId);
			byte[] Tmp = new byte[10];
			byte[] Data1 = new byte[16];
			Array.Copy(DD, 0, Tmp, 9 - DD.Length, DD.Length < 9 ? DD.Length : 9);
			Tmp[9] = 0xFF;
			//读取第二块数据
			DKQDev.M1_ReadCard(4, 0, qykeys, ref Data1, ref Err);
			Array.Copy(Tmp, 0, Data1, 0, 10);
			DKQDev.M1WriteBlock(4, 0, Data1);
			return 0;
		}

		//写入流水号
		public static int WritePT_LSId(string sPT_CradId)
		{
			string Err = "";
			byte[] DD;// = IntStrToBcd(sPT_CradId);
			if (sPT_CradId.Length == 0)
				DD = new byte[5];
			else
				DD = IntStrToBcd(sPT_CradId);

			byte[] Tmp = new byte[13];
			byte[] Data1 = new byte[16];
			Array.Copy(DD, 0, Tmp, 13 - DD.Length, DD.Length < 13 ? DD.Length : 13);
			//读取第二块数据
			DKQDev.M1_ReadCard(4, 2, qykeys, ref Data1, ref Err);
			Array.Copy(Tmp, 0, Data1, 2, 13);
			DKQDev.M1WriteBlock(4, 2, Data1);
			return 0;
		}

		//写入院区代码
		public static int WriteHospitalCode(string sHospitCode)
		{
			string Err = "";
			byte[] DD;// = IntStrToBcd(sPT_CradId);
			if (sHospitCode.Length == 0)
				DD = new byte[5];
			else
				DD = IntStrToBcd(sHospitCode);
			byte[] Tmp = new byte[5];
			byte[] Data1 = new byte[16];
			Array.Copy(DD, 0, Tmp, 5 - DD.Length, DD.Length < 5 ? DD.Length : 5);
			//读取第1块数据
			DKQDev.M1_ReadCard(4, 1, qykeys, ref Data1, ref Err);
			Array.Copy(Tmp, 0, Data1, 13, 3);
			DKQDev.M1WriteBlock(4, 1, Data1);
			//读取第2块数据
			DKQDev.M1_ReadCard(4, 2, qykeys, ref Data1, ref Err);
			Array.Copy(Tmp, 3, Data1, 0, 2);
			DKQDev.M1WriteBlock(4, 2, Data1);
			return 0;
		}

		//写入病人Id
		public static int Write_PatientId(string sPatientId)
		{
			string Err = "";
			byte[] Data1 = new byte[16];
			byte[] tmp = new byte[10];
			//if (sPatientName.Length > 11 || sPatientName.Length ==0) return -1;
			//转编码
			byte[] PatientId = Encoding.ASCII.GetBytes(sPatientId);

			if (PatientId.Length > 0) Array.Copy(PatientId, 0, tmp, 0, PatientId.Length > 10 ? 10 : PatientId.Length);

			//读取原数据 
			DKQDev.M1_ReadCard(3, 0, qykeys, ref Data1, ref Err);
			//填入联系方式
			Array.Copy(tmp, 0, Data1, 0, tmp.Length);
			DKQDev.M1WriteBlock(3, 0, Data1);
			return 0;
		}

		public static bool CheckJiaoyan(byte[] buf)
		{
			int d = CheckSum(buf, 15);
			byte[] BCD = IntStrToBcd(d.ToString());
			return (buf[15] == BCD[BCD.Length - 1]);
		}

		#region 计算半加和校验码
		/// <summary>
		/// 计算半加和校验码
		/// </summary>
		/// <param name="bys"></param>
		/// <param name="Length"></param>
		/// <returns></returns>
		public static int CheckSum(byte[] bys, int Length)
		{
			int Ret = 0;
			byte item;
			for (int i = 0; i < Length; i++)
			{
				item = bys[i];
				Ret += (item >> 4) + (item & 0x0f);
			}
			return Ret;
		}

		public static string TrimString(string dst)
		{
			int d = dst.IndexOf("\0");
			if (d > -1) dst = dst.Substring(0, d);
			return dst;
		}

		private static uint HornerScheme(uint num, uint divider, uint factor)
		{
			uint remainder = 0, quotient = 0, result = 0;
			remainder = num % divider;
			quotient = num / divider;
			if (!(quotient == 0 && remainder == 0))
				result += HornerScheme(quotient, divider, factor) * factor + remainder;
			return result;
		}

		/// <summary>
		/// Converts from binary coded decimal to integer
		/// </summary>
		/// <param name="num"></param>
		/// <returns></returns>
		public static uint BcdToDec(uint num)
		{
			return HornerScheme(num, 0x10, 10);
		}

		/// <summary>
		/// Converts from integer to binary coded decimal
		/// </summary>
		/// <param name="num"></param>
		/// <returns></returns>
		public static uint DecToBcd(uint num)
		{
			return HornerScheme(num, 10, 0x10);
		}

		/// <summary>
		/// 日期转BCD数组
		/// </summary>
		/// <param name="dateTime"></param>
		/// <param name="type">4 6 7</param>
		/// <returns></returns>
		public static byte[] DateTimeToBCD(DateTime dateTime, ushort type)
		{
			string strServerTime = string.Format("{0:yyyyMMddHHmmss}", dateTime);

			byte[] bcd = new byte[type];
			if (type == 4)
			{
				bcd[0] = byte.Parse(DecToBcd(uint.Parse(strServerTime.Substring(0, 2))).ToString("D2"));
				bcd[1] = byte.Parse(DecToBcd(uint.Parse(strServerTime.Substring(2, 2))).ToString("D2"));
				bcd[2] = byte.Parse(DecToBcd(uint.Parse(strServerTime.Substring(4, 2))).ToString("D2"));
				bcd[3] = byte.Parse(DecToBcd(uint.Parse(strServerTime.Substring(6, 2))).ToString("D2"));
			}
			if (type == 6)
			{
				bcd[0] = byte.Parse(DecToBcd(uint.Parse(strServerTime.Substring(2, 2))).ToString("D2"));
				bcd[1] = byte.Parse(DecToBcd(uint.Parse(strServerTime.Substring(4, 2))).ToString("D2"));
				bcd[2] = byte.Parse(DecToBcd(uint.Parse(strServerTime.Substring(6, 2))).ToString("D2"));
				bcd[3] = byte.Parse(DecToBcd(uint.Parse(strServerTime.Substring(8, 2))).ToString("D2"));
				bcd[4] = byte.Parse(DecToBcd(uint.Parse(strServerTime.Substring(10, 2))).ToString("D2"));
				bcd[5] = byte.Parse(DecToBcd(uint.Parse(strServerTime.Substring(12, 2))).ToString("D2"));
			}
			if (type == 7)
			{
				bcd[0] = byte.Parse(DecToBcd(uint.Parse(strServerTime.Substring(0, 2))).ToString("D2"));
				bcd[1] = byte.Parse(DecToBcd(uint.Parse(strServerTime.Substring(2, 2))).ToString("D2"));
				bcd[2] = byte.Parse(DecToBcd(uint.Parse(strServerTime.Substring(4, 2))).ToString("D2"));
				bcd[3] = byte.Parse(DecToBcd(uint.Parse(strServerTime.Substring(6, 2))).ToString("D2"));
				bcd[4] = byte.Parse(DecToBcd(uint.Parse(strServerTime.Substring(8, 2))).ToString("D2"));
				bcd[5] = byte.Parse(DecToBcd(uint.Parse(strServerTime.Substring(10, 2))).ToString("D2"));
				bcd[5] = byte.Parse(DecToBcd(uint.Parse(strServerTime.Substring(12, 2))).ToString("D2"));
			}
			return bcd;
		}

		/// <summary>
		/// BCD时间转日期时间
		/// </summary>
		/// <param name="bcdTime"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static DateTime BCDToDateTime(byte[] bcdTime, ushort type)
		{
			StringBuilder sb = new StringBuilder();
			if (type == 4) //4位BCD码的日期
			{
				sb.Append(BcdToDec(bcdTime[0]).ToString("D2"));
				sb.Append(BcdToDec(bcdTime[1]).ToString("D2"));
				sb.Append('-' + BcdToDec(bcdTime[2]).ToString("D2"));
				sb.Append('-' + BcdToDec(bcdTime[3]).ToString("D2") + " ");
			}
			if (type == 6) //6位BCD码的时间
			{
				sb.Append(DateTime.Now.ToString("yyyy").Substring(0, 2));
				sb.Append(BcdToDec(bcdTime[0]).ToString("D2"));
				sb.Append('-' + BcdToDec(bcdTime[1]).ToString("D2"));
				sb.Append('-' + BcdToDec(bcdTime[2]).ToString("D2") + " ");
				sb.Append(BcdToDec(bcdTime[3]).ToString("D2") + ":");
				sb.Append(BcdToDec(bcdTime[4]).ToString("D2") + ":");
				sb.Append(BcdToDec(bcdTime[5]));
			}
			if (type == 7) //7位BCD码的日期
			{
				sb.Append(BcdToDec(bcdTime[0]).ToString("D2"));
				sb.Append(BcdToDec(bcdTime[1]).ToString("D2"));
				sb.Append('-' + BcdToDec(bcdTime[2]).ToString("D2"));
				sb.Append('-' + BcdToDec(bcdTime[3]).ToString("D2") + " ");
				sb.Append(BcdToDec(bcdTime[4]).ToString("D2") + ":");
				sb.Append(BcdToDec(bcdTime[5]).ToString("D2") + ":");
				sb.Append(BcdToDec(bcdTime[6]));
			}

			DateTime dt;
			//2011-3-26 当日期出错时的处理
			DateTime.TryParse(sb.ToString(), out dt);

			return dt;
		}


		/// <summary>
		/// BCD码转为10进制串(阿拉伯数据) 
		/// </summary>
		/// <param name="bytes">BCD码 </param>
		/// <returns>10进制串 </returns>
		public static String BcdToIntString(byte[] bytes)
		{
			StringBuilder temp = new StringBuilder(bytes.Length * 2);

			for (int i = 0; i < bytes.Length; i++)
			{
				temp.Append((byte)((bytes[i] & 0xf0) >> 4));
				temp.Append((byte)(bytes[i] & 0x0f));
			}
			return temp.ToString().Substring(0, 1).Equals("0") ? temp.ToString().Substring(1) : temp.ToString();
		}

		/// <summary>
		/// 10进制串转为BCD码 
		/// </summary>
		/// <param name="asc">10进制串 </param>
		/// <returns>BCD码 </returns>
		public static byte[] IntStrToBcd(String asc)
		{
			int len = asc.Length;
			int mod = len % 2;

			if (mod != 0)
			{
				asc = "0" + asc;
				len = asc.Length;
			}

			byte[] abt = new byte[len];
			if (len >= 2)
			{
				len = len / 2;
			}

			byte[] bbt = new byte[len];
			abt = System.Text.Encoding.ASCII.GetBytes(asc);
			int j, k;

			for (int p = 0; p < asc.Length / 2; p++)
			{
				if ((abt[2 * p] >= '0') && (abt[2 * p] <= '9'))
				{
					j = abt[2 * p] - '0';
				}
				else if ((abt[2 * p] >= 'a') && (abt[2 * p] <= 'z'))
				{
					j = abt[2 * p] - 'a' + 0x0a;
				}
				else
				{
					j = abt[2 * p] - 'A' + 0x0a;
				}

				if ((abt[2 * p + 1] >= '0') && (abt[2 * p + 1] <= '9'))
				{
					k = abt[2 * p + 1] - '0';
				}
				else if ((abt[2 * p + 1] >= 'a') && (abt[2 * p + 1] <= 'z'))
				{
					k = abt[2 * p + 1] - 'a' + 0x0a;
				}
				else
				{
					k = abt[2 * p + 1] - 'A' + 0x0a;
				}

				int a = (j << 4) + k;
				byte b = (byte)a;
				bbt[p] = b;
			}
			return bbt;
		}

		public static void BcdToByte(byte[] arry, ref byte[] ArryOut, int Count)
		{
			ArryOut = new byte[Count];
			for (int i = 0; i < Count; i++)
			{
				ArryOut[i] = arry[Count - i - 1];
			}
		}

		public static byte[] NumberToBcd(string NumberStr)
		{
			if (NumberStr.Length % 2 != 0) NumberStr = "0" + NumberStr;
			int len = NumberStr.Length / 2;
			byte[] bRet = new byte[NumberStr.Length / 2];
			for (int i = 0; i < len; i++)
			{
				int pos = i * 2;
				string by = NumberStr.Substring(pos, 2);
				bRet[i] = Convert.ToByte(by, 16);
			}
			return bRet;
		}

		public static byte[] HexConvert(string hex)
		{
			hex = hex.ToUpper();
			//int len = hex.Length / 2;
			//if (len * 2 < hex.Length) len++;

			int len = (hex.Length / 2);
			byte[] result = new byte[len];
			char[] achar = hex.ToCharArray();
			string by = "";
			for (int i = 0; i < len; i++)
			{
				int pos = i * 2;
				//if (pos + 1 >= hex.Length)
				//{
				//     by = new string(new char[] { achar[pos]});
				//}
				//else
				//{
				by = new string(new char[] { achar[pos], achar[pos + 1] });
				//}
				result[i] = Convert.ToByte(by, 16);
			}
			return result;
		}

		//将字节数组转换成十六进制字符串
		private static string ByteToHexString(byte[] bytes, int Count)
		{
			StringBuilder result = new StringBuilder();
			for (int i = 0; i < Count; i++)
			{
				result.Append(bytes[i].ToString("X2"));
			}
			return result.ToString();
		}
		#endregion
	}
}