using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using ChryslerScanner.Helpers;
using ChryslerScanner.Properties;

namespace ChryslerScanner;

public class SCITCM
{
	public SCITCMDiagnosticsTable Diagnostics = new SCITCMDiagnosticsTable();

	public DataTable TransmissionDTC = new DataTable("TransmissionDTC");

	public List<byte> TransmissionFaultCodeList = new List<byte>();

	public bool TransmissionFaultCodesSaved = true;

	public byte[] TransmissionDTCList;

	public DataColumn column;

	public DataRow row;

	private const int HexBytesColumnStart = 2;

	private const int DescriptionColumnStart = 28;

	private const int ValueColumnStart = 82;

	private const int UnitColumnStart = 108;

	public string state;

	public string speed;

	public string logic;

	public string configuration;

	public string HeaderUnknown = "│ SCI-BUS (SAE J2610) TCM │ STATE: N/A                                                                                   ";

	public string HeaderDisabled = "│ SCI-BUS (SAE J2610) TCM │ STATE: DISABLED                                                                              ";

	public string HeaderEnabled = "│ SCI-BUS (SAE J2610) TCM │ STATE: ENABLED @ BAUD | LOGIC: | CONFIGURATION:                                              ";

	public string EmptyLine = "│                         │                                                     │                         │             │";

	public string HeaderModified = string.Empty;

	public SCITCM()
	{
		column = new DataColumn();
		column.DataType = typeof(byte);
		column.ColumnName = "id";
		column.ReadOnly = true;
		column.Unique = true;
		TransmissionDTC.Columns.Add(column);
		column = new DataColumn();
		column.DataType = typeof(string);
		column.ColumnName = "description";
		column.ReadOnly = true;
		column.Unique = false;
		TransmissionDTC.Columns.Add(column);
		DataColumn[] primaryKey = new DataColumn[1] { TransmissionDTC.Columns["id"] };
		TransmissionDTC.PrimaryKey = primaryKey;
		new DataSet().Tables.Add(TransmissionDTC);
		row = TransmissionDTC.NewRow();
		row["id"] = 0;
		row["description"] = "UNRECOGNIZED DTC";
		TransmissionDTC.Rows.Add(row);
		TransmissionDTCList = (from r in TransmissionDTC.AsEnumerable()
			select r.Field<byte>("id")).ToArray();
	}

	public void UpdateHeader(string state = "enabled", string speed = null, string logic = null, string configuration = null)
	{
		if (state != null)
		{
			this.state = state;
		}
		if (speed != null)
		{
			this.speed = speed;
		}
		if (logic != null)
		{
			this.logic = logic;
		}
		if (configuration != null)
		{
			this.configuration = configuration;
		}
		if (this.state == "enabled" && this.speed != null && this.logic != null && this.configuration != null)
		{
			HeaderModified = HeaderEnabled.Replace("@ BAUD", "@ " + this.speed.ToUpper()).Replace("LOGIC:", "LOGIC: " + this.logic.ToUpper()).Replace("CONFIGURATION: ", "CONFIGURATION: " + this.configuration);
			HeaderModified = Util.TruncateString(HeaderModified, EmptyLine.Length);
			Diagnostics.UpdateHeader(HeaderModified);
		}
		else if (this.state == "disabled")
		{
			Diagnostics.UpdateHeader(HeaderDisabled);
		}
		else
		{
			Diagnostics.UpdateHeader(HeaderUnknown);
		}
	}

