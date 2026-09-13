using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ChryslerScanner.Helpers;
using ChryslerScanner.Properties;

namespace ChryslerScanner;

public class PCI
{
	public PCIDiagnosticsTable Diagnostics = new PCIDiagnosticsTable();

	private const int HexBytesColumnStart = 2;

	private const int DescriptionColumnStart = 28;

	private const int ValueColumnStart = 82;

	private const int UnitColumnStart = 108;

	private string state = string.Empty;

	private string speed = "10416 baud";

	private string logic = "non-inverted";

	public string HeaderUnknown = "│ PCI-BUS (SAE J1850 VPW) │ STATE: N/A                                                                                   ";

	public string HeaderDisabled = "│ PCI-BUS (SAE J1850 VPW) │ STATE: DISABLED                                                                              ";

	public string HeaderEnabled = "│ PCI-BUS (SAE J1850 VPW) │ STATE: ENABLED @ BAUD | LOGIC: | ID BYTES:                                                   ";

	public string EmptyLine = "│                         │                                                     │                         │             │";

	public string HeaderModified = string.Empty;

	public string VIN = "-----------------";

	private byte[] SKIMPayload = new byte[5];

	private byte[] SKIMPayloadPCM = new byte[5];

	public void UpdateHeader(string state = "enabled", string speed = null, string logic = null)
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
		if (this.state == "enabled" && this.speed != null && this.logic != null)
		{
			HeaderModified = HeaderEnabled.Replace("@ BAUD", "@ " + this.speed.ToUpper()).Replace("LOGIC:", "LOGIC: " + this.logic.ToUpper()).Replace("ID BYTES: ", "ID BYTES: " + (Diagnostics.UniqueIDByteList.Count + Diagnostics.IDByte2426List.Count));
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
			array3 = new byte[data.Length - 6];
			Array.Copy(data, 5, array3, 0, array3.Length);
		}
		List<string> list = new List<string>();
		string text = string.Empty;
		string text2 = string.Empty;
		string text3 = string.Empty;
		byte b = array2[0];
		string text4;
		switch (b)
		{
		case 0:
			return;
		case 10:
			text4 = "AIRBAG LAMP REQUEST";
			if (array2.Length >= 4)
			{
			}
			break;
		case 16:
			text4 = "ENGINE SPEED | VEHICLE SPEED | MAP";
			if (array2.Length >= 7)
			{
				double value3 = (double)((array3[0] << 8) + array3[1]) * 0.25;
				double num2 = (double)((array3[2] << 8) + array3[3]) * 0.0049;
				double value4 = num2 * 1.609344;
				byte b3 = array3[4];
				double value5 = (double)(int)b3 * 0.14504;
				if (Settings.Default.Units == "imperial")
				{
					text4 = "ENGINE: " + Math.Round(value3, 1).ToString("0.0") + " RPM | VEHICLE: " + Math.Round(num2, 1).ToString("0.0") + " MPH";
					text2 = "MAP: " + Math.Round(value5, 1).ToString("0.0") + " PSI";
				}
				else if (Settings.Default.Units == "metric")
				{
					text4 = "ENGINE: " + Math.Round(value3, 1).ToString("0.0") + " RPM | VEHICLE: " + Math.Round(value4, 1).ToString("0.0") + " KM/H";
					text2 = "MAP: " + b3.ToString("0") + " KPA";
				}
			}
			break;
		case 20:
		{
			text4 = "VEHICLE SPEED SENSOR PULSE INTERVAL";
			if (array2.Length < 4)
			{
				break;
			}
			ushort num6 = (ushort)((array3[0] << 8) + array3[1]);
			if (num6 != 0)
			{
				double num7 = 28800.0 / (double)(int)num6;
				double value7 = num7 * 1.609344;
				if (Settings.Default.Units == "imperial")
				{
					text2 = ((num6 == ushort.MaxValue) ? "0.0" : Math.Round(num7, 1).ToString("0.0"));
					text3 = "MPH";
				}
				else if (Settings.Default.Units == "metric")
				{
					text2 = ((num6 == ushort.MaxValue) ? "0.0" : Math.Round(value7, 1).ToString("0.0"));
					text3 = "KM/H";
				}
			}
			break;
		}
		case 22:
			text4 = "STATUS: ";
			if (array2.Length < 5)
			{
				break;
			}
			if (array3[0] == 0 && array3[1] == 0)
			{
				text4 = "STATUS EMPTY";
				break;
			}
			if (Util.IsBitSet(array3[0], 7))
			{
				list.Add("-7-");
			}
			if (Util.IsBitSet(array3[0], 6))
			{
				list.Add("-6-");
			}
			if (Util.IsBitSet(array3[0], 5))
			{
				list.Add("-5-");
			}
			if (Util.IsBitSet(array3[0], 4))
			{
				list.Add("TFR");
			}
			if (Util.IsBitSet(array3[0], 3))
			{
				list.Add("-3-");
			}
			if (Util.IsBitSet(array3[0], 2))
			{
				list.Add("-2-");
			}
			if (Util.IsBitSet(array3[0], 1) && Util.IsBitSet(array3[0], 1))
			{
				list.Add("TMR");
			}
			else
			{
				if (Util.IsBitSet(array3[0], 1))
				{
					list.Add("TM1");
				}
				if (Util.IsBitSet(array3[0], 0))
				{
					list.Add("TM0");
				}
			}
			foreach (string item in list)
			{
				text = text + item + " | ";
			}
			if (text.Length > 2)
			{
				text = text.Remove(text.Length - 3);
			}
			text4 += text;
			break;
		case 26:
		{
			text4 = "TPS | CRUISE SET SPEED | CRUISE STATE | TARGET IDLE";
			if (array2.Length < 6)
			{
				break;
			}
			double value = (double)(int)array3[0] * 0.0196;
			double num = (int)array3[1];
			double value2 = num / 1.609344;
			byte b2 = array3[2];
			double a = (double)(int)array3[3] * 32.0 * 0.25;
			string text5 = string.Empty;
			if (b2 == 0)
			{
				text5 = Convert.ToString(b2, 2).PadLeft(8, '0');
			}
			else
			{
				if (Util.IsBitSet(b2, 7))
				{
					list.Add("-7-");
				}
				if (Util.IsBitSet(b2, 6))
				{
					list.Add("-6-");
				}
				if (Util.IsBitSet(b2, 5))
				{
					list.Add("-5-");
				}
				if (Util.IsBitSet(b2, 4))
				{
					list.Add("-4-");
				}
				if (Util.IsBitSet(b2, 3))
				{
					list.Add("-3-");
				}
				if (Util.IsBitSet(b2, 2))
				{
					list.Add("BPP");
				}
				if (Util.IsBitSet(b2, 1))
				{
					list.Add("CCL");
				}
				if (Util.IsBitSet(b2, 0))
				{
					list.Add("CCE");
				}
				foreach (string item2 in list)
				{
					text = text + item2 + " | ";
				}
				if (text.Length > 2)
				{
					text = text.Remove(text.Length - 3);
				}
			}
			if (Settings.Default.Units == "imperial")
			{
				text4 = "CRUISE SET SPD: " + Math.Round(value2, 1).ToString("0.0") + " MPH | STATE: " + text;
				text2 = "TARGET IDLE: " + Math.Round(a).ToString("0") + " RPM";
			}
			else if (Settings.Default.Units == "metric")
			{
				text4 = "CRUISE SET SPD: " + Math.Round(num, 1).ToString("0.0") + " KM/H | STATE: " + text5;
				text2 = "TARGET IDLE: " + Math.Round(a).ToString("0") + " RPM";
			}
			text3 = "TPS: " + Math.Round(value, 3).ToString("0.000") + "V";
			break;
		}
		case 31:
			text4 = "INSTRUMENT CLUSTER STATUS";
			if (array2.Length >= 4)
			{
				text2 = Util.ByteToHexString(array3, 0, 2);
			}
			break;
		case 36:
			text4 = "REQUEST  |";
			if (array2.Length >= 7)
			{
			}
			break;
		case 37:
			text4 = "FRONT DOOR AJAR SWITCH";
			if (array2.Length >= 4)
			{
			}
			break;
		case 38:
			text4 = "RESPONSE |";
			if (array2.Length >= 7)
			{
			}
			break;
		case 43:
			text4 = "AUTOMATIC TEMPERATURE CONTROL STATUS";
			if (array2.Length >= 5)
			{
				text2 = Util.ByteToHexString(array3, 0, 3);
			}
			break;
		case 45:
			text4 = "INSTRUMENT CLUSTER LAMP STATUS";
			if (array2.Length >= 4)
			{
				text2 = Util.ByteToHexString(array3, 0, 2);
			}
			break;
		case 51:
			text4 = "SEAT BELT SWITCH";
			if (array2.Length >= 3)
			{
				text2 = ((!Util.IsBitSet(array3[0], 0)) ? "OPEN" : "CLOSED");
			}
			break;
		case 53:
			text4 = "STATUS: ";
			if (array2.Length < 4)
			{
				break;
			}
			if (array3[0] == 0 && array3[1] == 0)
			{
				text4 += "ATX";
				break;
			}
			if (Util.IsBitSet(array3[1], 7))
			{
				list.Add("MTX");
			}
			else
			{
				list.Add("ATX");
			}
			if (Util.IsBitSet(array3[1], 6))
			{
				list.Add("-6-");
			}
			if (Util.IsBitSet(array3[1], 5))
			{
				list.Add("-5-");
			}
			if (Util.IsBitSet(array3[1], 4))
			{
				list.Add("-4-");
			}
			if (Util.IsBitSet(array3[1], 3))
			{
				list.Add("ACT");
			}
			if (Util.IsBitSet(array3[1], 2))
			{
				list.Add("BPP");
			}
			if (Util.IsBitSet(array3[1], 1))
			{
				list.Add("TPP");
			}
			if (Util.IsBitSet(array3[1], 0))
			{
				list.Add("CCE");
			}
			if (Util.IsBitSet(array3[0], 7))
			{
				list.Add("-7-");
			}
			if (Util.IsBitSet(array3[0], 6))
			{
				list.Add("-6-");
			}
			if (Util.IsBitSet(array3[0], 5))
			{
				list.Add("-5-");
			}
			if (Util.IsBitSet(array3[0], 4))
			{
				list.Add("-4-");
			}
			if (Util.IsBitSet(array3[0], 3))
			{
				list.Add("-3-");
			}
			if (Util.IsBitSet(array3[0], 2))
			{
				list.Add("CRL");
			}
			if (Util.IsBitSet(array3[0], 1))
			{
				list.Add("-1-");
			}
			if (Util.IsBitSet(array3[0], 0))
			{
				list.Add("SKF");
			}
			foreach (string item3 in list)
			{
				text4 = text4 + item3 + " | ";
			}
			if (text4.Length > 2)
			{
				text4 = text4.Remove(text4.Length - 3);
			}
			break;
		case 55:
			text4 = "SHIFT LEVER POSITION";
			if (array2.Length < 4)
			{
				break;
			}
			text2 = array3[0] switch
			{
				1 => "PARK", 
				2 => "REVERSE", 
				3 => "NEUTRAL", 
				5 => "DRIVE", 
				6 => "AUTOSTICK", 
				_ => "UNDEFINED", 
			};
			if (array3[0] == 6 && Util.IsBitSet(array3[1], 7))
			{
				switch (array3[1] & 0xF0)
				{
				case 144:
					text2 += " | 1ST";
					break;
				case 160:
					text2 += " | 2ND";
					break;
				case 176:
					text2 += " | 3RD";
					break;
				case 192:
					text2 += " | 4TH";
					break;
				}
			}
			break;
		case 58:
			text4 = "TRANSMISSION SELECTED GEAR";
			if (array2.Length >= 3)
			{
				text2 = string.Empty;
				if (Util.IsBitSet(array3[0], 0))
				{
					text2 += "NEUTRAL ";
				}
				if (Util.IsBitSet(array3[0], 1))
				{
					text2 += "REVERSE ";
				}
				if (Util.IsBitSet(array3[0], 2))
				{
					text2 += "1ST ";
				}
				if (Util.IsBitSet(array3[0], 3))
				{
					text2 += "2ND ";
				}
				if (Util.IsBitSet(array3[0], 4))
				{
					text2 += "3RD ";
				}
				if (Util.IsBitSet(array3[0], 5))
				{
					text2 += "4TH ";
				}
				switch ((array3[0] >> 6) & 3)
				{
				case 1:
					text2 += "| LOCK: PART";
					break;
				case 2:
					text2 += "| LOCK: FULL";
					break;
				}
			}
			break;
		case 63:
			text4 = "PCM SEED FOR SKIM";
			if (array2.Length < 6)
			{
				break;
			}
			text2 = Util.ByteToHexString(array3, 0, 4);
			if (!VIN.Contains("-"))
			{
				byte[] sKIMUnlockKey = UnlockAlgorithm.GetSKIMUnlockKey(array3, VIN);
				if (sKIMUnlockKey != null)
				{
					byte[] array4 = new byte[7]
					{
						79,
						192,
						0,
						sKIMUnlockKey[0],
						sKIMUnlockKey[1],
						sKIMUnlockKey[2],
						0
					};
					array4[^1] = Util.CRCCalculator(array4, 0, array4.Length - 1);
					text4 = text4 + " | KEY: " + Util.ByteToHexStringSimple(array4);
				}
			}
			break;
		case 66:
			text4 = "LAST ENGINE SHUTDOWN";
			if (array2.Length >= 4)
			{
				text2 = TimeSpan.FromMinutes((uint)(array3[0] * 60 + array3[1])).ToString("hh\\:mm");
				text3 = "HH:MM";
			}
			break;
		case 72:
			text4 = "REQUEST  |";
			if (array2.Length >= 7)
			{
			}
			break;
		case 79:
			text4 = "SKIM | SECRET KEY AND SEED/KEY VALIDATION";
			if (array2.Length < 7)
			{
				break;
			}
			switch (array3[0])
			{
			case 16:
				text4 = "PAYLOAD FROM PCM EEPROM";
				switch (array3[1] & 3)
				{
				case 1:
					Array.Copy(array3, 2, SKIMPayloadPCM, 0, 3);
					text2 = Util.ByteToHexString(array3, 2, 3);
					break;
				case 2:
					Array.Copy(array3, 2, SKIMPayloadPCM, 3, 2);
					text2 = Util.ByteToHexStringSimple(SKIMPayloadPCM);
					text3 = "EEPROM 01D8";
					break;
				default:
					text2 = "INVALID MSG";
					break;
				}
				break;
			case 64:
				text4 = "PAYLOAD FROM SKIM EEPROM";
				switch (array3[1] & 3)
				{
				case 1:
					Array.Copy(array3, 2, SKIMPayload, 0, 3);
					text2 = Util.ByteToHexString(array3, 2, 3);
					break;
				case 2:
					Array.Copy(array3, 2, SKIMPayload, 3, 2);
					text2 = Util.ByteToHexStringSimple(SKIMPayload);
					text3 = "EEPROM 01D8";
					break;
				default:
					text2 = "INVALID MSG";
					break;
				}
				break;
			case 192:
				switch (array3[1])
				{
				case 0:
					text4 = "SKIM | KEY RECEIVED";
					text2 = Util.ByteToHexString(array3, 2, 3);
					break;
				case 1:
					text4 = "SKIM | SECRET KEY RECEIVED";
					text2 = Util.ByteToHexString(array3, 2, 3);
					break;
				case 128:
					text4 = "SKIM | REQUEST SEED FROM PCM";
					break;
				}
				break;
			}
			break;
		case 82:
		{
			text4 = "A/C RELAY STATES";
			if (array2.Length < 3)
			{
				break;
			}
			List<string> list5 = new List<string>();
			if (array3[0] != 0)
			{
				text4 += " | ";
				if (Util.IsBitSet(array3[0], 7))
				{
					list5.Add("-7-");
				}
				if (Util.IsBitSet(array3[0], 6))
				{
					list5.Add("-6-");
				}
				if (Util.IsBitSet(array3[0], 5))
				{
					list5.Add("-5-");
				}
				if (Util.IsBitSet(array3[0], 4))
				{
					list5.Add("DEFRST");
				}
				if (Util.IsBitSet(array3[0], 3))
				{
					list5.Add("-3-");
				}
				if (Util.IsBitSet(array3[0], 2))
				{
					list5.Add("BLOWER");
				}
				if (Util.IsBitSet(array3[0], 1))
				{
					list5.Add("-1-");
				}
				if (Util.IsBitSet(array3[0], 0))
				{
					list5.Add("CLUTCH");
				}
				foreach (string item4 in list5)
				{
					text4 = text4 + item4 + " | ";
				}
				if (text4.Length > 2)
				{
					text4 = text4.Remove(text4.Length - 3);
				}
			}
			text2 = Convert.ToString(array3[0], 2).PadLeft(8, '0');
			break;
		}
		case 90:
			text4 = "IGNITION SWITCH STATUS";
			if (array2.Length >= 5)
			{
				text2 = Util.ByteToHexString(array3, 0, 3);
			}
			break;
		case 91:
			text4 = "RUN RELAY";
			if (array2.Length >= 4)
			{
			}
			break;
		case 93:
			text4 = "MILEAGE INCREMENT | INJECTOR PULSE WIDTH | FUEL USED";
			if (array2.Length >= 7)
			{
				double num12 = (double)(int)array3[0] * 0.000125;
				double value14 = num12 * 1.609344;
				double num13 = (double)((array3[1] << 8) + array3[2]) * (1.0 / 256.0);
				_ = array3[3];
				_ = array3[4];
				if (Settings.Default.Units == "imperial")
				{
					text2 = Math.Round(num12, 6).ToString("0.000000") + " | " + num13.ToString("0.000");
					text3 = "MI | MS";
				}
				else if (Settings.Default.Units == "metric")
				{
					text2 = Math.Round(value14, 6).ToString("0.000000") + " | " + num13.ToString("0.000");
					text3 = "KM | MS";
				}
			}
			break;
		case 96:
			text4 = "AUTO HEAD LAMP STATUS 1";
			if (array2.Length >= 4)
			{
				text2 = Util.ByteToHexString(array3, 0, 2);
			}
			break;
		case 104:
			text4 = "RESPONSE |";
			if (array2.Length >= 6)
			{
			}
			break;
		case 108:
			text4 = "TCM | FAULT CODE PRESENT";
			if (array2.Length >= 7)
			{
				if (array3[1] == 0 && array3[2] == 0)
				{
					text4 = "TCM | NO FAULT CODE";
				}
				else
				{
					text2 = "OBD2 P" + Util.ByteToHexString(array3, 1, 2).Replace(" ", "");
				}
			}
			break;
		case 110:
			text4 = "PCM BEACON PAYLOAD #1";
			if (array2.Length >= 7)
			{
				text2 = Util.ByteToHexString(array3, 0, 5);
			}
			break;
		case 111:
			text4 = "PCM BEACON PAYLOAD #2";
			if (array2.Length >= 7)
			{
				text2 = Util.ByteToHexString(array3, 0, 5);
			}
			break;
		case 114:
			text4 = "BCM MILEAGE";
			if (array2.Length >= 6)
			{
				double num3 = (double)(uint)((array3[0] << 24) | (array3[1] << 16) | (array3[2] << 8) | array3[3]) * 0.000125;
				double value6 = num3 * 1.609344;
				if (Settings.Default.Units == "imperial")
				{
					text2 = Math.Round(num3, 3).ToString("0.000");
					text3 = "MILE";
				}
				else if (Settings.Default.Units == "metric")
				{
					text2 = Math.Round(value6, 3).ToString("0.000");
					text3 = "KILOMETER";
				}
			}
			break;
		case 135:
			text4 = "UPDATE BEACON MESSAGE PAYLOAD IN PCM EEPROM";
			if (array2.Length >= 4)
			{
				text2 = Util.ByteToHexString(array3, 0, 2);
			}
			break;
		case 141:
			text4 = "RADIO STATUS";
			if (array2.Length >= 4)
			{
				text2 = Util.ByteToHexString(array3, 0, 2);
			}
			break;
		case 160:
			text4 = "DISTANCE TO EMPTY";
			if (array2.Length >= 4)
			{
				double num18 = (double)((array3[0] << 8) + array3[1]) * 0.1;
				double value17 = num18 * 1.609344;
				if (Settings.Default.Units == "imperial")
				{
					text2 = Math.Round(num18, 1).ToString("0.0");
					text3 = "MILE";
				}
				else if (Settings.Default.Units == "metric")
				{
					text2 = Math.Round(value17, 1).ToString("0.0");
					text3 = "KILOMETER";
				}
			}
			break;
		case 163:
			text4 = "AMBIENT TEMPERATURE SENSOR VOLTAGE";
			if (array2.Length >= 4)
			{
				text2 = Math.Round((double)(((array3[0] << 8) + array3[1] >> 2) & 0xFF) * 0.0196, 3).ToString("0.000");
				text3 = "V";
			}
			break;
		case 164:
			text4 = "FUEL LEVEL";
			if (array2.Length >= 3)
			{
				text2 = Math.Round((double)(int)array3[0] * 0.3921568627, 1).ToString("0.0");
				text3 = "PERCENT";
			}
			break;
		case 165:
			text4 = "FUEL LEVEL SENSOR VOLTAGE | FUEL LEVEL";
			if (array2.Length >= 4)
			{
				double value11 = (double)(int)array3[0] * 0.0196;
				double num11 = (double)(int)array3[1] * 0.125;
				double value12 = num11 * 3.785412;
				if (Settings.Default.Units == "imperial")
				{
					text2 = Math.Round(value11, 3).ToString("0.000") + " | " + Math.Round(num11, 1).ToString("0.0");
					text3 = "V | GALLON";
				}
				else if (Settings.Default.Units == "metric")
				{
					text2 = Math.Round(value11, 3).ToString("0.000") + " | " + Math.Round(value12, 1).ToString("0.0");
					text3 = "V | LITER";
				}
			}
			break;
		case 167:
			text4 = "FOB NUMBER/BUTTON";
			if (array2.Length >= 4)
			{
			}
			break;
		case 172:
			text4 = "RADIO CLOCK DISPLAY";
			if (array2.Length >= 4)
			{
				text2 = Util.ByteToHexString(array3) + ":" + Util.ByteToHexString(array3, 1);
				text3 = "HH:MM";
			}
			break;
		case 176:
			text4 = "CHECK ENGINE LAMP STATE";
			if (array2.Length >= 5)
			{
				text2 = ((!Util.IsBitSet(array3[0], 7)) ? "OFF" : "ON");
			}
			break;
		case 177:
		{
			text4 = "SKIM STATUS";
			if (array2.Length < 3)
			{
				break;
			}
			if (array3[0] == 0)
			{
				text2 = "NO WARNING";
				break;
			}
			List<string> list4 = new List<string>();
			if (Util.IsBitSet(array3[0], 7))
			{
				list4.Add("-7-");
			}
			if (Util.IsBitSet(array3[0], 6))
			{
				list4.Add("-6-");
			}
			if (Util.IsBitSet(array3[0], 5))
			{
				list4.Add("CLRKEY");
			}
			if (Util.IsBitSet(array3[0], 4))
			{
				list4.Add("PIN OK");
			}
			if (Util.IsBitSet(array3[0], 3))
			{
				list4.Add("-3-");
			}
			if (Util.IsBitSet(array3[0], 2))
			{
				list4.Add("-2-");
			}
			if (Util.IsBitSet(array3[0], 1))
			{
				list4.Add("FAILURE");
			}
			if (Util.IsBitSet(array3[0], 0))
			{
				list4.Add("WARNING");
			}
			foreach (string item5 in list4)
			{
				text2 = text2 + item5 + " | ";
			}
			if (text2.Length > 2)
			{
				text2 = text2.Remove(text2.Length - 3);
			}
			break;
		}
		case 184:
			text4 = "AIRBAG STATUS";
			if (array2.Length >= 4)
			{
				text2 = Util.ByteToHexString(array3, 0, 2);
			}
			break;
		case 192:
			text4 = "BATTERY | OIL | COOLANT | AMBIENT";
			if (array2.Length >= 6)
			{
				double value15 = (double)(int)array3[0] * 0.0625;
				double num15 = (double)(int)array3[1] * 0.5;
				double value16 = num15 * 6.894757;
				double num16 = array3[2] - 40;
				double a5 = 1.8 * num16 + 32.0;
				double num17 = array3[3] - 40;
				double a6 = 1.8 * num17 + 32.0;
				if (Settings.Default.Units == "imperial")
				{
					text4 = "BATTERY: " + Math.Round(value15, 1).ToString("0.0") + " V | OIL: " + Math.Round(num15, 1).ToString("0.0") + " PSI | COOLANT: " + Math.Round(a5).ToString("0") + " °F";
					text2 = "AMBIENT: " + Math.Round(a6).ToString("0") + " °F";
				}
				else if (Settings.Default.Units == "metric")
				{
					text4 = "BATTERY: " + Math.Round(value15, 1).ToString("0.0") + " V | OIL: " + Math.Round(value16, 1).ToString("0.0") + " KPA | COOLANT: " + num16.ToString("0") + " °C";
					text2 = "AMBIENT: " + num17.ToString("0") + " °C";
				}
			}
			break;
		case 204:
			text4 = "OUTSIDE AIR TEMPERATURE";
			if (array2.Length >= 4)
			{
				double num14 = (double)(int)array3[0] - 70.0;
				double a4 = 1.8 * num14 + 32.0;
				if (Settings.Default.Units == "imperial")
				{
					text2 = Math.Round(a4).ToString("0");
					text3 = "°F";
				}
				else if (Settings.Default.Units == "metric")
				{
					text2 = Math.Round(num14).ToString("0");
					text3 = "°C";
				}
			}
			break;
		case 208:
		{
			text4 = "LIMP-IN STATE";
			if (array2.Length < 4)
			{
				break;
			}
			List<string> list2 = new List<string>();
			if (Util.IsBitSet(array3[1], 7))
			{
				list2.Add("ATS");
			}
			if (Util.IsBitSet(array3[1], 6))
			{
				list2.Add("IAT");
			}
			if (Util.IsBitSet(array3[1], 5))
			{
				list2.Add("FSM");
			}
			if (Util.IsBitSet(array3[1], 4))
			{
				list2.Add("ACP");
			}
			if (Util.IsBitSet(array3[1], 3))
			{
				list2.Add("CHG");
			}
			if (Util.IsBitSet(array3[1], 2))
			{
				list2.Add("CHB");
			}
			if (Util.IsBitSet(array3[1], 1))
			{
				list2.Add("TPS");
			}
			if (Util.IsBitSet(array3[1], 0))
			{
				list2.Add("ECT");
			}
			if (list2.Count > 0)
			{
				text4 = "LIMP: ";
				foreach (string item6 in list2)
				{
					text4 = text4 + item6 + " | ";
				}
				if (text4.Length > 2)
				{
					text4 = text4.Remove(text4.Length - 3);
				}
			}
			else
			{
				text4 = "NO LIMP-IN STATE";
			}
			break;
		}
		case 209:
		{
			text4 = "LIMP-IN STATE | PWM FAN DUTY CYCLE";
			if (array2.Length < 7)
			{
				break;
			}
			List<string> list3 = new List<string>();
			if (Util.IsBitSet(array3[2], 7))
			{
				list3.Add("ATS");
			}
			if (Util.IsBitSet(array3[2], 6))
			{
				list3.Add("IAT");
			}
			if (Util.IsBitSet(array3[2], 5))
			{
				list3.Add("FSM");
			}
			if (Util.IsBitSet(array3[2], 4))
			{
				list3.Add("ACP");
			}
			if (Util.IsBitSet(array3[2], 3))
			{
				list3.Add("CHG");
			}
			if (Util.IsBitSet(array3[2], 2))
			{
				list3.Add("CHB");
			}
			if (Util.IsBitSet(array3[2], 1))
			{
				list3.Add("TPS");
			}
			if (Util.IsBitSet(array3[2], 0))
			{
				list3.Add("ECT");
			}
			if (list3.Count > 0)
			{
				text4 = "LIMP: ";
				foreach (string item7 in list3)
				{
					text4 = text4 + item7 + " | ";
				}
				if (text4.Length > 2)
				{
					text4 = text4.Remove(text4.Length - 3);
				}
			}
			else
			{
				text4 = "NO LIMP-IN STATE";
			}
			double value13 = (double)(int)array3[3] * 0.3921568627;
			text2 = "PWM FAN DUTY: " + Math.Round(value13, 1).ToString("0.0");
			text3 = "PERCENT";
			break;
		}
		case 210:
			text4 = "BARO | IAT | A/C HSP | ETHANOL PERCENT";
			if (array2.Length >= 6)
			{
				byte b4 = array3[0];
				double value8 = (double)(int)b4 * 0.14504;
				double num9 = array3[1] - 40;
				double a3 = 1.8 * num9 + 32.0;
				double num10 = (double)(int)array3[2] * 2.03;
				double value9 = num10 * 6.894757;
				double value10 = (double)(int)array3[3] * 0.5;
				if (Settings.Default.Units == "imperial")
				{
					text4 = "BARO: " + Math.Round(value8, 1).ToString("0.0") + " PSI | IAT: " + Math.Round(a3).ToString("0") + " °F | A/C HSP: " + Math.Round(num10, 1).ToString("0.0") + " PSI";
					text2 = "ETHANOL: " + Math.Round(value10, 1).ToString("0.0") + " PERCENT";
				}
				else if (Settings.Default.Units == "metric")
				{
					text4 = "BARO: " + b4.ToString("0.0") + " KPA | IAT: " + Math.Round(num9).ToString("0") + " °C | A/C HSP: " + Math.Round(value9, 1).ToString("0.0") + " KPA";
					text2 = "ETHANOL: " + Math.Round(value10, 1).ToString("0.0") + " PERCENT";
				}
			}
			break;
		case 223:
			text4 = "PCM MILEAGE";
			if (array2.Length >= 3)
			{
				double num8 = (double)(array3[0] * 256) * 8.192 * 0.25;
				double a2 = num8 * 1.609344;
				if (Settings.Default.Units == "imperial")
				{
					text2 = Math.Round(num8).ToString("0");
					text3 = "MILE";
				}
				else if (Settings.Default.Units == "metric")
				{
					text2 = Math.Round(a2).ToString("0");
					text3 = "KILOMETER";
				}
			}
			break;
		case 228:
			text4 = "AUTO HEAD LAMP STATUS 2";
			if (array2.Length >= 4)
			{
				text2 = Util.ByteToHexString(array3, 0, 2);
			}
			break;
		case 234:
			text4 = "TRANSMISSION TEMPERATURE";
			if (array2.Length >= 3)
			{
				double num4 = array3[0] - 40;
				double num5 = 1.8 * num4 + 32.0;
				if (Settings.Default.Units == "imperial")
				{
					text2 = num5.ToString("0");
					text3 = "°F";
				}
				else if (Settings.Default.Units == "metric")
				{
					text2 = num4.ToString("0");
					text3 = "°C";
				}
			}
			break;
		case 237:
			text4 = "CONFIGURATION | CRBFUL ENGDSP CYLVPC SALENG BSTYLE";
			if (array2.Length >= 7)
			{
				text2 = Util.ByteToHexString(array3, 0, 5);
			}
			break;
		case 240:
			text4 = "VEHICLE IDENTIFICATION NUMBER (VIN) CHARACTER";
			if ((array3[0] == 1 && array2.Length >= 4) || (array3[0] == 2 && array2.Length >= 7) || (array3[0] == 6 && array2.Length >= 7) || (array3[0] == 10 && array2.Length >= 7) || (array3[0] == 14 && array2.Length >= 7))
			{
				VIN = VIN.Remove(array3[0] - 1, array3.Length - 1).Insert(array3[0] - 1, Encoding.ASCII.GetString(array3.Skip(1).Take(array3.Length - 1).ToArray()));
				text2 = VIN;
			}
			break;
		default:
			text4 = string.Empty;
			break;
		}
		string text6 = ((array2.Length >= 9) ? (Util.ByteToHexString(array2, 0, 7) + " .. ") : (Util.ByteToHexString(array2, 0, array2.Length) + " "));
		if (text4.Length > 51)
		{
			text4 = Util.TruncateString(text4, 48) + "...";
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
		stringBuilder.Remove(2, text6.Length);
		stringBuilder.Insert(2, text6);
		stringBuilder.Remove(28, text4.Length);
		stringBuilder.Insert(28, text4);
		stringBuilder.Remove(82, text2.Length);
		stringBuilder.Insert(82, text2);
		stringBuilder.Remove(108, text3.Length);
		stringBuilder.Insert(108, text3);
		ushort modifiedID = ((b != 79) ? ((ushort)((uint)(b << 8) & 0xFF00u)) : ((array3.Length <= 1) ? ((ushort)((uint)(b << 8) & 0xFF00u)) : ((ushort)(((b << 8) & 0xFF00) + array3[0]))));
		Diagnostics.AddRow(modifiedID, stringBuilder.ToString());
		UpdateHeader();
		if (Settings.Default.Timestamp)
		{
			TimeSpan value18 = TimeSpan.FromMilliseconds((array[0] << 24) | (array[1] << 16) | (array[2] << 8) | array[3]);
			string contents = DateTime.Today.Add(value18).ToString("HH:mm:ss.fff") + ",";
			File.AppendAllText(MainForm.PCILogFilename, contents);
		}
		File.AppendAllText(MainForm.PCILogFilename, "PCI," + Util.ByteToHexStringSimple(array2) + Environment.NewLine);
	}
}
