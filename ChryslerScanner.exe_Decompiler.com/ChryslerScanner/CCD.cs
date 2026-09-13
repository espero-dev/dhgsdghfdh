using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ChryslerScanner.Helpers;
using ChryslerScanner.Properties;

namespace ChryslerScanner;

public class CCD
{
	public CCDDiagnosticsTable Diagnostics = new CCDDiagnosticsTable();

	private const int HexBytesColumnStart = 2;

	private const int DescriptionColumnStart = 28;

	private const int ValueColumnStart = 82;

	private const int UnitColumnStart = 108;

	private string state = string.Empty;

	private string speed = "7812.5 baud";

	private string logic = "non-inverted";

	public string HeaderUnknown = "│ CCD-BUS (SAE J1567)     │ STATE: N/A                                                                                   ";

	public string HeaderDisabled = "│ CCD-BUS (SAE J1567)     │ STATE: DISABLED                                                                              ";

	public string HeaderEnabled = "│ CCD-BUS (SAE J1567)     │ STATE: ENABLED @ BAUD | LOGIC: | ID BYTES:                                                   ";

	public string EmptyLine = "│                         │                                                     │                         │             │";

	public string HeaderModified = string.Empty;

	public bool TransmissionLRCVIRequested;

	public bool Transmission24CVIRequested;

	public bool TransmissionODCVIRequested;

	public bool TransmissionUDCVIRequested;

	public bool TransmissionTemperatureRequested;

	public string VIN = "-----------------";

