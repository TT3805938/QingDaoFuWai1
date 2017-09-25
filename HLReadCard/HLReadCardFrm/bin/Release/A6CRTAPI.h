/*++

Module Name:

    A6CRTAPI.H

Abstract:

    Master include file for applications that call 
    functions provided by A6CRTAPI.DLL

Revision History:

    2013-12-07 : created

--*/

#ifndef _A6CRTAPI_H
#define _A6CRTAPI_H

#include <Windows.h>

// Return codes of the APIs
//
#define A6_S_SUCCESS			0
#define A6_E_DEV_NOT_RECOGNIZED		0x300
#define A6_E_PORT_UNAVAILABLE		0x303
#define A6_E_UNKNOWN_ERROR		0x311
#define A6_E_COMM_TIMEOUT		0x320
#define A6_E_COMM_ERROR			0x321
#define A6_E_INVALID_HANDLE		0x330
#define A6_E_INVALID_PARAMETER		0x331
#define A6_E_NO_MEMORY			0x332
#define A6_E_BUFFER_TOO_SMALL		0x334
#define A6_E_UNDEFINED_COMMAND		0x400
#define A6_E_DISABLED_COMMAND		0x402
#define A6_E_COMMAND_DATA_ERROR		0x404
#define A6_E_VOLTAGE_ABNORMALITY	0x405
#define A6_E_LENGTH_ABNORMALITY		0x406
#define A6_E_POWER_DOWN			0x407
#define A6_E_COMMAND_FAILURE		0x410
#define A6_E_NO_CARD_IN			0x411
#define A6_E_CARD_UNRECOGNIZED		0x412
#define A6_E_NOT_IN_STD_POS		0x413
#define A6_E_CARD_SCRAPPED		0x414
#define A6_E_NO_RF_CARD			0x430
#define A6_E_SECTOR_NOT_CERTIFIED	0x431
#define A6_E_SN_ERROR			0x432
#define A6_E_INVALID_PASSWORD		0x433
#define A6_E_DATA_FORMAT_ERROR		0x434
#define A6_E_VALUE_OVERFLOW		0x435

// Data type
//
typedef LONG READERHANDLE;
typedef READERHANDLE *PREADERHANDLE;


// Basic Operations
//

LONG	
	WINAPI 
	A6_Connect
	( 
	__in DWORD dwPort, 
	__in DWORD dwSpeed, 
	__out PREADERHANDLE phReader 
	);
LONG	
	WINAPI 
	A6_Disconnect
	( 
	__in READERHANDLE hReader 
	);

#define RESET_ONLY		0x30
#define RESET_AND_EJECT		0x31
#define RESET_AND_CAPTURE	0x32

LONG	
	WINAPI 
	A6_Initialize
	( 
	__in READERHANDLE hReader,
	__in BYTE bResetMode,
	__out PBYTE pbVerBuff,
	__inout PDWORD pcbVerLength
	);


#define FCI_PROHIBITED		0x31
#define FCI_MAGCARD_ONLY	0x32
#define FCI_ALLOWED		0x33

#define RCI_ALLOWED		0x30
#define RCI_PROHIBITED		0x31

LONG	
	WINAPI 
	A6_SetCardIn
	( 
	__in READERHANDLE hReader,
	__in BYTE bFrontSet,
	__in BYTE bRearSet
	);

#define DPOS_FRONT_NH		0x30
#define DPOS_FRONT		0x31
#define DPOS_INTERNAL		0x32
#define DPOS_IC_POS		0x33
#define DPOS_REAR		0x34
#define DPOS_REAR_NH		0x35

LONG	
	WINAPI 
	A6_SetDockedPos
	( 
	__in READERHANDLE hReader,
	__in BYTE bDockedPos
	);
LONG	
	WINAPI 
	A6_SetBaudRate
	( 
	__in READERHANDLE hReader, 
	__in DWORD dwBaudRate 
	);

