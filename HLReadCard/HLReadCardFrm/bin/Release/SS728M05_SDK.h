#ifndef _SS728M05_SDK_HEAD_
#define _SS728M05_SDK_HEAD_

#ifdef SS728M05_SDK_LIB
#define SS728M05_SDK extern "C" __declspec(dllexport)
#else
#define SS728M05_SDK extern "C" __declspec(dllimport)
#endif

SS728M05_SDK int __stdcall Test_HIDModel(void);
SS728M05_SDK int __stdcall ICC_Reader_Open(void);
SS728M05_SDK int __stdcall ICC_Reader_Close(int ReaderHandle);
SS728M05_SDK int __stdcall ICC_Reader_Reset(int ReaderHandle,int ICC_Slot_No,unsigned char * Response,int *RespLen);
SS728M05_SDK int __stdcall ICC_Reader_PowerOff(int ReaderHandle,int ICC_Slot_No);
SS728M05_SDK int __stdcall ICC_Reader_Application(int ReaderHandle,int ICC_Slot_No,int Length_of_Command_APDU ,char * Command_APDU,char * Response_APDU,int *RespLen);
SS728M05_SDK int __stdcall ICC_Reader_GetLastError(void);
SS728M05_SDK int __stdcall ICC_Reader_Version(char * Info);
SS728M05_SDK int __stdcall SS_Reader_Reset(int ReaderHandle,unsigned char*  ICC_Slot_No,unsigned char * Response,int *RespLen);
SS728M05_SDK int __stdcall SS_Reader_Reset_bBeep(int ReaderHandle,unsigned char*  ICC_Slot_No,unsigned char * Response,int *RespLen,bool bBeep);
SS728M05_SDK int __stdcall SS_Reader_PowerOff(int ReaderHandle, unsigned char  ICC_Slot_No);
SS728M05_SDK int __stdcall SS_Reader_Application(  int ReaderHandle, unsigned char*  ICC_Slot_No,int Length_of_Command_APDU,char * Command_APDU,char * Response_APDU,int *RespLen);
SS728M05_SDK int __stdcall SS_Reader_GetLastError(void);
SS728M05_SDK int __stdcall SS_Reader_Version(char* devver);
SS728M05_SDK int __stdcall SS_Reader_GetVersionID(int index,char* VersionID);
SS728M05_SDK int __stdcall SS_Reader_UpdateVersionID(int index,char* VersionID);
SS728M05_SDK int __stdcall SS_Reader_GetUserInfo(int index,char* UserInfo);
SS728M05_SDK int __stdcall SS_Reader_UpdateUserInfo(int index,char* UserInfo);
SS728M05_SDK int __stdcall SS_Reader_AutoRecognition();
SS728M05_SDK long __stdcall ss_dev_beep(long icdev,unsigned short _Amount,unsigned short _Msec);
SS728M05_SDK long __stdcall ss_dev_led(long icdev,BYTE _LedClr,BYTE _LedCtrl,unsigned short _Amount,unsigned short _Msec);
SS728M05_SDK long __stdcall ss_id_ResetID2Card(long icdev);
SS728M05_SDK long __stdcall ss_id_read_card(long icdev,int Flag=0);
SS728M05_SDK long __stdcall ss_id_query_name(long icdev,char* _Name);
SS728M05_SDK long __stdcall ss_id_query_sex(long icdev,char* _Sex);
SS728M05_SDK long __stdcall ss_id_query_sexL(long icdev,char* _Sex);
SS728M05_SDK long __stdcall ss_id_query_folk(long icdev,char* _Folk);
SS728M05_SDK long __stdcall ss_id_query_folkL(long icdev,char* _Folk);
SS728M05_SDK long __stdcall ss_id_query_birth(long icdev,char* _Date);
SS728M05_SDK long __stdcall ss_id_query_address(long icdev,char* _Addr);
SS728M05_SDK long __stdcall ss_id_query_number(long icdev,char* _Number);
SS728M05_SDK long __stdcall ss_id_query_organ(long icdev,char* _Organ);
SS728M05_SDK long __stdcall ss_id_query_termbegin(long icdev,char* _Date);
SS728M05_SDK long __stdcall ss_id_query_termend(long icdev,char* _Date);
SS728M05_SDK long __stdcall ss_id_query_photo_data(long icdev,BYTE _Format,BYTE* ImageData,long* ImageLen);
SS728M05_SDK long __stdcall ss_id_query_photo_file(long icdev,BYTE _Format,char* ImagePath);
SS728M05_SDK long __stdcall ss_id_query_newaddress(long icdev,char* _Addr);
SS728M05_SDK long __stdcall ss_id_query_IDBaseInfo_text(long icdev,char* _Text);
SS728M05_SDK long __stdcall ss_id_GetSAMno( long icdev,unsigned long *RecvLength, unsigned char* RecvBuf);
SS728M05_SDK long __stdcall ss_id_GetSAMSerialNo(long icdev, char* RecvBuf);
SS728M05_SDK long __stdcall ss_id_GetSAMStatus( long icdev);
SS728M05_SDK long __stdcall ss_id_GetFPMsg( long icdev,BYTE* FPData,int* FPDataLen);
SS728M05_SDK long __stdcall ss_CardMifare_Reset(long icdev);
SS728M05_SDK long __stdcall ss_CardMifare_Authentication(long icdev,unsigned char _Mode,unsigned char _SecNr,unsigned char* Password);
SS728M05_SDK long __stdcall ss_CardMifare_ReadBlock(long icdev,unsigned char _Adr, unsigned char *_Data);
SS728M05_SDK long __stdcall ss_CardMifare_WriteBlock(long icdev,unsigned char _Adr, unsigned char *_Data);
SS728M05_SDK long __stdcall ss_CardMifare_Increment(long icdev,unsigned char _Adr, unsigned long _Value);
SS728M05_SDK long __stdcall ss_CardMifare_Decrement(long icdev,unsigned char _Adr, unsigned long _Value);
SS728M05_SDK long __stdcall ss_CardMifare_Copy(long icdev,unsigned char Source_Adr, unsigned char Target_Adr);
SS728M05_SDK long __stdcall ss_CardMifare_GetUID(long icdev, unsigned char *_UID);
SS728M05_SDK long __stdcall ss_mc_gettrackdata(long icdev,char* Track1,char* Track2,char* Track3,BYTE WaitTime);
SS728M05_SDK int __stdcall ICC_Reader_InsertCard(int ReaderHandle,int ICC_Connector_No,int ICC_ID,int WaitTime,char* Response);
SS728M05_SDK int __stdcall ICC_Reader_RemoveCard(int ReaderHandle,int ICC_Connector_No,int WaitTime);
SS728M05_SDK int __stdcall ICC_Reader_GetStatus(int ReaderHandle,int Flag,char* Response);
SS728M05_SDK int __stdcall ICC_Reader_Application_(int ReaderHandle,int ICC_Connector_No,int Length_of_Command_APDU,char* Command_APDU,char* Response_APDU);
SS728M05_SDK int __stdcall ICC_Reader_Abort(int ReaderHandle);
SS728M05_SDK int __stdcall ICC_Reader_Resyn(int ReaderHandle,int ICC_Connector_No);
SS728M05_SDK int __stdcall SS_GetTerminalState(int ReaderHandle,char* status,char * ErrMsg=NULL);
SS728M05_SDK int __stdcall SS_CT_ReadInfo(unsigned char *buf1 ,unsigned char *buf2 ,unsigned char *buf3,int WaitTime);
SS728M05_SDK int __stdcall SS_CT_ReadInfo_(unsigned char *buf1 ,unsigned char *buf2 ,unsigned char *buf3,int WaitTime);
SS728M05_SDK int __stdcall SS_ZW_Init();
SS728M05_SDK int __stdcall SS_ZW_Close();
SS728M05_SDK int __stdcall SS_ZW_GetErrorInfo(int nErrorNo,char pszErrorInfo[256]);
SS728M05_SDK int __stdcall SS_ZW_GetFPBmpData(int nChannel, unsigned char *pFileName);
SS728M05_SDK int __stdcall SS_ZW_GetCharFromSensor(unsigned char *pcharData);
SS728M05_SDK int __stdcall SS_ZW_Match2Char(unsigned char* pcharData1,unsigned char* pcharData2,int* Score);
SS728M05_SDK int __stdcall SS_ZW_GetCharFromBMP(unsigned char *pFilename,unsigned char *pcharData);
SS728M05_SDK int __stdcall SS_ZW_MatchCharFromSensor(unsigned char *pcharData,int *sorce);
SS728M05_SDK int __stdcall ss_sle_reset_card(long icdev,unsigned char slot,unsigned char * Response,int *RespLen);
SS728M05_SDK int __stdcall ss_sle4442_read_card(long icdev,int memoryno,int pos, char* info,int lenInfo);
SS728M05_SDK int __stdcall ss_sle4428_read_card(long icdev,int datatype,int pos, char* info,int lenInfo);
SS728M05_SDK int __stdcall MPS_LIVESCAN_Init();// 初始化采集器
SS728M05_SDK int __stdcall MPS_LIVESCAN_Close();//释放采集器
SS728M05_SDK int __stdcall MPS_LIVESCAN_GetChannelCount();//获得采集器通道数量
SS728M05_SDK int __stdcall MPS_LIVESCAN_SetBright(int nChannel,int nBright);//设置采集器当前的亮度
SS728M05_SDK int __stdcall MPS_LIVESCAN_SetContrast(int nChannel,int nContrast);//设置采集器当前对比度
SS728M05_SDK int __stdcall MPS_LIVESCAN_GetBright(int nChannel,int *pnBright);//获得采集器当前的亮度
SS728M05_SDK int __stdcall MPS_LIVESCAN_GetContrast(int nChannel,int *pnContrast);//获得采集器当前对比度
SS728M05_SDK int __stdcall MPS_LIVESCAN_GetMaxImageSize(int nChannel,int *pnWidth,int *pnHeight);
SS728M05_SDK int __stdcall MPS_LIVESCAN_GetCaptWindow(int nChannel,int *pnOriginX,int *pnOriginY,int *pnWidth,int *pnHeight);//获得采集器当前图像的采集位置、宽度和高度
SS728M05_SDK int __stdcall MPS_LIVESCAN_SetCaptWindow(int nChannel,int nOriginX,int nOriginY,int nWidth,int nHeight);//设置采集器当前图像的采集位置、宽度和高度
SS728M05_SDK int __stdcall MPS_LIVESCAN_Setup();//调用采集器的属性设置对话框
SS728M05_SDK int __stdcall MPS_LIVESCAN_BeginCapture(int nChannel);//准备采集一帧图像
SS728M05_SDK int __stdcall MPS_LIVESCAN_GetFPRawData(int nChannel,unsigned char *pRawData);//采集一帧图像
SS728M05_SDK int __stdcall MPS_LIVESCAN_GetFPBmpData(int nChannel, unsigned char *pBmpData);//采集一帧BMP格式图像
SS728M05_SDK int __stdcall MPS_LIVESCAN_EndCapture(int nChannel);//结束采集一帧图像
SS728M05_SDK int __stdcall MPS_LIVESCAN_GetVersion();//取得接口规范的版本
SS728M05_SDK int __stdcall MPS_LIVESCAN_GetDesc(char pszDesc[1024]);//获得接口规范的说明
SS728M05_SDK int __stdcall MPS_LIVESCAN_GetErrorInfo(int nErrorNo,wchar_t* pszErrorInfo);//获得采集接口错误信息
SS728M05_SDK int __stdcall MPS_LIVESCAN_SetBufferEmpty(unsigned char *pImageData,long imageLength);//设置存放采集数据的内存块为空
SS728M05_SDK int __stdcall MPS_FP_GetVersion(unsigned char code[4]); //版本信息获取
SS728M05_SDK int __stdcall MPS_FP_Begin();//初始化操作
SS728M05_SDK int __stdcall MPS_FP_FeatureExtract(unsigned char cScannerType,unsigned char cFingerCode,unsigned char * pFingerImgBuf,unsigned char *pFeatureData); //指纹图像特征提取
SS728M05_SDK int __stdcall MPS_FP_FeatureMatch(unsigned char *pFeatureData1,unsigned char *pFeatureData2,float *pfSimilarity);//指纹特征数据比对
SS728M05_SDK int __stdcall MPS_FP_ImageMatch(unsigned char * pFingerImgBuf,unsigned char * pFeatureData,float * pfSimilarity);//指纹图像数据与指纹特征数据比对
SS728M05_SDK int __stdcall MPS_FP_Compress(unsigned char cScannerType, unsigned charcEnrolResult,unsigned char cFingerCode,unsigned char *pFingerImgBuf,int nCompressRatio, unsigned char *pCompressedImgBuf, unsigned char strBuf[256]);  //指纹图像数据压缩
SS728M05_SDK int __stdcall MPS_FP_Decompress(unsigned char *pCompressedImgBuf,unsigned char *pFingerImgBuf,unsigned char strBuf[256]);//指纹图像数据复现
SS728M05_SDK int __stdcall MPS_FP_GetQualityScore(unsigned char *pFingerImgBuf,unsigned char *pnScore);//指纹图像质量值获取
SS728M05_SDK int __stdcall MPS_FP_GenFeatureFromEmpty1(unsigned char cScannerType,unsigned char cFingerCode,unsigned char *pFeatureData);//生成“注册失败”指纹特征数据
SS728M05_SDK int __stdcall MPS_FP_GenFeatureFromEmpty2(unsigned char cFingerCode,unsigned char *pFeatureData);//生成“未注册”指纹特征数据
SS728M05_SDK int __stdcall MPS_FP_End();//结束操作