	public string BeaconNote = "------------------";

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
			HeaderModified = HeaderEnabled.Replace("@ BAUD", "@ " + this.speed.ToUpper()).Replace("LOGIC:", "LOGIC: " + this.logic.ToUpper()).Replace("ID BYTES: ", "ID BYTES: " + (Diagnostics.UniqueIDByteList.Count + Diagnostics.B2F2IDByteList.Count));
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
		string text = string.Empty;
		string text2 = string.Empty;
		byte b = array2[0];
		string text3;
		switch (b)
		{
		case 0:
			return;
		case 2:
			text3 = "SHIFT LEVER POSITION";
			if (array2.Length >= 3)
			{
				text = array3[0] switch
				{
					1 => "PARK", 
					2 => "REVERSE", 
					3 => "NEUTRAL", 
					5 => "DRIVE", 
					6 => "AUTOSTICK", 
					_ => "UNDEFINED", 
				};
			}
			break;
		case 7:
			text3 = "RESTORE SKIM SECRET KEY FROM PCM EEPROM";
			if (array2.Length >= 4 && array3[0] >= 16 && array3[0] <= 20)
			{
				SKIMPayloadPCM[array3[0] - 16] = array3[1];
				text = Util.ByteToHexStringSimple(SKIMPayloadPCM);
			}
			break;
		case 10:
			text3 = "SEND DIAGNOSTIC FAILURE DATA";
			if (array2.Length >= 4)
			{
			}
			break;
		case 11:
		{
			text3 = "SKIM STATUS";
			if (array2.Length < 3)
			{
				break;
			}
			List<string> list4 = new List<string>();
			if (array3[0] == 0)
			{
				text = "NO WARNING";
				break;
			}
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
			foreach (string item in list4)
			{
				text = text + item + " | ";
			}
			if (text.Length > 2)
			{
				text = text.Remove(text.Length - 3);
			}
			break;
		}
		case 12:
			text3 = "BATTERY | OIL | COOLANT | AMBIENT";
			if (array2.Length >= 6)
			{
				double value29 = (double)(int)array3[0] * 0.125;
				double num21 = (double)(int)array3[1] * 0.5;
				double value30 = num21 * 6.894757;
				double num22 = array3[2] - 64;
				double value31 = 1.8 * num22 + 32.0;
				double num23 = array3[3] - 64;
				double a9 = 1.8 * num23 + 32.0;
				if (Settings.Default.Units == "imperial")
				{
					text3 = "BATTERY: " + Math.Round(value29, 1).ToString("0.0") + " V | OIL: " + Math.Round(num21, 1).ToString("0.0") + " PSI | COOLANT: " + Math.Round(value31, 1).ToString("0") + " °F";
					text = "AMBIENT: " + Math.Round(a9).ToString("0") + " °F";
				}
				else if (Settings.Default.Units == "metric")
				{
					text3 = "BATTERY: " + Math.Round(value29, 1).ToString("0.0") + " V | OIL: " + Math.Round(value30, 1).ToString("0.0") + " KPA | COOLANT: " + num22.ToString("0") + " °C";
					text = "AMBIENT: " + num23.ToString("0") + " °C";
				}
			}
			break;
		case 13:
			text3 = "PCM BEACON 0D";
			if (array2.Length >= 4)
			{
				string text21 = BeaconNote.Remove(8, 2);
				char c = (char)array3[0];
				string text22 = c.ToString();
				c = (char)array3[1];
				BeaconNote = text21.Insert(8, text22 + c);
				text = ((BeaconNote.Contains("-") || BeaconNote.Contains("ÿ")) ? Util.ByteToHexString(array3, 0, 2) : BeaconNote);
			}
			break;
		case 16:
			text3 = "HVAC MESSAGE";
			if (array2.Length >= 4)
			{
			}
			break;
		case 17:
			text3 = "PCM BEACON 11";
			if (array2.Length >= 4)
			{
				string text4 = BeaconNote.Remove(16, 2);
				char c = (char)array3[0];
				string text5 = c.ToString();
				c = (char)array3[1];
				BeaconNote = text4.Insert(16, text5 + c);
				text = Util.ByteToHexString(array3, 0, 2);
			}
			break;
		case 18:
			text3 = "REQUEST EEPROM READ - COMPASS MINI-TRIP";
			if (array2.Length >= 6)
			{
			}
			break;
		case 22:
			text3 = "SKIM SECURITY STATUS";
			if (array2.Length >= 3)
			{
				text = array3[0] switch
				{
					0 => "DISARMED", 
					1 => "TIMING OUT", 
					2 => "ARMED", 
					4 => "HORN AND LIGHTS", 
					8 => "LIGHTS ONLY", 
					16 => "TIMED OUT", 
					32 => "SELF DIAGS", 
					_ => "NONE", 
				};
			}
			break;
		case 27:
			text3 = "LAST OS TEMPERATURE";
			if (array2.Length >= 4)
			{
			}
			break;
		case 28:
			text3 = "FUEL LEVEL COUNTS";
			if (array2.Length >= 4)
			{
			}
			break;
		case 35:
			text3 = "COUNTRY CODE";
			if (array2.Length >= 4)
			{
				text = array3[0] switch
				{
					0 => "USA", 
					1 => "GULF COAST", 
					2 => "EUROPE", 
					3 => "JAPAN", 
					4 => "MALAYSIA", 
					5 => "INDONESIA", 
					6 => "AUSTRALIA", 
					7 => "ENGLAND", 
					8 => "VENEZUELA", 
					9 => "CANADA", 
					_ => "UNKNOWN", 
				};
			}
			break;
		case 36:
			text3 = "VEHICLE SPEED";
			if (array2.Length >= 4)
			{
				text = array3[0].ToString("0") + " | " + array3[1].ToString("0");
				text2 = "MPH | KM/H";
			}
			break;
		case 37:
			text3 = "FUEL LEVEL";
			if (array2.Length >= 3)
			{
				text = Math.Round((double)(int)array3[0] * 0.3921568627, 1).ToString("0.0");
				text2 = "PERCENT";
			}
			break;
		case 41:
			text3 = "LAST ENGINE SHUTDOWN";
			if (array2.Length >= 4)
			{
				text = TimeSpan.FromMinutes((uint)(array3[0] * 60 + array3[1])).ToString("hh\\:mm");
				text2 = "HH:MM";
			}
			break;
		case 44:
			text3 = "WIPER";
			if (array2.Length >= 3)
			{
				text = array3[0] switch
				{
					4 => "WIPERS ON", 
					8 => "WASH ON", 
					2 => "ARMED", 
					12 => "WIPE / WASH", 
					32 => "INT WIPE", 
					40 => "INT WIPE / WASH", 
					_ => "IDLE", 
				};
			}
			break;
		case 52:
			text3 = "BCM TO MIC MESSAGE";
			if (array2.Length >= 4)
			{
			}
			break;
		case 53:
			text3 = "US/METRIC STATUS | SEAT-BELT:";
			if (array2.Length >= 4)
			{
				text = ((!Util.IsBitSet(array3[0], 1)) ? "US" : "METRIC");
				text3 = ((!Util.IsBitSet(array3[0], 2)) ? "US/METRIC STATUS | SEATBELT: UNBUCKLED" : "US/METRIC STATUS | SEATBELT: BUCKLED");
			}
			break;
		case 54:
			text3 = "PCM BEACON 36";
			if (array2.Length >= 4)
			{
				string text19 = BeaconNote.Remove(0, 2);
				char c = (char)array3[0];
				string text20 = c.ToString();
				c = (char)array3[1];
				BeaconNote = text19.Insert(0, text20 + c);
				text = Util.ByteToHexString(array3, 0, 2);
			}
			break;
		case 58:
		{
			text3 = "INSTRUMENT CLUSTER LAMP STATES";
			if (array2.Length < 4)
			{
				break;
			}
			List<string> list8 = new List<string>();
			if (array3[0] == 0)
			{
				break;
			}
			if (Util.IsBitSet(array3[0], 7))
			{
				list8.Add("SBT");
			}
			if (Util.IsBitSet(array3[0], 6))
			{
				list8.Add("-6-");
			}
			if (Util.IsBitSet(array3[0], 5))
			{
				list8.Add("-5-");
			}
			if (Util.IsBitSet(array3[0], 4))
			{
				list8.Add("ABG");
			}
			if (Util.IsBitSet(array3[0], 3))
			{
				list8.Add("ABG");
			}
			if (Util.IsBitSet(array3[0], 2))
			{
				list8.Add("-2-");
			}
			if (Util.IsBitSet(array3[0], 1))
			{
				list8.Add("-1-");
			}
			if (Util.IsBitSet(array3[0], 0))
			{
				list8.Add("-0-");
			}
			foreach (string item2 in list8)
			{
				text = text + item2 + " | ";
			}
			if (text.Length > 2)
			{
				text = text.Remove(text.Length - 3);
			}
			break;
		}
		case 59:
			text3 = "SEND COMPENSATION AND CHECKSUM DATA";
			if (array2.Length >= 4)
			{
			}
			break;
		case 66:
			text3 = "DELTA TPS VOLTS | CRUISE SET SPEED";
			if (array2.Length >= 4)
			{
				double value21 = (double)(int)array3[0] * 0.0196;
				double num11 = (double)(int)array3[1] * 0.5;
				double value22 = num11 * 1.609344;
				if (Settings.Default.Units == "imperial")
				{
					text = Math.Round(value21, 3).ToString("0.000") + " | " + Math.Round(num11, 1).ToString("0.0");
					text2 = "V | MPH";
				}
				else if (Settings.Default.Units == "metric")
				{
					text = Math.Round(value21, 3).ToString("0.000") + " | " + Math.Round(value22, 1).ToString("0.0");
					text2 = "V | KM/H";
				}
			}
			break;
		case 68:
			text3 = "FUEL USED";
			if (array2.Length >= 4)
			{
				text = ((ushort)((array3[0] << 8) + array3[1])).ToString("0");
			}
			break;
		case 70:
			text3 = "REQUEST CALIBRATION DATA";
			if (array2.Length >= 4)
			{
			}
			break;
		case 75:
			text3 = "N/S AND E/W A/D";
			if (array2.Length >= 4)
			{
			}
			break;
		case 77:
			text3 = "PCM BEACON 4D";
			if (array2.Length >= 4)
			{
				string text8 = BeaconNote.Remove(12, 2);
				char c = (char)array3[0];
				string text9 = c.ToString();
				c = (char)array3[1];
				BeaconNote = text8.Insert(12, text9 + c);
				text = Util.ByteToHexString(array3, 0, 2);
			}
			break;
		case 80:
		{
			text3 = "AIRBAG LAMP REQUEST";
			if (array2.Length < 3)
			{
				break;
			}
			List<string> list7 = new List<string>();
			if (array3[0] == 0)
			{
				break;
			}
			if (Util.IsBitSet(array3[0], 7))
			{
				list7.Add("-7-");
			}
			if (Util.IsBitSet(array3[0], 6))
			{
				list7.Add("-6-");
			}
			if (Util.IsBitSet(array3[0], 5))
			{
				list7.Add("-5-");
			}
			if (Util.IsBitSet(array3[0], 4))
			{
				list7.Add("-4-");
			}
			if (Util.IsBitSet(array3[0], 3))
			{
				list7.Add("-3-");
			}
			if (Util.IsBitSet(array3[0], 2))
			{
				list7.Add("SBT");
			}
			if (Util.IsBitSet(array3[0], 1))
			{
				list7.Add("-1-");
			}
			if (Util.IsBitSet(array3[0], 0))
			{
				list7.Add("ABG");
			}
			foreach (string item3 in list7)
			{
				text = text + item3 + " | ";
			}
			if (text.Length > 2)
			{
				text = text.Remove(text.Length - 3);
			}
			break;
		}
		case 82:
			text3 = "TRANSMISSION GEAR REQUEST";
			if (array2.Length >= 4)
			{
				if (Util.IsBitSet(array3[0], 7) && (array3[0] & 0x70u) != 0)
				{
					text += "AUTOSTICK | ";
				}
				switch (array3[0] & 0x70)
				{
				case 16:
					text += "1ST";
					break;
				case 32:
					text += "2ND";
					break;
				case 48:
					text += "3RD";
					break;
				case 64:
					text += "4TH";
					break;
				}
			}
			break;
		case 84:
		{
			text3 = "BAROMETRIC PRESSURE | INTAKE AIR TEMPERATURE";
			if (array2.Length < 4)
			{
				break;
			}
			if (array3[0] == byte.MaxValue && array3[1] == byte.MaxValue)
			{
				text = "MSG NOT USED";
				break;
			}
			double num2 = (double)(int)array3[0] * 0.1217 * 0.4911542;
			double value15 = num2 * 6.894757;
			double num3 = array3[1] - 128;
			double a2 = 1.8 * num3 + 32.0;
			if (Settings.Default.Units == "imperial")
			{
				text = Math.Round(num2, 1).ToString("0.0") + " | " + Math.Round(a2).ToString("0");
				text2 = "PSI | °F";
			}
			else if (Settings.Default.Units == "metric")
			{
				text = Math.Round(value15, 1).ToString("0.0") + " | " + Math.Round(num3).ToString("0");
				text2 = "KPA | °C";
			}
			break;
		}
		case 86:
			text3 = "TCM | FAULT CODE PRESENT";
			if (array2.Length >= 6)
			{
				if (array3[1] == 0 && array3[2] == 0)
				{
					text3 = "TCM | NO FAULT CODE";
				}
				else
				{
					text = "OBD2 P" + Util.ByteToHexString(array3, 1, 2).Replace(" ", "");
				}
			}
			break;
		case 107:
			text3 = "COMPASS COMP. AND CHECKSUM DATA RECEIVED";
			if (array2.Length >= 4)
			{
			}
			break;
		case 108:
			text3 = "CRUISE STATUS";
			if (array2.Length >= 4)
			{
			}
			break;
		case 109:
			text3 = "VEHICLE IDENTIFICATION NUMBER (VIN) CHARACTER";
			if (array2.Length >= 4)
			{
				if (array3[0] > 0 && array3[0] < 18 && array3[1] >= 48 && array3[1] <= 90)
				{
					string text18 = VIN.Remove(array3[0] - 1, 1);
					int startIndex = array3[0] - 1;
					char c = (char)array3[1];
					VIN = text18.Insert(startIndex, c.ToString());
				}
				text = VIN;
			}
			break;
		case 117:
			text3 = "A/C HIGH-SIDE PRESSURE | FLEX FUEL ETHANOL PERCENT";
			if (array2.Length >= 4)
			{
				double num16 = (double)(int)array3[0] * 1.961;
				double value24 = num16 * 6.894757;
				double value25 = (double)((((ushort)((double)(int)array3[1] * 326.0) << 1) & 0xFF00) >> 8) / 2.0;
				if (Settings.Default.Units == "imperial")
				{
					text = Math.Round(num16, 1).ToString("0.0") + " | " + Math.Round(value25, 1).ToString("0.0");
					text2 = "PSI | %";
				}
				else if (Settings.Default.Units == "metric")
				{
					text = Math.Round(value24, 1).ToString("0.0") + " | " + Math.Round(value25, 1).ToString("0.0");
					text2 = "KPA | %";
				}
			}
			break;
		case 118:
			text3 = "PCM BEACON 76";
			if (array2.Length >= 4)
			{
				string text16 = BeaconNote.Remove(4, 2);
				char c = (char)array3[0];
				string text17 = c.ToString();
				c = (char)array3[1];
				BeaconNote = text16.Insert(4, text17 + c);
				text = Util.ByteToHexString(array3, 0, 2);
			}
			break;
		case 123:
		case 131:
			text3 = "OUTSIDE AIR TEMPERATURE";
			if (array2.Length >= 4)
			{
				double num10 = (double)(int)array3[0] - 70.0;
				double a5 = (num10 - 32.0) / 1.8;
				if (Settings.Default.Units == "imperial")
				{
					text = Math.Round(num10).ToString("0");
					text2 = "°F";
				}
				else if (Settings.Default.Units == "metric")
				{
					text = Math.Round(a5).ToString("0");
					text2 = "°C";
				}
			}
			break;
		case 124:
			text3 = "TRANSMISSION TEMPERATURE";
			if (array2.Length >= 4)
			{
				double num7 = (double)(int)array3[0] * 4.0;
				double num8 = (num7 - 32.0) / 1.8;
				if (Settings.Default.Units == "imperial")
				{
					text = num7.ToString("0");
					text2 = "°F";
				}
				else if (Settings.Default.Units == "metric")
				{
					text = num8.ToString("0");
					text2 = "°C";
				}
			}
			break;
		case 126:
		{
			text3 = "A/C RELAY STATES";
			if (array2.Length < 3)
			{
				break;
			}
			List<string> list10 = new List<string>();
			if (array3[0] != 0)
			{
				text3 += " | ";
				if (Util.IsBitSet(array3[0], 7))
				{
					list10.Add("-7-");
				}
				if (Util.IsBitSet(array3[0], 6))
				{
					list10.Add("-6-");
				}
				if (Util.IsBitSet(array3[0], 5))
				{
					list10.Add("-5-");
				}
				if (Util.IsBitSet(array3[0], 4))
				{
					list10.Add("DEFRST");
				}
				if (Util.IsBitSet(array3[0], 3))
				{
					list10.Add("-3-");
				}
				if (Util.IsBitSet(array3[0], 2))
				{
					list10.Add("BLOWER");
				}
				if (Util.IsBitSet(array3[0], 1))
				{
					list10.Add("-1-");
				}
				if (Util.IsBitSet(array3[0], 0))
				{
					list10.Add("CLUTCH");
				}
				foreach (string item4 in list10)
				{
					text3 = text3 + item4 + " | ";
				}
				if (text3.Length > 2)
				{
					text3 = text3.Remove(text3.Length - 3);
				}
			}
			text = Convert.ToString(array3[0], 2).PadLeft(8, '0');
			break;
		}
		case 129:
			text3 = "RADIO CLOCK DISPLAY";
			if (array2.Length >= 4)
			{
				text = Util.ByteToHexString(array3) + ":" + Util.ByteToHexString(array3, 1);
				text2 = "HH:MM";
			}
			break;
		case 132:
			text3 = "INJECTOR PULSE WIDTH | MILEAGE INCREMENT";
			if (array2.Length >= 4)
			{
				double num17 = (int)array3[0];
				double num18 = (double)(int)array3[1] * 0.000125;
				double value26 = num18 * 1.609344;
				if (Settings.Default.Units == "imperial")
				{
					text = num17.ToString("0") + " | " + Math.Round(num18, 6).ToString("0.000000");
					text2 = "MS | MI";
				}
				else if (Settings.Default.Units == "metric")
				{
					text = num17.ToString("0") + " | " + Math.Round(value26, 6).ToString("0.000000");
					text2 = "MS | KM";
				}
			}
			break;
		case 137:
			text3 = "FUEL EFFICIENCY";
			if (array2.Length >= 4)
			{
				text = Util.ByteToHexStringSimple(new byte[1] { array3[0] }) + " MPG | " + array3[1].ToString("0") + " L/100KM";
			}
			break;
		case 140:
			text3 = "ENGINE COOLANT TEMPERATURE | AMBIENT TEMPERATURE";
			if (array2.Length >= 4)
			{
				double num14 = array3[0] - 128;
				double a6 = 1.8 * num14 + 32.0;
				double num15 = array3[1] - 128;
				double a7 = 1.8 * num15 + 32.0;
				if (Settings.Default.Units == "imperial")
				{
					text = Math.Round(a6).ToString("0") + " | " + Math.Round(a7).ToString("0");
					text2 = "°F | °F";
				}
				else if (Settings.Default.Units == "metric")
				{
					text = num14.ToString("0") + " | " + num15.ToString("0");
					text2 = "°C | °C";
				}
			}
			break;
		case 141:
			text3 = "PCM BEACON 8D";
			if (array2.Length >= 4)
			{
				string text14 = BeaconNote.Remove(10, 2);
				char c = (char)array3[0];
				string text15 = c.ToString();
				c = (char)array3[1];
				BeaconNote = text14.Insert(10, text15 + c);
				text = Util.ByteToHexString(array3, 0, 2);
			}
			break;
		case 142:
			text3 = "STATUS 21";
			if (array2.Length >= 4)
			{
			}
			break;
		case 145:
			text3 = "UPDATE BEACON MESSAGE PAYLOAD IN PCM EEPROM";
			if (array2.Length >= 4)
			{
				text = Util.ByteToHexString(array3, 0, 2);
			}
			break;
		case 147:
			text3 = "SEND CALIBRATION AND VARIANCE DATA";
			if (array2.Length >= 4)
			{
			}
			break;
		case 148:
			text3 = "MIC GAUGE/LAMP STATE";
			if (array2.Length >= 4)
			{
				switch (array3[1])
				{
				case 0:
					text3 = "MIC GAUGE POSITION | FUEL LEVEL";
					text = Util.ByteToHexString(array3);
					break;
				case 1:
					text3 = "MIC GAUGE POSITION | COOLANT TEMPERATURE";
					text = Util.ByteToHexString(array3);
					break;
				case 2:
				case 34:
				case 50:
					text3 = "MIC GAUGE POSITION | SPEEDOMETER";
					text = Util.ByteToHexString(array3);
					break;
				case 3:
				case 7:
				case 35:
				case 39:
				case 51:
				case 55:
					text3 = "MIC GAUGE POSITION | TACHOMETER";
					text = Util.ByteToHexString(array3);
					break;
				default:
					text = Convert.ToString(array3[0], 2).PadLeft(8, '0');
					break;
				}
			}
			break;
		case 149:
			text3 = "FUEL LEVEL SENSOR VOLTAGE | FUEL LEVEL";
			if (array2.Length >= 4)
			{
				double value18 = (double)(int)array3[0] * 0.0196;
				double num6 = (double)(int)array3[1] * 0.125;
				double value19 = num6 * 3.785412;
				if (Settings.Default.Units == "imperial")
				{
					text = Math.Round(value18, 3).ToString("0.000") + " | " + Math.Round(num6, 1).ToString("0.0");
					text2 = "V | GALLON";
				}
				else if (Settings.Default.Units == "metric")
				{
					text = Math.Round(value18, 3).ToString("0.000") + " | " + Math.Round(value19, 1).ToString("0.0");
					text2 = "V | LITER";
				}
			}
			break;
		case 153:
			text3 = "COMPASS CALIBRATION AND VARIANCE DATA RECEIVED";
			if (array2.Length >= 4)
			{
			}
			break;
		case 164:
		{
			text3 = "STATUS: ";
			if (array2.Length < 4)
			{
				break;
			}
			List<string> list5 = new List<string>();
			if (array3[0] == 0 && array3[1] == 0)
			{
				text3 += "ATX";
				break;
			}
			if (Util.IsBitSet(array3[0], 7))
			{
				list5.Add("MTX");
			}
			else
			{
				list5.Add("ATX");
			}
			if (Util.IsBitSet(array3[0], 6))
			{
				list5.Add("-6-");
			}
			if (Util.IsBitSet(array3[0], 5))
			{
				list5.Add("CEL");
			}
			if (Util.IsBitSet(array3[0], 4))
			{
				list5.Add("-4-");
			}
			if (Util.IsBitSet(array3[0], 3))
			{
				list5.Add("ACT");
			}
			if (Util.IsBitSet(array3[0], 2))
			{
				list5.Add("BPP");
			}
			if (Util.IsBitSet(array3[0], 1))
			{
				list5.Add("TPP");
			}
			if (Util.IsBitSet(array3[0], 0))
			{
				list5.Add("CCE");
			}
			if (Util.IsBitSet(array3[1], 7))
			{
				list5.Add("-7-");
			}
			if (Util.IsBitSet(array3[1], 6))
			{
				list5.Add("-6-");
			}
			if (Util.IsBitSet(array3[1], 5))
			{
				list5.Add("-5-");
			}
			if (Util.IsBitSet(array3[1], 4))
			{
				list5.Add("TFR");
			}
			if (Util.IsBitSet(array3[1], 3))
			{
				list5.Add("-3-");
			}
			if (Util.IsBitSet(array3[1], 2))
			{
				list5.Add("CCL");
			}
			if (Util.IsBitSet(array3[1], 1) && Util.IsBitSet(array3[1], 0))
			{
				list5.Add("TMR");
			}
			else
			{
				if (Util.IsBitSet(array3[1], 1))
				{
					list5.Add("TM1");
				}
				if (Util.IsBitSet(array3[1], 0))
				{
					list5.Add("TM0");
				}
			}
			foreach (string item5 in list5)
			{
				text3 = text3 + item5 + " | ";
			}
			if (text3.Length > 2)
			{
				text3 = text3.Remove(text3.Length - 3);
			}
			break;
		}
		case 165:
			text3 = "PWM FAN DUTY CYCLE";
			if (array2.Length >= 4)
			{
				text = Math.Round((double)(int)array3[0] * 0.3921568627, 1).ToString("0.0");
				text2 = "PERCENT";
			}
			break;
		case 166:
			text3 = "PCM SEED FOR SKIM";
			if (array2.Length < 4)
			{
				break;
			}
			text = Util.ByteToHexString(array3, 0, 2);
			if (!VIN.Contains("-"))
			{
				byte[] sKIMUnlockKey = UnlockAlgorithm.GetSKIMUnlockKey(array3, VIN);
				if (sKIMUnlockKey != null)
				{
					byte[] array4 = new byte[6]
					{
						194,
						192,
						sKIMUnlockKey[0],
						sKIMUnlockKey[1],
						sKIMUnlockKey[2],
						0
					};
					array4[^1] = Util.ChecksumCalculator(array4, 0, array4.Length - 1);
					text3 = text3 + " | KEY: " + Util.ByteToHexStringSimple(array4);
				}
			}
			break;
		case 169:
			text3 = "LAST ENGINE SHUTDOWN";
			if (array2.Length >= 3)
			{
				text = TimeSpan.FromMinutes((int)array3[0]).ToString("hh\\:mm");
				if (array3[0] == byte.MaxValue)
				{
					text += "+";
				}
				text2 = "HH:MM";
			}
			break;
		case 170:
			text3 = "VEHICLE THEFT SECURITY STATUS";
			if (array2.Length >= 5)
			{
				text = array3[2] switch
				{
					0 => "DISARMED", 
					1 => "TIMING OUT", 
					2 => "ARMED", 
					4 => "HORN AND LIGHTS", 
					8 => "LIGHTS ONLY", 
					16 => "TIMED OUT", 
					32 => "SELF DIAGS", 
					_ => "INVALID", 
				};
			}
			break;
		case 172:
			text3 = "ENGINE TYPE: ";
			if (array2.Length >= 4)
			{
				text3 += (array3[0] >> 4).ToString("0");
				text3 = text3 + " | SIZE: " + (array3[0] & 0xF).ToString("0");
				text3 = text3 + " | STYLE: " + (array3[1] >> 4).ToString("0");
				text3 = text3 + " | MAKE: " + (array3[1] & 7).ToString("0");
				if (Util.IsBitSet(array3[1], 3))
				{
					text = "AWD";
				}
			}
			break;
		case 177:
		{
			text3 = "WARNING: ";
			if (array2.Length < 3)
			{
				break;
			}
			List<string> list9 = new List<string>();
			if (array3[0] == 0)
			{
				text3 = "NO WARNING";
				break;
			}
			if (Util.IsBitSet(array3[0], 7))
			{
				list9.Add("-7-");
			}
			if (Util.IsBitSet(array3[0], 6))
			{
				list9.Add("-6-");
			}
			if (Util.IsBitSet(array3[0], 5))
			{
				list9.Add("-5-");
			}
			if (Util.IsBitSet(array3[0], 4))
			{
				list9.Add("OVRSPD");
			}
			if (Util.IsBitSet(array3[0], 3))
			{
				list9.Add("-3-");
			}
			if (Util.IsBitSet(array3[0], 2))
			{
				list9.Add("EXTLMP");
			}
			if (Util.IsBitSet(array3[0], 1))
			{
				list9.Add("STBELT");
			}
			if (Util.IsBitSet(array3[0], 0))
			{
				list9.Add("KYNIGN");
			}
			foreach (string item6 in list9)
			{
				text3 = text3 + item6 + " | ";
			}
			if (text3.Length > 2)
			{
				text3 = text3.Remove(text3.Length - 3);
			}
			break;
		}
		case 178:
			text3 = "REQUEST  |";
			if (array2.Length < 6)
			{
				break;
			}
			switch (array3[0])
			{
			case 16:
				switch (array3[1])
				{
				case 0:
					text3 = "REQUEST  | VEHICLE INFO CENTER | RESET";
					break;
				case 16:
					text3 = "REQUEST  | VIC | ACTUATOR TEST";
					text = ((array3[2] != 16) ? Util.ByteToHexString(array3, 2) : "DISPLAY");
					text2 = Util.ByteToHexString(array3, 3);
					break;
				case 18:
					text3 = "REQUEST  | VIC | DIGITAL READ";
					text = ((array3[2] != 0) ? Util.ByteToHexString(array3, 2, 2) : "TRANSFER CASE POSITION");
					break;
				case 20:
					text3 = "REQUEST  | VIC | ANALOG READ";
					text = array3[2] switch
					{
						0 => "WASHER LEVEL", 
						1 => "COOLANT LEVEL", 
						2 => "IGNITION VOLTAGE", 
						_ => Util.ByteToHexString(array3, 2, 2), 
					};
					break;
				case 22:
					text3 = "REQUEST  | VIC | FAULT CODES";
					text = "PAGE: " + Util.ByteToHexString(array3, 2);
					break;
				case 36:
					text3 = "REQUEST  | VIC | SOFTWARE VERSION";
					break;
				case 64:
					text3 = "REQUEST  | VIC | ERASE FAULT CODES";
					text = "PAGE: " + Util.ByteToHexString(array3, 2);
					break;
				default:
					text3 = "REQUEST  | VIC | COMMAND: " + Util.ByteToHexString(array3, 1);
					text = Util.ByteToHexString(array3, 2, 2);
					break;
				}
				break;
			case 24:
			case 27:
				text3 = "REQUEST  | VTS | COMMAND: " + Util.ByteToHexString(array3, 1);
				text = Util.ByteToHexString(array3, 2, 2);
				break;
			case 25:
				switch (array3[1])
				{
				case 0:
					text3 = "REQUEST  | COMPASS MINI-TRIP | RESET";
					break;
				case 16:
					text3 = "REQUEST  | CMT | ACTUATOR TEST";
					text = ((array3[2] != 0) ? Util.ByteToHexString(array3, 2, 2) : "SELF TEST");
					break;
				case 17:
					text3 = "REQUEST  | CMT | ACTUATOR TEST STATUS";
					text = ((array3[2] != 0) ? Util.ByteToHexString(array3, 2, 2) : "SELF TEST");
					break;
				case 18:
					text3 = "REQUEST  | CMT | DIGITAL READ";
					text = ((array3[2] != 0) ? Util.ByteToHexString(array3, 2, 2) : "STEP SWITCH");
					break;
				case 22:
					text3 = "REQUEST  | CMT | FAULT CODES";
					text = "PAGE: " + Util.ByteToHexString(array3, 2);
					break;
				case 32:
					text3 = "REQUEST  | CMT | DIAGNOSTIC DATA";
					text = ((array3[2] != 0) ? Util.ByteToHexString(array3, 2, 2) : "TEMPERATURE");
					break;
				case 34:
					if (array3[2] == 194)
					{
						text3 = "REQUEST  | CMT | MIC AND PCM MESSAGES RECEIVED";
						break;
					}
					text3 = "REQUEST  | CMT | ROM DATA";
					text = Util.ByteToHexString(array3, 2, 2);
					break;
				case 36:
					switch (array3[2])
					{
					case 0:
						text3 = "REQUEST  | CMT | SOFTWARE VERSION";
						break;
					case 1:
						text3 = "REQUEST  | CMT | EEPROM VERSION";
						break;
					default:
						text = Util.ByteToHexString(array3, 2, 2);
						break;
					}
					break;
				case 64:
					text3 = "REQUEST  | CMT | ERASE FAULT CODES";
					text = "PAGE: " + Util.ByteToHexString(array3, 2);
					break;
				default:
					text3 = "REQUEST  | CMT | COMMAND: " + Util.ByteToHexString(array3, 1);
					text = Util.ByteToHexString(array3, 2, 2);
					break;
				}
				break;
			case 30:
				switch (array3[1])
				{
				case 0:
					text3 = "REQUEST  | AIRBAG CONTROL MODULE | RESET";
					break;
				case 22:
					text3 = "REQUEST  | ACM | FAULT CODES";
					text = "PAGE: " + Util.ByteToHexString(array3, 2);
					break;
				case 36:
					text3 = "REQUEST  | ACM | SOFTWARE VERSION";
					break;
				case 64:
					text3 = "REQUEST  | ACM | ERASE FAULT CODES";
					text = "PAGE: " + Util.ByteToHexString(array3, 2);
					break;
				default:
					text3 = "REQUEST  | ACM | COMMAND: " + Util.ByteToHexString(array3, 1);
					text = Util.ByteToHexString(array3, 2, 2);
					break;
				}
				break;
			case 32:
				switch (array3[1])
				{
				case 0:
					text3 = "REQUEST  | BODY CONTROL MODULE | RESET";
					break;
				case 16:
					text3 = "REQUEST  | BCM | ACTUATOR TEST";
					switch (array3[2])
					{
					case 0:
						text = array3[3] switch
						{
							8 => "CHIME", 
							32 => "COURTESY LAMPS", 
							_ => Util.ByteToHexString(array3, 2, 2), 
						};
						break;
					case 1:
						text = array3[3] switch
						{
							4 => "HEADLAMP RELAY", 
							8 => "HORN RELAY", 
							16 => "DOOR LOCK", 
							32 => "DOOR UNLOCK", 
							64 => "DR DOOR UNLOCK", 
							128 => "EBL RELAY", 
							_ => Util.ByteToHexString(array3, 2, 2), 
						};
						break;
					case 2:
						text = array3[3] switch
						{
							32 => "VTSS LAMP", 
							64 => "WIPERS LOW", 
							192 => "WIPERS HIGH", 
							_ => Util.ByteToHexString(array3, 2, 2), 
						};
						break;
					case 3:
						_ = array3[3];
						text = Util.ByteToHexString(array3, 2, 2);
						break;
					case 4:
						text = ((array3[3] != 0) ? Util.ByteToHexString(array3, 2, 2) : "RECAL ATC");
						break;
					case 5:
						_ = array3[3];
						text = Util.ByteToHexString(array3, 2, 2);
						break;
					case 6:
						_ = array3[3];
						text = Util.ByteToHexString(array3, 2, 2);
						break;
					case 7:
						_ = array3[3];
						text = Util.ByteToHexString(array3, 2, 2);
						break;
					case 9:
						_ = array3[3];
						text = Util.ByteToHexString(array3, 2, 2);
						break;
					case 10:
						text = ((array3[3] != 0) ? Util.ByteToHexString(array3, 2, 2) : "ENABLE VTSS");
						break;
					case 11:
						_ = array3[3];
						text = Util.ByteToHexString(array3, 2, 2);
						break;
					case 12:
						_ = array3[3];
						text = Util.ByteToHexString(array3, 2, 2);
						break;
					case 13:
						text = ((array3[3] != 16) ? Util.ByteToHexString(array3, 2, 2) : "ENABLE DOOR LOCK");
						break;
					case 14:
						text = ((array3[3] != 16) ? Util.ByteToHexString(array3, 2, 2) : "DISABLE DOOR LOCK");
						break;
					default:
						text = Util.ByteToHexString(array3, 2, 2);
						break;
					}
					break;
				case 18:
					text3 = "REQUEST  | BCM | DIGITAL READ";
					text = Util.ByteToHexString(array3, 2, 2);
					break;
				case 20:
					text3 = array3[2] switch
					{
						0 => "REQUEST  | BCM | ANALOG READ: PASSENGER DOOR DISARM", 
						1 => "REQUEST  | BCM | ANALOG READ: PANEL LAMPS", 
						2 => "REQUEST  | BCM | ANALOG READ: DRDOOR DISARM", 
						3 => "REQUEST  | BCM | ANALOG READ: HVAC CONTROL HEAD VOLTAGE", 
						4 => "REQUEST  | BCM | ANALOG READ: CONVERT SELECT", 
						5 => "REQUEST  | BCM | ANALOG READ: MODE DOOR", 
						6 => "REQUEST  | BCM | ANALOG READ: DOOR STALL", 
						7 => "REQUEST  | BCM | ANALOG READ: A/C SWITCH", 
						8 => "REQUEST  | BCM | ANALOG READ: DOOR LOCK SWITCH VOLTAGE", 
						9 => "REQUEST  | BCM | ANALOG READ: BATTERY VOLTAGE", 
						11 => "REQUEST  | BCM | ANALOG READ: FUEL LEVEL", 
						12 => "REQUEST  | BCM | ANALOG READ: EVAP TEMP VOLTAGE", 
						10 => "REQUEST  | BCM | ANALOG READ: IGNITION VOLTAGE", 
						13 => "REQUEST  | BCM | ANALOG READ: INTERMITTENT WIPER VOLTAGE", 
						_ => "REQUEST  | BCM | ANALOG READ", 
					};
					text = Util.ByteToHexString(array3, 2, 2);
					break;
				case 22:
					text3 = "REQUEST  | BCM | FAULT CODES";
					text = "PAGE: " + Util.ByteToHexString(array3, 2);
					break;
				case 34:
				{
					text3 = "REQUEST  | BCM | ROM DATA";
					int num19 = (array3[2] << 8) + array3[3] + 1;
					byte[] data2 = new byte[2]
					{
						(byte)((uint)(num19 >> 8) & 0xFFu),
						(byte)((uint)num19 & 0xFFu)
					};
					text = "OFFSET: " + Util.ByteToHexString(array2, 3, 2) + " | " + Util.ByteToHexStringSimple(data2);
					break;
				}
				case 36:
					text3 = "REQUEST  | BCM | MODULE ID";
					break;
				case 42:
					text3 = "REQUEST  | BCM | READ VIN";
					break;
				case 44:
					text3 = "REQUEST  | BCM | WRITE VIN";
					break;
				case 64:
					text3 = "REQUEST  | BCM | ERASE FAULT CODES";
					break;
				case 96:
					text3 = "REQUEST  | BCM | WRITE EEPROM | OFFSET: " + Util.ByteToHexStringSimple(new byte[2]
					{
						array3[2],
						array3[3]
					});
					break;
				case 176:
					text3 = "REQUEST  | BCM | WRITE SETTINGS";
					break;
				case 177:
					text3 = "REQUEST  | BCM | READ SETTINGS";
					break;
				default:
					text3 = "REQUEST  | BCM | COMMAND: " + Util.ByteToHexString(array3, 1);
					text = Util.ByteToHexString(array3, 2, 2);
					break;
				}
				break;
			case 34:
			case 96:
				switch (array3[1])
				{
				case 0:
					text3 = "REQUEST  | MECHANICAL INSTRUMENT CLUSTER | RESET";
					break;
				case 16:
					text3 = "REQUEST  | MIC | ACTUATOR TEST";
					text = array3[2] switch
					{
						0 => "ALL GAUGES", 
						1 => "ALL LAMPS", 
						2 => "ODO/TRIP/PRND3L", 
						3 => "PRND3L SEGMENTS", 
						_ => Util.ByteToHexString(array3, 2, 2), 
					};
					break;
				case 18:
					text3 = "REQUEST  | MIC | DIGITAL READ";
					text = ((array3[2] != 0) ? Util.ByteToHexString(array3, 2, 2) : "ALL SWITCHES");
					break;
				case 22:
					text3 = "REQUEST  | MIC | FAULT CODES";
					text = "PAGE: " + Util.ByteToHexString(array3, 2);
					break;
				case 36:
					text3 = "REQUEST  | MIC | SOFTWARE VERSION";
					break;
				case 64:
					text3 = "REQUEST  | MIC | ERASE FAULT CODES";
					text = "PAGE: " + Util.ByteToHexString(array3, 2);
					break;
				case 224:
					text3 = "REQUEST  | MIC | SELF TEST";
					break;
				default:
					text3 = "REQUEST  | MIC | COMMAND: " + Util.ByteToHexString(array3, 1);
					text = Util.ByteToHexString(array3, 2, 2);
					break;
				}
				break;
			case 65:
			case 66:
				switch (array3[1])
				{
				case 0:
					text3 = "REQUEST  | TRANSMISSION CONTROL MODULE | RESET";
					break;
				case 36:
					text3 = "REQUEST  | TCM | READ ANALOG PARAMETER";
					switch (array3[2])
					{
					case 11:
						TransmissionLRCVIRequested = true;
						text3 = "REQUEST  | TCM | LR CLUTCH VOLUME INDEX (CVI)";
						break;
					case 12:
						Transmission24CVIRequested = true;
						text3 = "REQUEST  | TCM | 24 CLUTCH VOLUME INDEX (CVI)";
						break;
					case 13:
						TransmissionODCVIRequested = true;
						text3 = "REQUEST  | TCM | OD CLUTCH VOLUME INDEX (CVI)";
						break;
					case 14:
						TransmissionUDCVIRequested = true;
						text3 = "REQUEST  | TCM | UD CLUTCH VOLUME INDEX (CVI)";
						break;
					case 16:
						TransmissionTemperatureRequested = true;
						text3 = "REQUEST  | TCM | TRANSMISSION TEMPERATURE";
						break;
					default:
						text = Util.ByteToHexString(array3, 2, 2);
						break;
					}
					break;
				default:
					text3 = "REQUEST  | TCM | COMMAND: " + Util.ByteToHexString(array3, 1);
					text = Util.ByteToHexString(array3, 2, 2);
					break;
				}
				break;
			case 67:
				if (array3[1] == 0)
				{
					text3 = "REQUEST  | ANTILOCK BRAKE SYSTEM | RESET";
					break;
				}
				text3 = "REQUEST  | ABS | COMMAND: " + Util.ByteToHexString(array3, 1);
				text = Util.ByteToHexString(array3, 2, 2);
				break;
			case 80:
				if (array3[1] == 0)
				{
					text3 = "REQUEST  | HVAC | RESET";
					break;
				}
				text3 = "REQUEST  | HVAC | COMMAND: " + Util.ByteToHexString(array3, 1);
				text = Util.ByteToHexString(array3, 2, 2);
				break;
			case 128:
				if (array3[1] == 0)
				{
					text3 = "REQUEST  | DRIVER DOOR MODULE | RESET";
					break;
				}
				text3 = "REQUEST  | DDM | COMMAND: " + Util.ByteToHexString(array3, 1);
				text = Util.ByteToHexString(array3, 2, 2);
				break;
			case 129:
				if (array3[1] == 0)
				{
					text3 = "REQUEST  | PASSENGER DOOR MODULE | RESET";
					break;
				}
				text3 = "REQUEST  | PDM | COMMAND: " + Util.ByteToHexString(array3, 1);
				text = Util.ByteToHexString(array3, 2, 2);
				break;
			case 130:
				if (array3[1] == 0)
				{
					text3 = "REQUEST  | MEMORY SEAT MODULE | RESET";
					break;
				}
				text3 = "REQUEST  | MSM | COMMAND: " + Util.ByteToHexString(array3, 1);
				text = Util.ByteToHexString(array3, 2, 2);
				break;
			case 150:
				if (array3[1] == 0)
				{
					text3 = "REQUEST  | AUDIO SYSTEM MODULE | RESET";
					break;
				}
				text3 = "REQUEST  | ASM | COMMAND: " + Util.ByteToHexString(array3, 1);
				text = Util.ByteToHexString(array3, 2, 2);
				break;
			case 192:
				if (array3[1] == 0)
				{
					text3 = "REQUEST  | SKIM | RESET";
					break;
				}
				text3 = "REQUEST  | SKIM | COMMAND: " + Util.ByteToHexString(array3, 1);
				text = Util.ByteToHexString(array3, 2, 2);
				break;
			case byte.MaxValue:
				if (array3[1] == 0)
				{
					text3 = "REQUEST  | RESET ALL CCD-BUS MODULES";
					break;
				}
				text3 = "REQUEST  | ALL CCD-BUS MODULES | COMMAND: " + Util.ByteToHexString(array3, 1);
				text = Util.ByteToHexString(array3, 2, 2);
				break;
			default:
				text3 = "REQUEST  | MODULE: " + Util.ByteToHexStringSimple(new byte[1] { array3[0] }) + " | COMMAND: " + Util.ByteToHexStringSimple(new byte[1] { array3[1] }) + " | PARAMS: " + Util.ByteToHexStringSimple(new byte[2]
				{
					array3[2],
					array3[3]
				});
				break;
			}
			if (Settings.Default.Timestamp)
			{
				TimeSpan value27 = TimeSpan.FromMilliseconds((array[0] << 24) | (array[1] << 16) | (array[2] << 8) | array[3]);
				string contents2 = DateTime.Today.Add(value27).ToString("HH:mm:ss.fff") + " ";
				File.AppendAllText(MainForm.CCDB2F2LogFilename, contents2);
			}
			File.AppendAllText(MainForm.CCDB2F2LogFilename, "CCD: " + Util.ByteToHexStringSimple(array2) + Environment.NewLine);
			break;
		case 180:
		case 196:
		{
			text3 = "VEHICLE SPEED SENSOR PULSE INTERVAL";
			if (array2.Length < 4)
			{
				break;
			}
			ushort num12 = (ushort)((array3[0] << 8) + array3[1]);
			if (num12 != 0)
			{
				double num13 = 28800.0 / (double)(int)num12;
				double value23 = num13 * 1.609344;
				if (Settings.Default.Units == "imperial")
				{
					text = ((num12 == ushort.MaxValue) ? "0.0" : Math.Round(num13, 1).ToString("0.0"));
					text2 = "MPH";
				}
				else if (Settings.Default.Units == "metric")
				{
					text = ((num12 == ushort.MaxValue) ? "0.0" : Math.Round(value23, 1).ToString("0.0"));
					text2 = "KM/H";
				}
			}
			break;
		}
		case 182:
			text3 = "PCM BEACON B6";
			if (array2.Length >= 4)
			{
				string text12 = BeaconNote.Remove(2, 2);
				char c = (char)array3[0];
				string text13 = c.ToString();
				c = (char)array3[1];
				BeaconNote = text12.Insert(2, text13 + c);
				text = Util.ByteToHexString(array3, 0, 2);
			}
			break;
		case 186:
			text3 = "REQUEST COMPASS CALIBRATION OR VARIANCE";
			if (array2.Length >= 4)
			{
			}
			break;
		case 190:
			text3 = "IGNITION SWITCH POSITION";
			if (array2.Length >= 3)
			{
				text = ((!Util.IsBitSet(array3[0], 4)) ? "OFF" : "ON");
			}
			break;
		case 194:
			text3 = "SKIM | SEED/KEY VALIDATION";
			if (array2.Length >= 6)
			{
				switch (array3[0])
				{
				case 192:
					text3 = "SKIM | KEY RECEIVED";
					text = Util.ByteToHexString(array3, 1, 3);
					break;
				case 193:
					text3 = "SKIM | PAYLOAD #1 TO BE WRITTEN TO PCM EEPROM";
					Array.Copy(array3, 1, SKIMPayload, 0, 3);
					text = Util.ByteToHexString(array3, 1, 3);
					break;
				case 194:
					text3 = "SKIM | PAYLOAD #2 TO BE WRITTEN TO PCM EEPROM";
					Array.Copy(array3, 1, SKIMPayload, 3, 2);
					text = Util.ByteToHexStringSimple(SKIMPayload);
					text2 = "EEPROM 01D8";
					break;
				case 200:
					text3 = "SKIM | REQUEST SEED FROM PCM";
					break;
				default:
					text = "INVALID MSG";
					break;
				}
			}
			break;
		case 202:
			text3 = "WRITE EEPROM";
			if (array2.Length >= 6)
			{
				text3 = array3[0] switch
				{
					27 => "WRITE EEPROM | VTS | ", 
					32 => "WRITE EEPROM | BCM | ", 
					67 => "WRITE EEPROM | ABS | ", 
					_ => "WRITE EEPROM | MODULE ID: " + Util.ByteToHexString(array3) + " | ", 
				} + "OFFSET: " + Util.ByteToHexString(array3, 1, 2);
				text = Util.ByteToHexString(array3, 3);
			}
			break;
		case 203:
			text3 = "SEND COMPASS AND LAST OUTSIDE AIR TEMPERATURE DATA";
			if (array2.Length >= 4)
			{
			}
			break;
		case 204:
			text3 = "PCM MILEAGE | TARGET ENGINE IDLE SPEED";
			if (array2.Length >= 4)
			{
				double num5 = (double)(array3[0] * 256) * 8.192 * 0.25;
				double a3 = num5 * 1.609344;
				double a4 = (double)(int)array3[1] * 32.0 * 0.25;
				if (array3[1] == byte.MaxValue)
				{
					a4 = 0.0;
				}
				if (Settings.Default.Units == "imperial")
				{
					text = Math.Round(num5).ToString("0") + " | " + Math.Round(a4).ToString("0");
					text2 = "MI | RPM";
				}
				else if (Settings.Default.Units == "metric")
				{
					text = Math.Round(a3).ToString("0") + " | " + Math.Round(a4).ToString("0");
					text2 = "KM | RPM";
				}
			}
			break;
		case 205:
			text3 = "PCM BEACON CD";
			if (array2.Length >= 4)
			{
				string text10 = BeaconNote.Remove(14, 2);
				char c = (char)array3[0];
				string text11 = c.ToString();
				c = (char)array3[1];
				BeaconNote = text10.Insert(14, text11 + c);
				text = Util.ByteToHexString(array3, 0, 2);
			}
			break;
		case 206:
			text3 = "BCM MILEAGE";
			if (array2.Length >= 6)
			{
				double num4 = (double)(uint)((array3[0] << 24) | (array3[1] << 16) | (array3[2] << 8) | array3[3]) * 0.000125;
				double value17 = num4 * 1.609344;
				if (Settings.Default.Units == "imperial")
				{
					text = Math.Round(num4, 3).ToString("0.000");
					text2 = "MILE";
				}
				else if (Settings.Default.Units == "metric")
				{
					text = Math.Round(value17, 3).ToString("0.000");
					text2 = "KILOMETER";
				}
			}
			break;
		case 211:
			text3 = "COMPASS DISPLAY";
			if (array2.Length >= 4)
			{
			}
			break;
		case 212:
			text3 = "BATTERY VOLTAGE | CHARGING VOLTAGE";
			if (array2.Length >= 4)
			{
				double value16 = (double)(int)array3[0] * 0.0625;
				text = string.Concat(str2: Math.Round((double)(int)array3[1] * 0.0625, 1).ToString("0.0"), str0: Math.Round(value16, 1).ToString("0.0"), str1: " | ");
				text2 = "V | V";
			}
			break;
		case 218:
			text3 = "MIC SWITCH/LAMP STATE";
			if (array2.Length >= 3)
			{
				text = ((!Util.IsBitSet(array3[0], 6)) ? "CEL OFF" : "CEL ON");
			}
			break;
		case 219:
			text3 = "COMPASS CALL DATA | A/C CLUTCH ON";
			if (array2.Length >= 4)
			{
			}
			break;
		case 220:
			text3 = "TRANSMISSION SELECTED GEAR";
			if (array2.Length >= 3)
			{
				text = string.Empty;
				if (Util.IsBitSet(array3[0], 0))
				{
					text += "NEUTRAL ";
				}
				if (Util.IsBitSet(array3[0], 1))
				{
					text += "REVERSE ";
				}
				if (Util.IsBitSet(array3[0], 2))
				{
					text += "1ST ";
				}
				if (Util.IsBitSet(array3[0], 3))
				{
					text += "2ND ";
				}
				if (Util.IsBitSet(array3[0], 4))
				{
					text += "3RD ";
				}
				if (Util.IsBitSet(array3[0], 5))
				{
					text += "4TH ";
				}
				switch ((array3[0] >> 6) & 3)
				{
				case 1:
					text += "| LOCK: PART";
					break;
				case 2:
					text += "| LOCK: FULL";
					break;
				}
			}
			break;
		case 228:
			text3 = "ENGINE SPEED | INTAKE MANIFOLD ABSOLUTE PRESSURE";
			if (array2.Length >= 4)
			{
				double a8 = (double)(int)array3[0] * 32.0;
				double num20 = (double)(int)array3[1] * 0.1217 * 0.4911542;
				double value28 = num20 * 6.894757;
				if (Settings.Default.Units == "imperial")
				{
					text = Math.Round(a8).ToString("0") + " | " + Math.Round(num20, 1).ToString("0.0");
					text2 = "RPM | PSI";
				}
				else if (Settings.Default.Units == "metric")
				{
					text = Math.Round(a8).ToString("0") + " | " + Math.Round(value28, 1).ToString("0.0");
					text2 = "RPM | KPA";
				}
			}
			break;
		case 236:
		{
			text3 = "LIMP-IN STATE";
			if (array2.Length < 4)
			{
				break;
			}
			List<string> list6 = new List<string>();
			if (Util.IsBitSet(array3[0], 7))
			{
				list6.Add("ATS");
			}
			if (Util.IsBitSet(array3[0], 6))
			{
				list6.Add("IAT");
			}
			if (Util.IsBitSet(array3[0], 5))
			{
				list6.Add("FSM");
			}
			if (Util.IsBitSet(array3[0], 4))
			{
				list6.Add("ACP");
			}
			if (Util.IsBitSet(array3[0], 3))
			{
				list6.Add("CHG");
			}
			if (Util.IsBitSet(array3[0], 2))
			{
				list6.Add("CHB");
			}
			if (Util.IsBitSet(array3[0], 1))
			{
				list6.Add("TPS");
			}
			if (Util.IsBitSet(array3[0], 0))
			{
				list6.Add("ECT");
			}
			if (list6.Count > 0)
			{
				text3 = "LIMP: ";
				foreach (string item7 in list6)
				{
					text3 = text3 + item7 + " | ";
				}
				if (text3.Length > 2)
				{
					text3 = text3.Remove(text3.Length - 3);
				}
			}
			else
			{
				text3 = "NO LIMP-IN STATE";
			}
			text = (array3[1] & 0x1C) switch
			{
				4 => "FUEL: UNLEADED GAS", 
				8 => "FUEL: LEADED GAS", 
				12 => "FUEL: FLEX", 
				16 => "FUEL: CNG", 
				24 => "FUEL: DIESEL", 
				_ => "FUEL: UNKNOWN", 
			};
			if (Util.IsBitSet(array3[1], 1) || Util.IsBitSet(array3[1], 0))
			{
				text2 = "SKIM: " + Convert.ToString(array3[1] & 3, 2).PadLeft(2, '0');
			}
			break;
		}
		case 238:
			text3 = "BCM TRIP DISTANCE";
			if (array2.Length >= 5)
			{
				double num9 = (double)(uint)((array3[0] << 16) | (array3[1] << 8) | array3[2]) * 0.016;
				double value20 = num9 * 1.609344;
				if (Settings.Default.Units == "imperial")
				{
					text = Math.Round(num9, 3).ToString("0.000");
					text2 = "MILE";
				}
				else if (Settings.Default.Units == "metric")
				{
					text = Math.Round(value20, 3).ToString("0.000");
					text2 = "KILOMETER";
				}
			}
			break;
		case 241:
		{
			text3 = "WARNING: ";
			if (array2.Length < 3)
			{
				break;
			}
			List<string> list3 = new List<string>();
			if (array3[0] == 0)
			{
				text3 = "NO WARNING";
				break;
			}
			if (Util.IsBitSet(array3[0], 7))
			{
				list3.Add("-7-");
			}
			if (Util.IsBitSet(array3[0], 6))
			{
				list3.Add("-6-");
			}
			if (Util.IsBitSet(array3[0], 5))
			{
				list3.Add("-5-");
			}
			if (Util.IsBitSet(array3[0], 4))
			{
				list3.Add("BPP");
			}
			if (Util.IsBitSet(array3[0], 3))
			{
				list3.Add("CRT");
			}
			if (Util.IsBitSet(array3[0], 2))
			{
				list3.Add("HIT");
			}
			if (Util.IsBitSet(array3[0], 1))
			{
				list3.Add("LWO");
			}
			if (Util.IsBitSet(array3[0], 0))
			{
				list3.Add("LWF");
			}
			foreach (string item8 in list3)
			{
				text3 = text3 + item8 + " | ";
			}
			if (text3.Length > 2)
			{
				text3 = text3.Remove(text3.Length - 3);
			}
			break;
		}
		case 242:
			text3 = "RESPONSE |";
			if (array2.Length < 6)
			{
				break;
			}
			switch (array3[0])
			{
			case 16:
				switch (array3[1])
				{
				case 0:
					text3 = "RESPONSE | VEHICLE INFO CENTER | RESET COMPLETE";
					break;
				case 16:
					if (array3[2] == 16)
					{
						text3 = "RESPONSE | VIC | DISPLAY TEST";
						break;
					}
					text3 = "RESPONSE | VIC | ACTUATOR TEST";
					text = Util.ByteToHexString(array3, 2, 2);
					break;
				case 18:
					if (array3[2] == 0)
					{
						text3 = "RESPONSE | VIC | TRANSFER CASE POSITION";
						text2 = (array3[3] & 0x1F) switch
						{
							6 => "4WD LO", 
							7 => "ALL 4WD", 
							11 => "PART 4WD", 
							13 => "FULL 4WD", 
							15 => "2WD", 
							31 => "NEUTRAL", 
							_ => "UNDEFINED", 
						};
						List<string> list2 = new List<string>();
						if (Util.IsBitClear(array3[3], 5))
						{
							list2.Add("OUTAGE");
						}
						if (Util.IsBitClear(array3[3], 6))
						{
							list2.Add("TURN");
						}
						if (list2.Count <= 0)
						{
							break;
						}
						foreach (string item9 in list2)
						{
							text = text + item9 + " | ";
						}
						if (text.Length > 2)
						{
							text = text.Remove(text.Length - 3);
						}
						text += " LAMP";
					}
					else
					{
						text3 = "RESPONSE | VIC | DIGITAL READ";
						text = Util.ByteToHexString(array3, 2, 2);
					}
					break;
				case 20:
					switch (array3[2])
					{
					case 0:
					{
						double value13 = (double)(int)array3[3] * 0.0196;
						text3 = "RESPONSE | VIC | WASHER LEVEL SENSOR VOLTAGE";
						text = Math.Round(value13, 3).ToString("0.000");
						text2 = "V";
						break;
					}
					case 1:
					{
						double value12 = (double)(int)array3[3] * 0.0196;
						text3 = "RESPONSE | VIC | COOLANT LEVEL SENSOR VOLTAGE";
						text = Math.Round(value12, 3).ToString("0.000");
						text2 = "V";
						break;
					}
					case 2:
					{
						double value11 = (double)(int)array3[3] * 0.099;
						text3 = "RESPONSE | VIC | IGNITION VOLTAGE";
						text = Math.Round(value11, 3).ToString("0.000");
						text2 = "V";
						break;
					}
					default:
						text3 = "RESPONSE | VIC | ANALOG READ";
						text = Util.ByteToHexString(array3, 2, 2);
						break;
					}
					break;
				case 22:
					text3 = "RESPONSE | VIC | FAULT CODES";
					text = ((array3[3] != 0) ? ("CODE: " + Util.ByteToHexString(array3, 2, 2)) : "NO FAULT CODE");
					break;
				case 36:
					text3 = "RESPONSE | VIC | SOFTWARE VERSION";
					text = Util.ByteToHexString(array3, 2, 2);
					break;
				case 64:
					text3 = "RESPONSE | VIC | ERASE FAULT CODES";
					text = ((array3[3] != 0) ? "FAILED" : "ERASED");
					break;
				case byte.MaxValue:
					text3 = "RESPONSE | VIC | COMMAND ERROR";
					Util.ByteToHexString(array3, 2, 2);
					break;
				default:
					text3 = "RESPONSE | VIC";
					text = Util.ByteToHexString(array3, 2, 2);
					break;
				}
				break;
			case 24:
			case 27:
				if (array3[1] == 0)
				{
					text3 = "RESPONSE | VTS | RESET COMPLETE";
					break;
				}
				text3 = "RESPONSE | VTS";
				text = Util.ByteToHexString(array3, 2, 2);
				break;
			case 25:
				switch (array3[1])
				{
				case 0:
					text3 = "RESPONSE | COMPASS MINI-TRIP | RESET COMPLETE";
					break;
				case 16:
					if (array3[2] == 0)
					{
						text3 = "RESPONSE | CMT | SELF TEST";
						text = ((array3[3] != 1) ? "DENIED" : "RUNNING");
					}
					else
					{
						text3 = "RESPONSE | CMT | ACTUATOR TEST";
						text = Util.ByteToHexString(array3, 2, 2);
					}
					break;
				case 17:
					text3 = "RESPONSE | CMT | ACTUATOR TEST STATUS";
					text = ((array3[2] != 0) ? Util.ByteToHexString(array3, 2, 2) : ((array3[3] != 1) ? "DENIED" : "RUNNING"));
					break;
				case 18:
					if (array3[2] == 0)
					{
						text3 = "RESPONSE | CMT | STEP SWITCH";
						text = ((!Util.IsBitSet(array3[3], 4)) ? "RELEASED" : "PRESSED");
					}
					else
					{
						text3 = "RESPONSE | CMT | DIGITAL READ";
						text = Util.ByteToHexString(array3, 2, 2);
					}
					break;
				case 22:
					text3 = "RESPONSE | CMT | FAULT CODES";
					text = ((array3[3] != 0) ? ("CODE: " + Util.ByteToHexString(array3, 2, 2)) : "NO FAULT CODE");
					break;
				case 32:
					if (array3[2] == 0)
					{
						double num = array3[3] - 40;
						double a = num * 0.555556 - 17.77778;
						text3 = "RESPONSE | CMT | TEMPERATURE";
						if (Settings.Default.Units == "imperial")
						{
							text = num.ToString("0");
							text2 = "°F";
						}
						else if (Settings.Default.Units == "metric")
						{
							text = Math.Round(a).ToString("0");
							text2 = "°C";
						}
					}
					else
					{
						text3 = "RESPONSE | CMT | DIAGNOSTIC DATA";
						text = Util.ByteToHexString(array3, 2, 2);
					}
					break;
				case 34:
					text3 = "RESPONSE | CMT | ROM DATA";
					text = Util.ByteToHexString(array3, 2, 2);
					break;
				case 36:
					switch (array3[2])
					{
					case 0:
						text3 = "RESPONSE | CMT | SOFTWARE VERSION";
						text = Util.ByteToHexString(array3, 2, 2);
						break;
					case 1:
						text3 = "RESPONSE | CMT | EEPROM VERSION";
						text = Util.ByteToHexString(array3, 2, 2);
						break;
					default:
						text3 = "RESPONSE | CMT";
						text = Util.ByteToHexString(array3, 2, 2);
						break;
					}
					break;
				case 64:
					text3 = "RESPONSE | CMT | ERASE FAULT CODES";
					text = ((array3[3] != 0) ? "FAILED" : "ERASED");
					break;
				case byte.MaxValue:
					text3 = "RESPONSE | CMT | COMMAND ERROR";
					text = Util.ByteToHexString(array3, 2, 2);
					break;
				default:
					text3 = "RESPONSE | CMT | COMMAND: " + Util.ByteToHexString(array3, 1);
					text = Util.ByteToHexString(array3, 2, 2);
					break;
				}
				break;
			case 30:
				switch (array3[1])
				{
				case 0:
					text3 = "RESPONSE | AIRBAG CONTROL MODULE | RESET COMPLETE";
					text = Util.ByteToHexString(array3, 2, 2);
					break;
				case 22:
					text3 = "RESPONSE | ACM | FAULT CODES";
					text = ((array3[3] != 0) ? ("CODE: " + Util.ByteToHexString(array3, 2, 2)) : "NO FAULT CODE");
					break;
				case 36:
					text3 = "RESPONSE | ACM | SOFTWARE VERSION";
					text = Util.ByteToHexString(array3, 2, 2);
					break;
				case 64:
					text3 = "RESPONSE | ACM | ERASE FAULT CODES";
					text = ((array3[3] != 0) ? "FAILED" : "ERASED");
					break;
				case byte.MaxValue:
					text3 = "RESPONSE | ACM | COMMAND ERROR";
					text = Util.ByteToHexString(array3, 2, 2);
					break;
				default:
					text3 = "RESPONSE | ACM | COMMAND: " + Util.ByteToHexString(array3, 1);
					text = Util.ByteToHexString(array3, 2, 2);
					break;
				}
				break;
			case 32:
				switch (array3[1])
				{
				case 0:
					text3 = "RESPONSE | BODY CONTROL MODULE | RESET COMPLETE";
					break;
				case 16:
					text3 = "RESPONSE | BCM | ACTUATOR TEST";
					text = Util.ByteToHexString(array3, 2, 2);
					break;
				case 18:
					text3 = "RESPONSE | BCM | DIGITAL READ";
					text = Util.ByteToHexString(array3, 2, 2);
					break;
				case 20:
					text3 = "RESPONSE | BCM | ANALOG READ";
					text = Util.ByteToHexString(array3, 2, 2);
					break;
				case 22:
					text3 = "RESPONSE | BCM | FAULT CODES";
					text = ((array3[3] != 0) ? ("CODE: " + Util.ByteToHexString(array3, 3)) : "NO FAULT CODE");
					break;
				case 34:
					text3 = "RESPONSE | BCM | ROM DATA";
					text = "VALUE:     " + Util.ByteToHexString(array3, 2) + " |    " + Util.ByteToHexString(array3, 3);
					break;
				case 36:
					text3 = "RESPONSE | BCM | MODULE ID";
					text = Util.ByteToHexString(array3, 2, 2);
					break;
				case 42:
					text3 = "RESPONSE | BCM | READ VIN";
					text = Util.ByteToHexString(array3, 2, 2);
					break;
				case 44:
					text3 = "RESPONSE | BCM | WRITE VIN";
					text = Util.ByteToHexString(array3, 2, 2);
					break;
				case 64:
					text3 = "RESPONSE | BCM | ERASE FAULT CODES";
					text = ((array3[3] != 0) ? "FAILED" : "ERASED");
					break;
				case 96:
					text3 = "RESPONSE | BCM | WRITE EEPROM OFFSET";
					text = Util.ByteToHexStringSimple(new byte[2]
					{
						array3[2],
						array3[3]
					});
					text2 = "OK";
					break;
				case 176:
					text3 = "RESPONSE | BCM | WRITE SETTINGS";
					text = Util.ByteToHexString(array3, 2, 2);
					break;
				case 177:
					text3 = "RESPONSE | BCM | READ SETTINGS";
					text = Util.ByteToHexString(array3, 2, 2);
					break;
				case byte.MaxValue:
					text3 = "RESPONSE | BCM | COMMAND ERROR";
					text = Util.ByteToHexString(array3, 2, 2);
					break;
				default:
					text3 = "RESPONSE | BCM | COMMAND: " + Util.ByteToHexString(array3, 1);
					text = Util.ByteToHexString(array3, 2, 2);
					break;
				}
				break;
			case 34:
			case 96:
				switch (array3[1])
				{
				case 0:
					text3 = "RESPONSE | MIC | RESET COMPLETE";
					break;
				case 16:
					text3 = array3[2] switch
					{
						0 => "RESPONSE | MIC | ALL GAUGES TEST", 
						1 => "RESPONSE | MIC | ALL LAMPS TEST", 
						2 => "RESPONSE | MIC | ODO/TRIP/PRND3L TEST", 
						3 => "RESPONSE | MIC | PRND3L SEGMENTS TEST", 
						_ => "RESPONSE | MIC | ACTUATOR TEST", 
					};
					text = Util.ByteToHexString(array3, 2, 2);
					break;
				case 18:
					text3 = "RESPONSE | MIC | DIGITAL READ";
					text = Util.ByteToHexString(array3, 2, 2);
					break;
				case 22:
				{
					text3 = "RESPONSE | MIC | FAULT CODES";
					if (array3[3] == 0)
					{
						text = "NO FAULT CODE";
						break;
					}
					List<string> list = new List<string>();
					if (Util.IsBitSet(array3[3], 1))
					{
						list.Add("NO BCM MSG");
					}
					if (Util.IsBitSet(array3[3], 2))
					{
						list.Add("NO PCM MSG");
					}
					if (Util.IsBitSet(array3[3], 4))
					{
						list.Add("BCM FAILURE");
					}
					if (Util.IsBitSet(array3[3], 6))
					{
						list.Add("RAM FAILURE");
					}
					if (Util.IsBitSet(array3[3], 7))
					{
						list.Add("ROM FAILURE");
					}
					foreach (string item10 in list)
					{
						text = text + item10 + " | ";
					}
					if (text.Length > 2)
					{
						text = text.Remove(text.Length - 3);
					}
					break;
				}
				case 36:
					text3 = "RESPONSE | MIC | SOFTWARE VERSION";
					text = Util.ByteToHexString(array3, 2, 2);
					break;
				case 64:
					text3 = "RESPONSE | MIC | ERASE FAULT CODES";
					text = ((array3[3] != 0) ? "FAILED" : "ERASED");
					break;
				case 224:
					text3 = "RESPONSE | MIC | SELF TEST";
					text = Util.ByteToHexString(array3, 2, 2);
					break;
				case byte.MaxValue:
					text3 = "RESPONSE | MIC | COMMAND ERROR";
					text = Util.ByteToHexString(array3, 2, 2);
					break;
				default:
					text3 = "RESPONSE | MIC | COMMAND: " + Util.ByteToHexString(array3, 1);
					text = Util.ByteToHexString(array3, 2, 2);
					break;
				}
				break;
			case 65:
			case 66:
				switch (array3[1])
				{
				case 0:
					text3 = "RESPONSE | TCM | RESET COMPLETE";
					break;
				case 36:
					text3 = "RESPONSE | TCM | READ ANALOG PARAMETER";
					if (TransmissionLRCVIRequested)
					{
						TransmissionLRCVIRequested = false;
						double value = (double)(int)array3[2] / 64.0;
						double value2 = (double)(int)array3[2] / 64.0 * 16.387064;
						text3 = "RESPONSE | TCM | LR CLUTCH VOLUME INDEX (CVI)";
						if (array3[2] != byte.MaxValue)
						{
							if (Settings.Default.Units == "imperial")
							{
								text = array3[2].ToString("0") + Math.Round(value, 3).ToString("0.000");
								text2 = "IN^3";
							}
							else if (Settings.Default.Units == "metric")
							{
								text = array3[2].ToString("0") + " = " + Math.Round(value2, 3).ToString("0.000");
								text2 = "CM^3";
							}
						}
						else
						{
							text = "ERROR";
						}
					}
					if (Transmission24CVIRequested)
					{
						Transmission24CVIRequested = false;
						double value3 = (double)(int)array3[2] / 64.0;
						double value4 = (double)(int)array3[2] / 64.0 * 16.387064;
						text3 = "RESPONSE | TCM | 24 CLUTCH VOLUME INDEX (CVI)";
						if (array3[2] != byte.MaxValue)
						{
							if (Settings.Default.Units == "imperial")
							{
								text = array3[2].ToString("0") + " = " + Math.Round(value3, 3).ToString("0.000");
								text2 = "IN^3";
							}
							else if (Settings.Default.Units == "metric")
							{
								text = array3[2].ToString("0") + " = " + Math.Round(value4, 3).ToString("0.000");
								text2 = "CM^3";
							}
						}
						else
						{
							text = "ERROR";
						}
					}
					if (TransmissionODCVIRequested)
					{
						TransmissionODCVIRequested = false;
						double value5 = (double)(int)array3[2] / 64.0;
						double value6 = (double)(int)array3[2] / 64.0 * 16.387064;
						text3 = "RESPONSE | TCM | OD CLUTCH VOLUME INDEX (CVI)";
						if (array3[2] != byte.MaxValue)
						{
							if (Settings.Default.Units == "imperial")
							{
								text = array3[2].ToString("0") + " = " + Math.Round(value5, 3).ToString("0.000");
								text2 = "IN^3";
							}
							else if (Settings.Default.Units == "metric")
							{
								text = array3[2].ToString("0") + " = " + Math.Round(value6, 3).ToString("0.000");
								text2 = "CM^3";
							}
						}
						else
						{
							text = "ERROR";
						}
					}
					if (TransmissionUDCVIRequested)
					{
						TransmissionUDCVIRequested = false;
						double value7 = (double)(int)array3[2] / 64.0;
						double value8 = (double)(int)array3[2] / 64.0 * 16.387064;
						text3 = "RESPONSE | TCM | UD CLUTCH VOLUME INDEX (CVI)";
						if (array3[2] != byte.MaxValue)
						{
							if (Settings.Default.Units == "imperial")
							{
								text = array3[2].ToString("0") + " = " + Math.Round(value7, 3).ToString("0.000");
								text2 = "IN^3";
							}
							else if (Settings.Default.Units == "metric")
							{
								text = array3[2].ToString("0") + " = " + Math.Round(value8, 3).ToString("0.000");
								text2 = "CM^3";
							}
						}
						else
						{
							text = "ERROR";
						}
					}
					if (!TransmissionTemperatureRequested)
					{
						break;
					}
					TransmissionTemperatureRequested = false;
					text3 = "RESPONSE | TCM | TRANSMISSION TEMPERATURE";
					if (array3[2] != byte.MaxValue)
					{
						double value9 = (double)((array3[2] << 8) + array3[3]) * 0.0156;
						double value10 = (double)((array3[2] << 8) + array3[3]) * 0.0156 * 0.555556 - 17.77778;
						if (Settings.Default.Units == "imperial")
						{
							text = Math.Round(value9, 1).ToString("0.0");
							text2 = "°F";
						}
						else if (Settings.Default.Units == "metric")
						{
							text = Math.Round(value10, 1).ToString("0.0");
							text2 = "°C";
						}
					}
					else
					{
						text = "ERROR";
					}
					break;
				case byte.MaxValue:
					text3 = "RESPONSE | TCM | COMMAND ERROR";
					text = Util.ByteToHexString(array3, 2, 2);
					break;
				default:
					text3 = "RESPONSE | TCM | COMMAND: " + Util.ByteToHexString(array3, 1);
					text = Util.ByteToHexString(array3, 2, 2);
					break;
				}
				break;
			case 67:
				switch (array3[1])
				{
				case 0:
					text3 = "RESPONSE | ANTILOCK BRAKE SYSTEM | RESET COMPLETE";
					break;
				case byte.MaxValue:
					text3 = "RESPONSE | ABS | COMMAND ERROR";
					text = Util.ByteToHexString(array3, 2, 2);
					break;
				default:
					text3 = "RESPONSE | ABS | COMMAND: " + Util.ByteToHexString(array3, 1);
					text = Util.ByteToHexString(array3, 2, 2);
					break;
				}
				break;
			case 80:
				switch (array3[1])
				{
				case 0:
					text3 = "RESPONSE | HVAC | RESET COMPLETE";
					text = Util.ByteToHexString(array3, 2, 2);
					break;
				case byte.MaxValue:
					text3 = "RESPONSE | HVAC | COMMAND ERROR";
					text = Util.ByteToHexString(array3, 2, 2);
					break;
				default:
					text3 = "RESPONSE | HVAC | COMMAND: " + Util.ByteToHexString(array3, 1);
					text = Util.ByteToHexString(array3, 2, 2);
					break;
				}
				break;
			case 128:
				switch (array3[1])
				{
				case 0:
					text3 = "RESPONSE | DRIVER DOOR MODULE | RESET COMPLETE";
					break;
				case byte.MaxValue:
					text3 = "RESPONSE | DDM | COMMAND ERROR";
					text = Util.ByteToHexString(array3, 2, 2);
					break;
				default:
					text3 = "RESPONSE | DDM | COMMAND: " + Util.ByteToHexString(array3, 1);
					text = Util.ByteToHexString(array3, 2, 2);
					break;
				}
				break;
			case 129:
				switch (array3[1])
				{
				case 0:
					text3 = "RESPONSE | PASSENGER DOOR MODULE | RESET COMPLETE";
					break;
				case byte.MaxValue:
					text3 = "RESPONSE | PDM | COMMAND ERROR";
					text = Util.ByteToHexString(array3, 2, 2);
					break;
				default:
					text3 = "RESPONSE | PDM | COMMAND " + Util.ByteToHexString(array3, 1);
					text = Util.ByteToHexString(array3, 2, 2);
					break;
				}
				break;
			case 130:
				switch (array3[1])
				{
				case 0:
					text3 = "RESPONSE | MEMORY SEAT MODULE | RESET COMPLETE";
					break;
				case byte.MaxValue:
					text3 = "RESPONSE | MSM | COMMAND ERROR";
					text = Util.ByteToHexString(array3, 2, 2);
					break;
				default:
					text3 = "RESPONSE | MSM | COMMAND: " + Util.ByteToHexString(array3, 1);
					text = Util.ByteToHexString(array3, 2, 2);
					break;
				}
				break;
			case 150:
				switch (array3[1])
				{
				case 0:
					text3 = "RESPONSE | AUDIO SYSTEM MODULE | RESET COMPLETE";
					break;
				case byte.MaxValue:
					text3 = "RESPONSE | ASM | COMMAND ERROR";
					text = Util.ByteToHexString(array3, 2, 2);
					break;
				default:
					text3 = "RESPONSE | ASM | COMMAND: " + Util.ByteToHexString(array3, 1);
					text = Util.ByteToHexString(array3, 2, 2);
					break;
				}
				break;
			case 192:
				switch (array3[1])
				{
				case 0:
					text3 = "RESPONSE | SKIM | RESET COMPLETE";
					break;
				case byte.MaxValue:
					text3 = "RESPONSE | SKIM | COMMAND ERROR";
					text = Util.ByteToHexString(array3, 2, 2);
					break;
				default:
					text3 = "RESPONSE | SKIM | COMMAND: " + Util.ByteToHexString(array3, 1);
					text = Util.ByteToHexString(array3, 2, 2);
					break;
				}
				break;
			default:
				text3 = "RESPONSE | MODULE: " + Util.ByteToHexStringSimple(new byte[1] { array3[0] }) + " | COMMAND: " + Util.ByteToHexStringSimple(new byte[1] { array3[1] }) + " | PARAMS: " + Util.ByteToHexStringSimple(new byte[2]
				{
					array3[2],
					array3[3]
				});
				break;
			}
			if (Settings.Default.Timestamp)
			{
				TimeSpan value14 = TimeSpan.FromMilliseconds((array[0] << 24) | (array[1] << 16) | (array[2] << 8) | array[3]);
				string contents = DateTime.Today.Add(value14).ToString("HH:mm:ss.fff") + " ";
				File.AppendAllText(MainForm.CCDB2F2LogFilename, contents);
			}
			File.AppendAllText(MainForm.CCDB2F2LogFilename, "CCD: " + Util.ByteToHexStringSimple(array2) + Environment.NewLine);
			break;
		case 243:
			text3 = "SWITCH MESSAGE";
			if (array2.Length >= 4)
			{
			}
			break;
		case 245:
			text3 = "ENGINE LAMP CTRL";
			if (array2.Length >= 4 && Util.IsBitSet(array3[0], 0))
			{
				text = "CEL ON";
			}
			break;
		case 246:
			text3 = "PCM BEACON F6";
			if (array2.Length >= 4)
			{
				string text6 = BeaconNote.Remove(6, 2);
				char c = (char)array3[0];
				string text7 = c.ToString();
				c = (char)array3[1];
				BeaconNote = text6.Insert(6, text7 + c);
				text = Util.ByteToHexString(array3, 0, 2);
			}
			break;
		case 253:
			text3 = "COMPASS COMP. AND TEMPERATURE DATA RECEIVED";
			if (array2.Length >= 4)
			{
			}
			break;
		case 254:
			text3 = "INTERIOR LAMP DIMMING";
			if (array2.Length >= 3)
			{
				text = Math.Round((double)(int)array3[0] * 0.3921568627, 1).ToString("0.0");
				text2 = "PERCENT";
			}
			break;
		case byte.MaxValue:
			text3 = "CCD-BUS WAKE UP";
			break;
		default:
			text3 = string.Empty;
			break;
		}
		string text23 = ((array2.Length >= 9) ? (Util.ByteToHexString(array2, 0, 7) + " .. ") : (Util.ByteToHexString(array2, 0, array2.Length) + " "));
		if (text3.Length > 51)
		{
			text3 = Util.TruncateString(text3, 48) + "...";
		}
		if (text.Length > 23)
		{
			text = Util.TruncateString(text, 20) + "...";
		}
		if (text2.Length > 11)
		{
			text2 = Util.TruncateString(text2, 8) + "...";
		}
		StringBuilder stringBuilder = new StringBuilder(EmptyLine);
		stringBuilder.Remove(2, text23.Length);
		stringBuilder.Insert(2, text23);
		stringBuilder.Remove(28, text3.Length);
		stringBuilder.Insert(28, text3);
		stringBuilder.Remove(82, text.Length);
		stringBuilder.Insert(82, text);
		stringBuilder.Remove(108, text2.Length);
		stringBuilder.Insert(108, text2);
		ushort modifiedID = b switch
		{
			148 => (array3.Length <= 1) ? ((ushort)((uint)(b << 8) & 0xFF00u)) : ((ushort)(((b << 8) & 0xFF00) + (array3[1] & 0xF))), 
			194 => (array3.Length == 0) ? ((ushort)((uint)(b << 8) & 0xFF00u)) : ((ushort)(((b << 8) & 0xFF00) + array3[0])), 
			_ => (ushort)((uint)(b << 8) & 0xFF00u), 
		};
		Diagnostics.AddRow(modifiedID, stringBuilder.ToString());
		UpdateHeader();
		if (Settings.Default.Timestamp)
		{
			TimeSpan value32 = TimeSpan.FromMilliseconds((array[0] << 24) | (array[1] << 16) | (array[2] << 8) | array[3]);
			string contents3 = DateTime.Today.Add(value32).ToString("HH:mm:ss.fff") + ",";
			File.AppendAllText(MainForm.CCDLogFilename, contents3);
		}
		File.AppendAllText(MainForm.CCDLogFilename, "CCD," + Util.ByteToHexStringSimple(array2) + Environment.NewLine);
	}
}