#define LS_LONG_CARD_IN		0x46
#define LS_SHORT_CARD_IN	0x47
#define LS_CARD_IN_FRONT_NH	0x48	// without holding
#define LS_CARD_IN_FRONT	0x49	// whih holding
#define LS_CARD_IN_RF_POS	0x4A
#define LS_CARD_IN_IC_POS	0x4B
#define LS_CARD_IN_REAR		0x4C	// with holding
#define LS_NO_CARD_IN		0x4E

typedef struct _CRSTATUS
{
	BYTE	bLaneStatus;
	BYTE	bFCI;
	BYTE	bRCI;
} CRSTATUS, *PCRSTATUS;

LONG	
	WINAPI 
	A6_GetCRCondition
	( 
	__in READERHANDLE hReader,
	__out PCRSTATUS pStatus
	);

#define NUM_SENSORS	8

LONG	
	WINAPI 
	A6_GetSensorStatus
	( 
	__in READERHANDLE hReader,
	__out BYTE (&bStatus)[NUM_SENSORS]
	);
LONG	
	WINAPI
	A6_GetSensorVoltages
	(
	__in READERHANDLE hReader,
	__out float (&fVoltages)[NUM_SENSORS]
	);

#define ICCTYPE_UNKNOWN		0x0
#define ICCTYPE_MIFARE_S50	0x10
#define ICCTYPE_MIFARE_S70	0x11
#define ICCTYPE_MIFARE_UL	0x12
#define ICCTYPE_TYPEA_CPU	0x13
#define ICCTYPE_TYPEB_CPU	0x14
#define ICCTYPE_T0_CPU		0x20
#define ICCTYPE_T1_CPU		0x21
#define ICCTYPE_AT24C01		0x30
#define ICCTYPE_AT24C02		0x31
#define ICCTYPE_AT24C04		0x32
#define ICCTYPE_AT24C08		0x33
#define ICCTYPE_AT24C16		0x34
#define ICCTYPE_AT24C32		0x35
#define ICCTYPE_AT24C64		0x36
#define ICCTYPE_SLE4442		0x40
#define ICCTYPE_SLE4428		0x41
#define ICCTYPE_AT88SC102	0x50
#define ICCTYPE_AT88SC1604	0x51
#define ICCTYPE_AT88SC1608	0x52
#define ICCTYPE_AT45DB041	0x53

LONG	
	WINAPI 
	A6_DetectIccType
	( 
	__in READERHANDLE hReader, 
	__out PBYTE pbType 
	);

#define ICCST_CONTACT_PRESSED	0x35
#define ICCST_ACTIVATED		0x31
#define ICCST_CONTINUOUS_TRAN	0x34
#define ICCST_TRAN_COMPLETE	0x30
#define ICCST_NO_IC_CARD	0x39

LONG
	WINAPI
	A6_GetIccStatus
	(
	__in READERHANDLE hReader,
	__out PBYTE	pbStatus
	);

#define MOVE_TO_FRONT_NH	0x30
#define MOVE_TO_FRONT		0x31
#define MOVE_TO_RF_POS		0x2E
#define MOVE_TO_IC_POS		0x2F
#define MOVE_TO_REAR		0x32
#define MOVE_TO_REAR_NH		0x33

LONG	
	WINAPI 
	A6_MoveCard
	( 
	__in READERHANDLE hReader,
	__in BYTE bMoveMethod
	);
LONG	
	WINAPI 
	A6_LedOn
	( 
	__in READERHANDLE hReader 
	);
LONG	
	WINAPI 
	A6_LedOff
	( 
	__in READERHANDLE hReader 
	);
LONG	
	WINAPI 
	A6_LedBlink
	( 
	__in READERHANDLE hReader,
	__in BYTE bOnTime,
	__in BYTE bOffTime
	);
LONG	
	WINAPI 
	A6_IccPowerOn
	( 
	__in READERHANDLE hReader 
	);
LONG	
	WINAPI 
	A6_IccPowerOff
	( 
	__in READERHANDLE hReader 
	);