SS728M05_SDK int __stdcall ss_reader_open(void);
SS728M05_SDK int __stdcall ss_reader_close(long ReaderHandle);
SS728M05_SDK long __stdcall ss_rf_yl_cpu_find_card(long icdev);
SS728M05_SDK long __stdcall ss_rf_yl_cpu_find_cardB(long icdev);
SS728M05_SDK long __stdcall ss_rf_yl_cpu_card_restinfo(char* info);
SS728M05_SDK long __stdcall ss_rf_cpu_yl_read_cardInfo(long icdev, char* content, unsigned int maxLength);
SS728M05_SDK long __stdcall ss_rf_cpu_yl_write_cardInfo(long icdev, char* content, unsigned int length);
SS728M05_SDK long __stdcall ss_rf_yl_cpu_read_issuingOrg(long icdev, unsigned char no, char* content, unsigned int maxLength);
SS728M05_SDK long __stdcall ss_rf_yl_cpu_write_issuingOrg(long icdev,unsigned char no, char* content, unsigned int length);
SS728M05_SDK long __stdcall ss_rf_yl_cpu_read_demographicInfo1(long icdev, unsigned char no, char* content, unsigned int maxLength);
SS728M05_SDK long __stdcall ss_rf_yl_cpu_write_demographicInfo1(long icdev,unsigned char no, char* content, unsigned int length);
SS728M05_SDK long __stdcall ss_rf_yl_cpu_read_hospital(long icdev, unsigned char no, char* content, unsigned int maxLength);
SS728M05_SDK long __stdcall ss_rf_yl_cpu_write_hospital(long icdev,unsigned char no, char* content, unsigned int length);
SS728M05_SDK long __stdcall ss_rf_cpu_yl_read_photo(long icdev, char* imgPath, int imgPathLength);
SS728M05_SDK long __stdcall ss_rf_cpu_yl_write_photo(long icdev, char* imgPath, unsigned int length);
SS728M05_SDK long __stdcall ss_rf_yl_cpu_read_address(long icdev, unsigned char no, char* content, unsigned int maxLength);
SS728M05_SDK long __stdcall ss_rf_yl_cpu_write_address(long icdev,unsigned char no, char* content, unsigned int length);
SS728M05_SDK long __stdcall ss_rf_yl_cpu_read_linkman(long icdev, unsigned char no, char* content, unsigned int maxLength);
SS728M05_SDK long __stdcall ss_rf_yl_cpu_write_linkman(long icdev,unsigned char no, char* content, unsigned int length);
SS728M05_SDK long __stdcall ss_rf_yl_cpu_read_demographicInfo2(long icdev, unsigned char no, char* content, unsigned int maxLength);
SS728M05_SDK long __stdcall ss_rf_yl_cpu_write_demographicInfo2(long icdev,unsigned char no, char* content, unsigned int length);
SS728M05_SDK long __stdcall ss_rf_yl_cpu_read_idcardInfo(long icdev, unsigned char no, char* content, unsigned int maxLength);
SS728M05_SDK long __stdcall ss_rf_yl_cpu_write_idcardInfo(long icdev,unsigned char no, char* content, unsigned int length);
SS728M05_SDK long __stdcall ss_al_ascii_to_bcd(const unsigned char* str, int strLength, unsigned char* bcd, int bcdLength);
SS728M05_SDK long __stdcall ss_al_bcd_to_ascii(const unsigned char* bcd, int bcdLength, unsigned char* str,  int strLength);
SS728M05_SDK long __stdcall ss_al_ascii_to_hex(unsigned char* charBuffer, int len,unsigned char* chartohex);
SS728M05_SDK long __stdcall ss_al_hex_to_ascii(const unsigned char* hexbuf, int hexbuf_len,int format, unsigned char* str);
SS728M05_SDK long __stdcall ss_rf_yl_cpu_read_illNum(long icdev, unsigned char no, char* content, unsigned int maxLength);
SS728M05_SDK long __stdcall ss_rf_yl_cpu_write_illNum(long icdev,char* content, unsigned int maxLength);
SS728M05_SDK long __stdcall ss_rf_yl_cpu_read_BiometricIdentifier(long icdev, unsigned char no, char* content, unsigned int maxLength);
SS728M05_SDK long __stdcall ss_rf_yl_cpu_write_BiometricIdentifier(long icdev, unsigned char no, char* content, unsigned int maxLength);
SS728M05_SDK long __stdcall ss_rf_yl_cpu_read_SpecialIdentifier(long icdev, char* content, unsigned int maxLength);
SS728M05_SDK long __stdcall ss_rf_yl_cpu_write_SpecialIdentifier(long icdev, char* content, unsigned int maxLength);
SS728M05_SDK long __stdcall ss_rf_yl_cpu_read_AllergicReaction(long icdev, unsigned char no, char* content, unsigned int maxLength);
SS728M05_SDK long __stdcall ss_rf_yl_cpu_write_AllergicReaction(long icdev,  char* content, unsigned int maxLength);
SS728M05_SDK long __stdcall ss_rf_yl_cpu_read_Immunization(long icdev, unsigned char no, char* content, unsigned int maxLength);
SS728M05_SDK long __stdcall ss_rf_yl_cpu_write_Immunization(long icdev,  char* content, unsigned int maxLength);
SS728M05_SDK long __stdcall ss_rf_yl_cpu_read_HospitalEffectiveSign(long icdev,unsigned char no, char* content, unsigned int maxLength);
SS728M05_SDK long __stdcall ss_rf_yl_cpu_write_HospitalEffectiveSign(long icdev, unsigned char no,char* content, unsigned int maxLength);
SS728M05_SDK long __stdcall ss_rf_yl_cpu_RewriteRecord_HospitalEffectiveSign(long icdev, unsigned char no);
SS728M05_SDK long __stdcall ss_rf_yl_cpu_EraseRecord_HospitalEffectiveSign(long icdev, unsigned char no);
SS728M05_SDK long __stdcall ss_rf_yl_cpu_read_OutpatientServiceEffectiveSign(long icdev, unsigned char no,char* content, unsigned int maxLength);
SS728M05_SDK long __stdcall ss_rf_yl_cpu_write_OutpatientServiceEffectiveSign(long icdev, unsigned char no,char* content, unsigned int maxLength);
SS728M05_SDK long __stdcall ss_rf_yl_cpu_RewriteRecord_OutpatientServiceEffectiveSign(long icdev, unsigned char no);
SS728M05_SDK long __stdcall ss_rf_yl_cpu_EraseRecord_OutpatientServiceEffectiveSign(long icdev, unsigned char no);
SS728M05_SDK long __stdcall ss_rf_cpu_yl_read_HospitalInformation1(long icdev,  unsigned int beginPos,char* content, unsigned int maxLength);
SS728M05_SDK long __stdcall ss_rf_cpu_yl_write_HospitalInformation1(long icdev,  unsigned int beginPos,char* content, unsigned int length);
SS728M05_SDK long __stdcall ss_rf_cpu_yl_read_HospitalInformation2(long icdev,  unsigned int beginPos,char* content, unsigned int maxLength);
SS728M05_SDK long __stdcall ss_rf_cpu_yl_write_HospitalInformation2(long icdev,  unsigned int beginPos,char* content, unsigned int length);
SS728M05_SDK long __stdcall ss_rf_cpu_yl_read_OutpatientServiceInformation1(long icdev,  unsigned int beginPos,char* content, unsigned int maxLength);
SS728M05_SDK long __stdcall ss_rf_cpu_yl_write_OutpatientServiceInformation1(long icdev,  unsigned int beginPos,char* content, unsigned int length);
SS728M05_SDK long __stdcall ss_rf_cpu_yl_read_OutpatientServiceInformation2(long icdev,  unsigned int beginPos,char* content, unsigned int maxLength);
SS728M05_SDK long __stdcall ss_rf_cpu_yl_write_OutpatientServiceInformation2(long icdev,  unsigned int beginPos,char* content, unsigned int length);
SS728M05_SDK long __stdcall ss_rf_cpu_yl_read_FingerprintFile(long icdev,  /*unsigned int beginPos,*/char* content, unsigned int maxLength);
SS728M05_SDK long __stdcall ss_rf_cpu_yl_write_FingerprintFile(long icdev, /*unsigned int beginPos,*/char* content, unsigned int length);
SS728M05_SDK long __stdcall ss_rf_cpu_yl_GetCardUid(long icdev,BYTE* recvbuffer,long *recvlen);
SS728M05_SDK long __stdcall ss_rf_cpu_yl_read_HospitalInformation3(long icdev,  unsigned int beginPos, char* content, unsigned int maxLength);
SS728M05_SDK long __stdcall ss_rf_cpu_yl_write_HospitalInformation3(long icdev,  unsigned int beginPos, char* content, unsigned int length);
SS728M05_SDK long __stdcall ss_rf_cpu_yl_read_OutpatientServiceInformation3(long icdev, unsigned int beginPos, char* content, unsigned int maxLength);
SS728M05_SDK long __stdcall ss_rf_cpu_yl_write_OutpatientServiceInformation3(long icdev, unsigned int beginPos, char* content, unsigned int length);
SS728M05_SDK long __stdcall ss_rf_cpu_yl_read_OutpatientServiceInformation4(long icdev, unsigned int beginPos, char* content, unsigned int maxLength);
SS728M05_SDK long __stdcall ss_rf_cpu_yl_write_OutpatientServiceInformation4(long icdev, unsigned int beginPos, char* content, unsigned int length);
SS728M05_SDK long __stdcall ss_rf_cpu_yl_read_OutpatientExpenses(long icdev, unsigned int beginPos, char* content, unsigned int maxLength);
SS728M05_SDK long __stdcall ss_rf_cpu_yl_write_OutpatientExpenses(long icdev, unsigned int beginPos, char* content, unsigned int length);
SS728M05_SDK long __stdcall ss_rf_cpu_yl_AppcationLock_ddf1(long icdev,bool TempLock=true);
SS728M05_SDK long __stdcall ss_rf_cpu_yl_AppcationUnLock_ddf1(long icdev);
SS728M05_SDK long __stdcall ss_rf_cpu_yl_AppcationLock_df01(long icdev,bool TempLock=true);
SS728M05_SDK long __stdcall ss_rf_cpu_yl_AppcationUnLock_df01(long icdev);
SS728M05_SDK long __stdcall ss_rf_cpu_yl_AppcationLock_df02(long icdev,bool TempLock=true);
SS728M05_SDK long __stdcall ss_rf_cpu_yl_AppcationUnLock_df02(long icdev);
SS728M05_SDK long __stdcall ss_rf_cpu_yl_AppcationLock_df03(long icdev,bool TempLock=true);
SS728M05_SDK long __stdcall ss_rf_cpu_yl_AppcationUnLock_df03(long icdev);
SS728M05_SDK long __stdcall ss_rf_cpu_yl_CardLock(long icdev);
SS728M05_SDK long __stdcall ss_rf_cpu_yl_read_PayInfoFile(long icdev, unsigned char no,char* content, unsigned int maxLength);
SS728M05_SDK long __stdcall ss_rf_cpu_yl_write_PayInfoFile(long icdev, unsigned char no,char* content, unsigned int maxLength);
SS728M05_SDK long __stdcall ss_rf_cpu_yl_read_HospitalInformation4(long icdev,  unsigned int beginPos, char* content, unsigned int maxLength);
SS728M05_SDK long __stdcall ss_rf_cpu_yl_write_HospitalInformation4(long icdev,  unsigned int beginPos, char* content, unsigned int length);
SS728M05_SDK long __stdcall ss_rf_cpu_yl_read_HospitalInformation5(long icdev,  unsigned int beginPos, char* content, unsigned int maxLength);
SS728M05_SDK long __stdcall ss_rf_cpu_yl_write_HospitalInformation5(long icdev,  unsigned int beginPos, char* content, unsigned int length);
SS728M05_SDK long __stdcall ss_rf_VerifyPINSAM1(long icdev,char* PIN, int PINLen);
SS728M05_SDK long __stdcall ss_rf_ResetSAM1AndVerifyPIN(long icdev,char* PIN, int PINLen);
SS728M05_SDK long __stdcall ss_rf_cpu_yl_read_Binaryphoto(long icdev, char* content, unsigned int maxLength);
SS728M05_SDK long __stdcall ss_rf_cpu_yl_write_Binaryphoto(long icdev, char* content, unsigned int maxLength);
SS728M05_SDK long __stdcall ss_rf_cup_yl_GetCardVersion();//by zangqi on 2014-01-14
SS728M05_SDK long __stdcall ss_rf_SAM_Reset(long icdev,int islot, char *oATR,char *oerrMsg);
SS728M05_SDK long __stdcall ss_rf_SAM_VerifyPIN(long icdev,const char *iszPIN, BYTE *opwdRetry, char *oerrMsg);
SS728M05_SDK long __stdcall ss_rf_SAM_ChangePIN(long icdev,char *ioldPin, char *inewPin, char *oerrMsg);
SS728M05_SDK long __stdcall ss_rf_SAM_Public(long icdev,char* oinfo, char *oerrMsg);
SS728M05_SDK long __stdcall ss_rf_SAM_Terminal(long icdev,char* oinfo, char *oerrMsg);
SS728M05_SDK long __stdcall ss_rf_SAM_PublicApp(long icdev,char* oinfo, char *oerrMsg);
SS728M05_SDK long __stdcall ss_rf_SAM_OrgCertificate(long icdev,char* oinfo, char *oerrMsg);
SS728M05_SDK long __stdcall ss_rf_SAM_Certificate(long icdev,char* oinfo, char *oerrMsg);
SS728M05_SDK long __stdcall ss_rf_SAM_SM3Digest(long icdev,char* iData,int iLen,char* oData,int* oLen,char *oerrMsg);
SS728M05_SDK long __stdcall ss_rf_SAM_SM2SignHash(long icdev,char* iData,int iLen,char* oData,int* oLen,char *oerrMsg);
SS728M05_SDK long __stdcall ss_rf_sb_FindCard(int no_psam = 0);
SS728M05_SDK long __stdcall ss_rf_sb_ReadCardIssuers(
	CHAR* CardIdentifier,			
	CHAR* CardType,					
	CHAR* CardVersion,				
	CHAR* IssuersID,				
	CHAR* IssuingDate,				
	CHAR* EffectiveData,			
	CHAR* CardID					
	);	