	public void AddMessage(byte[] data)
	{
		if (data == null || data.Length < 5)
		{
			return;
		}
		byte[] array = new byte[4];
		byte[] array2 = new byte[0];
		byte[] array3 = new byte[0];
		if (data.Length >= 4)
		{
			Array.Copy(data, 0, array, 0, 4);
		}
		if (data.Length >= 5)
		{
			array2 = new byte[data.Length - 4];
			Array.Copy(data, 4, array2, 0, array2.Length);
		}
		if (data.Length >= 6)
		{
			array3 = new byte[data.Length - 5];
			Array.Copy(data, 5, array3, 0, array3.Length);
		}
		string text = string.Empty;
		string text2 = string.Empty;
		string text3 = string.Empty;
		byte b = array2[0];
		if (speed == "976.5 baud" || speed == "7812.5 baud")
		{
			text = string.Empty;
		}
		else if (speed == "62500 baud" || speed == "125000 baud")
		{
			new List<byte>();
			new List<byte>();
			List<byte> list = new List<byte>();
			List<byte> list2 = new List<byte>();
			List<byte> list3 = new List<byte>();
			List<byte> list4 = new List<byte>();
			List<byte> list5 = new List<byte>();
			_ = array3.Length / 2;
			switch (b)
			{
			case 0:
				text = "TCM WAKE UP";
				break;
			case 6:
				text = "SET BOOTSTRAP BAUDRATE TO 62500 BAUD";
				text2 = "OK";
				break;
			case 17:
				text = "UPLOAD BOOTWORKER";
				if (array2.Length >= 3)
				{
					ushort num4 = (ushort)((array3[0] << 8) + array3[1]);
					ushort num5 = (ushort)(array3.Length - 3);
					text = "UPLOAD BOOTWORKER | SIZE: " + num4 + " BYTES";
					text2 = Util.ByteToHexString(array3, 2, array3.Length - 3);
					text3 = ((num5 != num4 || array3[^1] != 20) ? "ERROR" : "OK");
				}
				break;
			case 33:
				text = "START BOOTWORKER";
				if (array2.Length > 1)
				{
					text += " | RESULT";
					text2 = Util.ByteToHexStringSimple(array3.ToArray());
				}
				else if (array2.Length == 2 && array3[0] == 34)
				{
					text2 = "FINISHED";
				}
				break;
			case 34:
				text = "EXIT BOOTWORKER";
				break;
			case 36:
				text = "REQUEST/SEND BOOTSTRAP SECURITY SEED/KEY";
				if (array2.Length >= 5)
				{
					if (array2.Length == 5)
					{
						text = "REQUEST BOOTSTRAP SECURITY SEED";
						text2 = ((array2[4] == Util.ChecksumCalculator(array2, 0, array2.Length - 1)) ? ((array2[2] != 39 || array2[3] != 193) ? "ERROR" : "OK") : "CHECKSUM ERROR");
					}
					else if (array2.Length == 7)
					{
						text = "SEND BOOTSTRAP SECURITY KEY";
						text2 = ((array2[6] == Util.ChecksumCalculator(array2, 0, array2.Length - 1)) ? ((array2[2] != 39 || array2[3] != 194) ? "ERROR" : Util.ByteToHexString(array2, 4, 2)) : "CHECKSUM ERROR");
					}
					else
					{
						text = "REQUEST BOOTSTRAP SECURITY SEED";
					}
				}
				break;
			case 38:
				text = "BOOTSTRAP SECURITY STATUS";
				if (array2.Length < 5)
				{
					break;
				}
				if (array2.Length == 5)
				{
					_ = array2[0];
					_ = array2[1];
					_ = array2[2];
					_ = array2[3];
					text = "BOOTSTRAP SECURITY STATUS";
					text2 = ((array2[4] == Util.ChecksumCalculator(array2, 0, array2.Length - 1)) ? ((array2[2] != 103 || array2[3] != 194) ? "LOCKED" : "UNLOCKED") : "CHECKSUM ERROR");
				}
				else if (array2.Length == 7)
				{
					text = "BOOTSTRAP SECURITY SEED RECEIVED";
					if (array2[6] != Util.ChecksumCalculator(array2, 0, array2.Length - 1))
					{
						text2 = "CHECKSUM ERROR";
					}
					else if (array2[2] == 103 && array2[3] == 193)
					{
						text2 = Util.ByteToHexString(array3, 3, 2);
					}
				}
				break;
			case 49:
				text = "WRITE FLASH BLOCK";
				if (array2.Length >= 7)
				{
					list.AddRange(array3.Take(3));
					list2.AddRange(array3.Skip(3).Take(2));
					list3.AddRange(array3.Skip(5));
					ushort num = (ushort)((array3[3] << 8) + array3[4]);
					ushort num3 = (ushort)(array3.Length - 5);
					text = "WRITE FLASH BLOCK | OFFSET: " + Util.ByteToHexStringSimple(list.ToArray()) + " | SIZE: " + Util.ByteToHexStringSimple(list2.ToArray());
					if (num3 == num)
					{
						text2 = Util.ByteToHexStringSimple(list3.ToArray());
						text3 = "OK";
					}
					else
					{
						text2 = array2[^1] switch
						{
							1 => "WRITE ERROR", 
							128 => "INVALID BLOCK SIZE", 
							_ => "UNKNOWN ERROR", 
						};
					}
				}
				break;
			case 52:
			case 70:
				text = "READ FLASH BLOCK";
				if (array2.Length >= 7)
				{
					list.AddRange(array3.Take(3));
					list2.AddRange(array3.Skip(3).Take(2));
					list3.AddRange(array3.Skip(5));
					text = "READ FLASH BLOCK | OFFSET: " + Util.ByteToHexStringSimple(list.ToArray()) + " | SIZE: " + Util.ByteToHexStringSimple(list2.ToArray());
					ushort num = (ushort)((array3[3] << 8) + array3[4]);
					if ((ushort)(array3.Length - 5) == num)
					{
						text2 = Util.ByteToHexStringSimple(list3.ToArray());
						text3 = "OK";
					}
					else
					{
						text2 = ((array2[^1] != 128) ? "UNKNOWN ERROR" : "INVALID BLOCK SIZE");
					}
				}
				break;
			case 55:
				text = "WRITE EEPROM BLOCK";
				if (array2.Length >= 6)
				{
					list.AddRange(array3.Take(2));
					list2.AddRange(array3.Skip(2).Take(2));
					list3.AddRange(array3.Skip(4));
					text = "WRITE EEPROM BLOCK | OFFSET: " + Util.ByteToHexStringSimple(list.ToArray()) + " | SIZE: " + Util.ByteToHexStringSimple(list2.ToArray());
					ushort num = (ushort)((array3[2] << 8) + array3[3]);
					if ((ushort)(array3.Length - 4) == num)
					{
						text2 = Util.ByteToHexStringSimple(list3.ToArray());
						text3 = "OK";
					}
					else
					{
						text2 = array2[^1] switch
						{
							128 => "INVALID BLOCK SIZE", 
							131 => "INVALID OFFSET", 
							_ => "UNKNOWN ERROR", 
						};
					}
				}
				break;
			case 58:
				text = "READ EEPROM BLOCK";
				if (array2.Length >= 6)
				{
					list.AddRange(array3.Take(2));
					list2.AddRange(array3.Skip(2).Take(2));
					list3.AddRange(array3.Skip(4));
					text = "READ EEPROM BLOCK | OFFSET: " + Util.ByteToHexStringSimple(list.ToArray()) + " | SIZE: " + Util.ByteToHexStringSimple(list2.ToArray());
					ushort num = (ushort)((array3[2] << 8) + array3[3]);
					if ((ushort)(array3.Length - 4) == num)
					{
						text2 = Util.ByteToHexStringSimple(list3.ToArray());
						text3 = "OK";
					}
					else
					{
						text2 = array2[^1] switch
						{
							128 => "INVALID BLOCK SIZE", 
							131 => "INVALID OFFSET", 
							_ => "UNKNOWN ERROR", 
						};
					}
				}
				break;
			case 71:
				text = "START BOOTLOADER";
				if (array2.Length < 4)
				{
					text2 = "ERROR";
					break;
				}
				list.AddRange(array3.Take(2));
				text = "START BOOTLOADER | OFFSET: " + Util.ByteToHexStringSimple(list.ToArray());
				text2 = ((array3[2] != 34) ? "ERROR" : "OK");
				break;
			case 76:
				text = "UPLOAD BOOTLOADER";
				if (array2.Length >= 6)
				{
					list4.AddRange(array3.Take(2));
					list5.AddRange(array3.Skip(2).Take(2));
					text = "UPLOAD BOOTLOADER | START: " + Util.ByteToHexStringSimple(list4.ToArray()) + " | END: " + Util.ByteToHexStringSimple(list5.ToArray());
					text2 = Util.ByteToHexString(array3, 4, array3.Length - 4);
					ushort num2 = (ushort)((array3[0] << 8) + array3[1]);
					text3 = (((ushort)((array3[2] << 8) + array3[3]) - num2 + 1 != array3.Length - 4) ? "ERROR" : "OK");
				}
				break;
			case 219:
				text = string.Empty;
				if (array2.Length >= 5)
				{
					text = ((array3[0] != 47 || array3[1] != 216 || array3[2] != 62 || array3[3] != 35) ? "PING" : "BOOTSTRAP MODE NOT PROTECTED");
				}
				break;
			default:
				text = string.Empty;
				break;
			}
		}
		string text4 = ((array2.Length >= 9) ? (Util.ByteToHexString(array2, 0, 7) + " .. ") : (Util.ByteToHexString(array2, 0, array2.Length) + " "));
		if (text.Length > 51)
		{
			text = Util.TruncateString(text, 48) + "...";
		}
		if (text2.Length > 23)
		{
			text2 = Util.TruncateString(text2, 20) + "...";
		}
		if (text3.Length > 11)
		{
			text3 = Util.TruncateString(text3, 8) + "...";
		}
		StringBuilder stringBuilder = new StringBuilder(EmptyLine);
		stringBuilder.Remove(2, text4.Length);
		stringBuilder.Insert(2, text4);
		stringBuilder.Remove(28, text.Length);
		stringBuilder.Insert(28, text);
		stringBuilder.Remove(82, text2.Length);
		stringBuilder.Insert(82, text2);
		stringBuilder.Remove(108, text3.Length);
		stringBuilder.Insert(108, text3);
		ushort modifiedID = ((b != 20) ? ((ushort)((uint)(b << 8) & 0xFF00u)) : ((array3.Length == 0) ? ((ushort)((uint)(b << 8) & 0xFF00u)) : ((ushort)(((b << 8) & 0xFF00) + array3[0]))));
		Diagnostics.AddRow(modifiedID, stringBuilder.ToString());
		UpdateHeader();
		if (Settings.Default.Timestamp)
		{
			TimeSpan value = TimeSpan.FromMilliseconds((array[0] << 24) | (array[1] << 16) | (array[2] << 8) | array[3]);
			string contents = DateTime.Today.Add(value).ToString("HH:mm:ss.fff") + ",";
			File.AppendAllText(MainForm.TCMLogFilename, contents);
		}
		File.AppendAllText(MainForm.TCMLogFilename, "TCM," + Util.ByteToHexStringSimple(array2) + Environment.NewLine);
	}
}