LONG
	WINAPI
	A6_IccEnableExtIO
	(
	__in READERHANDLE hReader,
	__in BOOL fEnable
	);

// CPU Operations
//

#define ICC_PROTOCOL_T0		0x31
#define ICC_PROTOCOL_T1		0x32

LONG	
	WINAPI 
	A6_CpuColdReset
	( 
	__in READERHANDLE hReader, 
	__out PBYTE pbATRBuff, 
	__inout PDWORD pcbATRLength 
	);
LONG	
	WINAPI 
	A6_CpuWarmReset
	( 
	__in READERHANDLE hReader, 
	__out PBYTE pbATRBuff, 
	__inout PDWORD pcbATRLength 
	);
LONG	
	WINAPI 
	A6_CpuTransmit
	( 
	__in READERHANDLE hReader, 
	__in BYTE bProtocol,
	__in PBYTE pbSendBuff, 
	__in USHORT cbSendLength, 
	__out PBYTE pbRecvBuff, 
	__inout PDWORD pcbRecvLength 
	);


// Mag.card operations
//

// Track IDs
//
#define TRACKID_ISO1		0x10
#define TRACKID_ISO2		0x20
#define TRACKID_ISO3		0x40
#define TRACKID_ALL		0x70	//  (ISO1 + ISO2 + ISO3)


// Track status codes
//
#define TRACKST_NORMAL		0x30
#define TRACKST_NO_SS		0x31
#define TRACKST_NO_ES		0x32
#define TRACKST_PARITY_ERROR	0x33
#define TRACKST_LRC_ERROR	0x34
#define TRACKST_NO_DATA		0x35


// Read modes
//
#define READ_ASCII		0x30
#define READ_BINARY		0x31


// TRACKINFO
//
typedef struct _TRACKINFO
{
	char	Contents[3][512];
	int	Lengths[3];
	BYTE	Status[3];
} TRACKINFO, *PTRACKINFO;


LONG 
	WINAPI 
	A6_ReadTracks
	(
	__in	READERHANDLE hReader,
	__in	BYTE bMode,
	__in	UINT iTrackID,
	__out	PTRACKINFO pTrackInfo
	);


// SAM Operations
//

#define VOLTAGE_1_8		0x2E
#define VOLTAGE_3		0x2F
#define VOLTAGE_5		0x30

LONG	
	WINAPI 
	A6_SamActivate
	( 
	__in READERHANDLE hReader, 
	__in BYTE bSAMNumber, 
	__in BYTE bVoltage, 
	__out PBYTE pbATRBuff, 
	__inout PDWORD pcbATRLength
	);
LONG	
	WINAPI 
	A6_SamDeactivate
	( 
	__in READERHANDLE hReader 
	);
LONG	
	WINAPI 
	A6_SamTransmit
	( 
	__in READERHANDLE hReader, 
	__in BYTE bProtocol,
	__in BYTE bSAMNumber, 
	__in PBYTE pbSendBuff, 
	__in USHORT cbSendLength, 
	__out PBYTE pbRecvBuff, 
	__inout PDWORD pcbRecvLength 
	);

// I2C Operations
//

LONG	
	WINAPI 
	A6_I2cSelect
	( 
	__in READERHANDLE hReader, 
	__in BYTE bCardType 
	);
LONG	
	WINAPI 
	A6_I2cRead
	(
	__in READERHANDLE hReader, 
	__in BYTE bCardType, 
	__in WORD wAddress, 
	__in BYTE bBytesToRead, 
	__out PBYTE pbBuffer,
	__inout PDWORD pcbLength
	);
LONG	
	WINAPI 
	A6_I2cWriteWithoutVerification
	( 
	__in READERHANDLE hReader, 
	__in BYTE bCardType, 
	__in WORD wAddress, 
	__in BYTE bBytesToWrite, 
	__in PBYTE pbBuffer 
	);