SS728M05_SDK long __stdcall ss_rf_sb_ReadCardholder(
	CHAR* CardID,					
	CHAR* Name,		
	CHAR* Name_,	
	CHAR* Sex,						
	CHAR* Nation,					
	CHAR* Address,					
	CHAR* Birthday					
	);
SS728M05_SDK long __stdcall ss_rf_sb_ReadFingerprint(
	CHAR* Fingerprint
	);
SS728M05_SDK long __stdcall ss_rf_sb_ReadPhoto(
	CHAR* PhotoData
	);
SS728M05_SDK long __stdcall ss_jn_sb_FindCard(long icdev);
SS728M05_SDK long __stdcall ss_jn_sb_QuerryCardNumber(long icdev, char *CardNumber);
SS728M05_SDK long __stdcall ss_jn_sb_QuerryIDNumber(long icdev, char *IDNumber);
SS728M05_SDK int __stdcall SS_WSB_OpenDevice();
SS728M05_SDK int __stdcall SS_WSB_CloseDevice(int iReaderHandle ,char *oerrMsg);
SS728M05_SDK int __stdcall SS_WSB_PowerOn(int iReaderHandle,int islot,char *oATR, int *atrlen,char *oerrMsg);
SS728M05_SDK int __stdcall SS_WSB_SendAPDU(int iReaderHandle,unsigned char ibySlot,unsigned char *ipbyC_Command,unsigned long ibyLen,unsigned char *pbyR_Command,int *opnRes,char *oerrMsg);
SS728M05_SDK int __stdcall SS_WSB_VerifyPIN(int iReaderHandle,int iindexPsam,const char *iszPIN, BYTE  *opwdRetry, char *oerrMsg) ;
SS728M05_SDK int __stdcall SS_WSB_SM3Digest(int iReaderHandle,char *ipbData,int inDataLen,BYTE *opbHash,BYTE *opbHashLen,char *oerrMsg);
SS728M05_SDK int __stdcall SS_WSB_SM2SignHash(int iReaderHandle,BYTE *ipbData,BYTE ibLen,BYTE *opbSignedData,BYTE *opbLength,char *oerrMsg);
SS728M05_SDK int __stdcall SS_WSB_RDDF1EF05(int iReaderHandle,char * oKLB, char * oGFBB, char * oFKJGMC, char * oFKJGDM, char * oFKJGZS, char * oFKSJ, char * oKH, char * oAQM, char * oXPXLH, char * oYYCSDM, char *oerrMsg);
SS728M05_SDK int __stdcall SS_WSB_RDDF1EF06(int iReaderHandle,char * oXM, char * oXB, char * oMZ, char * oCSRQ, char * oSFZH, char *oerrMsg);
SS728M05_SDK int __stdcall SS_WSB_RDDF1EF07(int iReaderHandle,BYTE * oZHAOPIAN, char *oerrMsg);
SS728M05_SDK int __stdcall SS_WSB_WDDF1EF07(int iReaderHandle,BYTE * iZHAOPIAN, char *oerrMsg);
SS728M05_SDK int __stdcall SS_WSB_RDDF1EF08(int iReaderHandle,char * oKYXQ, char * oBRDH1, char * oBRDH2, char * oYLFYZFFS1, char * oYLFYZFFS2,char * oYLFYZFFS3, char *oerrMsg);
SS728M05_SDK int __stdcall SS_WSB_WDDF1EF08(int iReaderHandle,char * iKYXQ, char * iBRDH1, char * iBRDH2, char * iYLFYZFFS1, char * iYLFYZFFS2,char * iYLFYZFFS3, char *oerrMsg);
SS728M05_SDK int __stdcall SS_WSB_RDF01EF05(int iReaderHandle,char * oDZLB1, char * oDZ1, char * oDZLB2, char * oDZ2, char *oerrMsg);
SS728M05_SDK int __stdcall SS_WSB_WDF01EF05(int iReaderHandle,char * iDZLB1, char * iDZ1, char * iDZLB2, char * iDZ2,char *oerrMsg);
SS728M05_SDK int __stdcall SS_WSB_RDF01EF06(int iReaderHandle,char * oLXRXM1, char * oLXRGX1, char * oLXRDH1, char * oLXRXM2, char * oLXRGX2, char * oLXRDH2, char * oLXRXM3, char * oLXRGX3, char * oLXRDH3,char *oerrMsg);
SS728M05_SDK int __stdcall SS_WSB_WDF01EF06(int iReaderHandle,char * iLXRXM1, char * iLXRGX1, char * iLXRDH1, char * iLXRXM2, char * iLXRGX2, char * iLXRDH2, char * iLXRXM3, char * iLXRGX3, char * iLXRDH3,char *oerrMsg);
SS728M05_SDK int __stdcall SS_WSB_RDF01EF07(int iReaderHandle,char * oWHCD, char * oHYZK, char * oZY,char *oerrMsg);
SS728M05_SDK int __stdcall SS_WSB_WDF01EF07(int iReaderHandle,char * iWHCD, char * iHYZK, char * iZY,char *oerrMsg);
SS728M05_SDK int __stdcall SS_WSB_RDF01EF08(int iReaderHandle,char * oZJLB, char * oZJHM, char * oJKDAH, char * oXNHZH,char *oerrMsg);
SS728M05_SDK int __stdcall SS_WSB_WDF01EF08(int iReaderHandle,char * iZJLB, char * iZJHM, char * iJKDAH, char * iXNHZH,char *oerrMsg);
SS728M05_SDK int __stdcall SS_WSB_RDF02EF05(int iReaderHandle,char * oABOXX, char * oRHXX, char * oXCBZ, char * oXZBBZ, char * oXNXGBBZ, char * oDXBBZ, char * oNXWLBZ, char * oTNBBZ,char * oQGYBZ, char * oTXBZ, char * oQGYZBZ, char * oQGQSBZ,  char * oKZXYZBZ, char * oXZQBQBZ, char * oQTYXJSMC,char *oerrMsg);
SS728M05_SDK int __stdcall SS_WSB_WDF02EF05(int iReaderHandle,char * iABOXX, char * iRHXX, char * iXCBZ, char * iXZBBZ, char * iXNXGBBZ, char * iDXBBZ, char * iNXWLBZ, char * iTNBBZ,char * iQGYBZ, char * iTXBZ, char * iQGYZBZ, char * iQGQSBZ,  char * iKZXYZBZ, char * iXZQBQBZ, char * iQTYXJSMC,char *oerrMsg);
SS728M05_SDK int __stdcall SS_WSB_RDF02EF06(int iReaderHandle,char * oJSBBZ,char *oerrMsg);
SS728M05_SDK int __stdcall SS_WSB_WDF02EF06(int iReaderHandle,char * iJSBBZ,char *oerrMsg);
SS728M05_SDK int __stdcall SS_WSB_RDF02EF07(int iReaderHandle,int inRecorderNo, char* oGMWZMC, char* oGMFY,char *oerrMsg);
SS728M05_SDK int __stdcall SS_WSB_WDF02EF07(int iReaderHandle,char* iGMWZMC, char* iGMFY,char *oerrMsg);
SS728M05_SDK int __stdcall SS_WSB_RDF02EF08(int iReaderHandle,int inRecorderNo, char* oMYJZMC, char* oMYJZSJ,char *oerrMsg);
SS728M05_SDK int __stdcall SS_WSB_WDF02EF08(int iReaderHandle,char* iMYJZMC, char* iMYJZSJ,char *oerrMsg);
SS728M05_SDK int __stdcall SS_WSB_RDF03EF05(int iReaderHandle,char * oZYJLBS1, char * oZYJLBS2, char * oZYJLBS3,char *oerrMsg);
SS728M05_SDK int __stdcall SS_WSB_WDF03EF05(int iReaderHandle,int inRecorderNo,char *oerrMsg);
SS728M05_SDK int __stdcall SS_WSB_EDF03EF05(int iReaderHandle,int inRecorderNo,char *oerrMsg);
SS728M05_SDK int __stdcall SS_WSB_RDF03EF06(int iReaderHandle,char * oMZJLBS1, char * oMZJLBS2, char * oMZJLBS3, char * oMZJLBS4, char * oMZJLBS5,char *oerrMsg);
SS728M05_SDK int __stdcall SS_WSB_WDF03EF06(int iReaderHandle,int inRecorderNo,char *oerrMsg);
SS728M05_SDK int __stdcall SS_WSB_EDF03EF06(int iReaderHandle,int inRecorderNo,char *oerrMsg);
SS728M05_SDK int __stdcall SS_WSB_RDF03EE00(int iReaderHandle,int inRecorderNo,char *oszData,int inPos,int inLen,int inStyle,char *oerrMsg );
SS728M05_SDK int __stdcall SS_WSB_WDF03EE00(int iReaderHandle,int inRecorderNo,char * iszData,int inPos,int inLen,int inStyle,char *oerrMsg);
SS728M05_SDK int __stdcall SS_WSB_RDF03ED00(int iReaderHandle,int inRecorderNo,char * oszData,int inPos,int inLen,int inStyle,char *oerrMsg );
SS728M05_SDK int __stdcall SS_WSB_WDF03ED00(int iReaderHandle,int inRecorderNo,char * iszData,int inPos,int inLen,int inStyle,char *oerrMsg);
SS728M05_SDK int __stdcall SS_DC_PowerOff(int iReaderHandle, int iIC_Slot_No, char *oerrMsg);
SS728M05_SDK int __stdcall SS_DC_Get_Version(int  iReaderHandle,char* oinfo, char *oerrMsg) ;
SS728M05_SDK int __stdcall SS_DC_ChangePIN(int iReaderHandle,char *ioldPin, char *inewPin, char *oerrMsg);
SS728M05_SDK int __stdcall SS_DC_RSAMPublic(int iReaderHandle ,char* oinfo, char *oerrMsg);
SS728M05_SDK int __stdcall SS_DC_RSAMTerminal(int iReaderHandle,char *oinfo, char *oerrMsg);
SS728M05_SDK int __stdcall SS_DC_RSAMPublicApp(int iReaderHandle,char* oinfo, char *oerrMsg);
SS728M05_SDK int __stdcall SS_DC_RSAMOrgCertificate(int iReaderHandle,char* oinfo, char *oerrMsg) ;
SS728M05_SDK int __stdcall SS_DC_RSAMCertificate(int iReaderHandle,char* oinfo,  char *oerrMsg);
SS728M05_SDK int __stdcall SS_DC_RSign_DF03EE00(int iReaderHandle,int inRecorderNo, BYTE* osignData,char *oerrMsg,int num_EE00Begin = 64);
SS728M05_SDK int __stdcall SS_DC_RDF03EE00(int iReaderHandle,int inRecorderNo, char * oszData,char *oerrMsg,int num_EE00Begin = 64);
SS728M05_SDK int __stdcall SS_DC_WDF03EE00(int iReaderHandle,int inRecorderNo, char * iszData,char *oerrMsg,int num_EE00Begin = 64);
SS728M05_SDK int __stdcall SS_DC_RSign_DF03ED00(int iReaderHandle,int inRecorderNo,BYTE * osignData,char *oerrMsg,int num_ED00Begin = 213);
SS728M05_SDK int __stdcall SS_DC_RDF03ED00(int iReaderHandle,int inRecorderNo, char * oszData,char *oerrMsg,int num_ED00Begin = 213);
SS728M05_SDK int __stdcall SS_DC_WDF03ED00(int iReaderHandle,int inRecorderNo, char * iszData,char *oerrMsg,int num_ED00Begin = 213);
SS728M05_SDK int __stdcall SS_DC_RDF03EDEE_Item(int iReaderHandle,int iRecoderNo,int iItemNo,char *oData,char *oErrMsg,int num_EE00Begin = 64,int num_ED00Begin = 213);
SS728M05_SDK int __stdcall SS_DC_SetSpacer(char* str);

