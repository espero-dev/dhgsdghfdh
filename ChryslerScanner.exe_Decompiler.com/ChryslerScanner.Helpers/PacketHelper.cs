using System;
using ChryslerScanner.Models;

namespace ChryslerScanner.Helpers;

public static class PacketHelper
{
	public enum Bus : byte
	{
		USB,
		CCD,
		PCM,
		TCM,
		PCI
	}

	public enum Command : byte
	{
		Reset = 0,
		Handshake = 1,
		Status = 2,
		Settings = 3,
		Request = 4,
		Response = 5,
		MsgTx = 6,
		MsgRx = 7,
		File = 12,
		Boot = 13,
		Debug = 14,
		Error = 15
	}

	public enum ResetMode : byte
	{
		ResetInit,
		ResetDone
	}

	public enum HandshakeMode : byte
	{
		HandshakeOnly,
		HandshakeAndStatus
	}

	public enum StatusMode : byte
	{
		None
	}

	public enum SettingsMode : byte
	{
		LEDs = 1,
		SetCCDBus = 2,
		SetSCIBus = 3,
		SetLCD = 5,
		SetPCIBus = 6,
		SetProgVolt = 7,
		SetUARTBaudrate = 8
	}

	public enum RequestMode : byte
	{
		HardwareFirmwareInfo = 1,
		Timestamp,
		BatteryVoltage,
		ExtEEPROMChecksum,
		CCDBusVoltages,
		VBBVolts,
		VPPVolts,
		AllVolts
	}

	public enum ResponseMode : byte
	{
		HardwareFirmwareInfo = 1,
		Timestamp,
		BatteryVoltage,
		ExtEEPROMChecksum,
		CCDBusVoltages,
		VBBVolts,
		VPPVolts,
		AllVolts
	}

	public enum MsgTxMode : byte
	{
		Stop = 1,
		Single = 2,
		List = 3,
		Repeat = 4,
		SingleVPP = 130
	}

	public enum MsgRxMode : byte
	{
		Stop = 1,
		Single = 2,
		List = 3,
		Repeat = 4,
		SingleVPP = 130
	}

	public enum FileMode : byte
	{
		StorageInfo = 1,
		FileInfo,
		UploadFile,
		DownloadFile,
		RenameFile,
		DeleteFile,
		FormatStorage
	}

	public enum BootMode : byte
	{
		FlashWrite = 1,
		FlashRead,
		EEPROMWrite,
		EEPROMRead,
		UploadLoader,
		UploadWorker,
		StartWorker,
		ExitWorker,
		CancellationRequest,
		Read68HC11K
	}

	public enum DebugMode : byte
	{
		RandomCCDBusMessages = 1,
		ReadIntEEPROMbyte = 2,
		ReadIntEEPROMblock = 3,
		ReadExtEEPROMbyte = 4,
		ReadExtEEPROMblock = 5,
		WriteIntEEPROMbyte = 6,
		WriteIntEEPROMblock = 7,
		WriteExtEEPROMbyte = 8,
		WriteExtEEPROMblock = 9,
		SetArbitraryUARTSpeed = 10,
		DefaultSettings = 224,
		GetRandomNumber = 225,
		RestorePCMEEPROM = 240,
		GetAW9523Data = 254,
		Test = byte.MaxValue
	}

	public enum ErrorMode : byte
	{
		Ok = 0,
		ErrorLengthInvalidValue = 1,
		ErrorDatacodeInvalidCommand = 2,
		ErrorSubDatacodeInvalidValue = 3,
		ErrorPayloadInvalidValues = 4,
		ErrorPacketChecksumInvalidValue = 5,
		ErrorPacketTimeoutOccured = 6,
		ErrorBufferOverflow = 7,
		ErrorInvalidBus = 8,
		ErrorSCILsNoResponse = 246,
		ErrorNotEnoughMCURAM = 247,
		ErrorSCIHsMemoryPtrNoResponse = 248,
		ErrorSCIHsInvalidMemoryPtr = 249,
		ErrorSCIHsNoResponse = 250,
		ErrorEEPNotFound = 251,
		ErrorEEPRead = 252,
		ErrorEEPWrite = 253,
		ErrorInternal = 254,
		ErrorFatal = byte.MaxValue
	}

	public enum OnOffMode : byte
	{
		Off,
		On
	}

	public enum BaudMode : byte
	{
		ExtraLowBaud = 1,
		LowBaud,
		HighBaud,
		ExtraHighBaud
	}

	public enum SCISpeedMode : byte
	{
		LowSpeed = 1,
		HighSpeed
	}

	public const byte PacketSync = 61;

	public const ushort MinPacketSize = 6;

	public const ushort MaxPacketSize = 4096;

	public static byte[] ExpectedHandshake_V1 = new byte[27]
	{
		61, 0, 23, 129, 0, 67, 72, 82, 89, 83,
		76, 69, 82, 67, 67, 68, 83, 67, 73, 83,
		67, 65, 78, 78, 69, 82, 244
	};

	public static byte[] ExpectedHandshake_V2 = new byte[21]
	{
		61, 0, 17, 129, 0, 67, 72, 82, 89, 83,
		76, 69, 82, 83, 67, 65, 78, 78, 69, 82,
		69
	};

	public static byte[] Serialize(Packet packet)
	{
		if (packet == null)
		{
			return null;
		}
		packet.Sync = 61;
		packet.Length = 2;
		if (packet.Payload != null)
		{
			packet.Length += packet.Payload.Length;
		}
		if (packet.Length > 4092)
		{
			return null;
		}
		byte[] array = new byte[packet.Length + 4];
		array[0] = packet.Sync;
		array[1] = (byte)((uint)(packet.Length >> 8) & 0xFFu);
		array[2] = (byte)((uint)packet.Length & 0xFFu);
		array[3] = (byte)(((packet.Bus << 4) & 0x70) + (packet.Command & 0xF));
		if (packet.Direction)
		{
			array[3] += 128;
		}
		array[4] = packet.Mode;
		if (packet.Length > 2)
		{
			if (packet.Payload == null || packet.Payload.Length < packet.Length - 2)
			{
				return null;
			}
			Array.Copy(packet.Payload, 0, array, 5, packet.Payload.Length);
		}
		packet.Checksum = Util.ChecksumCalculator(array, 0, array.Length - 1);
		array[^1] = packet.Checksum;
		return array;
	}

	public static Packet Deserialize(byte[] bytes)
	{
		if (bytes.Length < 6)
		{
			return null;
		}
		if (bytes[0] != 61)
		{
			return null;
		}
		Packet packet = new Packet();
		packet.Sync = bytes[0];
		packet.Length = (bytes[1] << 8) + bytes[2];
		if (packet.Length > 4092)
		{
			return null;
		}
		if (Util.IsBitSet(bytes[3], 7))
		{
			packet.Direction = true;
		}
		else
		{
			packet.Direction = false;
		}
		packet.Bus = (byte)((uint)(bytes[3] >> 4) & 7u);
		packet.Command = (byte)(bytes[3] & 0xFu);
		packet.Mode = bytes[4];
		if (packet.Length > 2)
		{
			if (bytes.Length < packet.Length - 2)
			{
				return null;
			}
			int num = packet.Length - 2;
			packet.Payload = new byte[num];
			Array.Copy(bytes, 5, packet.Payload, 0, num);
		}
		else
		{
			packet.Payload = null;
		}
		packet.Checksum = bytes[^1];
		if (packet.Checksum != Util.ChecksumCalculator(bytes, 0, bytes.Length - 1))
		{
			return null;
		}
		return packet;
	}
}