LONG	
	WINAPI 
	A6_I2cWriteWithVerification
	( 
	__in READERHANDLE hReader, 
	__in BYTE bCardType, 
	__in WORD wAddress, 
	__in BYTE bBytesToWrite, 
	__in PBYTE pbBuffer 
	);

// SLE4442 Operations
//

LONG	
	WINAPI 
	A6_Sle4442Reset
	(
	__in READERHANDLE hReader, 
	__out PBYTE pbATRBuff, 
	__inout PDWORD pcbATRLength 
	);
LONG	
	WINAPI 
	A6_Sle4442ReadMainMemory
	( 
	__in READERHANDLE hReader, 
	__in BYTE bAddress, 
	__in BYTE bBytesToRead, 
	__out PBYTE pbBuffer,
	__inout PDWORD pcbLength
	);
LONG	
	WINAPI 
	A6_Sle4442ReadProtectionMemory
	( 
	__in READERHANDLE hReader, 
	__out BYTE (&bBuffer)[32]
	);
LONG	
	WINAPI 
	A6_Sle4442ReadSecurityMemory
	( 
	__in READERHANDLE hReader, 
	__out BYTE (&bBuffer)[4]
	);
LONG	
	WINAPI 
	A6_Sle4442WriteMainMemory
	( 
	__in READERHANDLE hReader, 
	__in BYTE bAddress, 
	__in BYTE bBytesToWrite, 
	__in PBYTE pbBuffer  
	);
LONG	
	WINAPI 
	A6_Sle4442WriteProtectionMemory
	( 
	__in READERHANDLE hReader, 
	__in BYTE bAddress, 
	__in BYTE bBytesToWrite, 
	__in PBYTE pbBuffer  
	);
LONG	
	WINAPI 
	A6_Sle4442VerifyPSC
	( 
	__in READERHANDLE hReader, 
	__in BYTE bPSCByte1, 
	__in BYTE bPSCByte2, 
	__in BYTE bPSCByte3 
	);
LONG	
	WINAPI 
	A6_Sle4442UpdatePSC
	( 
	__in READERHANDLE hReader, 
	__in BYTE bPSCByte1, 
	__in BYTE bPSCByte2, 
	__in BYTE bPSCByte3 
	);

// SLE4428 Operations
//

LONG	
	WINAPI 
	A6_Sle4428Reset
	( 
	__in READERHANDLE hReader, 
	__out PBYTE pbATRBuff, 
	__inout PDWORD pcbATRLength 
	);
LONG	
	WINAPI 
	A6_Sle4428ReadWithoutPB
	( 
	__in READERHANDLE hReader, 
	__in WORD wAddress, 
	__in BYTE bBytesToRead, 
	__out PBYTE pbBuffer,
	__inout PDWORD pcbLength
	);
LONG	
	WINAPI 
	A6_Sle4428ReadProtectionBits
	( 
	__in READERHANDLE hReader, 
	__in WORD wAddress, 
	__in BYTE bBytesToRead, 
	__out PBYTE pbBuffer, 
	__inout PDWORD pcbLength
	);
LONG	
	WINAPI 
	A6_Sle4428WriteWithoutPB
	( 
	__in READERHANDLE hReader, 
	__in WORD wAddress, 
	__in BYTE bBytesToWrite, 
	__in PBYTE pbBuffer  
	);
LONG	
	WINAPI 
	A6_Sle4428WriteWithPB
	( 
	__in READERHANDLE hReader, 
	__in WORD wAddress, 
	__in BYTE bBytesToWrite, 
	__in PBYTE pbBuffer  
	);
LONG	
	WINAPI 
	A6_Sle4428VerifyPSC
	( 
	__in READERHANDLE hReader, 
	__in BYTE bPSCByte1, 
	__in BYTE bPSCByte2 
	);
LONG	
	WINAPI 
	A6_Sle4428UpdatePSC
	( 
	__in READERHANDLE hReader, 
	__in BYTE bOldPSCByte1, 
	__in BYTE bOldPSCByte2, 
	__in BYTE bNewPSCByte1, 
	__in BYTE bNewPSCByte2 
	);