SS728M05_SDK long __stdcall SS_SM1_DownloadKey(BYTE Index, BYTE* Key, BYTE* CHK);
SS728M05_SDK long __stdcall SS_SM1_ReloadKey();
SS728M05_SDK long __stdcall SS_SM1_Key(BYTE Type, BYTE* Key, BYTE* DataBuf, long DataLen, BYTE* Result, long* ResultLen);
SS728M05_SDK long __stdcall SS_SM1_KeyIndex(BYTE Type, BYTE KeyIndex, BYTE* DataBuf, long DataLen, BYTE* Result, long* ResultLen);

SS728M05_SDK long __stdcall SS_Reader_AutoFindCard(long ReaderHandle);

//省立医院读写磁条卡接口
SS728M05_SDK int __stdcall SS_MCReader_OpenDevive();
SS728M05_SDK int __stdcall SS_MCReader_CloseDevive();
SS728M05_SDK int __stdcall SS_MCReader_Read(int tracks,unsigned char* trackData2,unsigned char* trackData3,int timeoutsecs);
SS728M05_SDK int __stdcall SS_MCReader_CancelCommand();
SS728M05_SDK int __stdcall SS_MCWriter_OpenDevive();
SS728M05_SDK int __stdcall SS_MCWriter_CloseDevive();
SS728M05_SDK int __stdcall SS_MCWriter_Write(int tracks,unsigned char* trackData2,unsigned char* trackData3);
SS728M05_SDK int __stdcall SS_MCWriter_CancelCommand();
#endif