// AT45DB041 Operations
//

LONG 
	WINAPI 
	A6_Db041Reset
	(
	__in READERHANDLE hReader
	);
LONG 
	WINAPI 
	A6_Db041ReadOnePage
	(
	__in READERHANDLE hReader,
	__in WORD wAddress, 
	__out BYTE (&bBuffer)[264]
	);
LONG 
	WINAPI 
	A6_Db041WriteOnePage
	(
	__in READERHANDLE hReader,
	__in WORD wAddress, 
	__in BYTE (&bBuffer)[264]
	);


// AT88SC102 Operations
//

LONG
	WINAPI 
	A6_Sc102Reset
	(
	__in READERHANDLE hReader
	);
LONG 
	WINAPI 
	A6_Sc102VerifySC
	(
	__in READERHANDLE hReader,
	__in BYTE bSCByte1,
	__in BYTE bSCByte2
	);
LONG 
	WINAPI 
	A6_Sc102UpdateSC
	(
	__in READERHANDLE hReader,
	__in BYTE bSCByte1,
	__in BYTE bSCByte2
	);
LONG 
	WINAPI 
	A6_Sc102ReadMemory
	(
	__in READERHANDLE hReader,
	__in BYTE bAddress,
	__in BYTE bBytesToRead,
	__out PBYTE pbBuffer,
	__inout PDWORD pcbLength
	);
LONG 
	WINAPI 
	A6_Sc102WriteMemory
	(
	__in READERHANDLE hReader, 
	__in BYTE bAddress, 
	__in BYTE bBytesToWrite,
	__in PBYTE pbBuffer
	);
LONG 
	WINAPI 
	A6_Sc102EraseMemory
	(
	__in READERHANDLE hReader, 
	__in BYTE bAddress, 
	__in BYTE bBytesToErase
	);
LONG 
	WINAPI 
	A6_Sc102EraseAZ1
	(
	__in READERHANDLE hReader,
	__in BYTE (&bKeyBytes)[6]
	);
LONG 
	WINAPI 
	A6_Sc102EraseAZ2
	(
	__in READERHANDLE hReader,
	__in BOOL fEC2Enabled,
	__in BYTE (&bKeyBytes)[4]
	);
LONG 
	WINAPI 
	A6_Sc102UpdateEZ1
	(
	__in READERHANDLE hReader,
	__in BYTE (&bKeyBytes)[6]
	);
LONG 
	WINAPI 
	A6_Sc102UpdateEZ2
	(
	__in READERHANDLE hReader,
	__in BYTE (&bKeyBytes)[4]
	);

// Personalization Modes
//
#define PERSONALIZATION_TEST	0x30
#define PERSONALIZATION_LOGOUT	0x31
#define PERSONALIZATION_REAL	0x32

LONG 
	WINAPI 
	A6_Sc102Personalize
	(
	__in READERHANDLE hReader,
	__in BYTE bMode
	);


// AT88SC1604 Operations
//

LONG 
	WINAPI 
	A6_Sc1604Reset
	(
	__in READERHANDLE hReader
	);

#define PWDTYPE_SC		0x30
#define PWDTYPE_SC1		0x31
#define PWDTYPE_EZ1		0x32
#define PWDTYPE_SC2		0x33
#define PWDTYPE_EZ2		0x34
#define PWDTYPE_SC3		0x35
#define PWDTYPE_EZ3		0x36
#define PWDTYPE_SC4		0x37
#define PWDTYPE_EZ4		0x38

LONG 
	WINAPI 
	A6_Sc1604VerifyPassword
	(
	__in READERHANDLE hReader,
	__in BYTE bPwdType,
	__in BYTE bPwdByte1,
	__in BYTE bPwdByte2
	);
LONG 
	WINAPI 
	A6_Sc1604UpdatePassword
	(
	__in READERHANDLE hReader,
	__in BYTE bPwdType,
	__in BYTE bPwdByte1,
	__in BYTE bPwdByte2
	);
LONG 
	WINAPI 
	A6_Sc1604ReadMemory
	(
	__in READERHANDLE hReader,
	__in WORD wAddress,
	__in BYTE bBytesToRead,
	__out PBYTE pbBuffer,
	__inout PDWORD pcbLength
	);
LONG 
	WINAPI 
	A6_Sc1604WriteMemory
	(
	__in READERHANDLE hReader,
	__in WORD wAddress,
	__in BYTE bBytesToWrite,
	__in PBYTE pbBuffer
	);
LONG 
	WINAPI 
	A6_Sc1604EraseMemory
	(
	__in READERHANDLE hReader,
	__in WORD wAddress,
	__in BYTE bBytesToErase
	);
LONG 
	WINAPI 
	A6_Sc1604Personalize
	(
	__in READERHANDLE hReader,
	__in BYTE bMode		// see A6_Sc102Personalize
	);

// AT88SC1608 Operations
//

#define ZONEID_USER0		0x30
#define ZONEID_USER1		0x31
#define ZONEID_USER2		0x32
#define ZONEID_USER3		0x33
#define ZONEID_USER4		0x34
#define ZONEID_USER5		0x35
#define ZONEID_USER6		0x36
#define ZONEID_USER7		0x37
#define ZONEID_CONFIG		0x38

LONG 
	WINAPI 
	A6_Sc1608Reset
	(
	__in READERHANDLE hReader
	);
LONG 
	WINAPI 
	A6_Sc1608VerifyReadPassword
	(
	__in READERHANDLE hReader,
	__in BYTE bZoneID,
	__in BYTE bPwdByte1,
	__in BYTE bPwdByte2,
	__in BYTE bPwdByte3
	);
LONG 
	WINAPI 
	A6_Sc1608VerifyWritePassword
	(
	__in READERHANDLE hReader,
	__in BYTE bZoneID,
	__in BYTE bPwdByte1,
	__in BYTE bPwdByte2,
	__in BYTE bPwdByte3
	);
LONG 
	WINAPI 
	A6_Sc1608ReadMemory
	(
	__in READERHANDLE hReader,
	__in BYTE bZoneID,
	__in BYTE bAddress,
	__in BYTE bBytesToRead,
	__out PBYTE pbBuffer,
	__inout PDWORD pcbLength
	);
LONG 
	WINAPI 
	A6_Sc1608WriteMemory
	(
	__in READERHANDLE hReader,
	__in BYTE bZoneID,
	__in BYTE bAddress,
	__in BYTE bBytesToWrite,
	__in PBYTE pbBuffer
	);

#define FUSE_BURNED		0x30
#define FUSE_UNBURED		0x31

LONG 
	WINAPI 
	A6_Sc1608ReadFuses
	(
	__in READERHANDLE hReader,
	__out PBYTE pbFAB,
	__out PBYTE pbCMA,
	__out PBYTE pbPER
	);
LONG 
	WINAPI 
	A6_Sc1608WriteFuses
	(
	__in READERHANDLE hReader
	);
LONG 
	WINAPI 
	A6_Sc1608InitAuth
	(
	__in READERHANDLE hReader,
	__in BYTE (&bRandomNumberBytes)[8]
	);
LONG 
	WINAPI 
	A6_Sc1608VerifyAuth
	(
	__in READERHANDLE hReader,
	__in BYTE (&bChallengeBytes)[8]
	);


// Mifare 1k
//

LONG
	WINAPI
	A6_SxxSelect
	(
	__in READERHANDLE hReader
	);
LONG
	WINAPI
	A6_SxxGetUID
	(
	__in READERHANDLE hReader,
	__out PBYTE pbUIDBuff,
	__inout PDWORD pcbUIDLength
	);
LONG
	WINAPI
	A6_SxxVerifyPassword
	(
	__in READERHANDLE hReader,
	__in BYTE bSectorNumber,
	__in BOOL bWithKeyA,
	__in BYTE (&bKeyBytes)[6]
	);
LONG
	WINAPI
	A6_SxxUpdatePassword
	(
	__in READERHANDLE hReader,
	__in BYTE bSectorNumber,
	__in BYTE (&bKeyBytes)[6]
	);
LONG
	WINAPI
	A6_SxxReadBlock
	(
	__in READERHANDLE hReader,
	__in BYTE bSectorNumber,
	__in BYTE bBlockNumber,
	__out BYTE (&bBuffer)[16]
	);
LONG
	WINAPI
	A6_SxxWriteBlock
	(
	__in READERHANDLE hReader,
	__in BYTE bSectorNumber,
	__in BYTE bBlockNumber,
	__in BYTE (&bBuffer)[16]
	);
LONG
	WINAPI
	A6_S50InitializeValue
	(
	__in READERHANDLE hReader,
	__in BYTE bSectorNumber,
	__in BYTE bBlockNumber,
	__in UINT uValue
	);
LONG
	WINAPI
	A6_S70InitializeValue
	(
	__in READERHANDLE hReader,
	__in BYTE bSectorNumber,
	__in BYTE bBlockNumber,
	__in UINT uValue
	);
LONG
	WINAPI
	A6_S50ReadValue
	(
	__in READERHANDLE hReader,
	__in BYTE bSectorNumber,
	__in BYTE bBlockNumber,
	__out PUINT puValue
	);
LONG
	WINAPI
	A6_S70ReadValue
	(
	__in READERHANDLE hReader,
	__in BYTE bSectorNumber,
	__in BYTE bBlockNumber,
	__out PUINT puValue
	);
LONG
	WINAPI
	A6_SxxIncrement
	(
	__in READERHANDLE hReader,
	__in BYTE bSectorNumber,
	__in BYTE bBlockNumber,
	__in UINT uValue
	);
LONG
	WINAPI
	A6_SxxDecrement
	(
	__in READERHANDLE hReader,
	__in BYTE bSectorNumber,
	__in BYTE bBlockNumber,
	__in UINT uValue
	);

// Ultralight
//

LONG 
	WINAPI 
	A6_UlSelect
	(
	__in	READERHANDLE hReader
	);
LONG 
	WINAPI 
	A6_UlGetUID
	(
	__in	READERHANDLE hReader,
	__out	PBYTE	pbUIDBuff,
	__inout	PDWORD	pcbUIDLength
	);
LONG 
	WINAPI 
	A6_UlReadSector
	(
	__in	READERHANDLE hReader,
	__in	BYTE	bSectorNumber,
	__out	BYTE	(&bBuffer)[4]
	);
LONG 
	WINAPI 
	A6_UlWriteSector
	(
	__in	READERHANDLE hReader,
	__in	BYTE	bSectorNumber,
	__in	BYTE	(&bBuffer)[4]
	);

// Type A/B CPU
//

LONG 
	WINAPI 
	A6_TypeACpuSelect
	(
	__in	READERHANDLE hReader,
	__out	PBYTE	pbATRBuff,
	__inout	PDWORD	pcbATRLength
	);
LONG 
	WINAPI 
	A6_TypeBCpuSelect
	(
	__in	READERHANDLE hReader,
	__out	PBYTE	pbATRBuff,
	__inout	PDWORD	pcbATRLength
	);
LONG 
	WINAPI 
	A6_TypeABCpuDeselect
	(
	__in	READERHANDLE hReader
	);
LONG 
	WINAPI 
	A6_TypeABCpuTransmit
	(
	__in	READERHANDLE hReader,
	__in	PBYTE	pbSendBuff,
	__in	USHORT	cbSendLength,
	__out	PBYTE	pbRecvBuff,
	__inout	PDWORD	pcbRecvLength
	);
LONG 
	WINAPI 
	A6_TypeACpuGetUID
	(
	__in	READERHANDLE hReader,
	__out	PBYTE	pbUIDBuff,
	__inout	PDWORD	pcbUIDLength
	);

#endif // _A6CRTAPI_H