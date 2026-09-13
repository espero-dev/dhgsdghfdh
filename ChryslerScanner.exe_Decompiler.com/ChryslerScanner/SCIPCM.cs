using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using ChryslerScanner.Helpers;
using ChryslerScanner.Properties;

namespace ChryslerScanner;

public class SCIPCM
{
	public SCIPCMDiagnosticsTable Diagnostics = new SCIPCMDiagnosticsTable();

	public DataTable SBEC3EngineDTC = new DataTable("SBEC3EngineDTC");

	public List<byte> StoredFaultCodeList = new List<byte>();

	public bool StoredFaultCodesSaved = true;

	public List<byte> PendingFaultCodeList = new List<byte>();

	public bool PendingFaultCodesSaved = true;

	public List<byte> FaultCode1TList = new List<byte>();

	public bool FaultCodes1TSaved = true;

	public byte[] SBEC3EngineDTCList;

	public DataColumn Column;

	public DataRow Row;

	private const int HexBytesColumnStart = 2;

	private const int DescriptionColumnStart = 28;

	private const int ValueColumnStart = 82;

	private const int UnitColumnStart = 108;

	public string state;

	public string speed;

	public string logic;

	public string configuration;

	public string HeaderUnknown = "│ SCI-BUS (SAE J2610) PCM │ STATE: N/A                                                                                   ";

	public string HeaderDisabled = "│ SCI-BUS (SAE J2610) PCM │ STATE: DISABLED                                                                              ";

	public string HeaderEnabled = "│ SCI-BUS (SAE J2610) PCM │ STATE: ENABLED @ BAUD | LOGIC: | CONFIGURATION:                                              ";

	public string EmptyLine = "│                         │                                                     │                         │             │";

	public string HeaderModified = string.Empty;

	public byte ControllerHardwareType;

	public byte[] PartNumberChars = new byte[6];

	public string[] EngineToolsStatusBarTextItems = new string[12];

	public int Year = 2003;

	public bool CumminsSelected = true;

	public SCIPCM()
	{
		Column = new DataColumn
		{
			DataType = typeof(byte),
			ColumnName = "id",
			ReadOnly = true,
			Unique = true
		};
		SBEC3EngineDTC.Columns.Add(Column);
		Column = new DataColumn
		{
			DataType = typeof(string),
			ColumnName = "description",
			ReadOnly = true,
			Unique = false
		};
		SBEC3EngineDTC.Columns.Add(Column);
		DataColumn[] primaryKey = new DataColumn[1] { SBEC3EngineDTC.Columns["id"] };
		SBEC3EngineDTC.PrimaryKey = primaryKey;
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 0;
		Row["description"] = "UNRECOGNIZED DTC";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 1;
		Row["description"] = "NO CAM SIGNAL AT PCM";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 2;
		Row["description"] = "INTERNAL CONTROLLER FAILURE";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 3;
		Row["description"] = "LEFT BANK O2 SENSOR STAYS ABOVE CENTER (RICH)";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 4;
		Row["description"] = "LEFT BANK O2 SENSOR STAYS BELOW CENTER (LEAN)";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 5;
		Row["description"] = "CHARGING SYSTEM VOLTAGE LOW";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 6;
		Row["description"] = "CHARGING SYSTEM VOLTAGE HIGH";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 7;
		Row["description"] = "TURBO BOOST LIMIT EXCEEDED";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 8;
		Row["description"] = "RIGHT BANK O2 SENSOR STAYS ABOVE CENTER (RICH)";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 9;
		Row["description"] = "RIGHT BANK O2 SENSOR STAYS BELOW CENTER (LEAN)";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 10;
		Row["description"] = "AUTO SHUTDOWN RELAY CONTROL CIRCUIT";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 11;
		Row["description"] = "GENERATOR FIELD NOT SWITCHING PROPERLY";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 12;
		Row["description"] = "TORQUE CONVERTER CLUTCH SOLENOID / TRANS RELAY CIRCUITS";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 13;
		Row["description"] = "TURBOCHARGER WASTEGATE SOLENOID CIRCUIT";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 14;
		Row["description"] = "LOW SPEED FAN CONTROL RELAY CIRCUIT";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 15;
		Row["description"] = "CRUISE CONTROL SOLENOID CIRCUITS";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 16;
		Row["description"] = "A/C CLUTCH RELAY CIRCUIT";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 17;
		Row["description"] = "EGR SOLENOID CIRCUIT";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 18;
		Row["description"] = "EVAP PURGE SOLENOID CIRCUIT";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 19;
		Row["description"] = "INJECTOR #3 CONTROL CIRCUIT";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 20;
		Row["description"] = "INJECTOR #2 CONTROL CIRCUIT";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 21;
		Row["description"] = "INJECTOR #1 CONTROL CIRCUIT";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 22;
		Row["description"] = "INJECTOR #3 PEAK CURRENT NOT REACHED";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 23;
		Row["description"] = "INJECTOR #2 PEAK CURRENT NOT REACHED";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 24;
		Row["description"] = "INJECTOR #1 PEAK CURRENT NOT REACHED";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 25;
		Row["description"] = "IDLE AIR CONTROL MOTOR CIRCUITS";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 26;
		Row["description"] = "THROTTLE POSITION SENSOR VOLTAGE LOW";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 27;
		Row["description"] = "THROTTLE POSITION SENSOR VOLTAGE HIGH";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 28;
		Row["description"] = "THROTTLE BODY TEMP SENSOR VOLTAGE LOW";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 29;
		Row["description"] = "THROTTLE BODY TEMP SENSOR VOLTAGE HIGH";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 30;
		Row["description"] = "COOLANT TEMPERATURE SENSOR VOLTAGE LOW";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 31;
		Row["description"] = "COOLANT TEMPERATURE SENSOR VOLTAGE HIGH";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 32;
		Row["description"] = "UPSTREAM O2 SENSOR STAYS AT CENTER";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 33;
		Row["description"] = "ENGINE IS COLD TOO LONG";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 34;
		Row["description"] = "SKIP SHIFT SOLENOID CIRCUIT";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 35;
		Row["description"] = "NO VEHICLE SPEED SENSOR SIGNAL";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 36;
		Row["description"] = "MAP SENSOR VOLTAGE LOW";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 37;
		Row["description"] = "MAP SENSOR VOLTAGE HIGH";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 38;
		Row["description"] = "SLOW CHANGE IN IDLE MAP SENSOR SIGNAL";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 39;
		Row["description"] = "NO CHANGE IN MAP FROM START TO RUN";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 40;
		Row["description"] = "NO CRANKSHAFT REFERENCE SIGNAL AT PCM";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 41;
		Row["description"] = "IGNITION COIL #3 PRIMARY CIRCUIT";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 42;
		Row["description"] = "IGNITION COIL #2 PRIMARY CIRCUIT";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 43;
		Row["description"] = "IGNITION COIL #1 PRIMARY CIRCUIT";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 44;
		Row["description"] = "NO ASD RELAY OUTPUT VOLTAGE AT PCM";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 45;
		Row["description"] = "SYSTEM RICH, L-IDLE ADAPTIVE AT LEAN LIMIT";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 46;
		Row["description"] = "EGR SYSTEM FAILURE";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 47;
		Row["description"] = "BAROMETRIC READ SOLENOID CIRCUIT";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 48;
		Row["description"] = "PCM FAILURE SRI MILE NOT STORED";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 49;
		Row["description"] = "PCM FAILURE EEPROM WRITE DENIED";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 50;
		Row["description"] = "TRANSMISSION 3-4 SHIFT SOLENOID / TRANSMISSION RELAY CIRCUITS";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 51;
		Row["description"] = "SECONDARY AIR SOLENOID CIRCUIT";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 52;
		Row["description"] = "IDLE SWITCH SHORTED TO GROUND";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 53;
		Row["description"] = "IDLE SWITCH OPEN CIRCUIT";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 54;
		Row["description"] = "SURGE VALVE SOLENOID CIRCUIT";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 55;
		Row["description"] = "INJECTOR #9 CONTROL CIRCUIT";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 56;
		Row["description"] = "INJECTOR #10 CONTROL CIRCUIT";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 57;
		Row["description"] = "INTAKE AIR TEMPERATURE SENSOR VOLTAGE LOW";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 58;
		Row["description"] = "INTAKE AIR TEMPERATURE SENSOR VOLTAGE HIGH";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 59;
		Row["description"] = "KNOCK SENSOR #1 CIRCUIT";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 60;
		Row["description"] = "BAROMETRIC PRESSURE OUT OF RANGE";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 61;
		Row["description"] = "INJECTOR #4 CONTROL CIRCUIT";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 62;
		Row["description"] = "LEFT BANK UPSTREAM O2 SENSOR SHORTED TO VOLTAGE";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 63;
		Row["description"] = "FUEL SYSTEM RICH, R-IDLE ADAPTIVE AT LEAN LIMIT";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 64;
		Row["description"] = "WASTEGATE #2 CIRCUIT";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 65;
		Row["description"] = "RIGHT BANK UPSTREAM O2 SENSOR STAYS AT CENTER";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 66;
		Row["description"] = "RIGHT BANK UPSTREAM O2 SENSOR SHORTED TO VOLTAGE";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 67;
		Row["description"] = "FUEL SYSTEM LEAN, R-IDLE ADAPTIVE AT RICH LIMIT";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 68;
		Row["description"] = "PCM FAILURE SPI COMMUNICATIONS";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 69;
		Row["description"] = "INJECTOR #5 CONTROL CIRCUIT";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 70;
		Row["description"] = "INJECTOR #6 CONTROL CIRCUIT";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 71;
		Row["description"] = "BATTERY TEMPERATURE SENSOR VOLTS OUT OF RNG";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 72;
		Row["description"] = "NO CMP AT IGNITION / INJ DRIVER MODULE";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 73;
		Row["description"] = "NO CKP AT IGNITION/ INJ DRIVER MODULE";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 74;
		Row["description"] = "TRANSMISSION TEMPERATURE SENSOR VOLTAGE LOW";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 75;
		Row["description"] = "TRANSMISSION TEMPERATURE SENSOR VOLTAGE HIGH";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 76;
		Row["description"] = "IGNITION COIL #4 PRIMARY CIRCUIT";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 77;
		Row["description"] = "IGNITION COIL #5 PRIMARY CIRCUIT";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 78;
		Row["description"] = "FUEL SYSTEM LEAN, L-IDLE ADAPTIVE AT RICH LIMIT";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 79;
		Row["description"] = "INJECTOR #7 CONTROL CIRCUIT";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 80;
		Row["description"] = "INJECTOR #8 CONTROL CIRCUIT";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 81;
		Row["description"] = "FUEL PUMP RESISTOR BYPASS RELAY CIRCUIT";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 82;
		Row["description"] = "CRUISE CONTROL POWER RELAY OR 12V DRIVER CIRCUIT";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 83;
		Row["description"] = "KNOCK SENSOR #2 CIRCUIT";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 84;
		Row["description"] = "FLEX FUEL SENSOR VOLTS HIGH";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 85;
		Row["description"] = "FLEX FUEL SENSOR VOLTS LOW";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 86;
		Row["description"] = "CRUISE CONTROL SWITCH ALWAYS HIGH";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 87;
		Row["description"] = "CRUISE CONTROL SWITCH ALWAYS LOW";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 88;
		Row["description"] = "MANIFOLD TUNE VALVE SOLENOID CIRCUIT";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 89;
		Row["description"] = "NO BUS MESSAGES";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 90;
		Row["description"] = "A/C PRESSURE SENSOR VOLTS HIGH";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 91;
		Row["description"] = "A/C PRESSURE SENSOR VOLTS LOW";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 92;
		Row["description"] = "LOW SPEED FAN CONTROL RELAY CIRCUIT";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 93;
		Row["description"] = "HIGH SPEED CONDENSER FAN CTRL RELAY CIRCUIT";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 94;
		Row["description"] = "CNG TEMPERATURE SENSOR VOLTAGE LOW";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 95;
		Row["description"] = "CNG TEMPERATURE SENSOR VOLTAGE HIGH";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 96;
		Row["description"] = "NO CCD/PCI BUS MESSAGES FROM TCM";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 97;
		Row["description"] = "NO CCD/PCI BUS MESSAGE FROM BCM";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 98;
		Row["description"] = "CNG PRESSURE SENSOR VOLTAGE HIGH";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 99;
		Row["description"] = "CNG PRESSURE SENSOR VOLTAGE LOW";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 100;
		Row["description"] = "LOSS OF FLEX FUEL CALIBRATION SIGNAL";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 101;
		Row["description"] = "FUEL PUMP RELAY CONTROL CIRCUIT";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 102;
		Row["description"] = "LEFT BANK UPSTREAM O2 SENSOR SLOW RESPONSE";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 103;
		Row["description"] = "LEFT BANK UPSTREAM O2 SENSOR HEATER FAILURE";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 104;
		Row["description"] = "DOWNSTREAM O2 SENSOR UNABLE TO SWITCH RICH/LEAN";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 105;
		Row["description"] = "DOWNSTREAM O2 SENSOR HEATER FAILURE";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 106;
		Row["description"] = "MULTIPLE CYLINDER MISFIRE";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 107;
		Row["description"] = "CYLINDER #1 MISFIRE";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 108;
		Row["description"] = "CYLINDER #2 MISFIRE";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 109;
		Row["description"] = "CYLINDER #3 MISFIRE";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 110;
		Row["description"] = "CYLINDER #4 MISFIRE";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 111;
		Row["description"] = "TOO LITTLE SECONDARY AIR";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 112;
		Row["description"] = "CATALYTIC CONVERTER EFFICIENCY FAILURE";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 113;
		Row["description"] = "EVAP PURGE FLOW MONITOR FAILURE";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 114;
		Row["description"] = "P/N SWITCH STUCK IN PARK OR IN GEAR";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 115;
		Row["description"] = "POWER STEERING SWITCH FAILURE";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 116;
		Row["description"] = "DESIRED FUEL TIMING ADVANCE NOT REACHED";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 117;
		Row["description"] = "LOST FUEL INJECTION TIMING SIGNAL";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 118;
		Row["description"] = "LEFT BANK FUEL SYSTEM RICH";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 119;
		Row["description"] = "LEFT BANK FUEL SYSTEM LEAN";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 120;
		Row["description"] = "RIGHT BANK FUEL SYSTEM RICH";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 121;
		Row["description"] = "RIGHT BANK FUEL SYSTEM LEAN";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 122;
		Row["description"] = "RIGHT BANK UPSTREAM O2 SENSOR SLOW RESPONSE";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 123;
		Row["description"] = "RIGHT BANK DOWNSTREAM O2 SENSOR SLOW RESPONSE";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 124;
		Row["description"] = "RIGHT BANK UPSTREAM O2 SENSOR HEATER FAILURE";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 125;
		Row["description"] = "RIGHT BANK DOWNSTREAM O2 SENSOR HEATER FAILURE";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 126;
		Row["description"] = "DOWNSTREAM O2 SENSOR SHORTED TO VOLTAGE";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 127;
		Row["description"] = "RIGHT BANK DOWNSTREAM O2 SENSOR SHORTED TO VOLTAGE";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 128;
		Row["description"] = "CLOSED LOOP TEMPERATURE NOT REACHED";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 129;
		Row["description"] = "LEFT BANK DOWNSTREAM O2 SENSOR STAYS AT CENTER";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 130;
		Row["description"] = "RIGHT BANK DOWNSTREAM O2 SENSOR STAYS AT CENTER";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 131;
		Row["description"] = "LEAN OPERATION AT WIDE OPEN THROTTLE";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 132;
		Row["description"] = "TPS VOLTAGE DOES NOT AGREE WITH MAP";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 133;
		Row["description"] = "TIMING BELT SKIPPED 1 TOOTH OR MORE";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 134;
		Row["description"] = "NO 5 VOLTS TO A/C PRESSURE SENSOR";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 135;
		Row["description"] = "NO 5 VOLTS TO MAP SENSOR";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 136;
		Row["description"] = "NO 5 VOLTS TO TPS";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 137;
		Row["description"] = "EATX CONTROLLER DTC PRESENT";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 138;
		Row["description"] = "TARGET IDLE NOT REACHED";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 139;
		Row["description"] = "HIGH SPEED RADIATOR FAN CONTROL RELAY CIRCUIT";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 140;
		Row["description"] = "DIESEL EGR SYSTEM FAILURE";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 141;
		Row["description"] = "GOVERNOR PRESSURE NOT EQUAL TO TARGET @ 15 - 20 PSI";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 142;
		Row["description"] = "GOVERNOR PRESSURE ABOVE 3 PSI IN GEAR WITH 0 MPH";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 143;
		Row["description"] = "STARTER RELAY CONTROL CIRCUIT";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 144;
		Row["description"] = "DOWNSTREAM O2 SENSOR SHORTED TO GROUND";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 145;
		Row["description"] = "VACUUM LEAK FOUND (IAC FULLY SEATED)";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 146;
		Row["description"] = "5 VOLT SUPPLY, OUTPUT LOW";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 147;
		Row["description"] = "DOWNSTREAM O2 SENSOR SHORTED TO VOLTAGE";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 148;
		Row["description"] = "TORQUE CONVERTER CLUTCH, NO RPM DROP AT LOCKUP";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 149;
		Row["description"] = "FUEL LEVEL SENDING UNIT VOLTS LOW";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 150;
		Row["description"] = "FUEL LEVEL SENDING UNIT VOLTS HIGH";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 151;
		Row["description"] = "FUEL LEVEL UNIT NO CHANGE OVER MILES";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 152;
		Row["description"] = "BRAKE SWITCH STUCK PRESSED OR RELEASED";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 153;
		Row["description"] = "BATTERY TEMPERATURE SENSOR VOLTS LOW";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 154;
		Row["description"] = "BATTERY TEMPERATURE SENSOR VOLTS HIGH";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 155;
		Row["description"] = "LEFT BANK UPSTREAM O2 SENSOR SHORTED TO GROUND";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 156;
		Row["description"] = "DOWNSTREAM O2 SENSOR SHORTED TO GROUND";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 157;
		Row["description"] = "INTERMITTENT LOSS OF CMP OR CKP";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 158;
		Row["description"] = "TOO MUCH SECONDARY AIR";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 159;
		Row["description"] = "DOWNSTREAM O2 SENSOR SLOW RESPONSE";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 160;
		Row["description"] = "EVAP LEAK MONITOR SMALL LEAK DETECTED";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 161;
		Row["description"] = "EVAP LEAK MONITOR LARGE LEAK DETECTED";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 162;
		Row["description"] = "NO TEMPERATURE RISE SEEN FROM INTAKE HEATERS";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 163;
		Row["description"] = "WAIT TO START LAMP CIRCUIT";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 164;
		Row["description"] = "TRANSMISSION TEMPERATURE SENSOR, NO TEMPERATURE RISE AFTR START";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 165;
		Row["description"] = "3-4 SHIFT SOLENOID, NO RPM DROP @ 3-4 SHIFT";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 166;
		Row["description"] = "LOW OUTPUT SPEED SENSOR RPM, ABOVE 15 MPH";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 167;
		Row["description"] = "GOVERNOR PRESSURE SENSOR VOLTS LOW";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 168;
		Row["description"] = "GOVERNOR PRESSURE SENSOR VOLTS HIGH";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 169;
		Row["description"] = "GOVERNOR PRESSURE SENSOR OFFSET VOLTS LOW OR HIGH";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 170;
		Row["description"] = "PCM NOT PROGRAMMED";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 171;
		Row["description"] = "GOVERNOR PRESSURE SOLENOID CONTROL / TRANSMISSION RELAY CIRCUITS";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 172;
		Row["description"] = "DOWNSTREAM O2 SENSOR STUCK AT CENTER";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 173;
		Row["description"] = "TRANSMISSION 12 VOLT SUPPLY RELAY CONTROL CIRCUIT";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 174;
		Row["description"] = "CYLINDER #5 MIS-FIRE";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 175;
		Row["description"] = "CYLINDER #6 MIS-FIRE";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 176;
		Row["description"] = "CYLINDER #7 MIS-FIRE";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 177;
		Row["description"] = "CYLINDER #8 MIS-FIRE";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 178;
		Row["description"] = "CYLINDER #9 MIS-FIRE";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 179;
		Row["description"] = "CYLINDER #10 MIS-FIRE";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 180;
		Row["description"] = "RIGHT BANK CATALYST EFFICIENCY FAILURE";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 181;
		Row["description"] = "REAR BANK UPSTREAM O2 SENSOR SHORTED TO GROUND";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 182;
		Row["description"] = "REAR BANK DOWNSTREAM O2 SENSOR SHORTED TO GROUND";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 183;
		Row["description"] = "LEAK DETECTION PUMP SOLENOID CIRCUIT";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 184;
		Row["description"] = "LEAK DETECT PUMP SWITCH OR MECHANICAL FAULT";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 185;
		Row["description"] = "AUXILIARY 5 VOLT SUPPLY OUTPUT LOW";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 186;
		Row["description"] = "MISFIRE ADAPTIVE NUMERATOR AT LIMIT";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 187;
		Row["description"] = "EVAP LEAK MONITOR PINCHED HOSE FOUND";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 188;
		Row["description"] = "O/D SWITCH PRESSED (LOW) MORE THAN 5 MIN";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 189;
		Row["description"] = "DOWNSTREAM O2 SENSOR HEATER FAILURE";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 197;
		Row["description"] = "HIGH SPEED RADIATOR FAN GROUND CONTROL RELAY CIRCUIT";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 198;
		Row["description"] = "ONE OF THE IGNITION COILS DRAWS TOO MUCH CURRENT";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 199;
		Row["description"] = "AW4 TRANSMISSION SHIFT SOLENOID B FUNCTIONAL FAILURE";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 200;
		Row["description"] = "RADIATOR TEMPERATURE SENSOR VOLTS LOW";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 201;
		Row["description"] = "RADIATOR TEMPERATURE SENSOR VOLTS HIGH";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 202;
		Row["description"] = "NO I/P CLUSTER CCD/PCI BUS MESSAGES RECEIVED";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 203;
		Row["description"] = "AW4 TRANSMISSION INTERNAL FAILURE (ROM CHECK)";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 204;
		Row["description"] = "UPSTREAM O2 SENSOR SLOW RESPONSE";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 205;
		Row["description"] = "UPSTREAM O2 SENSOR HEATER FAILURE";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 206;
		Row["description"] = "UPSTREAM O2 SENSOR SHORTED TO VOLTAGE";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 207;
		Row["description"] = "UPSTREAM O2 SENSOR SHORTED TO GROUND";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 208;
		Row["description"] = "NO CAM SYNC SIGNAL AT PCM";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 209;
		Row["description"] = "GLOW PLUG RELAY CONTROL CIRCUIT";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 210;
		Row["description"] = "HIGH SPEED CONDENSER FAN CONTROL RELAY CIRCUIT";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 211;
		Row["description"] = "AW4 TRANSMISSION SHIFT SOLENOID B (2-3) SHORTED TO VOLTAGE (12V)";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 212;
		Row["description"] = "EGR POSITION SENSOR VOLTS LOW";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 213;
		Row["description"] = "EGR POSITION SENSOR VOLTS HIGH";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 214;
		Row["description"] = "NO 5 VOLTS TO EGR POSITION SENSOR";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 215;
		Row["description"] = "EGR POSITION SENSOR RATIONALITY FAILURE";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 216;
		Row["description"] = "IGNITION COIL #6 PRIMARY CIRCUIT";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 217;
		Row["description"] = "INTAKE MANIFOLD SHORT RUNNER SOLENOID CIRCUIT";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 218;
		Row["description"] = "AIR ASSIST INJECTION SOLENOID CIRCUIT";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 219;
		Row["description"] = "CATALYST TEMPERATURE SENSOR VOLTS HIGH";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 220;
		Row["description"] = "CATALYST TEMPERATURE SENSOR VOLTS LOW";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 221;
		Row["description"] = "EATX RPM PULSE PERFORMANCE CONDITION";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 222;
		Row["description"] = "NO BUS MESSAGE RECEIVED FROM COMPANION MODULE";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 223;
		Row["description"] = "MIL FAULT IN COMPANION MODULE";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 224;
		Row["description"] = "COOLANT TEMPERATURE SENSOR PERFORMANCE";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 225;
		Row["description"] = "NO MIC BUS MESSAGE";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 226;
		Row["description"] = "NO SKIM BUS MESSAGE RECEIVED";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 227;
		Row["description"] = "IGNITION COIL #7 PRIMARY CIRCUIT";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 228;
		Row["description"] = "IGNITION COIL #8 PRIMARY CIRCUIT";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 229;
		Row["description"] = "PCV SOLENOID CIRCUIT";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 230;
		Row["description"] = "TRANSMISSION FAN RELAY CIRCUIT";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 231;
		Row["description"] = "TCC OR O/D SOLENOID PERFORMANCE";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 232;
		Row["description"] = "WRONG OR INVALID KEY MESSAGE RECEIVED FROM SKIM";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 234;
		Row["description"] = "AW4 TRANSMISSION SOLENOID A 1-2/3-4 OR TCC SOLENOID C FUNCTIONAL FAIL";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 235;
		Row["description"] = "AW4 TRANSMISSION TCC SOLENOID C SHORTED TO GROUND";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 236;
		Row["description"] = "AW4 TRANSMISSION TCC SOLENOID C SHORTED TO VOLTAGE (12V)";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 237;
		Row["description"] = "AW4 TRANSMISSION BATTERY VOLTS SENSE LOW";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 238;
		Row["description"] = "AW4 TRANSMISSION BATTERY VOLTS SENSE HIGH";
		SBEC3EngineDTC.Rows.Add(Row);
		Row = SBEC3EngineDTC.NewRow();
		Row["id"] = 239;
		Row["description"] = "AISIN AW4 TRANSMISSION DTC PRESENT";
		SBEC3EngineDTC.Rows.Add(Row);
		SBEC3EngineDTCList = (from r in SBEC3EngineDTC.AsEnumerable()
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
		byte[] array4 = new byte[0];
		byte[] array5 = new byte[0];
		byte[] array6 = new byte[0];
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
		if (array2.Length >= 3)
		{
			if (array2[0] == 16 || array2[0] == 50)
			{
				array4 = new byte[array2.Length - 2];
				Array.Copy(array2, 1, array4, 0, array4.Length);
			}
			else if (array2[0] == 17)
			{
				array5 = new byte[2];
				Array.Copy(array2, 1, array5, 0, array5.Length);
			}
			else if (array2[0] == 46)
			{
				array6 = new byte[array2.Length - 2];
				Array.Copy(array2, 1, array6, 0, array6.Length);
			}
		}
		string text = string.Empty;
		string text2 = string.Empty;
		string text3 = string.Empty;
		byte b = array2[0];
		if (speed == "976.5 baud" || speed == "7812.5 baud")
		{
			switch (b)
			{
			case 0:
				text = "PCM WAKE UP";
				break;
			case 16:
			case 50:
			{
				text = "STORED FAULT CODE LIST";
				if (array2.Length < 3)
				{
					break;
				}
				byte b5 = 0;
				byte b6 = (byte)(array2.Length - 1);
				for (int i = 0; i < b6; i++)
				{
					b5 += array2[i];
				}
				if (b5 != array2[b6])
				{
					StoredFaultCodeList.Clear();
					text2 = "CHECKSUM ERROR";
					StoredFaultCodesSaved = true;
					break;
				}
				StoredFaultCodeList.Clear();
				StoredFaultCodeList.AddRange(array4);
				StoredFaultCodeList.Remove(253);
				StoredFaultCodeList.Remove(254);
				if (StoredFaultCodeList.Count == 0)
				{
					StoredFaultCodeList.Clear();
					text2 = "NO FAULT CODE";
					StoredFaultCodesSaved = false;
				}
				else
				{
					text2 = Util.ByteToHexStringSimple(StoredFaultCodeList.ToArray());
					StoredFaultCodesSaved = false;
				}
				break;
			}
			case 17:
				text = "PENDING FAULT CODE LIST";
				if (array2.Length >= 3)
				{
					if ((array3[0] == 0 || array3[0] == 253) && array3[1] == 0)
					{
						PendingFaultCodeList.Clear();
						text2 = "NO FAULT CODE";
						PendingFaultCodesSaved = false;
					}
					else
					{
						PendingFaultCodeList.AddRange(array5);
						text2 = Util.ByteToHexString(array3, 0, 2);
						PendingFaultCodesSaved = false;
					}
				}
				break;
			case 18:
				text = "SELECT HIGH-SPEED MODE";
				break;
			case 19:
				text = "ACTUATOR TEST";
				if (array2.Length >= 3)
				{
					switch (array3[0])
					{
					case 0:
						text2 = "STOPPED";
						break;
					case 1:
						text = "ACTUATOR TEST | IGNITION COIL BANK #1";
						break;
					case 2:
						text = "ACTUATOR TEST | IGNITION COIL BANK #2";
						break;
					case 3:
						text = "ACTUATOR TEST | IGNITION COIL BANK #3";
						break;
					case 4:
						text = "ACTUATOR TEST | FUEL INJECTOR BANK #1";
						break;
					case 5:
						text = "ACTUATOR TEST | FUEL INJECTOR BANK #2";
						break;
					case 6:
						text = "ACTUATOR TEST | FUEL INJECTOR BANK #3";
						break;
					case 7:
						text = "ACTUATOR TEST | IDLE AIR CONTROL MOTOR";
						break;
					case 8:
						text = "ACTUATOR TEST | RADIATOR FAN RELAY";
						break;
					case 9:
						text = "ACTUATOR TEST | A/C CLUTCH RELAY";
						break;
					case 10:
						text = "ACTUATOR TEST | AUTOMATIC SHUTDOWN RELAY";
						break;
					case 11:
						text = "ACTUATOR TEST | EVAP PURGE SOLENOID";
						break;
					case 12:
						text = "ACTUATOR TEST | CRUISE CONTROL SOLENOIDS";
						break;
					case 13:
						text = "ACTUATOR TEST | ALTERNATOR FIELD";
						break;
					case 14:
						text = "ACTUATOR TEST | TACHOMETER OUTPUT";
						break;
					case 15:
						text = "ACTUATOR TEST | TORQUE CONVERTER CLUTCH RELAY";
						break;
					case 16:
						text = "ACTUATOR TEST | EGR SOLENOID";
						break;
					case 17:
						text = "ACTUATOR TEST | WASTEGATE SOLENOID";
						break;
					case 18:
						text = "ACTUATOR TEST | BAROMETER SOLENOID";
						break;
					case 20:
						text = "ACTUATOR TEST | ALL SOLENOIDS / RELAYS";
						break;
					case 22:
						text = "ACTUATOR TEST | TRANSMISSION O/D SOLENOID";
						break;
					case 23:
						text = "ACTUATOR TEST | SHIFT INDICATOR LAMP";
						break;
					case 25:
						text = "ACTUATOR TEST | SURGE VALVE SOLENOID";
						break;
					case 26:
						text = "ACTUATOR TEST | CRUISE CONTROL VENT SOLENOID";
						break;
					case 27:
						text = "ACTUATOR TEST | CRUISE CONTROL VACUUM SOLENOID";
						break;
					case 28:
						text = "ACTUATOR TEST | ASD FUEL SYSTEM";
						break;
					case 29:
						text = "ACTUATOR TEST | FUEL INJECTOR BANK #4";
						break;
					case 30:
						text = "ACTUATOR TEST | FUEL INJECTOR BANK #5";
						break;
					case 31:
						text = "ACTUATOR TEST | FUEL INJECTOR BANK #6";
						break;
					case 35:
						text = "ACTUATOR TEST | IGNITION COIL BANK #4";
						break;
					case 36:
						text = "ACTUATOR TEST | IGNITION COIL BANK #5";
						break;
					case 37:
						text = "ACTUATOR TEST | FUEL INJECTOR BANK #7";
						break;
					case 38:
						text = "ACTUATOR TEST | FUEL INJECTOR BANK #8";
						break;
					case 40:
						text = "ACTUATOR TEST | INTAKE HEATER BANK #1";
						break;
					case 41:
						text = "ACTUATOR TEST | INTAKE HEATER BANK #2";
						break;
					case 44:
						text = "ACTUATOR TEST | CRUISE CONTROL 12V FEED";
						break;
					case 45:
						text = "ACTUATOR TEST | INTAKE MANIFOLD TUNE VALVE";
						break;
					case 46:
						text = "ACTUATOR TEST | LOW SPEED RADIATOR FAN RELAY";
						break;
					case 47:
						text = "ACTUATOR TEST | HIGH SPEED RADIATOR FAN RELAY";
						break;
					case 48:
						text = "ACTUATOR TEST | FUEL INJECTOR BANK #9";
						break;
					case 49:
						text = "ACTUATOR TEST | FUEL INJECTOR BANK #10";
						break;
					case 50:
						text = "ACTUATOR TEST | 2-3 LOCKOUT SOLENOID";
						break;
					case 51:
						text = "ACTUATOR TEST | FUEL PUMP RELAY";
						break;
					case 59:
						text = "ACTUATOR TEST | IAC MOTOR STEP UP";
						break;
					case 60:
						text = "ACTUATOR TEST | IAC MOTOR STEP DOWN";
						break;
					case 61:
						text = "ACTUATOR TEST | LD PUMP SOLENOID";
						break;
					case 62:
						text = "ACTUATOR TEST | ALL RADIATOR FAN RELAYS";
						break;
					case 64:
						text = "ACTUATOR TEST | O2 SENSOR HEATER RELAY";
						break;
					case 65:
						text = "ACTUATOR TEST | OVERDRIVE LAMP";
						break;
					case 67:
						text = "ACTUATOR TEST | TRANSMISSION 12V RELAY";
						break;
					case 68:
						text = "ACTUATOR TEST | REVERSE LOCKOUT SOLENOID";
						break;
					case 70:
						text = "ACTUATOR TEST | SHORT RUNNER VALVE";
						break;
					case 73:
						text = "ACTUATOR TEST | WAIT TO START LAMP";
						break;
					case 80:
						text = "ACTUATOR TEST | TRANSMISSION FAN RELAY";
						break;
					case 81:
						text = "ACTUATOR TEST | TRANSMISSION PTU SOLENOID";
						break;
					case 82:
						text = "ACTUATOR TEST | O2 X/1 SENSOR HEATER RELAY";
						break;
					case 83:
						text = "ACTUATOR TEST | O2 X/2 SENSOR HEATER RELAY";
						break;
					case 86:
						text = "ACTUATOR TEST | 1/1 O2 SENSOR HEATER RELAY";
						break;
					case 87:
						text = "ACTUATOR TEST | O2 SENSOR HEATER RELAY";
						break;
					case 90:
						text = "ACTUATOR TEST | RADIATOR FAN SOLENOID";
						break;
					case 91:
						text = "ACTUATOR TEST | 1/2 O2 SENSOR HEATER RELAY";
						break;
					case 93:
						text = "ACTUATOR TEST | EXHAUST BRAKE";
						break;
					case 94:
						text = "ACTUATOR TEST | FUEL CONTROL";
						break;
					case 95:
						text = "ACTUATOR TEST | PWM RADIATOR FAN";
						break;
					default:
						text = "ACTUATOR TEST | MODE: " + Util.ByteToHexString(array3);
						break;
					}
					if (array3[0] != 0)
					{
						text2 = ((array3[0] != array3[1]) ? "MODE NOT AVAILABLE" : "RUNNING");
					}
				}
				break;
			case 20:
				text = "REQUEST DIAGNOSTIC DATA";
				if (array2.Length < 3)
				{
					break;
				}
				switch (array3[0])
				{
				case 1:
				{
					text = "BATTERY TEMPERATURE SENSOR VOLTAGE";
					double value31 = (double)(int)array3[1] * 0.0196;
					text2 = Math.Round(value31, 3).ToString("0.000");
					text3 = "V";
					break;
				}
				case 2:
				{
					text = "UPSTREAM O2 1/1 SENSOR VOLTAGE";
					double value32 = (double)(int)array3[1] * 0.0196;
					text2 = Math.Round(value32, 3).ToString("0.000");
					text3 = "V";
					break;
				}
				case 5:
				{
					text = "ENGINE COOLANT TEMPERATURE";
					double num9 = array3[1] - 128;
					double a4 = 1.8 * num9 + 32.0;
					if (Settings.Default.Units == "imperial")
					{
						text2 = Math.Round(a4).ToString("0");
						text3 = "°F";
					}
					else if (Settings.Default.Units == "metric")
					{
						text2 = num9.ToString("0");
						text3 = "°C";
					}
					break;
				}
				case 6:
				{
					text = "ENGINE COOLANT TEMPERATURE SENSOR VOLTAGE";
					double value42 = (double)(int)array3[1] * 0.0196;
					text2 = Math.Round(value42, 3).ToString("0.000");
					text3 = "V";
					break;
				}
				case 7:
				{
					text = "THROTTLE POSITION SENSOR VOLTAGE";
					double value7 = (double)(int)array3[1] * 0.0196;
					text2 = Math.Round(value7, 3).ToString("0.000");
					text3 = "V";
					break;
				}
				case 8:
				{
					text = "MINIMUM TPS VOLTAGE";
					double value39 = (double)(int)array3[1] * 0.0196;
					text2 = Math.Round(value39, 3).ToString("0.000");
					text3 = "V";
					break;
				}
				case 9:
				{
					text = "KNOCK SENSOR 1 VOLTAGE";
					double value40 = (double)(int)array3[1] * 0.0196;
					text2 = Math.Round(value40, 3).ToString("0.000");
					text3 = "V";
					break;
				}
				case 10:
				{
					text = "BATTERY VOLTAGE";
					double value36 = (double)(int)array3[1] * 0.0625;
					text2 = Math.Round(value36, 3).ToString("0.000");
					text3 = "V";
					break;
				}
				case 11:
				{
					text = "INTAKE MANIFOLD ABSOLUTE PRESSURE (MAP)";
					double num8 = (double)(int)array3[1] * 0.059756;
					double value25 = num8 * 6.894757;
					if (Settings.Default.Units == "imperial")
					{
						text2 = Math.Round(num8, 1).ToString("0.0");
						text3 = "PSI";
					}
					else if (Settings.Default.Units == "metric")
					{
						text2 = Math.Round(value25, 1).ToString("0.0");
						text3 = "KPA";
					}
					break;
				}
				case 12:
					text = "TARGET IAC STEPPER MOTOR POSITION";
					text2 = array3[1].ToString("0");
					break;
				case 14:
				{
					text = "LONG TERM FUEL TRIM 1";
					double num15 = (double)(int)array3[1] * 0.196;
					if (array3[1] >= 128)
					{
						num15 -= 50.0;
					}
					text2 = Math.Round(num15, 3).ToString("0.000");
					text3 = "PERCENT";
					break;
				}
				case 15:
				{
					text = "BAROMETRIC PRESSURE";
					double num12 = (double)(int)array3[1] * 0.059756;
					double value37 = num12 * 6.894757;
					if (Settings.Default.Units == "imperial")
					{
						text2 = Math.Round(num12, 1).ToString("0.0");
						text3 = "PSI";
					}
					else if (Settings.Default.Units == "metric")
					{
						text2 = Math.Round(value37, 1).ToString("0.0");
						text3 = "KPA";
					}
					break;
				}
				case 16:
					text = "MINIMUM AIR FLOW TEST";
					text2 = ((array3[1] != 0) ? "RUNNING" : "STOPPED");
					break;
				case 17:
					text = "ENGINE SPEED";
					text2 = ((double)(int)array3[1] * 32.0).ToString("0");
					text3 = "RPM";
					break;
				case 18:
					text = "CAM/CRANK SYNC SENSE";
					text2 = ((!Util.IsBitSet(array3[1], 4)) ? "ENGINE STOPPED" : "IN-SYNC");
					break;
				case 19:
					text = "KEY-ON CYCLES ERROR 1";
					text2 = array3[1].ToString("0");
					break;
				case 21:
				{
					text = "SPARK ADVANCE";
					double value19 = (double)(int)array3[1] * 0.5;
					text2 = Math.Round(value19, 1).ToString("0.0");
					text3 = "DEG";
					break;
				}
				case 22:
				case 33:
				{
					text = "CYLINDER 1 RETARD";
					double value20 = (double)(int)array3[1] * 0.5;
					text2 = Math.Round(value20, 1).ToString("0.0");
					text3 = "DEG";
					break;
				}
				case 23:
				{
					text = "CYLINDER 2 RETARD";
					double value15 = (double)(int)array3[1] * 0.5;
					text2 = Math.Round(value15, 1).ToString("0.0");
					text3 = "DEG";
					break;
				}
				case 24:
				{
					text = "CYLINDER 3 RETARD";
					double value16 = (double)(int)array3[1] * 0.5;
					text2 = Math.Round(value16, 1).ToString("0.0");
					text3 = "DEG";
					break;
				}
				case 25:
				{
					text = "CYLINDER 4 RETARD";
					double value8 = (double)(int)array3[1] * 0.5;
					text2 = Math.Round(value8, 1).ToString("0.0");
					text3 = "DEG";
					break;
				}
				case 26:
				{
					text = "TARGET BOOST";
					double num14 = (double)(int)array3[1] * 0.115294117;
					double value41 = num14 * 6.89475729;
					if (Settings.Default.Units == "imperial")
					{
						text2 = Math.Round(num14, 1).ToString("0.0");
						text3 = "PSI";
					}
					else if (Settings.Default.Units == "metric")
					{
						text2 = Math.Round(value41, 1).ToString("0.0");
						text3 = "KPA";
					}
					break;
				}
				case 27:
				{
					text = "INTAKE AIR TEMPERATURE";
					double num13 = array3[1] - 128;
					double a7 = 1.8 * num13 + 32.0;
					if (Settings.Default.Units == "imperial")
					{
						text2 = Math.Round(a7).ToString("0");
						text3 = "°F";
					}
					else if (Settings.Default.Units == "metric")
					{
						text2 = num13.ToString("0");
						text3 = "°C";
					}
					break;
				}
				case 28:
				{
					text = "INTAKE AIR TEMPERATURE SENSOR VOLTAGE";
					double value38 = (double)(int)array3[1] * 0.0196;
					text2 = Math.Round(value38, 3).ToString("0.000");
					text3 = "V";
					break;
				}
				case 29:
				{
					text = "CRUISE SET SPEED";
					double num11 = (double)(int)array3[1] * 0.5;
					double a6 = num11 * 1.609344;
					if (Settings.Default.Units == "imperial")
					{
						text2 = num11.ToString("0");
						text3 = "MPH";
					}
					else if (Settings.Default.Units == "metric")
					{
						text2 = Math.Round(a6).ToString("0");
						text3 = "KM/H";
					}
					break;
				}
				case 30:
					text = "KEY-ON CYCLES ERROR 2";
					text2 = array3[1].ToString("0");
					break;
				case 31:
					text = "KEY-ON CYCLES ERROR 3";
					text2 = array3[1].ToString("0");
					break;
				case 32:
				{
					string text6 = (array3[1] & 0xF0) switch
					{
						0 => "ON/OFF SW", 
						16 => "SPEED SEN", 
						32 => "RPM LIMIT", 
						48 => "BRAKE SW", 
						64 => "P/N SW", 
						80 => "RPM/SPEED", 
						96 => "CLUTCH", 
						112 => "DTC PRESENT", 
						128 => "KEY OFF", 
						144 => "ACTIVE", 
						160 => "CLUTCH UP", 
						176 => "N/A", 
						192 => "SW DTC", 
						208 => "CANCEL SW", 
						224 => "TPS LIMP-IN", 
						240 => "12V DTC", 
						_ => "N/A", 
					};
					string text7 = (array3[1] & 0xF) switch
					{
						0 => "ON/OFF SW", 
						1 => "SPEED SEN", 
						2 => "RPM LIMIT", 
						3 => "BRAKE SW", 
						4 => "P/N SW", 
						5 => "RPM/SPEED", 
						6 => "CLUTCH", 
						7 => "DTC PRESENT", 
						8 => "ALLOWED", 
						9 => "ACTIVE", 
						10 => "CLUTCH UP", 
						11 => "N/A", 
						12 => "SW DTC", 
						13 => "CANCEL SW", 
						14 => "TPS LIMP-IN", 
						15 => "12V DTC", 
						_ => "N/A", 
					};
					if ((array3[1] & 0xF) == 8)
					{
						text = "CRUISE | LAST CUTOUT: " + text6 + " | STATE: " + text7;
						text2 = "STOPPED";
					}
					else if ((array3[1] & 0xF) == 9)
					{
						text = "CRUISE | LAST CUTOUT: " + text6 + " | STATE: " + text7;
						text2 = "ENGAGED";
					}
					else
					{
						text = "CRUISE | LAST CUTOUT: " + text6 + " | DENIED: " + text7;
						text2 = "STOPPED";
					}
					break;
				}
				case 36:
				{
					text = "BATTERY CHARGING VOLTAGE";
					double value23 = (double)(int)array3[1] * 0.0625;
					text2 = Math.Round(value23, 3).ToString("0.000");
					text3 = "V";
					break;
				}
				case 37:
					text = "OVER 5 PSI BOOST TIMER";
					text2 = array3[1].ToString("0");
					break;
				case 40:
				{
					text = "WASTEGATE DUTY CYCLE";
					double value22 = (double)(int)array3[1] * 0.5;
					text2 = Math.Round(value22, 1).ToString("0.0");
					text3 = "PERCENT";
					break;
				}
				case 39:
					text = "VEHICLE THEFT ALARM STATUS";
					text2 = Convert.ToString(array3[1], 2).PadLeft(8, '0');
					break;
				case 41:
					text = "READ FUEL SETTING";
					text2 = Util.ByteToHexString(array3, 1);
					break;
				case 42:
				{
					text = "READ SET FUEL SYNC";
					int num6 = array3[1];
					text2 = num6.ToString();
					text3 = "DEG";
					break;
				}
				case 44:
				{
					text = "CRUISE SWITCH VOLTAGE SENSE";
					double value21 = (double)(int)array3[1] * 0.0196;
					text2 = Math.Round(value21, 3).ToString("0.000");
					text3 = "V";
					break;
				}
				case 45:
				{
					text = "AMBIENT/BATTERY TEMPERATURE";
					double num5 = array3[1] - 128;
					double a3 = 1.8 * num5 + 32.0;
					if (Settings.Default.Units == "imperial")
					{
						text2 = Math.Round(a3).ToString("0");
						text3 = "°F";
					}
					else if (Settings.Default.Units == "metric")
					{
						text2 = num5.ToString("0");
						text3 = "°C";
					}
					break;
				}
				case 46:
					text = "FUEL FACTOR (NOT LH)";
					text2 = array3[1].ToString("0");
					break;
				case 47:
				{
					text = "UPSTREAM O2 2/1 SENSOR VOLTAGE";
					double value18 = (double)(int)array3[1] * 0.0196;
					text2 = Math.Round(value18, 3).ToString("0.000");
					text3 = "V";
					break;
				}
				case 48:
				{
					text = "KNOCK SENSOR 2 VOLTAGE";
					double value17 = (double)(int)array3[1] * 0.0196;
					text2 = Math.Round(value17, 3).ToString("0.000");
					text3 = "V";
					break;
				}
				case 49:
				{
					text = "LONG TERM FUEL TRIM 2";
					double num4 = (double)(int)array3[1] * 0.196;
					if (array3[1] >= 128)
					{
						num4 -= 50.0;
					}
					text2 = Math.Round(num4, 3).ToString("0.000");
					text3 = "PERCENT";
					break;
				}
				case 50:
				{
					text = "A/C HIGH-SIDE PRESSURE SENSOR VOLTAGE";
					double value14 = (double)(int)array3[1] * 0.0196;
					text2 = Math.Round(value14, 3).ToString("0.000");
					text3 = "V";
					break;
				}
				case 51:
				{
					text = "A/C HIGH-SIDE PRESSURE";
					double num2 = (double)(int)array3[1] * 1.961;
					double value2 = num2 * 6.894757;
					if (Settings.Default.Units == "imperial")
					{
						text2 = Math.Round(num2, 1).ToString("0.0");
						text3 = "PSI";
					}
					else if (Settings.Default.Units == "metric")
					{
						text2 = Math.Round(value2, 1).ToString("0.0");
						text3 = "KPA";
					}
					break;
				}
				case 52:
				{
					text = "FLEX FUEL SENSOR VOLTAGE";
					double value = (double)(int)array3[1] * 0.0196;
					text2 = Math.Round(value, 3).ToString("0.000");
					text3 = "V";
					break;
				}
				case 53:
					text = "FLEX FUEL INFO 1";
					text2 = array3[1].ToString("0");
					break;
				case 59:
					text = "FUEL SYSTEM STATUS 1";
					if (Util.IsBitSet(array3[1], 0))
					{
						text2 = "OPEN LOOP";
					}
					if (Util.IsBitSet(array3[1], 1))
					{
						text2 = "CLOSED LOOP";
					}
					if (Util.IsBitSet(array3[1], 2))
					{
						text2 = "OPEN LOOP / DRIVE";
					}
					if (Util.IsBitSet(array3[1], 3))
					{
						text2 = "OPEN LOOP / DTC";
					}
					if (Util.IsBitSet(array3[1], 4))
					{
						text2 = "CLOSED LOOP / DTC";
					}
					break;
				case 62:
				{
					text = "CALPOT VOLTAGE";
					double value35 = (double)(int)array3[1] * 0.0196;
					text2 = Math.Round(value35, 3).ToString("0.000");
					break;
				}
				case 63:
				{
					text = "DOWNSTREAM O2 1/2 SENSOR VOLTAGE";
					double value34 = (double)(int)array3[1] * 0.0196;
					text2 = Math.Round(value34, 3).ToString("0.000");
					text3 = "V";
					break;
				}
				case 64:
				{
					text = "MAP SENSOR VOLTAGE";
					double value33 = (double)(int)array3[1] * 0.0196;
					text2 = Math.Round(value33, 3).ToString("0.000");
					text3 = "V";
					break;
				}
				case 65:
				{
					text = "VEHICLE SPEED";
					byte b2 = array3[1];
					double a5 = (double)(int)b2 * 1.609344;
					if (Settings.Default.Units == "imperial")
					{
						text2 = b2.ToString("0");
						text3 = "MPH";
					}
					else if (Settings.Default.Units == "metric")
					{
						text2 = Math.Round(a5).ToString("0");
						text3 = "KM/H";
					}
					break;
				}
				case 66:
					text = "UPSTREAM O2 1/1 SENSOR LEVEL";
					text2 = array3[1] switch
					{
						160 => "LEAN", 
						177 => "RICH", 
						byte.MaxValue => "CENTER", 
						_ => "N/A", 
					};
					break;
				case 69:
				{
					text = "MAP VACUUM";
					double num10 = (double)(int)array3[1] * 0.059756;
					double value30 = num10 * 6.894757;
					if (Settings.Default.Units == "imperial")
					{
						text2 = Math.Round(num10, 1).ToString("0.0");
						text3 = "PSI";
					}
					else if (Settings.Default.Units == "metric")
					{
						text2 = Math.Round(value30, 1).ToString("0.0");
						text3 = "KPA";
					}
					break;
				}
				case 70:
				{
					text = "DELTA THROTTLE POSITION";
					double value29 = (double)(int)array3[1] * 0.0196;
					text2 = Math.Round(value29, 3).ToString("0.000");
					text3 = "V";
					break;
				}
				case 71:
				{
					text = "SPARK ADVANCE";
					double value28 = (double)(int)array3[1] * 0.5;
					text2 = Math.Round(value28, 1).ToString("0.0");
					text3 = "DEG";
					break;
				}
				case 72:
					text = "UPSTREAM O2 2/1 SENSOR LEVEL";
					text2 = array3[1] switch
					{
						160 => "LEAN", 
						177 => "RICH", 
						byte.MaxValue => "CENTER", 
						_ => "N/A", 
					};
					break;
				case 73:
				{
					text = "DOWNSTREAM O2 2/2 SENSOR VOLTAGE";
					double value27 = (double)(int)array3[1] * 0.0196;
					text2 = Math.Round(value27, 3).ToString("0.000");
					text3 = "V";
					break;
				}
				case 74:
					text = "DOWNSTREAM O2 1/2 SENSOR LEVEL";
					text2 = array3[1] switch
					{
						160 => "LEAN", 
						177 => "RICH", 
						byte.MaxValue => "CENTER", 
						_ => "N/A", 
					};
					break;
				case 75:
					text = "DOWNSTREAM O2 2/2 SENSOR LEVEL";
					text2 = array3[1] switch
					{
						160 => "LEAN", 
						177 => "RICH", 
						byte.MaxValue => "CENTER", 
						_ => "N/A", 
					};
					break;
				case 78:
				{
					text = "FUEL LEVEL SENSOR VOLTAGE";
					double value26 = (double)(int)array3[1] * 0.0196;
					text2 = Math.Round(value26, 3).ToString("0.000");
					text3 = "V";
					break;
				}
				case 79:
				{
					text = "FUEL LEVEL";
					double num7 = (double)(int)array3[1] * 0.125;
					double value24 = num7 * 3.785412;
					if (Settings.Default.Units == "imperial")
					{
						text2 = Math.Round(num7, 1).ToString("0.0");
						text3 = "GALLON";
					}
					else if (Settings.Default.Units == "metric")
					{
						text2 = Math.Round(value24, 1).ToString("0.0");
						text3 = "LITER";
					}
					break;
				}
				case 87:
				{
					text = "FUEL SYSTEM STATUS 2";
					List<string> list = new List<string>();
					if (Util.IsBitSet(array3[1], 0))
					{
						list.Add("OPEN LOOP");
					}
					if (Util.IsBitSet(array3[1], 1))
					{
						list.Add("CLOSED LOOP");
					}
					if (Util.IsBitSet(array3[1], 2))
					{
						list.Add("OPEN LOOP / DRIVE");
					}
					if (Util.IsBitSet(array3[1], 3))
					{
						list.Add("OPEN LOOP / DTC");
					}
					if (Util.IsBitSet(array3[1], 4))
					{
						list.Add("CLOSED LOOP / DTC");
					}
					if (list.Count == 0)
					{
						text2 = "N/A";
						break;
					}
					foreach (string item in list)
					{
						text2 = text2 + item + " | ";
					}
					if (text2.Length > 2)
					{
						text2 = text2.Remove(text2.Length - 3);
					}
					break;
				}
				case 88:
				{
					string text4 = (array3[1] & 0xF0) switch
					{
						0 => "ON/OFF SW", 
						16 => "SPEED SEN", 
						32 => "RPM LIMIT", 
						48 => "BRAKE SW", 
						64 => "P/N SW", 
						80 => "RPM/SPEED", 
						96 => "CLUTCH", 
						112 => "DTC PRESENT", 
						128 => "KEY OFF", 
						144 => "ACTIVE", 
						160 => "CLUTCH UP", 
						176 => "N/A", 
						192 => "SW DTC", 
						208 => "CANCEL", 
						224 => "TPS LIMP-IN", 
						240 => "12V DTC", 
						_ => "N/A", 
					};
					string text5 = (array3[1] & 0xF) switch
					{
						0 => "ON/OFF SW", 
						1 => "SPEED SEN", 
						2 => "RPM LIMIT", 
						3 => "BRAKE SW", 
						4 => "P/N SW", 
						5 => "RPM/SPEED", 
						6 => "CLUTCH", 
						7 => "DTC PRESENT", 
						8 => "ALLOWED", 
						9 => "ACTIVE", 
						10 => "CLUTCH UP", 
						11 => "N/A", 
						12 => "SW DTC", 
						13 => "CANCEL", 
						14 => "TPS LIMP-IN", 
						15 => "12V DTC", 
						_ => "N/A", 
					};
					if ((array3[1] & 0xF) == 8)
					{
						text = "CRUISE | LAST CUTOUT: " + text4 + " | STATE: " + text5;
						text2 = "STOPPED";
					}
					else if ((array3[1] & 0xF) == 9)
					{
						text = "CRUISE | LAST CUTOUT: " + text4 + " | STATE: " + text5;
						text2 = "ENGAGED";
					}
					else
					{
						text = "CRUISE | LAST CUTOUT: " + text4 + " | DENIED: " + text5;
						text2 = "STOPPED";
					}
					break;
				}
				case 89:
					text = "CRUISE CONTROL OPERATING MODE";
					text2 = (array3[1] & 0xF) switch
					{
						8 => "DISENGAGED", 
						9 => "NORMAL", 
						10 => "ACCELERATING", 
						11 => "DECELERATING", 
						_ => "N/A", 
					};
					break;
				case 90:
					text = "OUTPUT SHAFT SPEED";
					text2 = ((double)(int)array3[1] * 20.0).ToString("0");
					text3 = "RPM";
					break;
				case 91:
				{
					text = "GOVERNOR PRESSURE DUTY CYCLE";
					double value13 = (double)(int)array3[1] * 0.3921568627;
					text2 = Math.Round(value13, 1).ToString("0.0");
					text3 = "PERCENT";
					break;
				}
				case 92:
				{
					text = "ENGINE LOAD";
					double value12 = (double)(int)array3[1] * 0.3921568627;
					text2 = Math.Round(value12, 1).ToString("0.0");
					text3 = "PERCENT";
					break;
				}
				case 95:
				{
					text = "EGR POSITION SENSOR VOLTAGE";
					double value11 = (double)(int)array3[1] * 0.0196;
					text2 = Math.Round(value11, 3).ToString("0.000");
					text3 = "V";
					break;
				}
				case 96:
					text = "EGR ZREF UPDATE D.C.";
					text2 = array3[1].ToString("0");
					break;
				case 100:
				{
					text = "ACTUAL PURGE CURRENT";
					double value10 = (double)(int)array3[1] * 0.0196;
					text2 = Math.Round(value10, 1).ToString("0.0");
					text3 = "A";
					break;
				}
				case 101:
				{
					text = "CATALYST TEMPERATURE SENSOR VOLTAGE";
					double value9 = (double)(int)array3[1] * 0.0196;
					text2 = Math.Round(value9, 3).ToString("0.000");
					text3 = "V";
					break;
				}
				case 102:
				{
					text = "CATALYST TEMPERATURE";
					double num3 = array3[1] - 128;
					double a2 = 1.8 * num3 + 32.0;
					if (Settings.Default.Units == "imperial")
					{
						text2 = Math.Round(a2).ToString("0");
						text3 = "°F";
					}
					else if (Settings.Default.Units == "metric")
					{
						text2 = num3.ToString("0");
						text3 = "°C";
					}
					break;
				}
				case 105:
				{
					text = "AMBIENT TEMPERATURE SENSOR VOLTAGE";
					double value6 = (double)(int)array3[1] * 0.0196;
					text2 = Math.Round(value6, 3).ToString("0.000");
					text3 = "V";
					break;
				}
				case 109:
				{
					text = "T-CASE SWITCH VOLTAGE";
					double value5 = (double)(int)array3[1] * 0.0196;
					text2 = Math.Round(value5, 3).ToString("0.000");
					text3 = "V";
					break;
				}
				case 122:
				{
					text = "FCA CURRENT";
					double value4 = (double)(int)array3[1] * 0.0196;
					text2 = Math.Round(value4, 1).ToString("0.0");
					text3 = "A";
					break;
				}
				case 124:
				{
					text = "OIL TEMPERATURE SENSOR VOLTAGE";
					double value3 = (double)(int)array3[1] * 0.0196;
					text2 = Math.Round(value3, 3).ToString("0.000");
					text3 = "V";
					break;
				}
				case 125:
				{
					text = "OIL TEMPERATURE";
					double num = array3[1] - 64;
					double a = 1.8 * num + 32.0;
					if (Settings.Default.Units == "imperial")
					{
						text2 = Math.Round(a).ToString("0");
						text3 = "°F";
					}
					else if (Settings.Default.Units == "metric")
					{
						text2 = num.ToString("0");
						text3 = "°C";
					}
					break;
				}
				default:
					text = "REQUEST DIAGNOSTIC DATA | OFFSET: " + Util.ByteToHexString(array3);
					text2 = Util.ByteToHexString(array3, 1, array3.Length - 1);
					break;
				}
				break;
			case 21:
				text = "READ MEMORY";
				if (array2.Length >= 4)
				{
					text = "READ MEMORY | OFFSET: " + Util.ByteToHexString(array3, 0, 2);
					text2 = Util.ByteToHexString(array3, 2);
				}
				break;
			case 22:
			{
				text = "CONFIGURATION";
				if (array2.Length < 7)
				{
					break;
				}
				text = "CONFIGURATION | PAGE: " + Util.ByteToHexStringSimple(new byte[1] { array3[0] });
				ControllerHardwareType = 3;
				if (Util.ChecksumCalculator(array2, 0, array2.Length - 1) != array2[^1])
				{
					text2 = "CHECKSUM ERROR";
					break;
				}
				text2 = Util.ByteToHexString(array3, 1, 4);
				byte b3 = array3[0];
				if (b3 == 128)
				{
					PartNumberChars[0] = array3[1];
					PartNumberChars[1] = array3[2];
					PartNumberChars[2] = array3[3];
					PartNumberChars[3] = array3[4];
					text += " | PART NUMBER";
					text2 = text2.Replace(" ", "");
				}
				break;
			}
			case 23:
				text = "ERASE ENGINE FAULT CODES";
				if (array2.Length >= 2)
				{
					text2 = array3[0] switch
					{
						0 => "DENIED (STOP ENGINE)", 
						224 => "ERASED", 
						byte.MaxValue => "DENIED (START ENGINE)", 
						_ => "UNKNOWN RESULT", 
					};
				}
				break;
			case 24:
				text = "CONTROL ASD RELAY";
				if (array2.Length >= 3)
				{
					text2 = Util.ByteToHexString(array3, 1);
				}
				break;
			case 25:
				text = "SET ENGINE IDLE SPEED";
				if (array2.Length >= 2)
				{
					text2 = (array3[0] << 3).ToString("0");
					text3 = "RPM";
				}
				break;
			case 26:
			{
				text = "SWITCH TEST";
				if (array2.Length < 3)
				{
					break;
				}
				List<string> list2 = new List<string>();
				switch (array3[0])
				{
				case 1:
					if (Util.IsBitSet(array3[1], 0))
					{
						list2.Add("WAIT TO START LAMP");
					}
					if (Util.IsBitSet(array3[1], 1))
					{
						list2.Add("INTAKE HEATER #1");
					}
					if (Util.IsBitSet(array3[1], 2))
					{
						list2.Add("INTAKE HEATER #2");
					}
					if (Util.IsBitSet(array3[1], 3))
					{
						list2.Add("IDLE VALIDATION SW1");
					}
					if (Util.IsBitSet(array3[1], 4))
					{
						list2.Add("IDLE VALIDATION SW2");
					}
					if (Util.IsBitSet(array3[1], 5))
					{
						list2.Add("IDLE SELECT");
					}
					if (Util.IsBitSet(array3[1], 6))
					{
						list2.Add("TRANSFER PMPDR");
					}
					break;
				case 2:
					if (Util.IsBitSet(array3[1], 0))
					{
						list2.Add("INJ PUMP");
					}
					if (Util.IsBitSet(array3[1], 2))
					{
						list2.Add("A/C CLUTCH");
					}
					if (Util.IsBitSet(array3[1], 3))
					{
						list2.Add("EXHAUST BRAKE");
					}
					if (Util.IsBitSet(array3[1], 4))
					{
						list2.Add("BRAKE");
					}
					if (Util.IsBitSet(array3[1], 5))
					{
						list2.Add("EVAP PURGE");
					}
					if (Util.IsBitSet(array3[1], 7))
					{
						list2.Add("LOW OIL");
					}
					break;
				case 3:
					if (Util.IsBitSet(array3[1], 1))
					{
						list2.Add("MIL");
					}
					if (Util.IsBitSet(array3[1], 2))
					{
						list2.Add("GENERATOR LAMP");
					}
					if (Util.IsBitSet(array3[1], 3))
					{
						list2.Add("GENERATOR FIELD");
					}
					if (Util.IsBitSet(array3[1], 4))
					{
						list2.Add("12V FEED");
					}
					if (Util.IsBitSet(array3[1], 6))
					{
						list2.Add("TRANS O/D");
					}
					if (Util.IsBitSet(array3[1], 7))
					{
						list2.Add("TRANS TOW MODE");
					}
					break;
				case 4:
					if (Util.IsBitSet(array3[1], 1))
					{
						list2.Add("TRD LINK");
					}
					if (Util.IsBitSet(array3[1], 4))
					{
						list2.Add("ASD");
					}
					if (Util.IsBitSet(array3[1], 6))
					{
						list2.Add("IGNITION");
					}
					break;
				default:
					text = "SWITCH TEST | OFFSET: " + Util.ByteToHexString(array3);
					break;
				}
				if (list2.Count == 0)
				{
					break;
				}
				text += " | ";
				foreach (string item2 in list2)
				{
					text = text + item2 + " | ";
				}
				if (text.Length > 2)
				{
					text = text.Remove(text.Length - 3);
				}
				break;
			}
			case 27:
				text = "INIT BYTE MODE DOWNLOAD";
				break;
			case 28:
				text = "WRITE EEPROM";
				if (array2.Length >= 4)
				{
					if (array3[0] < 16)
					{
						text = text + " | PCM MILEAGE " + (array3[0] + 1).ToString("0");
					}
					else if (array3[0] >= 16 && array3[0] <= 17)
					{
						text = text + " | EMR " + (array3[0] - 15).ToString("0");
					}
					else if (array3[0] >= 178 && array3[0] <= 194)
					{
						text = text + " | VIN " + (array3[0] - 177).ToString("0");
					}
					else
					{
						byte b3 = array3[0];
						text = ((b3 != 26) ? (text + " | OFFSET: " + Util.ByteToHexString(array3)) : (array3[1] switch
						{
							0 => text + " | DISABLE VARIABLE IDLE", 
							byte.MaxValue => text + " | ENABLE VARIABLE IDLE", 
							_ => text + " | VARIABLE IDLE SETTING", 
						}));
					}
					text2 = Util.ByteToHexString(array3, 1);
					switch (array3[2])
					{
					case 0:
						text2 = "DENIED (MODULE BUSY)";
						break;
					case 226:
						text3 = "OK";
						break;
					}
				}
				break;
			case 29:
			case 30:
			case 31:
				text = "WRITE RAM";
				if (array2.Length >= 4)
				{
					text = text + " | OFFSET: " + Util.ByteToHexString(array3);
					switch (array3[2])
					{
					case 0:
						text2 = "DENIED (INVALID OFFSET)";
						break;
					case 229:
						text2 = Util.ByteToHexString(array3, 1);
						text3 = "OK";
						break;
					case 241:
						text2 = "DENIED (SECURITY LEVEL)";
						break;
					default:
						text2 = Util.ByteToHexString(array3, 2);
						break;
					}
				}
				break;
			case 32:
				text = "RUN RAM WORKER";
				if (array2.Length >= 4)
				{
					switch (array3[2])
					{
					case 0:
						text2 = "DENIED (NO RTS FOUND)";
						break;
					case 1:
						text2 = "DENIED (RTS OFFSET)";
						break;
					case 2:
						text2 = "DENIED (INVALID OFFSET)";
						break;
					case 229:
						text2 = Util.ByteToHexString(array3, 1);
						text3 = "OK";
						break;
					case 241:
						text2 = "DENIED (SECURITY LEVEL)";
						break;
					default:
						text2 = Util.ByteToHexString(array3, 2);
						break;
					}
				}
				break;
			case 33:
				text = "IGNITION TIMING";
				if (array2.Length >= 3)
				{
					switch (array3[0])
					{
					case 0:
						text += " | UNKILL SPARK SCATTER";
						break;
					case 1:
					case 16:
						text += " | KILL SPARK SCATTER";
						break;
					default:
						text = text + " | MODE: " + Util.ByteToHexString(array3);
						break;
					}
					switch (array3[1])
					{
					case 0:
						text2 = "BASIC TIMING ABOLISHED";
						break;
					case 1:
						text2 = "BASIC TIMING INITIATED";
						break;
					case 2:
						text2 = "REJECTED (OPEN THR)";
						break;
					case 3:
						text2 = "REJECTED (IN DRIVE)";
						break;
					default:
						text2 = Util.ByteToHexString(array3, 1);
						text3 = "UNDEFINED";
						break;
					}
				}
				break;
			case 34:
				text = "READ ENGINE PARAMETER";
				if (array2.Length >= 4)
				{
					switch (array3[0])
					{
					case 1:
					{
						double value50 = (double)((array3[1] << 8) + array3[2]) * 0.125;
						text = "ENGINE SPEED";
						text2 = Math.Round(value50, 3).ToString("0.000");
						text3 = "RPM";
						break;
					}
					case 2:
					{
						double value49 = (double)((array3[1] << 8) + array3[2]) * (1.0 / 256.0);
						text = "INJECTOR PULSE WIDTH 1";
						text2 = Math.Round(value49, 3).ToString("0.000");
						text3 = "MS";
						break;
					}
					case 3:
					{
						double value48 = (double)((array3[1] << 8) + array3[2]) * 0.125;
						text = "TARGET IDLE SPEED";
						text2 = Math.Round(value48, 3).ToString("0.000");
						text3 = "RPM";
						break;
					}
					case 4:
					{
						double value47 = (double)((array3[1] << 8) + array3[2]) * (1.0 / 256.0);
						text = "INJECTOR PULSE WIDTH 2";
						text2 = Math.Round(value47, 3).ToString("0.000");
						text3 = "MS";
						break;
					}
					default:
						text = "SEND ENGINE PARAMETER | OFFSET: " + Util.ByteToHexString(array3);
						text2 = Util.ByteToHexString(array3, 1, array3.Length - 1);
						break;
					}
				}
				break;
			case 35:
				text = "RESET MEMORY";
				if (array2.Length >= 3)
				{
					text = array3[0] switch
					{
						1 => "ERASE ALL FAULT DATA", 
						2 => "RESET ADAPTIVE FUEL FACTOR (LTFT)", 
						3 => "RESET IAC COUNTER", 
						4 => "RESET MINIMUM TPS VOLTS", 
						5 => "RESET FLEX FUEL PERCENT", 
						6 => "RESET CAM/CRANK SYNC", 
						7 => "RESET FUEL SHUTOFF", 
						8 => "RESET RUNTIME AT STALL", 
						9 => "DOOR LOCK ENABLE", 
						10 => "DOOR LOCK DISABLE", 
						11 => "RESET CAM/CRANK TIMING REFERENCE", 
						12 => "A/C FAULT ENABLE", 
						13 => "A/C FAULT DISABLE", 
						14 => "CRUISE FAULT ENABLE", 
						15 => "CRUISE FAULT DISABLE", 
						16 => "PS FAULT ENABLE", 
						17 => "PS FAULT DISABLE", 
						18 => "RESET EEPROM / ADAPTIVE NUMERATOR", 
						19 => "SKIM REPLACED / SEND SECRET KEY FROM PCM", 
						20 => "RESET DUTY CYCLE MONITOR", 
						21 => "RESET TRIP/IDLE/CRUISE/INJ", 
						32 => "RESET TPS ADAPTATION FOR ETC", 
						33 => "RESET MIN PEDAL VALUE", 
						34 => "RESET LEARNED KNOCK CORRECTION", 
						35 => "RESET LEARNED MISFIRE CORRECTION", 
						36 => "RESET IDLE ADAPTATION", 
						_ => "RESET MEMORY | OFFSET: " + Util.ByteToHexString(array3), 
					};
					text2 = array3[1] switch
					{
						0 => "STOP ENGINE", 
						1 => "MODE NOT AVAILABLE", 
						2 => "DENIED (MODULE BUSY)", 
						3 => "DENIED (SECURITY LEVEL)", 
						240 => "OK", 
						_ => Util.ByteToHexString(array3, 1), 
					};
				}
				break;
			case 37:
			{
				text = "OVERRIDE";
				if (array2.Length < 4)
				{
					break;
				}
				if (array3[0] == 0 && array3[1] == 0 && array3[2] == 0)
				{
					text += " | ALL SUSPENDED";
					break;
				}
				text = array3[0] switch
				{
					1 => text + " | PPS DUTY CYCLE", 
					2 => text + " | ", 
					3 => text + " | ", 
					4 => text + " | LINEAR EGR STEPS", 
					5 => text + " | FUEL INJECTOR #1", 
					6 => text + " | FUEL INJECTOR #2", 
					7 => text + " | FUEL INJECTOR #3", 
					8 => text + " | FUEL INJECTOR #4", 
					9 => text + " | FUEL INJECTOR #5", 
					10 => text + " | FUEL INJECTOR #6", 
					11 => text + " | ", 
					12 => text + " | ", 
					13 => text + " | ", 
					14 => text + " | ", 
					15 => text + " | MINIMUM AIR FLOW", 
					16 => text + " | CALPOT LHBL", 
					17 => text + " | ALTERNATOR FIELD", 
					18 => text + " | ", 
					19 => text + " | LEAK DETECTION PUMP SYSTEM", 
					28 => text + " | MISFIRE MONITOR", 
					29 => text + " | EVAPORATIVE EMISSION CONTROL SYSTEM", 
					33 => text + " | LINEAR IAC MOTOR", 
					37 => text + " | CYLINDER PERFORMANCE TEST", 
					38 => text + " | HIGH-PRESSURE SAFETY VALVE TEST", 
					_ => text + " | SETTING: " + Util.ByteToHexString(array3), 
				};
				switch (array3[1])
				{
				case 0:
					text2 = "RESET";
					break;
				case 1:
					text2 = "ENABLE";
					break;
				case 2:
					text2 = "DISABLE";
					break;
				default:
					if (array3[1] >= 128)
					{
						text2 = array3[1].ToString("0");
					}
					break;
				}
				if (array3[2] == array3[1])
				{
					text3 = "OK";
					break;
				}
				byte b3 = array3[2];
				text2 = "ERROR";
				text3 = Util.ByteToHexString(array3, 2);
				break;
			}
			case 38:
				text = "READ FLASH MEMORY";
				if (array2.Length >= 5)
				{
					text = "READ FLASH MEMORY | OFFSET: " + Util.ByteToHexString(array3, 0, 3);
					text2 = Util.ByteToHexString(array3, 3);
				}
				break;
			case 39:
				text = "WRITE EEPROM";
				if (array2.Length >= 5)
				{
					text = "WRITE EEPROM | OFFSET: " + Util.ByteToHexString(array3, 0, 2);
					switch (array3[3])
					{
					case 226:
						text2 = Util.ByteToHexString(array3, 2);
						text3 = "OK";
						break;
					case 228:
						text2 = "UNKNOWN RESULT";
						break;
					case 229:
						text2 = "UNKNOWN RESULT";
						break;
					case 240:
						text2 = "DENIED (INVALID OFFSET)";
						break;
					case 241:
						text2 = "DENIED (SECURITY LEVEL)";
						break;
					default:
						text2 = Util.ByteToHexString(array3, 3);
						break;
					}
				}
				break;
			case 40:
				text = "READ EEPROM";
				if (array2.Length >= 4)
				{
					text = "READ EEPROM | OFFSET: " + Util.ByteToHexString(array3, 0, 2);
					text2 = Util.ByteToHexString(array3, 2);
				}
				break;
			case 41:
				text = "WRITE RAM";
				if (array2.Length >= 5)
				{
					text = "WRITE RAM | OFFSET: " + Util.ByteToHexString(array3, 0, 2);
					switch (array3[3])
					{
					case 229:
						text2 = Util.ByteToHexString(array3, 2);
						text3 = "OK";
						break;
					case 240:
						text2 = "DENIED (INVALID OFFSET)";
						break;
					case 241:
						text2 = "DENIED (SECURITY LEVEL)";
						break;
					default:
						text2 = Util.ByteToHexString(array3, 3);
						break;
					}
				}
				break;
			case 42:
				text = "CONFIGURATION";
				if (array2.Length < 3)
				{
					break;
				}
				switch (array3[0])
				{
				case 1:
					text += " | PART NUMBER 1-2";
					text2 = Util.ByteToHexString(array3, 1);
					PartNumberChars[0] = array3[1];
					break;
				case 2:
					text += " | PART NUMBER 3-4";
					text2 = Util.ByteToHexString(array3, 1);
					PartNumberChars[1] = array3[1];
					break;
				case 3:
					text += " | PART NUMBER 5-6";
					text2 = Util.ByteToHexString(array3, 1);
					PartNumberChars[2] = array3[1];
					break;
				case 4:
					text += " | PART NUMBER 7-8 | CHECKSUM";
					text2 = Util.ByteToHexString(array3, 1);
					PartNumberChars[3] = array3[1];
					break;
				case 6:
					text += " | EMISSION STANDARD";
					text2 = array3[1] switch
					{
						0 => "FEDERAL HIGH ALTITUDE", 
						1 => "TRUCK MODULE", 
						2 => "MEXICAN MODULE", 
						3 => "CA/NY/MA/CT STATE", 
						4 => "FEDERAL/CANADIAN", 
						5 => "BUX/ECE", 
						6 => "GULF STATES", 
						7 => "50 STATE/CANADIAN", 
						8 => "TRANSITORY LOW-EM (NBT)", 
						9 => "LOW EMISSION VEH (NBV)", 
						10 => "CARB OBD2 TRUCK MODULE", 
						11 => "EPA FEDERAL OBD TRUCK M", 
						12 => "HEAVY DUTY TRUCK MODULE", 
						13 => "CANADIAN ONLY", 
						14 => "FEDERAL ONLY", 
						15 => "50 STATE ONLY", 
						16 => "ZERO EMISSION VEH (NBZ)", 
						17 => "ULTRA LOW-EM VEH (NBU)", 
						18 => "JAPAN EMISSIONS (NGJ)", 
						19 => "EURO STAGE 3 OBD (NB3)", 
						20 => "NATIONAL LOW EMS (NLEV)", 
						21 => "EURO STAGE 2 (NB2)", 
						22 => "EURO STAGE 4 (NB4)", 
						23 => "SUPER ULTRA LO-EM (NBS)", 
						24 => "ENHARENTLY LOW-EM (NBI)", 
						25 => "PARTIAL ZERO-EM (NBP)", 
						_ => Util.ByteToHexString(array3, 1), 
					};
					EngineToolsStatusBarTextItems[6] = text2;
					break;
				case 7:
					text += " | CHASSIS TYPE";
					text2 = array3[1] switch
					{
						1 => "MTX RWD HEAVY DUTY", 
						2 => "ATX RWD HEAVY DUTY", 
						3 => "MTX RWD", 
						4 => "ATX RWD", 
						5 => "MTX FWD HEAVY DUTY", 
						6 => "ATX FWD HEAVY DUTY", 
						7 => "MTX FWD", 
						8 => "ATX FWD", 
						9 => "MTX AWD HEAVY DUTY", 
						10 => "ATX AWD HEAVY DUTY", 
						11 => "MTX AWD", 
						12 => "ATX AWD", 
						13 => "4X2 MTX", 
						14 => "4X2 ATX", 
						15 => "4X4 MTX", 
						16 => "4X4 ATX", 
						_ => Util.ByteToHexString(array3, 1), 
					};
					EngineToolsStatusBarTextItems[9] = text2;
					break;
				case 8:
					text += " | ASPIRATION TYPE";
					text2 = array3[1] switch
					{
						1 => "NATURAL", 
						2 => "TURBO I", 
						3 => "TURBO II", 
						4 => "TURBO III", 
						5 => "TURBO IV", 
						6 => "TURBO DIESEL", 
						7 => "TWO-STROKE", 
						_ => Util.ByteToHexString(array3, 1), 
					};
					EngineToolsStatusBarTextItems[7] = text2;
					if (EngineToolsStatusBarTextItems[7] == "NATURAL")
					{
						EngineToolsStatusBarTextItems[7] = "NATURAL ASPIRATION";
					}
					break;
				case 9:
					text += " | INJECTION TYPE";
					text2 = array3[1] switch
					{
						1 => "TBI SINGLE", 
						2 => "TBI DOUBLE", 
						3 => "MPI BANKED", 
						4 => "SFI", 
						5 => "DIRECT", 
						_ => Util.ByteToHexString(array3, 1), 
					};
					EngineToolsStatusBarTextItems[5] = text2;
					if (EngineToolsStatusBarTextItems[5] == "DIRECT")
					{
						EngineToolsStatusBarTextItems[5] = "DIRECT INJ";
					}
					break;
				case 10:
					text += " | FUEL TYPE";
					text2 = array3[1] switch
					{
						1 => "UNLEADED GAS", 
						2 => "DIESEL", 
						3 => "PROPANE", 
						4 => "METHANOL", 
						5 => "LEADED GAS", 
						6 => "SENSORLESS FLEX", 
						7 => "CNG", 
						8 => "LEAD-ACID ELECTRIC", 
						9 => "NIMH ELECTRIC", 
						_ => Util.ByteToHexString(array3, 1), 
					};
					EngineToolsStatusBarTextItems[4] = text2;
					break;
				case 11:
					text += " | MODEL YEAR";
					Year = 1990 + array3[1];
					text2 = Year.ToString("0");
					EngineToolsStatusBarTextItems[0] = text2;
					break;
				case 12:
					text += " | ENGINE DISPLACEMENT AND CYL ORIENT.";
					text2 = array3[1] switch
					{
						1 => "2.2L I4 E-W", 
						2 => "2.5L I4 E-W", 
						3 => "3.0L V6 E-W", 
						4 => "3.3L V6 E-W", 
						5 => "3.9L V6 N-S", 
						6 => "5.2L V8 N-S", 
						7 => "5.9L V8 N-S", 
						8 => "3.8L V6 E-W", 
						9 => "4.0L I6 N-S", 
						10 => "2.0L I4 E-W SOHC", 
						11 => "3.5L V6 N-S", 
						12 => "8.0L V10 N-S", 
						13 => "2.4L I4 E-W", 
						14 => "2.5L I4 N-S", 
						15 => "2.5L V6 N-S", 
						16 => "2.0L I4 E-W DOHC", 
						17 => "2.5L V6 E-W", 
						18 => "5.9L I6 N-S", 
						19 => "3.3L V6 N-S", 
						20 => "2.7L V6 N-S", 
						21 => "3.2L V6 N-S", 
						22 => "1.8L I4 E-W", 
						23 => "3.7L V6 N-S", 
						24 => "4.7L V8 N-S", 
						25 => "1.9L I4 E-W", 
						26 => "3.1L I5 N-S", 
						27 => "1.6L I4 E-W", 
						28 => "2.7L V6 E-W", 
						_ => Util.ByteToHexString(array3, 1), 
					};
					EngineToolsStatusBarTextItems[3] = text2;
					break;
				case 13:
					text += " | COOLING FAN";
					text2 = array3[1] switch
					{
						1 => "NO ELECTRIC FAN", 
						2 => "SINGLE FAN SINGLE SPEED", 
						3 => "SINGLE FAN TWO SPEED", 
						4 => "SINGLE FAN VAR SPEED", 
						5 => "TWO FANS SINGLE SPEED", 
						6 => "TWO FANS TWO SPEED", 
						7 => "TWO FANS VAR SPEED", 
						8 => "THREE FANS SINGLE SPD", 
						9 => "THREE FANS TWO SPEED", 
						10 => "THREE FANS VAR SPEED", 
						11 => "AUX COOLING FAN", 
						12 => "SINGLE FAN VAR HYDR", 
						_ => Util.ByteToHexString(array3, 1), 
					};
					EngineToolsStatusBarTextItems[8] = text2;
					break;
				case 14:
					text += " | ENGINE MANUFACTURER";
					text2 = array3[1] switch
					{
						1 => "CHRYSLER", 
						2 => "JEEP", 
						3 => "MITSUBISHI", 
						4 => "LOTUS", 
						5 => "DITOMASO", 
						6 => "PRV", 
						7 => "CUMMINS", 
						8 => "NORTHRUP/GRUMMAN", 
						9 => "VM MOTORI", 
						10 => "MERCEDES DIESEL", 
						_ => Util.ByteToHexString(array3, 1), 
					};
					EngineToolsStatusBarTextItems[2] = text2;
					break;
				case 15:
					text += " | CONTROLLER HARDWARE TYPE";
					switch (array3[1])
					{
					case 1:
						text2 = "FCC";
						break;
					case 2:
						text2 = "SBEC1";
						break;
					case 3:
						text2 = "SBEC2/JTEC (OBD1)";
						break;
					case 4:
						text2 = "SBEC2A";
						break;
					case 5:
						text2 = "SBEC3";
						break;
					case 6:
						text2 = "JTEC";
						break;
					case 7:
						text2 = "SBEC3A";
						break;
					case 8:
						text2 = "SBEC3+";
						break;
					case 9:
						text2 = "CUMMINS";
						CumminsSelected = true;
						break;
					case 10:
						text2 = "BOSCH";
						break;
					case 11:
						text2 = "NORTHROP EV SCU";
						break;
					case 12:
						text2 = "JTEC+";
						break;
					case 13:
						text2 = "JTEC (TCM ONLY)";
						break;
					case 14:
						text2 = "JTEC+ (TCM ONLY)";
						break;
					case 15:
						text2 = "BOSCH EDC15-V";
						break;
					case 16:
						text2 = "BOSCH EDC15-C5";
						break;
					case 17:
						text2 = "SIEMENS SIM-70";
						break;
					case 18:
						text2 = "SBEC3A+";
						break;
					case 19:
						text2 = "SBEC3B";
						break;
					case 20:
						text2 = "GENERIC JTEC";
						break;
					case 21:
						text2 = "CUMMINS 845";
						CumminsSelected = true;
						break;
					case 22:
						text2 = "CUMMINS 846";
						CumminsSelected = true;
						break;
					case 23:
						text2 = "GENERIC CUMMINS";
						CumminsSelected = true;
						break;
					case 24:
						text2 = "CUMMINS 848";
						CumminsSelected = true;
						break;
					case 25:
						text2 = "EDC16-C2";
						break;
					case 26:
						text2 = "";
						break;
					case 27:
						text2 = "NGC";
						break;
					case 28:
						text2 = "EDC16-C2";
						break;
					case 29:
						text2 = "CUMMINS 2";
						CumminsSelected = true;
						break;
					default:
						text2 = Util.ByteToHexString(array3, 1);
						break;
					}
					ControllerHardwareType = array3[1];
					break;
				case 16:
					text += " | BODY STYLE";
					text2 = array3[1] switch
					{
						1 => "YJ", 
						2 => "XJ", 
						3 => "ZJ/ZG", 
						4 => "FJ", 
						5 => "PL", 
						6 => "JA", 
						7 => "AA/AG/AJ/AP", 
						8 => "AC/AY", 
						9 => "AS/ES", 
						10 => "LH", 
						11 => "NS/GS", 
						12 => "AB", 
						13 => "AN", 
						14 => "BR", 
						15 => "SR", 
						16 => "AN/BR", 
						17 => "AN/AB", 
						18 => "JX", 
						19 => "PR", 
						20 => "TJ", 
						21 => "DN", 
						22 => "WJ/WG", 
						23 => "SJ", 
						24 => "JR", 
						25 => "PT", 
						26 => "RS/RG", 
						27 => "KJ", 
						28 => "DR", 
						29 => "F24S/FJ22", 
						30 => "FJ22", 
						31 => "AA/AJ/AP", 
						_ => Util.ByteToHexString(array3, 1), 
					};
					EngineToolsStatusBarTextItems[1] = text2;
					break;
				case 17:
					text += " | MODULE SOFTWARE PHASE";
					text2 = array3[1].ToString("0");
					break;
				case 18:
					text += " | MODULE SOFTWARE VERSION";
					text2 = array3[1].ToString("0");
					break;
				case 19:
					text += " | MODULE SOFTWARE FAMILY";
					text2 = Util.ByteToHexString(array3, 1);
					break;
				case 20:
					text += " | MODULE SOFTWARE GROUP AND MONTH";
					text2 = (array3[1] & 0xF) switch
					{
						1 => "JANUARY", 
						2 => "FEBRUARY", 
						3 => "MARCH", 
						4 => "APRIL", 
						5 => "MAY", 
						6 => "JUNE", 
						7 => "JULY", 
						8 => "AUGUST", 
						9 => "SEPTEMBER", 
						10 => "OCTOBER", 
						11 => "NOVEMBER", 
						12 => "DECEMBER", 
						_ => Util.ByteToHexString(array3, 1), 
					};
					break;
				case 21:
					text += " | MODULE SOFTWARE DAY";
					text2 = array3[1].ToString("0");
					break;
				case 22:
					text += " | TRANSMISSION TYPE";
					text2 = array3[1] switch
					{
						1 => "ATX ?-SPEED (PTU)", 
						2 => "MTX", 
						3 => "ATX 3-SPEED", 
						4 => "ATX 4-SPEED", 
						_ => Util.ByteToHexString(array3, 1), 
					};
					EngineToolsStatusBarTextItems[10] = text2;
					break;
				case 23:
					text += " | PART NUMBER REVISION 1";
					text2 = Encoding.ASCII.GetString(array3, 1, 1);
					PartNumberChars[4] = array3[1];
					break;
				case 24:
					text += " | PART NUMBER REVISION 2";
					text2 = Encoding.ASCII.GetString(array3, 1, 1);
					PartNumberChars[5] = array3[1];
					break;
				case 25:
					text += " | SOFTWARE REVISION LEVEL";
					text2 = Util.ByteToHexString(array3, 1);
					break;
				case 26:
				case 27:
				case 28:
				case 29:
				case 30:
				case 31:
					text = text + " | HOMOLOGATION ID " + (array3[0] - 25).ToString("0");
					text2 = Util.ByteToHexString(array3, 1);
					if (array3[1] >= 32 && array3[1] <= 126)
					{
						text2 = text2 + " | " + Encoding.ASCII.GetString(array3, 1, 1);
					}
					break;
				default:
					text = text + " | OFFSET: " + Util.ByteToHexString(array3);
					text2 = Util.ByteToHexString(array3, 1);
					break;
				case 5:
					break;
				}
				break;
			case 43:
			{
				text = "GET SECURITY SEED";
				if (array2.Length < 4)
				{
					break;
				}
				if (array3[2] != Util.ChecksumCalculator(array2, 0, array2.Length - 1))
				{
					text2 = "CHECKSUM ERROR";
					break;
				}
				if (array3[0] == 0 && array3[1] == 0)
				{
					text += " | PCM ALREADY UNLOCKED";
					break;
				}
				byte[] securityKey = UnlockAlgorithm.GetSecurityKey(UnlockAlgorithm.Controllers.SBEC, UnlockAlgorithm.SecurityLevels.Level1, array3.Take(2).ToArray());
				if (securityKey != null)
				{
					byte[] data3 = new byte[4]
					{
						44,
						securityKey[0],
						securityKey[1],
						(byte)(44 + securityKey[0] + securityKey[1])
					};
					text = text + " | KEY: " + Util.ByteToHexStringSimple(data3);
					text2 = Util.ByteToHexString(array3, 0, 2);
				}
				break;
			}
			case 44:
				text = "SEND SECURITY KEY";
				if (array2.Length >= 5)
				{
					text2 = array3[3] switch
					{
						0 => "ACCEPTED", 
						1 => "INCORRECT KEY", 
						2 => "CHECKSUM ERROR", 
						3 => "BLOCKED | RESTART PCM", 
						_ => Util.ByteToHexString(array3, 3), 
					};
				}
				break;
			case 45:
				text = "CONSTANTS";
				if (array2.Length < 5)
				{
					break;
				}
				switch (array3[0])
				{
				case 0:
					text += " | MONITORS";
					switch (array3[1])
					{
					case 1:
					{
						text += " | MIN ENG COOL TEMP";
						double num19 = array3[3] - 128;
						double a10 = 1.8 * num19 + 32.0;
						if (Settings.Default.Units == "imperial")
						{
							text2 = Math.Round(a10).ToString("0");
							text3 = "°F";
						}
						else if (Settings.Default.Units == "metric")
						{
							text2 = Math.Round(num19).ToString("0");
							text3 = "°C";
						}
						break;
					}
					case 2:
					{
						text += " | MAX ENG COOL TEMP";
						double num21 = array3[3] - 128;
						double a11 = 1.8 * num21 + 32.0;
						if (Settings.Default.Units == "imperial")
						{
							text2 = Math.Round(a11).ToString("0");
							text3 = "°F";
						}
						else if (Settings.Default.Units == "metric")
						{
							text2 = Math.Round(num21).ToString("0");
							text3 = "°C";
						}
						break;
					}
					case 3:
					{
						text += " | MIN AMB/BATT TEMP";
						double num18 = array3[3] - 128;
						double a9 = 1.8 * num18 + 32.0;
						if (Settings.Default.Units == "imperial")
						{
							text2 = Math.Round(a9).ToString("0");
							text3 = "°F";
						}
						else if (Settings.Default.Units == "metric")
						{
							text2 = Math.Round(num18).ToString("0");
							text3 = "°C";
						}
						break;
					}
					case 4:
					{
						text += " | MAX AMB/BATT TEMP";
						double num22 = array3[3] - 128;
						double a12 = 1.8 * num22 + 32.0;
						if (Settings.Default.Units == "imperial")
						{
							text2 = Math.Round(a12).ToString("0");
							text3 = "°F";
						}
						else if (Settings.Default.Units == "metric")
						{
							text2 = Math.Round(num22).ToString("0");
							text3 = "°C";
						}
						break;
					}
					case 5:
					{
						text += " | MIN TIME FROM START";
						double value45 = (double)(int)array3[3] * 0.0471;
						text2 = Math.Round(value45, 1).ToString("0.0");
						text3 = "MINUTES";
						break;
					}
					case 6:
					{
						text += " | MIN VEH SPEED MPH";
						double num17 = (double)((array3[2] << 8) + array3[3]) * (1.0 / 64.0);
						double value43 = num17 * 1.609344;
						if (Settings.Default.Units == "imperial")
						{
							text2 = Math.Round(num17, 1).ToString("0.0");
							text3 = "MPH";
						}
						else if (Settings.Default.Units == "metric")
						{
							text2 = Math.Round(value43, 1).ToString("0.0");
							text3 = "KM/H";
						}
						break;
					}
					case 7:
					{
						text += " | MIN OPEN THROTTLE TIME";
						double value46 = (double)(int)array3[3] * 3.211;
						text2 = Math.Round(value46, 1).ToString("0.0");
						text3 = "MINUTES";
						break;
					}
					case 8:
						text += " | MAX RPM/SPEED RATIO";
						text2 = ((double)(int)array3[3]).ToString("0");
						break;
					case 9:
					{
						text += " | MIN BARO PRESSURE";
						double num20 = (double)(int)array3[3] * 0.059756;
						double value44 = num20 * 6.894757;
						if (Settings.Default.Units == "imperial")
						{
							text2 = Math.Round(num20, 1).ToString("0.0");
							text3 = "PSI";
						}
						else if (Settings.Default.Units == "metric")
						{
							text2 = Math.Round(value44, 1).ToString("0.0");
							text3 = "KPA";
						}
						break;
					}
					case 10:
					{
						text += " | MAX ECT/AMB TEMP DIFF";
						double num16 = (double)(int)array3[3] * 1.8;
						double a8 = (num16 - 32.0) / 1.8;
						if (Settings.Default.Units == "imperial")
						{
							text2 = Math.Round(num16).ToString("0");
							text3 = "°F";
						}
						else if (Settings.Default.Units == "metric")
						{
							text2 = Math.Round(a8).ToString("0");
							text3 = "°C";
						}
						break;
					}
					case 12:
						text += " | MIN ENGINE RPM";
						break;
					case 13:
						text += " | MAX ENGINE RPM";
						break;
					case 14:
						text += " | MIN MAP VACUUM";
						break;
					case 15:
						text += " | MAX MAP VACUUM";
						break;
					case 16:
						text += " | MIN TPS PERCENT DIFF";
						break;
					case 17:
						text += " | MAX TPS PERCENT DIFF";
						break;
					case 18:
						text += " | MAX VEH SPEED MPH";
						break;
					case 19:
						text += " | MIN CATALYST TEMP";
						break;
					case 20:
						text += " | MAX CATALYST TEMP";
						break;
					case 21:
						text += " | MIN CAT MILEAGE";
						break;
					case 22:
						text += " | MAX CAT MILEAGE";
						break;
					case 23:
						text += " | MIN DELAY TIME";
						break;
					case 24:
						text += " | MAX DELAY TIME";
						break;
					case 25:
						text += " | MIN TIME ABOVE VEH SPEED";
						break;
					case 26:
						text += " | MIN UPSTREAM O2 VOLTS";
						break;
					case 27:
						text += " | MIN DOWNSTREAM O2 VOLTS";
						break;
					case 28:
						text += " | MAX TEMP FOR .02 LEAK DETECT";
						break;
					case 29:
						text += " | MIN FUEL LEVEL";
						break;
					case 30:
						text += " | MAX FUEL LEVEL";
						break;
					case 32:
						text += " | MIN ENGINE RPM";
						break;
					case 33:
						text += " | MAX ENGINE RPM";
						break;
					case 34:
						text += " | MIN MAP VACUUM";
						break;
					case 35:
						text += " | MAX MAP VACUUM";
						break;
					case 36:
						text += " | MIN TPS PERCENT DIFF";
						break;
					case 37:
						text += " | MAX TPS PERCENT DIFF";
						break;
					case 38:
						text += " | MIN STFT PERCENT";
						break;
					case 39:
						text += " | MAX STFT PERCENT";
						break;
					case 40:
						text += " | MIN LTFT PERCENT";
						break;
					case 41:
						text += " | MAX LTFT PERCENT";
						break;
					case 42:
						text += " | MAX VEH SPEED MPH";
						break;
					case 43:
						text += " | MAX IGN OFF TIME";
						break;
					case 44:
						text += " | MIN CHARGING VOLTS";
						break;
					case 45:
						text += " | MAX VOLT FROM TARGET";
						break;
					case 46:
						text += " | MAX RPM FROM TARGET";
						break;
					case 47:
						text += " | MIN MAP VACUUM";
						break;
					case 48:
						text += " | DELTA OPEN/CLOSE THR";
						break;
					case 49:
						text += " | MAX BATTERY VOLTS";
						break;
					case 50:
						text += " | MIN CAT TEMP";
						break;
					case 51:
						text += " | MAX CAT TEMP";
						break;
					case 52:
						text += " | MIN VEH SPEED MPH";
						break;
					case 53:
						text += " | MIN TEST TIME";
						break;
					case 54:
						text += " | MAX TEST TIME";
						break;
					case 55:
						text += " | ADAP MEM TEST CELL ID";
						break;
					case 57:
						text += " | MIN O2 HEATER DUTY CYCLE";
						break;
					case 58:
						text += " | MAX O2 HEATER DUTY CYCLE";
						break;
					case 59:
						text += " | MIN MASS AIR FLOW";
						break;
					case 60:
						text += " | MAX MASS AIR FLOW";
						break;
					case 61:
						text += " | MAX DELTA MAF";
						break;
					case 62:
						text += " | MIN MAF (GPS)";
						break;
					case 63:
						text += " | MAX MAF (GPS)";
						break;
					case 67:
						text += " | MAX CANISTER LOAD";
						break;
					default:
						text = text + " | ITEM: " + Util.ByteToHexString(array3, 1);
						text2 = Util.ByteToHexString(array3, 2, 2);
						break;
					}
					break;
				case 1:
				{
					text += " | EGR MONITOR";
					byte b4 = array3[1];
					text = text + " | ITEM: " + Util.ByteToHexString(array3, 1);
					text2 = Util.ByteToHexString(array3, 2, 2);
					break;
				}
				case 2:
				{
					text += " | O2 HEATER";
					byte b4 = array3[1];
					text = text + " | ITEM: " + Util.ByteToHexString(array3, 1);
					text2 = Util.ByteToHexString(array3, 2, 2);
					break;
				}
				case 3:
				{
					text += " | O2 MONITOR";
					byte b4 = array3[1];
					text = text + " | ITEM: " + Util.ByteToHexString(array3, 1);
					text2 = Util.ByteToHexString(array3, 2, 2);
					break;
				}
				case 4:
				{
					text += " | MISFIRE MONITOR";
					byte b4 = array3[1];
					text = text + " | ITEM: " + Util.ByteToHexString(array3, 1);
					text2 = Util.ByteToHexString(array3, 2, 2);
					break;
				}
				case 5:
				{
					text += " | ADAPTIVE MEMORY";
					byte b4 = array3[1];
					text = text + " | ITEM: " + Util.ByteToHexString(array3, 1);
					text2 = Util.ByteToHexString(array3, 2, 2);
					break;
				}
				case 6:
				{
					text += " | PURGE MONITOR";
					byte b4 = array3[1];
					text = text + " | ITEM: " + Util.ByteToHexString(array3, 1);
					text2 = Util.ByteToHexString(array3, 2, 2);
					break;
				}
				case 7:
				{
					text += " | LDP MONITOR";
					byte b4 = array3[1];
					text = text + " | ITEM: " + Util.ByteToHexString(array3, 1);
					text2 = Util.ByteToHexString(array3, 2, 2);
					break;
				}
				case 9:
				{
					text += " | CATALYST MONITOR";
					byte b4 = array3[1];
					text = text + " | ITEM: " + Util.ByteToHexString(array3, 1);
					text2 = Util.ByteToHexString(array3, 2, 2);
					break;
				}
				default:
					text = text + " | PAGE: " + Util.ByteToHexString(array3) + " | ITEM: " + Util.ByteToHexString(array3, 1);
					text2 = Util.ByteToHexString(array3, 2, 2);
					break;
				}
				break;
			case 46:
			case 51:
				text = "ONE-TRIP FAULT CODE LIST";
				if (array2.Length >= 2)
				{
					FaultCode1TList.Clear();
					FaultCode1TList.AddRange(array6);
					FaultCode1TList.Remove(253);
					FaultCode1TList.Remove(254);
					if (FaultCode1TList.Count == 0)
					{
						FaultCode1TList.Clear();
						text2 = "NO FAULT CODE";
						FaultCodes1TSaved = false;
					}
					else
					{
						text2 = Util.ByteToHexStringSimple(FaultCode1TList.ToArray());
						FaultCodes1TSaved = false;
					}
				}
				break;
			case 53:
			{
				text = "GET SECURITY SEED";
				if (array2.Length < 5)
				{
					break;
				}
				switch (array3[0])
				{
				case 1:
					text += " #1";
					break;
				case 2:
					text += " #2";
					break;
				}
				if (array3[3] != Util.ChecksumCalculator(array2, 0, array2.Length - 1))
				{
					text2 = "CHECKSUM ERROR";
					break;
				}
				if (array3[1] == 0 && array3[2] == 0)
				{
					text += " | PCM ALREADY UNLOCKED";
					break;
				}
				byte[] array7 = null;
				switch (array3[0])
				{
				case 1:
					array7 = UnlockAlgorithm.GetSecurityKey(UnlockAlgorithm.Controllers.SBEC, UnlockAlgorithm.SecurityLevels.Level1, array3.Skip(1).Take(2).ToArray());
					break;
				case 2:
					array7 = UnlockAlgorithm.GetSecurityKey(UnlockAlgorithm.Controllers.SBEC, UnlockAlgorithm.SecurityLevels.Level2, array3.Skip(1).Take(2).ToArray());
					break;
				}
				if (array7 != null)
				{
					byte[] data2 = new byte[4]
					{
						44,
						array7[0],
						array7[1],
						(byte)(44 + array7[0] + array7[1])
					};
					text = text + " | KEY: " + Util.ByteToHexStringSimple(data2);
					text2 = Util.ByteToHexString(array3, 1, 2);
				}
				break;
			}
			case 254:
				text = "SELECT LOW-SPEED MODE";
				break;
			case byte.MaxValue:
				text = "PCM WAKE UP";
				break;
			default:
				text = string.Empty;
				break;
			case 54:
				break;
			}
		}
		else if (speed == "62500 baud" || speed == "125000 baud")
		{
			List<byte> list3 = new List<byte>();
			List<byte> list4 = new List<byte>();
			List<byte> list5 = new List<byte>();
			List<byte> list6 = new List<byte>();
			List<byte> list7 = new List<byte>();
			List<byte> list8 = new List<byte>();
			List<byte> list9 = new List<byte>();
			ushort num23 = (ushort)(array3.Length / 2);
			switch (b)
			{
			case 0:
				text = "PCM WAKE UP";
				break;
			case 6:
				text = "SET BOOTSTRAP BAUDRATE TO 62500 BAUD";
				text2 = "OK";
				break;
			case 17:
				text = "UPLOAD BOOTWORKER";
				if (array2.Length >= 3)
				{
					ushort num67 = (ushort)((array3[0] << 8) + array3[1]);
					ushort num68 = (ushort)(array3.Length - 3);
					text = "UPLOAD BOOTWORKER | SIZE: " + num67 + " BYTES";
					text2 = Util.ByteToHexString(array3, 2, array3.Length - 3);
					text3 = ((num68 != num67 || array3[^1] != 20) ? "ERROR" : "OK");
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
					byte b7 = (byte)(array2[0] + array2[1] + array2[2] + array2[3]);
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
					list5.AddRange(array3.Take(3));
					list6.AddRange(array3.Skip(3).Take(2));
					list7.AddRange(array3.Skip(5));
					ushort num63 = (ushort)((array3[3] << 8) + array3[4]);
					ushort num64 = (ushort)(array3.Length - 5);
					text = "WRITE FLASH BLOCK | OFFSET: " + Util.ByteToHexStringSimple(list5.ToArray()) + " | SIZE: " + Util.ByteToHexStringSimple(list6.ToArray());
					if (num64 == num63)
					{
						text2 = Util.ByteToHexStringSimple(list7.ToArray());
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
					list5.AddRange(array3.Take(3));
					list6.AddRange(array3.Skip(3).Take(2));
					list7.AddRange(array3.Skip(5));
					text = "READ FLASH BLOCK | OFFSET: " + Util.ByteToHexStringSimple(list5.ToArray()) + " | SIZE: " + Util.ByteToHexStringSimple(list6.ToArray());
					ushort num63 = (ushort)((array3[3] << 8) + array3[4]);
					ushort num64 = (ushort)(array3.Length - 5);
					if (num64 == num63)
					{
						text2 = Util.ByteToHexStringSimple(list7.ToArray());
						text3 = "OK";
					}
					else
					{
						byte b3 = array2[^1];
						text2 = ((b3 != 128) ? "UNKNOWN ERROR" : "INVALID BLOCK SIZE");
					}
				}
				break;
			case 55:
				text = "WRITE EEPROM BLOCK";
				if (array2.Length >= 6)
				{
					list5.AddRange(array3.Take(2));
					list6.AddRange(array3.Skip(2).Take(2));
					list7.AddRange(array3.Skip(4));
					text = "WRITE EEPROM BLOCK | OFFSET: " + Util.ByteToHexStringSimple(list5.ToArray()) + " | SIZE: " + Util.ByteToHexStringSimple(list6.ToArray());
					ushort num63 = (ushort)((array3[2] << 8) + array3[3]);
					ushort num64 = (ushort)(array3.Length - 4);
					if (num64 == num63)
					{
						text2 = Util.ByteToHexStringSimple(list7.ToArray());
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
					list5.AddRange(array3.Take(2));
					list6.AddRange(array3.Skip(2).Take(2));
					list7.AddRange(array3.Skip(4));
					text = "READ EEPROM BLOCK | OFFSET: " + Util.ByteToHexStringSimple(list5.ToArray()) + " | SIZE: " + Util.ByteToHexStringSimple(list6.ToArray());
					ushort num63 = (ushort)((array3[2] << 8) + array3[3]);
					ushort num64 = (ushort)(array3.Length - 4);
					if (num64 == num63)
					{
						text2 = Util.ByteToHexStringSimple(list7.ToArray());
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
				list5.AddRange(array3.Take(2));
				text = "START BOOTLOADER | OFFSET: " + Util.ByteToHexStringSimple(list5.ToArray());
				text2 = ((array3[2] != 34) ? "ERROR" : "OK");
				break;
			case 76:
				text = "UPLOAD BOOTLOADER";
				if (array2.Length >= 6)
				{
					list8.AddRange(array3.Take(2));
					list9.AddRange(array3.Skip(2).Take(2));
					text = "UPLOAD BOOTLOADER | START: " + Util.ByteToHexStringSimple(list8.ToArray()) + " | END: " + Util.ByteToHexStringSimple(list9.ToArray());
					text2 = Util.ByteToHexString(array3, 4, array3.Length - 4);
					ushort num47 = (ushort)((array3[0] << 8) + array3[1]);
					ushort num48 = (ushort)((array3[2] << 8) + array3[3]);
					text3 = ((num48 - num47 + 1 != array3.Length - 4) ? "ERROR" : "OK");
				}
				break;
			case 219:
				text = string.Empty;
				if (array2.Length >= 5)
				{
					text = ((array3[0] != 47 || array3[1] != 216 || array3[2] != 62 || array3[3] != 35) ? "PING" : "BOOTSTRAP MODE NOT PROTECTED");
				}
				break;
			case 240:
				text = "F0 RAM TABLE SELECTED";
				if (array2.Length >= 3)
				{
					byte b3 = array3[0];
					for (int num65 = 0; num65 < num23; num65++)
					{
						list3.Add(array3[num65 * 2]);
						list4.Add(array3[num65 * 2 + 1]);
					}
					text = "F0 RAM TABLE | OFFSET: " + Util.ByteToHexStringSimple(list3.ToArray());
					text2 = Util.ByteToHexStringSimple(list4.ToArray());
				}
				break;
			case 241:
				text = "F1 RAM TABLE SELECTED";
				if (array2.Length >= 3)
				{
					byte b3 = array3[0];
					for (int num61 = 0; num61 < num23; num61++)
					{
						list3.Add(array3[num61 * 2]);
						list4.Add(array3[num61 * 2 + 1]);
					}
					text = "F1 RAM TABLE | OFFSET: " + Util.ByteToHexStringSimple(list3.ToArray());
					text2 = Util.ByteToHexStringSimple(list4.ToArray());
				}
				break;
			case 242:
				text = "F2 RAM TABLE SELECTED";
				if (array2.Length >= 3)
				{
					byte b3 = array3[0];
					for (int num132 = 0; num132 < num23; num132++)
					{
						list3.Add(array3[num132 * 2]);
						list4.Add(array3[num132 * 2 + 1]);
					}
					text = "F2 RAM TABLE | OFFSET: " + Util.ByteToHexStringSimple(list3.ToArray());
					text2 = Util.ByteToHexStringSimple(list4.ToArray());
				}
				break;
			case 243:
				text = "F3 RAM TABLE SELECTED";
				if (array2.Length >= 3)
				{
					byte b3 = array3[0];
					for (int num108 = 0; num108 < num23; num108++)
					{
						list3.Add(array3[num108 * 2]);
						list4.Add(array3[num108 * 2 + 1]);
					}
					text = "F3 RAM TABLE | OFFSET: " + Util.ByteToHexStringSimple(list3.ToArray());
					text2 = Util.ByteToHexStringSimple(list4.ToArray());
				}
				break;
			case 244:
				text = "F4 RAM TABLE SELECTED";
				if (array2.Length < 3)
				{
					break;
				}
				switch (array3[0])
				{
				case 1:
				{
					text = "DTC 1: ";
					if (array3[1] == 0)
					{
						text2 = "EMPTY SLOT";
						break;
					}
					int num99 = SBEC3EngineDTC.Rows.IndexOf(SBEC3EngineDTC.Rows.Find(array3[1]));
					text = ((num99 <= -1) ? (text + "UNRECOGNIZED DTC") : (text + SBEC3EngineDTC.Rows[num99]["description"]));
					break;
				}
				case 2:
				{
					text = "DTC 8: ";
					if (array3[1] == 0)
					{
						text2 = "EMPTY SLOT";
						break;
					}
					int num104 = SBEC3EngineDTC.Rows.IndexOf(SBEC3EngineDTC.Rows.Find(array3[1]));
					text = ((num104 <= -1) ? (text + "UNRECOGNIZED DTC") : (text + SBEC3EngineDTC.Rows[num104]["description"]));
					break;
				}
				case 3:
					text = "KEY-ON CYCLES ERROR 1";
					text2 = array3[1].ToString("0");
					break;
				case 4:
					text = "KEY-ON CYCLES ERROR 2";
					text2 = array3[1].ToString("0");
					break;
				case 5:
					text = "KEY-ON CYCLES ERROR 3";
					text2 = array3[1].ToString("0");
					break;
				case 6:
					text = "DTC COUNTER 1";
					text2 = array3[1].ToString("0");
					break;
				case 7:
					text = "DTC COUNTER 2";
					text2 = array3[1].ToString("0");
					break;
				case 8:
					text = "DTC COUNTER 3";
					text2 = array3[1].ToString("0");
					break;
				case 9:
					text = "DTC COUNTER 4";
					text2 = array3[1].ToString("0");
					break;
				case 10:
					text = "ENGINE SPEED";
					if (array2.Length >= 5)
					{
						if (array3[2] != array3[0] + 1)
						{
							text += " | ERROR: REQUEST F4 0A 0B";
							break;
						}
						double value166 = (double)((array3[1] << 8) + array3[3]) * 0.125;
						text2 = Math.Round(value166, 3).ToString("0.000");
						text3 = "RPM";
					}
					break;
				case 11:
					text = "ENGINE SPEED | ERROR: REQUEST F4 0A 0B";
					break;
				case 12:
				{
					text = "VEHICLE SPEED";
					if (array2.Length < 5)
					{
						break;
					}
					if (array3[2] != array3[0] + 1)
					{
						text += " | ERROR: REQUEST F4 0C 0D";
						break;
					}
					double num101 = (double)((array3[1] << 8) + array3[3]) * (1.0 / 64.0);
					double value181 = num101 * 1.609344;
					if (Settings.Default.Units == "imperial")
					{
						text2 = Math.Round(num101, 3).ToString("0.000");
						text3 = "MPH";
					}
					else if (Settings.Default.Units == "metric")
					{
						text2 = Math.Round(value181, 3).ToString("0.000");
						text3 = "KM/H";
					}
					break;
				}
				case 13:
					text = "VEHICLE SPEED | ERROR: REQUEST F4 0C 0D";
					break;
				case 14:
				{
					text = "CRUISE | BUTTON PRESSED";
					List<string> list16 = new List<string>();
					if (Util.IsBitSet(array3[1], 7))
					{
						list16.Add("BRAKE");
					}
					if (Util.IsBitSet(array3[1], 6))
					{
						list16.Add("-6-");
					}
					if (Util.IsBitSet(array3[1], 5))
					{
						list16.Add("-5-");
					}
					if (Util.IsBitSet(array3[1], 4))
					{
						list16.Add("ON/OFF");
					}
					if (Util.IsBitSet(array3[1], 3))
					{
						list16.Add("ACC/RES");
					}
					if (Util.IsBitClear(array3[1], 2))
					{
						list16.Add("SET");
					}
					if (Util.IsBitSet(array3[1], 1))
					{
						list16.Add("COAST");
					}
					if (Util.IsBitSet(array3[1], 0))
					{
						list16.Add("CANCEL");
					}
					if (list16.Count == 0)
					{
						break;
					}
					foreach (string item3 in list16)
					{
						text2 = text2 + item3 + " | ";
					}
					if (text2.Length > 2)
					{
						text2 = text2.Remove(text2.Length - 3);
					}
					break;
				}
				case 15:
				{
					text = "BATTERY VOLTAGE";
					double value165 = (double)(int)array3[1] * 0.0625;
					text2 = Math.Round(value165, 3).ToString("0.000");
					text3 = "V";
					break;
				}
				case 16:
				{
					text = "AMBIENT TEMPERATURE SENSOR VOLTAGE";
					double value151 = (double)(int)array3[1] * 0.0196;
					text2 = Math.Round(value151, 3).ToString("0.000");
					text3 = "V";
					break;
				}
				case 17:
				{
					text = "AMBIENT TEMPERATURE";
					double num74 = array3[1] - 128;
					double a13 = 1.8 * num74 + 32.0;
					if (Settings.Default.Units == "imperial")
					{
						text2 = Math.Round(a13).ToString("0");
						text3 = "°F";
					}
					else if (Settings.Default.Units == "metric")
					{
						text2 = num74.ToString("0");
						text3 = "°C";
					}
					break;
				}
				case 18:
				{
					text = "THROTTLE POSITION SENSOR (TPS) VOLTAGE";
					double value132 = (double)(int)array3[1] * 0.0196;
					text2 = Math.Round(value132, 3).ToString("0.000");
					text3 = "V";
					break;
				}
				case 19:
				{
					text = "MINIMUM TPS VOLTAGE";
					double value130 = (double)(int)array3[1] * 0.0196;
					text2 = Math.Round(value130, 3).ToString("0.000");
					text3 = "V";
					break;
				}
				case 20:
				{
					text = "CALCULATED TPS VOLTAGE";
					double value188 = (double)(int)array3[1] * 0.0196;
					text2 = Math.Round(value188, 3).ToString("0.000");
					text3 = "V";
					break;
				}
				case 21:
				{
					text = "ENGINE COOLANT TEMPERATURE SENSOR VOLTAGE";
					double value134 = (double)(int)array3[1] * 0.0196;
					text2 = Math.Round(value134, 3).ToString("0.000");
					text3 = "V";
					break;
				}
				case 22:
				{
					text = "ENGINE COOLANT TEMPERATURE";
					double num103 = array3[1] - 128;
					double a20 = 1.8 * num103 + 32.0;
					if (Settings.Default.Units == "imperial")
					{
						text2 = Math.Round(a20).ToString("0");
						text3 = "°F";
					}
					else if (Settings.Default.Units == "metric")
					{
						text2 = num103.ToString("0");
						text3 = "°C";
					}
					break;
				}
				case 23:
				{
					text = "INTAKE MANIFOLD ABSOLUTE PRESSURE SENSOR VOLTAGE";
					double value182 = (double)(int)array3[1] * 0.0196;
					text2 = Math.Round(value182, 3).ToString("0.000");
					text3 = "V";
					break;
				}
				case 24:
				{
					text = "INTAKE MANIFOLD ABSOLUTE PRESSURE";
					double num96 = (double)(int)array3[1] * 0.059756;
					double value180 = num96 * 6.894757;
					if (Settings.Default.Units == "imperial")
					{
						text2 = Math.Round(num96, 1).ToString("0.0");
						text3 = "PSI";
					}
					else if (Settings.Default.Units == "metric")
					{
						text2 = Math.Round(value180, 1).ToString("0.0");
						text3 = "KPA";
					}
					break;
				}
				case 25:
				{
					text = "BAROMETRIC PRESSURE";
					double num93 = (double)(int)array3[1] * 0.059756;
					double value175 = num93 * 6.894757;
					if (Settings.Default.Units == "imperial")
					{
						text2 = Math.Round(num93, 1).ToString("0.0");
						text3 = "PSI";
					}
					else if (Settings.Default.Units == "metric")
					{
						text2 = Math.Round(value175, 1).ToString("0.0");
						text3 = "KPA";
					}
					break;
				}
				case 26:
				{
					text = "MAP VACUUM";
					double num86 = (double)(int)array3[1] * 0.059756;
					double value162 = num86 * 6.894757;
					if (Settings.Default.Units == "imperial")
					{
						text2 = Math.Round(num86, 1).ToString("0.0");
						text3 = "PSI";
					}
					else if (Settings.Default.Units == "metric")
					{
						text2 = Math.Round(value162, 1).ToString("0.0");
						text3 = "KPA";
					}
					break;
				}
				case 27:
				{
					text = "UPSTREAM O2 1/1 SENSOR VOLTAGE";
					double value161 = (double)(int)array3[1] * 0.0196;
					text2 = Math.Round(value161, 3).ToString("0.000");
					text3 = "V";
					break;
				}
				case 28:
				{
					text = "UPSTREAM O2 2/1 SENSOR VOLTAGE";
					double value160 = (double)(int)array3[1] * 0.0196;
					text2 = Math.Round(value160, 3).ToString("0.000");
					text3 = "V";
					break;
				}
				case 29:
				{
					text = "INTAKE AIR TEMPERATURE SENSOR VOLTAGE";
					double value152 = (double)(int)array3[1] * 0.0196;
					text2 = Math.Round(value152, 3).ToString("0.000");
					text3 = "V";
					break;
				}
				case 30:
				{
					text = "INTAKE AIR TEMPERATURE";
					double num78 = array3[1] - 64;
					double a14 = 1.8 * num78 + 32.0;
					if (Settings.Default.Units == "imperial")
					{
						text2 = Math.Round(a14).ToString("0");
						text3 = "°F";
					}
					else if (Settings.Default.Units == "metric")
					{
						text2 = num78.ToString("0");
						text3 = "°C";
					}
					break;
				}
				case 31:
				{
					text = "KNOCK SENSOR 1 VOLTAGE";
					double value143 = (double)(int)array3[1] * 0.0196;
					text2 = Math.Round(value143, 3).ToString("0.000");
					text3 = "V";
					break;
				}
				case 32:
				{
					text = "KNOCK SENSOR 2 VOLTAGE";
					double value144 = (double)(int)array3[1] * 0.0196;
					text2 = Math.Round(value144, 3).ToString("0.000");
					text3 = "V";
					break;
				}
				case 33:
				{
					text = "CRUISE | SWITCH VOLTAGE";
					double value140 = (double)(int)array3[1] * 0.0196;
					text2 = Math.Round(value140, 3).ToString("0.000");
					text3 = "V";
					break;
				}
				case 34:
				{
					text = "BATTERY TEMPERATURE SENSOR VOLTAGE";
					double value141 = (double)(int)array3[1] * 0.0196;
					text2 = Math.Round(value141, 3).ToString("0.000");
					text3 = "V";
					break;
				}
				case 35:
				{
					text = "FLEX FUEL SENSOR VOLTAGE";
					double value139 = (double)(int)array3[1] * 0.0196;
					text2 = Math.Round(value139, 3).ToString("0.000");
					text3 = "V";
					break;
				}
				case 36:
				{
					text = "FLEX FUEL ETHANOL PERCENT";
					double value136 = (double)(int)array3[1] * 0.5;
					text2 = Math.Round(value136, 1).ToString("0.0");
					text3 = "PERCENT";
					break;
				}
				case 37:
				{
					text = "A/C HIGH-SIDE PRESSURE SENSOR VOLTAGE";
					double value135 = (double)(int)array3[1] * 0.0196;
					text2 = Math.Round(value135, 3).ToString("0.000");
					text3 = "V";
					break;
				}
				case 38:
				{
					text = "A/C HIGH-SIDE PRESSURE";
					double num75 = (double)(int)array3[1] * 1.961;
					double value133 = num75 * 6.894757;
					if (Settings.Default.Units == "imperial")
					{
						text2 = Math.Round(num75, 1).ToString("0.0");
						text3 = "PSI";
					}
					else if (Settings.Default.Units == "metric")
					{
						text2 = Math.Round(value133, 1).ToString("0.0");
						text3 = "KPA";
					}
					break;
				}
				case 39:
					text = "INJECTOR PULSE WIDTH 1";
					if (array2.Length >= 5)
					{
						if (array3[2] != array3[0] + 1)
						{
							text += " | ERROR: REQUEST F4 27 28";
							break;
						}
						double value187 = (double)((array3[1] << 8) + array3[3]) * (1.0 / 256.0);
						text2 = Math.Round(value187, 3).ToString("0.000");
						text3 = "MS";
					}
					break;
				case 40:
					text = "INJECTOR PULSE WIDTH 1 | ERROR: REQUEST F4 27 28";
					break;
				case 41:
					text = "INJECTOR PULSE WIDTH 2";
					if (array2.Length >= 5)
					{
						if (array3[2] != array3[0] + 1)
						{
							text += " | ERROR: REQUEST F4 29 2A";
							break;
						}
						double value184 = (double)((array3[1] << 8) + array3[3]) * (1.0 / 256.0);
						text2 = Math.Round(value184, 3).ToString("0.000");
						text3 = "MS";
					}
					break;
				case 42:
					text = "INJECTOR PULSE WIDTH 2 | ERROR: REQUEST F4 29 2A";
					break;
				case 43:
				{
					text = "LONG TERM FUEL TRIM 1";
					double num102 = (double)(int)array3[1] * 0.196;
					if (array3[1] >= 128)
					{
						num102 -= 50.0;
					}
					text2 = Math.Round(num102, 3).ToString("0.000");
					text3 = "PERCENT";
					break;
				}
				case 44:
				{
					text = "LONG TERM FUEL TRIM 2";
					double num97 = (double)(int)array3[1] * 0.196;
					if (array3[1] >= 128)
					{
						num97 -= 50.0;
					}
					text2 = Math.Round(num97, 3).ToString("0.000");
					text3 = "PERCENT";
					break;
				}
				case 45:
				{
					text = "ENGINE COOLANT TEMPERATURE 2";
					double num94 = array3[1] - 128;
					double a19 = 1.8 * num94 + 32.0;
					if (Settings.Default.Units == "imperial")
					{
						text2 = Math.Round(a19).ToString("0");
						text3 = "°F";
					}
					else if (Settings.Default.Units == "metric")
					{
						text2 = num94.ToString("0");
						text3 = "°C";
					}
					break;
				}
				case 46:
				{
					text = "ENGINE COOLANT TEMPERATURE 3";
					double num92 = array3[1] - 128;
					double a18 = 1.8 * num92 + 32.0;
					if (Settings.Default.Units == "imperial")
					{
						text2 = Math.Round(a18).ToString("0");
						text3 = "°F";
					}
					else if (Settings.Default.Units == "metric")
					{
						text2 = num92.ToString("0");
						text3 = "°C";
					}
					break;
				}
				case 47:
				{
					text = "SPARK ADVANCE";
					double value173 = (double)(int)array3[1] * 0.5;
					text2 = Math.Round(value173, 1).ToString("0.0");
					text3 = "DEG";
					break;
				}
				case 48:
				{
					text = "TOTAL KNOCK RETARD";
					double value172 = (double)(int)array3[1] * 0.5;
					text2 = Math.Round(value172, 1).ToString("0.0");
					text3 = "DEG";
					break;
				}
				case 49:
				{
					text = "CYLINDER 1 RETARD";
					double value171 = (double)(int)array3[1] * 0.5;
					text2 = Math.Round(value171, 1).ToString("0.0");
					text3 = "DEG";
					break;
				}
				case 50:
				{
					text = "CYLINDER 2 RETARD";
					double value169 = (double)(int)array3[1] * 0.5;
					text2 = Math.Round(value169, 1).ToString("0.0");
					text3 = "DEG";
					break;
				}
				case 51:
				{
					text = "CYLINDER 3 RETARD";
					double value168 = (double)(int)array3[1] * 0.5;
					text2 = Math.Round(value168, 1).ToString("0.0");
					text3 = "DEG";
					break;
				}
				case 52:
				{
					text = "CYLINDER 4 RETARD";
					double value167 = (double)(int)array3[1] * 0.5;
					text2 = Math.Round(value167, 1).ToString("0.0");
					text3 = "DEG";
					break;
				}
				case 53:
					text = "TARGET IDLE SPEED";
					if (array2.Length >= 5)
					{
						if (array3[2] != array3[0] + 1)
						{
							text += " | ERROR: REQUEST F4 35 36";
							break;
						}
						double value164 = (double)((array3[1] << 8) + array3[3]) * 0.125;
						text2 = Math.Round(value164, 3).ToString("0.000");
						text3 = "RPM";
					}
					break;
				case 54:
					text = "TARGET IDLE SPEED | ERROR: REQUEST F4 35 36";
					break;
				case 55:
					text = "TARGET IDLE AIR CONTROL MOTOR STEPS";
					text2 = array3[1].ToString();
					break;
				case 58:
				{
					text = "CHARGING VOLTAGE";
					double value163 = (double)(int)array3[1] * 0.0625;
					text2 = Math.Round(value163, 3).ToString("0.000");
					text3 = "V";
					break;
				}
				case 59:
				{
					text = "CRUISE | SET SPEED";
					double num82 = (double)(int)array3[1] * 0.5;
					double value155 = num82 * 1.609344;
					if (Settings.Default.Units == "imperial")
					{
						text2 = Math.Round(num82, 1).ToString("0.0");
						text3 = "MPH";
					}
					else if (Settings.Default.Units == "metric")
					{
						text2 = Math.Round(value155, 1).ToString("0.0");
						text3 = "KM/H";
					}
					break;
				}
				case 60:
					text = "BIT STATE 5";
					if (array2.Length >= 5)
					{
						if (array3[2] != array3[0] + 1)
						{
							text += " | ERROR: REQUEST F4 3C 3D";
						}
						else
						{
							text2 = Convert.ToString(array3[1], 2).PadLeft(8, '0') + " " + Convert.ToString(array3[3], 2).PadLeft(8, '0');
						}
					}
					break;
				case 61:
					text = "BIT STATE 5 | ERROR: REQUEST F4 3C 3D";
					break;
				case 62:
					text = "IDLE AIR CONTROL MOTOR STEPS";
					text2 = array3[1].ToString();
					break;
				case 63:
				case 237:
				{
					string text8 = (array3[1] & 0xF0) switch
					{
						0 => "ON/OFF SW", 
						16 => "SPEED SEN", 
						32 => "RPM LIMIT", 
						48 => "BRAKE SW", 
						64 => "P/N SW", 
						80 => "RPM/SPEED", 
						96 => "CLUTCH", 
						112 => "DTC PRESENT", 
						128 => "KEY OFF", 
						144 => "ACTIVE", 
						160 => "CLUTCH UP", 
						176 => "N/A", 
						192 => "SW DTC", 
						208 => "CANCEL", 
						224 => "TPS LIMP-IN", 
						240 => "12V DTC", 
						_ => "N/A", 
					};
					string text9 = (array3[1] & 0xF) switch
					{
						0 => "ON/OFF SW", 
						1 => "SPEED SEN", 
						2 => "RPM LIMIT", 
						3 => "BRAKE SW", 
						4 => "P/N SW", 
						5 => "RPM/SPEED", 
						6 => "CLUTCH", 
						7 => "DTC PRESENT", 
						8 => "ALLOWED", 
						9 => "ACTIVE", 
						10 => "CLUTCH UP", 
						11 => "N/A", 
						12 => "SW DTC", 
						13 => "CANCEL", 
						14 => "TPS LIMP-IN", 
						15 => "12V DTC", 
						_ => "N/A", 
					};
					if ((array3[1] & 0xF) == 8)
					{
						text = "CRUISE | LAST CUTOUT: " + text8 + " | STATE: " + text9;
						text2 = "STOPPED";
					}
					else if ((array3[1] & 0xF) == 9)
					{
						text = "CRUISE | LAST CUTOUT: " + text8 + " | STATE: " + text9;
						text2 = "ENGAGED";
					}
					else
					{
						text = "CRUISE | LAST CUTOUT: " + text8 + " | DENIED: " + text9;
						text2 = "STOPPED";
					}
					break;
				}
				case 64:
					text = "VEHICLE THEFT ALARM STATUS";
					text2 = ((!Util.IsBitSet(array3[1], 5)) ? "FUEL ON" : "KILL FUEL");
					break;
				case 65:
					text = ((!Util.IsBitSet(array3[1], 5)) ? "CKP: LOST | " : "CKP: PRESENT | ");
					text = ((!Util.IsBitSet(array3[1], 6)) ? (text + "CMP: LOST | ") : (text + "CMP: PRESENT | "));
					text = ((!Util.IsBitSet(array3[1], 4)) ? (text + "CKP/CMP: OUT-OF-SYNC") : (text + "CKP/CMP: IN-SYNC"));
					text2 = ((!Util.IsBitSet(array3[1], 0)) ? "HISTORY: OUT-OF-SYNC" : "HISTORY: IN-SYNC");
					break;
				case 66:
					text = "FUEL SYSTEM STATUS 1";
					if (Util.IsBitSet(array3[1], 0))
					{
						text2 = "OPEN LOOP";
					}
					if (Util.IsBitSet(array3[1], 1))
					{
						text2 = "CLOSED LOOP";
					}
					if (Util.IsBitSet(array3[1], 2))
					{
						text2 = "OPEN LOOP / DRIVE";
					}
					if (Util.IsBitSet(array3[1], 3))
					{
						text2 = "OPEN LOOP / DTC";
					}
					if (Util.IsBitSet(array3[1], 4))
					{
						text2 = "CLOSED LOOP / DTC";
					}
					break;
				case 67:
					text = "CURRENT ADAPTIVE CELL ID";
					text2 = Util.ByteToHexString(array3, 1);
					break;
				case 68:
				{
					text = "SHORT TERM FUEL TRIM 1";
					double num98 = (double)(int)array3[1] * 0.196;
					if (array3[1] >= 128)
					{
						num98 -= 50.0;
					}
					text2 = Math.Round(num98, 3).ToString("0.000");
					text3 = "PERCENT";
					break;
				}
				case 69:
				{
					text = "SHORT TERM FUEL TRIM 2";
					double num95 = (double)(int)array3[1] * 0.196;
					if (array3[1] >= 128)
					{
						num95 -= 50.0;
					}
					text2 = Math.Round(num95, 3).ToString("0.000");
					text3 = "PERCENT";
					break;
				}
				case 70:
					text = "EMISSION SETTINGS 1";
					text2 = Convert.ToString(array3[1], 2).PadLeft(8, '0');
					break;
				case 71:
					text = "EMISSION SETTINGS 2";
					text2 = Convert.ToString(array3[1], 2).PadLeft(8, '0');
					break;
				case 72:
				{
					text = "DOWNSTREAM O2 1/2 SENSOR VOLTAGE";
					double value179 = (double)(int)array3[1] * 0.0196;
					text2 = Math.Round(value179, 3).ToString("0.000");
					text3 = "V";
					break;
				}
				case 73:
				{
					text = "DOWNSTREAM O2 2/2 SENSOR VOLTAGE";
					double value178 = (double)(int)array3[1] * 0.0196;
					text2 = Math.Round(value178, 3).ToString("0.000");
					text3 = "V";
					break;
				}
				case 74:
				{
					text = "CLOSED LOOP TIMER";
					double value177 = (double)(int)array3[1] * 0.0535;
					text2 = Math.Round(value177, 3).ToString("0.000");
					text3 = "MINUTES";
					break;
				}
				case 75:
					text = "TIME FROM START/RUN";
					if (array2.Length >= 5)
					{
						if (array3[2] != array3[0] + 1)
						{
							text += " | ERROR: REQUEST F4 4B 4C";
							break;
						}
						double value176 = (double)((array3[1] << 8) + array3[3]) * 0.000208984375;
						text2 = Math.Round(value176, 3).ToString("0.000");
						text3 = "MINUTES";
					}
					break;
				case 76:
					text = "TIME FROM START/RUN | ERROR: REQUEST F4 4B 4C";
					break;
				case 77:
				{
					text = "RUNTIME AT STALL";
					double value174 = (double)(int)array3[1] * 0.0535;
					text2 = Math.Round(value174, 3).ToString("0.000");
					text3 = "MINUTES";
					break;
				}
				case 78:
				{
					text = "CURRENT FUEL SHUTOFF";
					byte b9 = (byte)(array3[1] & 0xF0u);
					if (b9 == 0)
					{
						text2 = "NONE";
					}
					else if (Util.IsBitSet(b9, 4))
					{
						text2 = "IN DECEL";
					}
					else if (Util.IsBitSet(b9, 5))
					{
						text2 = "TORQUE MGMT";
					}
					else if (Util.IsBitSet(b9, 6))
					{
						text2 = "REV LIMITER";
					}
					else if (Util.IsBitSet(b9, 7))
					{
						text2 = "ABOVE 112 MPH";
					}
					break;
				}
				case 79:
				{
					text = "HISTORY OF FUEL SHUTOFF";
					byte b8 = (byte)(array3[1] & 0xF0u);
					if (b8 == 0)
					{
						text2 = "NONE";
					}
					else if (Util.IsBitSet(b8, 4))
					{
						text2 = "IN DECEL";
					}
					else if (Util.IsBitSet(b8, 5))
					{
						text2 = "TORQUE MGMT";
					}
					else if (Util.IsBitSet(b8, 6))
					{
						text2 = "REV LIMITER";
					}
					else if (Util.IsBitSet(b8, 7))
					{
						text2 = "ABOVE 112 MPH";
					}
					break;
				}
				case 81:
					text = "ADAPTIVE NUMERATOR 1";
					text2 = Util.ByteToHexString(array3, 1);
					break;
				case 87:
					text = "RPM/VSS RATIO";
					text2 = array3[1].ToString("0");
					break;
				case 88:
					text = "TRANSMISSION SELECTED GEAR 2";
					text2 = Util.ByteToHexString(array3, 1);
					break;
				case 90:
				{
					text = "DWELL COIL 1 (CYL1_4)";
					double value158 = (double)(int)array3[1] * 0.008;
					text2 = Math.Round(value158, 3).ToString("0.000");
					text3 = "MS";
					break;
				}
				case 91:
				{
					text = "DWELL COIL 2 (CYL2_3)";
					double value157 = (double)(int)array3[1] * 0.008;
					text2 = Math.Round(value157, 3).ToString("0.000");
					text3 = "MS";
					break;
				}
				case 92:
				{
					text = "DWELL COIL 3 (CYL3_6)";
					double value154 = (double)(int)array3[1] * 0.008;
					text2 = Math.Round(value154, 3).ToString("0.000");
					text3 = "MS";
					break;
				}
				case 93:
				{
					text = "FAN DUTY CYCLE";
					double value153 = (double)(int)array3[1] * 0.3921568627;
					text2 = Math.Round(value153, 1).ToString("0.0");
					text3 = "PERCENT";
					break;
				}
				case 96:
					text = "A/C RELAY STATE";
					text2 = Convert.ToString(array3[1], 2).PadLeft(8, '0');
					break;
				case 97:
				{
					text = "DISTANCE TRAVELED UP TO 4.2 MILES";
					double num81 = (double)(int)array3[1] * 0.032;
					double value150 = num81 * 1.609344;
					if (Settings.Default.Units == "imperial")
					{
						text2 = Math.Round(num81, 3).ToString("0.000");
						text3 = "MILE";
					}
					else if (Settings.Default.Units == "metric")
					{
						text2 = Math.Round(value150, 3).ToString("0.000");
						text3 = "KILOMETER";
					}
					break;
				}
				case 115:
				{
					text = "LIMP-IN: ";
					List<string> list15 = new List<string>();
					if (Util.IsBitSet(array3[1], 0))
					{
						list15.Add("ATS");
					}
					if (Util.IsBitSet(array3[1], 3))
					{
						list15.Add("IAT");
					}
					if (Util.IsBitSet(array3[1], 4))
					{
						list15.Add("TPS");
					}
					if (Util.IsBitSet(array3[1], 5))
					{
						list15.Add("MPE");
					}
					if (Util.IsBitSet(array3[1], 6))
					{
						list15.Add("MPV");
					}
					if (Util.IsBitSet(array3[1], 7))
					{
						list15.Add("ECT");
					}
					if (list15.Count == 0)
					{
						text = "NO LIMP-IN STATE";
						break;
					}
					foreach (string item4 in list15)
					{
						text = text + item4 + " | ";
					}
					if (text.Length > 2)
					{
						text = text.Remove(text.Length - 3);
					}
					break;
				}
				case 116:
				case 117:
				case 118:
				case 119:
				case 120:
				case 121:
				{
					text = "DTC " + (array3[0] - 114).ToString("0") + ": ";
					if (array3[1] == 0)
					{
						text2 = "EMPTY SLOT";
						break;
					}
					int num107 = SBEC3EngineDTC.Rows.IndexOf(SBEC3EngineDTC.Rows.Find(array3[1]));
					text = ((num107 <= -1) ? (text + "UNRECOGNIZED DTC") : (text + SBEC3EngineDTC.Rows[num107]["description"]));
					break;
				}
				case 122:
					text = "SPI TRANSFER RESULT";
					if (array2.Length >= 5)
					{
						if (array3[2] != array3[0] + 1)
						{
							text += " | ERROR: REQUEST F4 7A 7B";
						}
						else
						{
							text2 = Convert.ToString(array3[1], 2).PadLeft(8, '0') + " " + Convert.ToString(array3[3], 2).PadLeft(8, '0');
						}
					}
					break;
				case 123:
					text = "SPI TRANSFER RESULT | ERROR: REQUEST F4 7A 7B";
					break;
				case 142:
					text = "BIT STATE 7";
					text2 = Convert.ToString(array3[1], 2).PadLeft(8, '0');
					break;
				case 148:
					text = "EGR ZREF UPDATE D.C.";
					text2 = array3[1].ToString("0");
					break;
				case 149:
				{
					text = "EGR POSITION SENSOR VOLTAGE";
					double value186 = (double)(int)array3[1] * 0.0196;
					text2 = Math.Round(value186, 3).ToString("0.000");
					text3 = "V";
					break;
				}
				case 150:
				{
					text = "ACTUAL PURGE CURRENT";
					double value185 = (double)(int)array3[1] * 0.0196;
					text2 = Math.Round(value185, 3).ToString("0.000");
					text3 = "A";
					break;
				}
				case 152:
					text = "TPS INTERMITTENT COUNTER";
					text2 = array3[1].ToString("0");
					break;
				case 155:
				{
					text = "TRANSMISSION TEMPERATURE";
					double num105 = (double)(int)array3[1] * 4.0;
					double num106 = (num105 - 32.0) / 1.8;
					if (Settings.Default.Units == "imperial")
					{
						text2 = num105.ToString("0");
						text3 = "°F";
					}
					else if (Settings.Default.Units == "metric")
					{
						text2 = num106.ToString("0");
						text3 = "°C";
					}
					break;
				}
				case 163:
				{
					text = "CAM TIMING POSITION";
					double value183 = (double)(int)array3[1] * 0.5;
					text2 = Math.Round(value183, 1).ToString("0.0");
					text3 = "DEG";
					break;
				}
				case 164:
					text = "ENGINE GOOD TRIP COUNTER";
					text2 = array3[1].ToString("0");
					break;
				case 165:
					text = "ENGINE WARM-UP CYCLE COUNTER";
					text2 = array3[1].ToString("0");
					break;
				case 166:
					text = "OBD2 MONITOR TEST RESULTS 1";
					text2 = Convert.ToString(array3[1], 2).PadLeft(8, '0');
					break;
				case 167:
					text = "FREEZE FRAME PRIORITY LEVEL";
					text2 = array3[1].ToString("0");
					text3 = "0=LO 7=HI";
					break;
				case 168:
				{
					text = "FREEZE FRAME DTC: ";
					if (array3[1] == 0)
					{
						text += "EMPTY SLOT";
						break;
					}
					int num100 = SBEC3EngineDTC.Rows.IndexOf(SBEC3EngineDTC.Rows.Find(array3[1]));
					text = ((num100 <= -1) ? (text + "UNRECOGNIZED DTC") : (text + SBEC3EngineDTC.Rows[num100]["description"]));
					text2 = Util.ByteToHexString(array3, 1);
					break;
				}
				case 169:
					text = "FUEL SYSTEM STATUS 1";
					if (array3[1] == 0)
					{
						text2 = "N/A";
						break;
					}
					if (Util.IsBitSet(array3[1], 0))
					{
						text2 = "OPEN LOOP";
					}
					if (Util.IsBitSet(array3[1], 1))
					{
						text2 = "CLOSED LOOP";
					}
					if (Util.IsBitSet(array3[1], 2))
					{
						text2 = "OPEN LOOP / DRIVE";
					}
					if (Util.IsBitSet(array3[1], 3))
					{
						text2 = "OPEN LOOP / DTC";
					}
					if (Util.IsBitSet(array3[1], 4))
					{
						text2 = "CLOSED LOOP / DTC";
					}
					break;
				case 170:
					text = "FUEL SYSTEM STATUS 2";
					if (array3[1] == 0)
					{
						text2 = "N/A";
						break;
					}
					if (Util.IsBitSet(array3[1], 0))
					{
						text2 = "OPEN LOOP";
					}
					if (Util.IsBitSet(array3[1], 1))
					{
						text2 = "CLOSED LOOP";
					}
					if (Util.IsBitSet(array3[1], 2))
					{
						text2 = "OPEN LOOP / DRIVE";
					}
					if (Util.IsBitSet(array3[1], 3))
					{
						text2 = "OPEN LOOP / DTC";
					}
					if (Util.IsBitSet(array3[1], 4))
					{
						text2 = "CLOSED LOOP / DTC";
					}
					break;
				case 171:
				{
					text = "ENGINE LOAD";
					double value170 = (double)(int)array3[1] * 0.3921568627;
					text2 = Math.Round(value170, 1).ToString("0.0");
					text3 = "PERCENT";
					break;
				}
				case 172:
				{
					text = "ENGINE COOLANT TEMPERATURE";
					double num91 = array3[1] - 128;
					double a17 = 1.8 * num91 + 32.0;
					if (Settings.Default.Units == "imperial")
					{
						text2 = Math.Round(a17).ToString("0");
						text3 = "°F";
					}
					else if (Settings.Default.Units == "metric")
					{
						text2 = num91.ToString("0");
						text3 = "°C";
					}
					break;
				}
				case 173:
				{
					text = "SHORT TERM FUEL TRIM 1";
					double num90 = (double)(int)array3[1] * 0.196;
					if (array3[1] >= 128)
					{
						num90 -= 50.0;
					}
					text2 = Math.Round(num90, 3).ToString("0.000");
					text3 = "PERCENT";
					break;
				}
				case 174:
				{
					text = "LONG TERM FUEL TRIM 1";
					double num89 = (double)(int)array3[1] * 0.196;
					if (array3[1] >= 128)
					{
						num89 -= 50.0;
					}
					text2 = Math.Round(num89, 3).ToString("0.000");
					text3 = "PERCENT";
					break;
				}
				case 175:
				{
					text = "SHORT TERM FUEL TRIM 2";
					double num88 = (double)(int)array3[1] * 0.196;
					if (array3[1] >= 128)
					{
						num88 -= 50.0;
					}
					text2 = Math.Round(num88, 3).ToString("0.000");
					text3 = "PERCENT";
					break;
				}
				case 176:
				{
					text = "LONG TERM FUEL TRIM 2";
					double num87 = (double)(int)array3[1] * 0.196;
					if (array3[1] >= 128)
					{
						num87 -= 50.0;
					}
					text2 = Math.Round(num87, 3).ToString("0.000");
					text3 = "PERCENT";
					break;
				}
				case 177:
				{
					text = "INTAKE MANIFOLD ABSOLUTE PRESSURE (MAP)";
					double num85 = (double)(int)array3[1] * 0.059756;
					double value159 = num85 * 6.894757;
					if (Settings.Default.Units == "imperial")
					{
						text2 = Math.Round(num85, 1).ToString("0.0");
						text3 = "PSI";
					}
					else if (Settings.Default.Units == "metric")
					{
						text2 = Math.Round(value159, 1).ToString("0.0");
						text3 = "KPA";
					}
					break;
				}
				case 178:
					text = "ENGINE SPEED";
					text2 = ((double)(int)array3[1] * 32.0).ToString("0");
					text3 = "RPM";
					break;
				case 179:
				{
					text = "VEHICLE SPEED";
					double num84 = (double)(int)array3[1] * 0.5;
					double a16 = num84 * 1.609344;
					if (Settings.Default.Units == "imperial")
					{
						text2 = Math.Round(num84).ToString("0");
						text3 = "MPH";
					}
					else if (Settings.Default.Units == "metric")
					{
						text2 = Math.Round(a16).ToString("0");
						text3 = "KM/H";
					}
					break;
				}
				case 180:
				{
					text = "MAP VACUUM";
					double num83 = (double)(int)array3[1] * 0.059756;
					double value156 = num83 * 6.894757;
					if (Settings.Default.Units == "imperial")
					{
						text2 = Math.Round(num83, 1).ToString("0.0");
						text3 = "PSI";
					}
					else if (Settings.Default.Units == "metric")
					{
						text2 = Math.Round(value156, 1).ToString("0.0");
						text3 = "KPA";
					}
					break;
				}
				case 181:
				{
					text = "DTC E";
					if (array3[1] == 0)
					{
						text2 = "EMPTY SLOT";
						break;
					}
					int num80 = SBEC3EngineDTC.Rows.IndexOf(SBEC3EngineDTC.Rows.Find(array3[1]));
					text = ((num80 <= -1) ? (text + ": UNRECOGNIZED DTC") : (text + ": " + SBEC3EngineDTC.Rows[num80]["description"]));
					break;
				}
				case 182:
				{
					text = "CATALYST TEMPERATURE SENSOR VOLTAGE";
					double value149 = (double)(int)array3[1] * 0.0196;
					text2 = Math.Round(value149, 3).ToString("0.000");
					text3 = "V";
					break;
				}
				case 183:
				{
					text = "PURGE DUTY CYCLE";
					double value148 = (double)(int)array3[1] * 0.3921568627;
					text2 = Math.Round(value148, 1).ToString("0.0");
					text3 = "PERCENT";
					break;
				}
				case 189:
					text = "Brake switch monitor result 0";
					text2 = Convert.ToString(array3[1], 2).PadLeft(8, '0');
					break;
				case 190:
					text = "Brake switch monitor result 1";
					text2 = Convert.ToString(array3[1], 2).PadLeft(8, '0');
					break;
				case 191:
					text = "Brake switch monitor result 2";
					text2 = Convert.ToString(array3[1], 2).PadLeft(8, '0');
					break;
				case 192:
				{
					text = "CATALYST TEMPERATURE";
					double num79 = array3[1] - 128;
					double a15 = 1.8 * num79 + 32.0;
					if (Settings.Default.Units == "imperial")
					{
						text2 = Math.Round(a15).ToString("0");
						text3 = "°F";
					}
					else if (Settings.Default.Units == "metric")
					{
						text2 = num79.ToString("0");
						text3 = "°C";
					}
					break;
				}
				case 193:
					text = "FUEL LEVEL STATUS 1";
					text2 = Convert.ToString(array3[1], 2).PadLeft(8, '0');
					break;
				case 194:
				{
					text = "FUEL LEVEL SENSOR VOLTAGE 3";
					double value147 = (double)(int)array3[1] * 0.0196;
					text2 = Math.Round(value147, 3).ToString("0.000");
					text3 = "V";
					break;
				}
				case 210:
					text = "SENSOR RATIONALITY RESULT 0";
					text2 = Convert.ToString(array3[1], 2).PadLeft(8, '0');
					break;
				case 211:
					text = "SENSOR RATIONALITY RESULT 1";
					text2 = Convert.ToString(array3[1], 2).PadLeft(8, '0');
					break;
				case 212:
					text = "O2 SENSOR RATIONALITY RESULT 0";
					text2 = Convert.ToString(array3[1], 2).PadLeft(8, '0');
					break;
				case 213:
					text = "O2 SENSOR RATIONALITY RESULT 1";
					text2 = Convert.ToString(array3[1], 2).PadLeft(8, '0');
					break;
				case 214:
					text = "TORQUE CONVERTER CLUTCH MONITOR RESULT";
					text2 = ((!Util.IsBitSet(array3[1], 7)) ? "OK" : "PTU SOL RAT");
					break;
				case 215:
					text = "SENSOR RATIONALITY RESULT 2";
					text2 = Convert.ToString(array3[1], 2).PadLeft(8, '0');
					break;
				case 216:
					text = "SENSOR RATIONALITY RESULT 3";
					text2 = Convert.ToString(array3[1], 2).PadLeft(8, '0');
					break;
				case 218:
					text = "P_PCM_NOC_STRDCAMTIME";
					text2 = Util.ByteToHexString(array3, 1);
					break;
				case 219:
				{
					text = "FUEL LEVEL SENSOR VOLTAGE 2";
					double value146 = (double)(int)array3[1] * 0.0196;
					text2 = Math.Round(value146, 3).ToString("0.000");
					text3 = "V";
					break;
				}
				case 221:
					text = "CONFIGURATION 1";
					text2 = Convert.ToString(array3[1], 2).PadLeft(8, '0');
					break;
				case 222:
				{
					text = "FUEL LEVEL SENSOR VOLTAGE 1";
					double value145 = (double)(int)array3[1] * 0.0196;
					text2 = Math.Round(value145, 3).ToString("0.000");
					text3 = "V";
					break;
				}
				case 223:
				{
					text = "FUEL LEVEL";
					double num77 = (double)(int)array3[1] * 0.125;
					double value142 = num77 * 3.785412;
					if (Settings.Default.Units == "imperial")
					{
						text2 = Math.Round(num77, 1).ToString("0.0");
						text3 = "GALLON";
					}
					else if (Settings.Default.Units == "metric")
					{
						text2 = Math.Round(value142, 1).ToString("0.0");
						text3 = "LITER";
					}
					break;
				}
				case 224:
				{
					text = "FUEL USED";
					double num76 = (double)(int)array3[1] * 0.125;
					double value138 = num76 * 3.785412;
					if (Settings.Default.Units == "imperial")
					{
						text2 = Math.Round(num76, 1).ToString("0.0");
						text3 = "GALLON";
					}
					else if (Settings.Default.Units == "metric")
					{
						text2 = Math.Round(value138, 1).ToString("0.0");
						text3 = "LITER";
					}
					break;
				}
				case 227:
				{
					text = "ENGINE LOAD";
					double value137 = (double)(int)array3[1] * 0.3921568627;
					text2 = Math.Round(value137, 1).ToString("0.0");
					text3 = "PERCENT";
					break;
				}
				case 228:
					text = "OBD2 MONITOR TEST RESULTS 2";
					text2 = Convert.ToString(array3[1], 2).PadLeft(8, '0');
					break;
				case 229:
					text = "CONFIGURATION 2";
					text2 = Convert.ToString(array3[1], 2).PadLeft(8, '0');
					break;
				case 231:
					text = "CELL #1 - IDLE CELL";
					text2 = Util.ByteToHexString(array3, 1);
					break;
				case 232:
					text = "CELL #2 - 1ST OFF IDLE CELL";
					text2 = Util.ByteToHexString(array3, 1);
					break;
				case 233:
					text = "CELL #3 - 2ND OFF IDLE CELL";
					text2 = Util.ByteToHexString(array3, 1);
					break;
				case 236:
					text = "FUEL SYSTEM STATUS 2";
					if (Util.IsBitSet(array3[1], 0))
					{
						text2 = "OPEN LOOP";
					}
					if (Util.IsBitSet(array3[1], 1))
					{
						text2 = "CLOSED LOOP";
					}
					if (Util.IsBitSet(array3[1], 2))
					{
						text2 = "OPEN LOOP / DRIVE";
					}
					if (Util.IsBitSet(array3[1], 3))
					{
						text2 = "OPEN LOOP / DTC";
					}
					if (Util.IsBitSet(array3[1], 4))
					{
						text2 = "CLOSED LOOP / DTC";
					}
					break;
				case 238:
					text = "CRUISE | OPERATING MODE";
					text2 = (array3[1] & 0xF) switch
					{
						8 => "DISENGAGED", 
						9 => "NORMAL", 
						10 => "ACCELERATING", 
						11 => "DECELERATING", 
						_ => "N/A", 
					};
					break;
				case 239:
				{
					text = "CALCULATED TPS VOLTAGE";
					double value131 = (double)(int)array3[1] * 0.0196;
					text2 = Math.Round(value131, 3).ToString("0.000");
					text3 = "V";
					break;
				}
				default:
				{
					for (int num73 = 0; num73 < num23; num73++)
					{
						list3.Add(array3[num73 * 2]);
						list4.Add(array3[num73 * 2 + 1]);
					}
					text = "F4 RAM TABLE | OFFSET: " + Util.ByteToHexStringSimple(list3.ToArray());
					text2 = Util.ByteToHexStringSimple(list4.ToArray());
					break;
				}
				}
				break;
			case 245:
				text = "F5 RAM TABLE SELECTED";
				if (array2.Length < 3)
				{
					break;
				}
				switch (array3[0])
				{
				case 30:
				{
					text = "ACTUAL GOVERNOR PRESSURE";
					if (array2.Length < 5)
					{
						break;
					}
					if (array3[2] != array3[0] + 1)
					{
						text += " | ERROR: REQUEST F5 1E 1F";
						break;
					}
					double num71 = (double)((array3[1] << 8) + array3[3]) * (1.0 / 128.0);
					double value128 = num71 * 6.894757;
					if (Settings.Default.Units == "imperial")
					{
						text2 = Math.Round(num71, 3).ToString("0.000");
						text3 = "PSI";
					}
					else if (Settings.Default.Units == "metric")
					{
						text2 = Math.Round(value128, 3).ToString("0.000");
						text3 = "KPA";
					}
					break;
				}
				case 31:
					text = "ACTUAL GOVERNOR PRESSURE | ERROR: REQUEST F5 1E 1F";
					break;
				case 71:
					text = "TTVA ADJUSTED POSITION";
					if (array2.Length >= 5)
					{
						if (array3[2] != array3[0] + 1)
						{
							text += " | ERROR: REQUEST F5 47 48";
							break;
						}
						double value126 = (double)((array3[1] << 8) + array3[3]) * (1.0 / 128.0);
						text2 = Math.Round(value126, 3).ToString("0.000");
						text3 = "DEG";
					}
					break;
				case 72:
					text = "TTVA ADJUSTED POSITION | ERROR: REQUEST F5 47 48";
					break;
				case 73:
					text = "TTVA TARGET POSITION";
					if (array2.Length >= 5)
					{
						if (array3[2] != array3[0] + 1)
						{
							text += " | ERROR: REQUEST F5 49 4A";
							break;
						}
						double value125 = (double)((array3[1] << 8) + array3[3]) * (1.0 / 128.0);
						text2 = Math.Round(value125, 3).ToString("0.000");
						text3 = "DEG";
					}
					break;
				case 74:
					text = "TTVA TARGET POSITION | ERROR: REQUEST F5 49 4A";
					break;
				case 75:
					text = "TTVA ACTUAL POSITION";
					if (array2.Length >= 5)
					{
						if (array3[2] != array3[0] + 1)
						{
							text += " | ERROR: REQUEST F5 4B 4C";
							break;
						}
						double value127 = (double)((array3[1] << 8) + array3[3]) * (1.0 / 128.0);
						text2 = Math.Round(value127, 3).ToString("0.000");
						text3 = "DEG";
					}
					break;
				case 76:
					text = "TTVA ACTUAL POSITION | ERROR: REQUEST F5 4B 4C";
					break;
				case 77:
					text = "TTVA DUTY CYCLE";
					if (array2.Length >= 5)
					{
						if (array3[2] != array3[0] + 1)
						{
							text += " | ERROR: REQUEST F5 4D 4E";
							break;
						}
						double value129 = (double)((array3[1] << 8) + array3[3]) * (1.0 / 128.0);
						text2 = Math.Round(value129, 3).ToString("0.000");
						text3 = "PERCENT";
					}
					break;
				case 78:
					text = "TTVA DUTY CYCLE | ERROR: REQUEST F5 4D 4E";
					break;
				case 203:
					text = "TCM | FAULT CODE PRESENT";
					if (array2.Length >= 5)
					{
						if (array3[2] != array3[0] + 1)
						{
							text += " | ERROR: REQUEST F5 CB CC";
							break;
						}
						if (array3[1] == 0 && array3[3] == 0)
						{
							text = "TCM | NO FAULT CODE";
							break;
						}
						text2 = "OBD2 P" + Util.ByteToHexStringSimple(new byte[2]
						{
							array3[1],
							array3[3]
						}).Replace(" ", "");
					}
					break;
				case 204:
					text = "TCM | FAULT CODE PRESENT | ERROR: REQUEST F5 CB CC";
					break;
				default:
				{
					for (int num70 = 0; num70 < num23; num70++)
					{
						list3.Add(array3[num70 * 2]);
						list4.Add(array3[num70 * 2 + 1]);
					}
					text = "F5 RAM TABLE | OFFSET: " + Util.ByteToHexStringSimple(list3.ToArray());
					text2 = Util.ByteToHexStringSimple(list4.ToArray());
					break;
				}
				}
				break;
			case 246:
				text = "F6 RAM TABLE SELECTED";
				if (array2.Length >= 3)
				{
					byte b3 = array3[0];
					for (int num66 = 0; num66 < num23; num66++)
					{
						list3.Add(array3[num66 * 2]);
						list4.Add(array3[num66 * 2 + 1]);
					}
					text = "F6 RAM TABLE | OFFSET: " + Util.ByteToHexStringSimple(list3.ToArray());
					text2 = Util.ByteToHexStringSimple(list4.ToArray());
				}
				break;
			case 247:
				text = "F7 RAM TABLE SELECTED";
				if (array2.Length >= 3)
				{
					byte b3 = array3[0];
					for (int num62 = 0; num62 < num23; num62++)
					{
						list3.Add(array3[num62 * 2]);
						list4.Add(array3[num62 * 2 + 1]);
					}
					text = "F7 RAM TABLE | OFFSET: " + Util.ByteToHexStringSimple(list3.ToArray());
					text2 = Util.ByteToHexStringSimple(list4.ToArray());
				}
				break;
			case 248:
				text = "F8 RAM TABLE SELECTED";
				if (array2.Length < 3)
				{
					break;
				}
				if (Year < 2003 && CumminsSelected)
				{
					switch (array3[0])
					{
					case 7:
					{
						text = "VEHICLE SPEED";
						if (array2.Length < 5)
						{
							break;
						}
						if (array3[2] != array3[0] + 1)
						{
							text += " | ERROR: REQUEST F8 07 08";
							break;
						}
						double num116 = (double)((array3[1] << 8) + array3[3]) * (1.0 / 256.0);
						double value201 = num116 * 1.609344;
						if (Settings.Default.Units == "imperial")
						{
							text2 = Math.Round(num116, 3).ToString("0.000");
							text3 = "MPH";
						}
						else if (Settings.Default.Units == "metric")
						{
							text2 = Math.Round(value201, 3).ToString("0.000");
							text3 = "KM/H";
						}
						break;
					}
					case 8:
						text = "VEHICLE SPEED | ERROR: REQUEST F8 07 08";
						break;
					case 9:
						text = "ENGINE SPEED";
						if (array2.Length >= 5)
						{
							if (array3[2] != array3[0] + 1)
							{
								text += " | ERROR: REQUEST F8 09 0A";
								break;
							}
							double value189 = (double)((array3[1] << 8) + array3[3]) * 0.125;
							text2 = Math.Round(value189, 3).ToString("0.000");
							text3 = "RPM";
						}
						break;
					case 10:
						text = "ENGINE SPEED | ERROR: REQUEST F8 09 0A";
						break;
					case 11:
					{
						text = "SWITCH STATUS 1";
						List<string> list20 = new List<string>();
						if (Util.IsBitSet(array3[1], 7))
						{
							list20.Add("-7-");
						}
						if (Util.IsBitSet(array3[1], 6))
						{
							list20.Add("BRAKE");
						}
						if (Util.IsBitSet(array3[1], 5))
						{
							list20.Add("IDLE");
						}
						if (Util.IsBitSet(array3[1], 4))
						{
							list20.Add("-4-");
						}
						if (Util.IsBitSet(array3[1], 3))
						{
							list20.Add("-3-");
						}
						if (Util.IsBitSet(array3[1], 2))
						{
							list20.Add("NOT-IDLE");
						}
						if (Util.IsBitSet(array3[1], 1))
						{
							list20.Add("-1-");
						}
						if (Util.IsBitSet(array3[1], 0))
						{
							list20.Add("-0-");
						}
						if (list20.Count == 0)
						{
							break;
						}
						text += ": ";
						foreach (string item5 in list20)
						{
							text = text + item5 + " | ";
						}
						if (text.Length > 2)
						{
							text = text.Remove(text.Length - 3);
						}
						break;
					}
					case 13:
						text = "ENGINE LOAD";
						if (array2.Length >= 5)
						{
							if (array3[2] != array3[0] + 1)
							{
								text += " | ERROR: REQUEST F8 0D 0E";
								break;
							}
							double value194 = (double)((array3[1] << 8) + array3[3]) * (1.0 / 256.0);
							text2 = Math.Round(value194, 1).ToString("0.0");
							text3 = "PERCENT";
						}
						break;
					case 14:
						text = "ENGINE LOAD | ERROR: REQUEST F8 0D 0E";
						break;
					case 15:
						text = "APP SENSOR PERCENT";
						if (array2.Length >= 5)
						{
							if (array3[2] != array3[0] + 1)
							{
								text += " | ERROR: REQUEST F8 0F 10";
								break;
							}
							double value208 = (double)((array3[1] << 8) + array3[3]) * 0.25;
							text2 = Math.Round(value208, 1).ToString("0.0");
							text3 = "PERCENT";
						}
						break;
					case 16:
						text = "APP SENSOR PERCENT | ERROR: REQUEST F8 0F 10";
						break;
					case 17:
					{
						text = "BOOST PRESSURE";
						if (array2.Length < 5)
						{
							break;
						}
						if (array3[2] != array3[0] + 1)
						{
							text += " | ERROR: REQUEST F8 11 12";
							break;
						}
						double num120 = (double)((array3[1] << 8) + array3[3]) * (1.0 / 128.0);
						double value214 = num120 * 6.894757;
						if (Settings.Default.Units == "imperial")
						{
							text2 = Math.Round(num120, 3).ToString("0.000");
							text3 = "PSI";
						}
						else if (Settings.Default.Units == "metric")
						{
							text2 = Math.Round(value214, 3).ToString("0.000");
							text3 = "KPA";
						}
						break;
					}
					case 18:
						text = "BOOST PRESSURE | ERROR: REQUEST F8 11 12";
						break;
					case 19:
					{
						text = "ENGINE COOLANT TEMPERATURE";
						if (array2.Length < 5)
						{
							break;
						}
						if (array3[2] != array3[0] + 1)
						{
							text += " | ERROR: REQUEST F8 13 14";
							break;
						}
						double num114 = (double)((array3[1] << 8) + array3[3]) * (1.0 / 64.0);
						double value197 = (num114 - 32.0) / 1.8;
						if (Settings.Default.Units == "imperial")
						{
							text2 = Math.Round(num114, 1).ToString("0.0");
							text3 = "°F";
						}
						else if (Settings.Default.Units == "metric")
						{
							text2 = Math.Round(value197, 1).ToString("0.0");
							text3 = "°C";
						}
						break;
					}
					case 20:
						text = "ENGINE COOLANT TEMPERATURE | ERROR: REQUEST F8 13 14";
						break;
					case 21:
					{
						text = "INTAKE AIR TEMPERATURE";
						if (array2.Length < 5)
						{
							break;
						}
						if (array3[2] != array3[0] + 1)
						{
							text += " | ERROR: REQUEST F8 15 16";
							break;
						}
						double num119 = (double)((array3[1] << 8) + array3[3]) * (1.0 / 64.0);
						double value209 = (num119 - 32.0) / 1.8;
						if (Settings.Default.Units == "imperial")
						{
							text2 = Math.Round(num119, 1).ToString("0.0");
							text3 = "°F";
						}
						else if (Settings.Default.Units == "metric")
						{
							text2 = Math.Round(value209, 1).ToString("0.0");
							text3 = "°C";
						}
						break;
					}
					case 22:
						text = "INTAKE AIR TEMPERATURE | ERROR: REQUEST F8 15 16";
						break;
					case 23:
					{
						text = "OIL PRESSURE";
						if (array2.Length < 5)
						{
							break;
						}
						if (array3[2] != array3[0] + 1)
						{
							text += " | ERROR: REQUEST F8 17 18";
							break;
						}
						double num112 = (double)((array3[1] << 8) + array3[3]) * (1.0 / 128.0);
						double value192 = num112 * 6.894757;
						if (Settings.Default.Units == "imperial")
						{
							text2 = Math.Round(num112, 3).ToString("0.000");
							text3 = "PSI";
						}
						else if (Settings.Default.Units == "metric")
						{
							text2 = Math.Round(value192, 3).ToString("0.000");
							text3 = "KPA";
						}
						break;
					}
					case 24:
						text = "OIL PRESSURE | ERROR: REQUEST F8 17 18";
						break;
					case 25:
					{
						text = "SWITCH STATUS 2";
						List<string> list17 = new List<string>();
						if (Util.IsBitSet(array3[1], 7))
						{
							list17.Add("-7-");
						}
						if (Util.IsBitSet(array3[1], 6))
						{
							list17.Add("-6-");
						}
						if (Util.IsBitSet(array3[1], 5))
						{
							list17.Add("-5-");
						}
						if (Util.IsBitSet(array3[1], 4))
						{
							list17.Add("-4-");
						}
						if (Util.IsBitSet(array3[1], 3))
						{
							list17.Add("INTHEAT2");
						}
						if (Util.IsBitSet(array3[1], 2))
						{
							list17.Add("INTHEAT1");
						}
						if (Util.IsBitSet(array3[1], 1))
						{
							list17.Add("TRFPMPDR");
						}
						if (Util.IsBitSet(array3[1], 0))
						{
							list17.Add("-0-");
						}
						if (list17.Count == 0)
						{
							break;
						}
						text += ": ";
						foreach (string item6 in list17)
						{
							text = text + item6 + " | ";
						}
						if (text.Length > 2)
						{
							text = text.Remove(text.Length - 3);
						}
						break;
					}
					case 32:
						text = "FINAL FUEL STATE";
						switch (array3[1])
						{
						case 0:
							text2 = "NOT SET";
							break;
						case 1:
							text2 = "JCOM TORQUE";
							break;
						case 2:
							text2 = "JCOM SPEED";
							break;
						case 3:
							text2 = "PROGRSV SHIFT";
							break;
						case 4:
							text2 = "PTO";
							break;
						case 5:
							text2 = "USER COMMAND";
							break;
						case 6:
							text2 = "LIMP HOME";
							break;
						case 7:
							text2 = "ASG THROTTLE";
							break;
						case 8:
							text2 = "4-D FUELING";
							break;
						case 9:
							text2 = "CRUISE CONTROL";
							break;
						case 10:
							text2 = "ROAD SPEED GOV";
							break;
						case 11:
							text2 = "LOW SPEED GOV";
							break;
						case 12:
							text2 = "HIGH SPEED GOV";
							break;
						case 13:
							text2 = "TORQUE DERATE OVERRIDE";
							break;
						case 14:
							text2 = "LOW GEAR";
							break;
						case 15:
							text2 = "ALTITUDE DERATE";
							break;
						case 16:
							text2 = "AFC DERATE";
							break;
						case 17:
							text2 = "ANC DERATE";
							break;
						case 18:
							text2 = "ENGINE PROTECT";
							break;
						case 19:
							text2 = "TORQUE CRV LIMIT";
							break;
						case 20:
							text2 = "JCOM TORQUE DERATE";
							break;
						case 21:
							text2 = "OUT OF GEAR";
							break;
						case 22:
							text2 = "CRANKING";
							break;
						case 23:
							text2 = "USER OVERRIDE";
							break;
						case 24:
							text2 = "ENGINE BRAKE";
							break;
						case 25:
							text2 = "ENGINE OVERSPEED";
							break;
						case 26:
							text2 = "ENGINE STOPPED";
							break;
						case 27:
							text2 = "SHUTDOWN";
							break;
						case 28:
							text2 = "FUEL DTC DERATE";
							break;
						case 29:
							text2 = "ENGINE PROTECT";
							break;
						case 30:
							text2 = "ALL SPD GOV APP";
							break;
						case 31:
							text2 = "ALT TORQUE";
							break;
						case 32:
							text2 = "MASTER/SLAVE OVERRIDE";
							break;
						case 33:
							text2 = "STARTUP OIL LIMIT";
							break;
						case 34:
							text2 = "PTO DERATE";
							break;
						case 35:
							text2 = "TORQUE CONTROL";
							break;
						case 36:
							text2 = "POWERTRAIN PROTECT";
							break;
						case 37:
							text2 = "T2 SPEED";
							break;
						case 38:
							text2 = "T2 TORQUE DERATE";
							break;
						case 39:
							text2 = "T2 DERATE";
							break;
						case 40:
							text2 = "NO DERATE";
							break;
						case 41:
							text2 = "ANTI THEFT DERATE";
							break;
						case 42:
							text2 = "PART THROTTLE LIMIT";
							break;
						case 43:
							text2 = "STEADY-STATE AMB DERATE";
							break;
						case 44:
							text2 = "TRSNT COOLANT DERATE";
							break;
						}
						break;
					case 33:
						text = "BATTERY VOLTAGE";
						if (array2.Length >= 5)
						{
							if (array3[2] != array3[0] + 1)
							{
								text += " | ERROR: REQUEST F8 21 22";
								break;
							}
							double value213 = (double)((array3[1] << 8) + array3[3]) * 0.0625;
							text2 = Math.Round(value213, 3).ToString("0.000");
							text3 = "V";
						}
						break;
					case 34:
						text = "BATTERY VOLTAGE | ERROR: REQUEST F8 21 22";
						break;
					case 35:
						text = "CALCULATED FUEL";
						if (array2.Length >= 5)
						{
							if (array3[2] != array3[0] + 1)
							{
								text += " | ERROR: REQUEST F8 23 24";
								break;
							}
							double value210 = (double)((array3[1] << 8) + array3[3]) * 0.001953155;
							text2 = Math.Round(value210, 3).ToString("0.000");
							text3 = "PERCENT";
						}
						break;
					case 36:
						text = "CALCULATED FUEL | ERROR: REQUEST F8 23 24";
						break;
					case 37:
						text = "CALCULATED TIMING";
						if (array2.Length >= 5)
						{
							if (array3[2] != array3[0] + 1)
							{
								text += " | ERROR: REQUEST F8 25 26";
								break;
							}
							double value206 = (double)((array3[1] << 8) + array3[3]) * 0.1176475 - 20.0;
							text2 = Math.Round(value206, 3).ToString("0.000");
							text3 = "DEG";
						}
						break;
					case 38:
						text = "CALCULATED TIMING | ERROR: REQUEST F8 25 26";
						break;
					case 39:
						text = "REGULATOR VALVE CURRENT";
						if (array2.Length >= 5)
						{
							if (array3[2] != array3[0] + 1)
							{
								text += " | ERROR: REQUEST F8 27 28";
								break;
							}
							double value196 = (double)((array3[1] << 8) + array3[3]) * 1.220721752;
							text2 = Math.Round(value196, 3).ToString("0.000");
							text3 = "MILLIAMPS";
						}
						break;
					case 40:
						text = "REGULATOR VALVE CURRENT | ERROR: REQUEST F8 27 28";
						break;
					case 41:
					{
						text = "INJECTOR PUMP FUEL TEMPERATURE";
						if (array2.Length < 5)
						{
							break;
						}
						if (array3[2] != array3[0] + 1)
						{
							text += " | ERROR: REQUEST F8 29 2A";
							break;
						}
						double num110 = (double)((array3[1] << 8) + array3[3]) * 0.0625;
						double value190 = (num110 - 32.0) / 1.8;
						if (Settings.Default.Units == "imperial")
						{
							text2 = Math.Round(num110, 1).ToString("0.0");
							text3 = "°F";
						}
						else if (Settings.Default.Units == "metric")
						{
							text2 = Math.Round(value190, 1).ToString("0.0");
							text3 = "°C";
						}
						break;
					}
					case 42:
						text = "INJECTOR PUMP FUEL TEMPERATURE | ERROR: REQUEST F8 29 2A";
						break;
					case 43:
						text = "INJECTOR PUMP ENGINE SPEED";
						if (array2.Length >= 5)
						{
							if (array3[2] != array3[0] + 1)
							{
								text += " | ERROR: REQUEST F8 2B 2C";
								break;
							}
							double value215 = (double)((array3[1] << 8) + array3[3]) * 0.125;
							text2 = Math.Round(value215, 3).ToString("0.000");
							text3 = "RPM";
						}
						break;
					case 44:
						text = "INJECTOR PUMP ENGINE SPEED | ERROR: REQUEST F8 2B 2C";
						break;
					case 47:
						text = "FREEZE FRM DTC 1:";
						if (array2.Length >= 5)
						{
							if (array3[2] != array3[0] + 1)
							{
								text += " | ERROR: REQUEST F8 2F 30";
								break;
							}
							if (array3[1] == 0 && array3[3] == 0)
							{
								text += "EMPTY SLOT";
								break;
							}
							text2 = "OBD2 P" + Util.ByteToHexString(new byte[2]
							{
								array3[1],
								array3[3]
							}, 0, 2).Replace(" ", "");
						}
						break;
					case 48:
						text = "FREEZE FRAME DTC 1: | ERROR: REQUEST F8 2F 30";
						break;
					case 55:
					{
						text = "VEHICLE SPEED";
						if (array2.Length < 5)
						{
							break;
						}
						if (array3[2] != array3[0] + 1)
						{
							text += " | ERROR: REQUEST F8 37 38";
							break;
						}
						double num118 = (double)((array3[1] << 8) + array3[3]) * (1.0 / 256.0);
						double value205 = num118 * 1.609344;
						if (Settings.Default.Units == "imperial")
						{
							text2 = Math.Round(num118, 3).ToString("0.000");
							text3 = "MPH";
						}
						else if (Settings.Default.Units == "metric")
						{
							text2 = Math.Round(value205, 3).ToString("0.000");
							text3 = "KM/H";
						}
						break;
					}
					case 56:
						text = "VEHICLE SPEED | ERROR: REQUEST F8 37 38";
						break;
					case 57:
						text = "ENGINE SPEED";
						if (array2.Length >= 5)
						{
							if (array3[2] != array3[0] + 1)
							{
								text += " | ERROR: REQUEST F8 39 3A";
								break;
							}
							double value200 = (double)((array3[1] << 8) + array3[3]) * 0.125;
							text2 = Math.Round(value200, 3).ToString("0.000");
							text3 = "RPM";
						}
						break;
					case 58:
						text = "ENGINE SPEED | ERROR: REQUEST F8 39 3A";
						break;
					case 59:
					{
						text = "SWITCH STATUS 1";
						List<string> list19 = new List<string>();
						if (Util.IsBitSet(array3[1], 7))
						{
							list19.Add("-7-");
						}
						if (Util.IsBitSet(array3[1], 6))
						{
							list19.Add("BRAKE");
						}
						if (Util.IsBitSet(array3[1], 5))
						{
							list19.Add("IDLE");
						}
						if (Util.IsBitSet(array3[1], 4))
						{
							list19.Add("-4-");
						}
						if (Util.IsBitSet(array3[1], 3))
						{
							list19.Add("-3-");
						}
						if (Util.IsBitSet(array3[1], 2))
						{
							list19.Add("NOT-IDLE");
						}
						if (Util.IsBitSet(array3[1], 1))
						{
							list19.Add("-1-");
						}
						if (Util.IsBitSet(array3[1], 0))
						{
							list19.Add("-0-");
						}
						if (list19.Count == 0)
						{
							break;
						}
						text += ": ";
						foreach (string item7 in list19)
						{
							text = text + item7 + " | ";
						}
						if (text.Length > 2)
						{
							text = text.Remove(text.Length - 3);
						}
						break;
					}
					case 61:
						text = "ENGINE LOAD";
						if (array2.Length >= 5)
						{
							if (array3[2] != array3[0] + 1)
							{
								text += " | ERROR: REQUEST F8 3D 3E";
								break;
							}
							double value212 = (double)((array3[1] << 8) + array3[3]) * (1.0 / 256.0);
							text2 = Math.Round(value212, 1).ToString("0.0");
							text3 = "PERCENT";
						}
						break;
					case 62:
						text = "ENGINE LOAD | ERROR: REQUEST F8 3D 3E";
						break;
					case 63:
						text = "APP SENSOR PERCENT";
						if (array2.Length >= 5)
						{
							if (array3[2] != array3[0] + 1)
							{
								text += " | ERROR: REQUEST F8 3F 40";
								break;
							}
							double value211 = (double)((array3[1] << 8) + array3[3]) * 0.25;
							text2 = Math.Round(value211, 1).ToString("0.0");
							text3 = "PERCENT";
						}
						break;
					case 64:
						text = "APP SENSOR PERCENT | ERROR: REQUEST F8 3F 40";
						break;
					case 65:
					{
						text = "BOOST PRESSURE";
						if (array2.Length < 5)
						{
							break;
						}
						if (array3[2] != array3[0] + 1)
						{
							text += " | ERROR: REQUEST F8 42 43";
							break;
						}
						double num117 = (double)((array3[1] << 8) + array3[3]) * (1.0 / 128.0);
						double value204 = num117 * 6.894757;
						if (Settings.Default.Units == "imperial")
						{
							text2 = Math.Round(num117, 3).ToString("0.000");
							text3 = "PSI";
						}
						else if (Settings.Default.Units == "metric")
						{
							text2 = Math.Round(value204, 3).ToString("0.000");
							text3 = "KPA";
						}
						break;
					}
					case 66:
						text = "BOOST PRESSURE | ERROR: REQUEST F8 42 43";
						break;
					case 67:
					{
						text = "ENGINE COOLANT TEMPERATURE";
						if (array2.Length < 5)
						{
							break;
						}
						if (array3[2] != array3[0] + 1)
						{
							text += " | ERROR: REQUEST F8 44 45";
							break;
						}
						double num115 = (double)((array3[1] << 8) + array3[3]) * (1.0 / 64.0);
						double value198 = (num115 - 32.0) / 1.8;
						if (Settings.Default.Units == "imperial")
						{
							text2 = Math.Round(num115, 1).ToString("0.0");
							text3 = "°F";
						}
						else if (Settings.Default.Units == "metric")
						{
							text2 = Math.Round(value198, 1).ToString("0.0");
							text3 = "°C";
						}
						break;
					}
					case 68:
						text = "ENGINE COOLANT TEMPERATURE | ERROR: REQUEST F8 44 45";
						break;
					case 69:
					{
						text = "INTAKE AIR TEMPERATURE";
						if (array2.Length < 5)
						{
							break;
						}
						if (array3[2] != array3[0] + 1)
						{
							text += " | ERROR: REQUEST F8 46 47";
							break;
						}
						double num111 = (double)((array3[1] << 8) + array3[3]) * (1.0 / 64.0);
						double value191 = (num111 - 32.0) / 1.8;
						if (Settings.Default.Units == "imperial")
						{
							text2 = Math.Round(num111, 1).ToString("0.0");
							text3 = "°F";
						}
						else if (Settings.Default.Units == "metric")
						{
							text2 = Math.Round(value191, 1).ToString("0.0");
							text3 = "°C";
						}
						break;
					}
					case 70:
						text = "INTAKE AIR TEMPERATURE | ERROR: REQUEST F8 46 47";
						break;
					case 71:
					{
						text = "OIL PRESSURE";
						if (array2.Length < 5)
						{
							break;
						}
						if (array3[2] != array3[0] + 1)
						{
							text += " | ERROR: REQUEST F8 48 49";
							break;
						}
						double num121 = (double)((array3[1] << 8) + array3[3]) * (1.0 / 128.0);
						double value216 = num121 * 6.894757;
						if (Settings.Default.Units == "imperial")
						{
							text2 = Math.Round(num121, 3).ToString("0.000");
							text3 = "PSI";
						}
						else if (Settings.Default.Units == "metric")
						{
							text2 = Math.Round(value216, 3).ToString("0.000");
							text3 = "KPA";
						}
						break;
					}
					case 72:
						text = "OIL PRESSURE | ERROR: REQUEST F8 48 49";
						break;
					case 73:
					{
						text = "SWITCH STATUS 2";
						List<string> list18 = new List<string>();
						if (Util.IsBitSet(array3[1], 7))
						{
							list18.Add("-7-");
						}
						if (Util.IsBitSet(array3[1], 6))
						{
							list18.Add("-6-");
						}
						if (Util.IsBitSet(array3[1], 5))
						{
							list18.Add("-5-");
						}
						if (Util.IsBitSet(array3[1], 4))
						{
							list18.Add("-4-");
						}
						if (Util.IsBitSet(array3[1], 3))
						{
							list18.Add("INTHEAT2");
						}
						if (Util.IsBitSet(array3[1], 2))
						{
							list18.Add("INTHEAT1");
						}
						if (Util.IsBitSet(array3[1], 1))
						{
							list18.Add("TRFPMPDR");
						}
						if (Util.IsBitSet(array3[1], 0))
						{
							list18.Add("-0-");
						}
						if (list18.Count == 0)
						{
							break;
						}
						text += ": ";
						foreach (string item8 in list18)
						{
							text = text + item8 + " | ";
						}
						if (text.Length > 2)
						{
							text = text.Remove(text.Length - 3);
						}
						break;
					}
					case 80:
						text = "FINAL FUEL STATE";
						switch (array3[1])
						{
						case 0:
							text2 = "NOT SET";
							break;
						case 1:
							text2 = "JCOM TORQUE";
							break;
						case 2:
							text2 = "JCOM SPEED";
							break;
						case 3:
							text2 = "PROGRSV SHIFT";
							break;
						case 4:
							text2 = "PTO";
							break;
						case 5:
							text2 = "USER COMMAND";
							break;
						case 6:
							text2 = "LIMP HOME";
							break;
						case 7:
							text2 = "ASG THROTTLE";
							break;
						case 8:
							text2 = "4-D FUELING";
							break;
						case 9:
							text2 = "CRUISE CONTROL";
							break;
						case 10:
							text2 = "ROAD SPEED GOV";
							break;
						case 11:
							text2 = "LOW SPEED GOV";
							break;
						case 12:
							text2 = "HIGH SPEED GOV";
							break;
						case 13:
							text2 = "TORQUE DERATE OVERRIDE";
							break;
						case 14:
							text2 = "LOW GEAR";
							break;
						case 15:
							text2 = "ALTITUDE DERATE";
							break;
						case 16:
							text2 = "AFC DERATE";
							break;
						case 17:
							text2 = "ANC DERATE";
							break;
						case 18:
							text2 = "ENGINE PROTECT";
							break;
						case 19:
							text2 = "TORQUE CRV LIMIT";
							break;
						case 20:
							text2 = "JCOM TORQUE DERATE";
							break;
						case 21:
							text2 = "OUT OF GEAR";
							break;
						case 22:
							text2 = "CRANKING";
							break;
						case 23:
							text2 = "USER OVERRIDE";
							break;
						case 24:
							text2 = "ENGINE BRAKE";
							break;
						case 25:
							text2 = "ENGINE OVERSPEED";
							break;
						case 26:
							text2 = "ENGINE STOPPED";
							break;
						case 27:
							text2 = "SHUTDOWN";
							break;
						case 28:
							text2 = "FUEL DTC DERATE";
							break;
						case 29:
							text2 = "ENGINE PROTECT";
							break;
						case 30:
							text2 = "ALL SPD GOV APP";
							break;
						case 31:
							text2 = "ALT TORQUE";
							break;
						case 32:
							text2 = "MASTER/SLAVE OVERRIDE";
							break;
						case 33:
							text2 = "STARTUP OIL LIMIT";
							break;
						case 34:
							text2 = "PTO DERATE";
							break;
						case 35:
							text2 = "TORQUE CONTROL";
							break;
						case 36:
							text2 = "POWERTRAIN PROTECT";
							break;
						case 37:
							text2 = "T2 SPEED";
							break;
						case 38:
							text2 = "T2 TORQUE DERATE";
							break;
						case 39:
							text2 = "T2 DERATE";
							break;
						case 40:
							text2 = "NO DERATE";
							break;
						case 41:
							text2 = "ANTI THEFT DERATE";
							break;
						case 42:
							text2 = "PART THROTTLE LIMIT";
							break;
						case 43:
							text2 = "STEADY-STATE AMB DERATE";
							break;
						case 44:
							text2 = "TRSNT COOLANT DERATE";
							break;
						}
						break;
					case 81:
						text = "BATTERY VOLTAGE";
						if (array2.Length >= 5)
						{
							if (array3[2] != array3[0] + 1)
							{
								text += " | ERROR: REQUEST F8 51 52";
								break;
							}
							double value207 = (double)((array3[1] << 8) + array3[3]) * 0.0625;
							text2 = Math.Round(value207, 3).ToString("0.000");
							text3 = "V";
						}
						break;
					case 82:
						text = "BATTERY VOLTAGE | ERROR: REQUEST F8 51 52";
						break;
					case 83:
						text = "CALCULATED FUEL";
						if (array2.Length >= 5)
						{
							if (array3[2] != array3[0] + 1)
							{
								text += " | ERROR: REQUEST F8 53 54";
								break;
							}
							double value203 = (double)((array3[1] << 8) + array3[3]) * 0.001953155;
							text2 = Math.Round(value203, 3).ToString("0.000");
							text3 = "PERCENT";
						}
						break;
					case 84:
						text = "CALCULATED FUEL | ERROR: REQUEST F8 53 54";
						break;
					case 85:
						text = "CALCULATED TIMING";
						if (array2.Length >= 5)
						{
							if (array3[2] != array3[0] + 1)
							{
								text += " | ERROR: REQUEST F8 55 56";
								break;
							}
							double value202 = (double)((array3[1] << 8) + array3[3]) * 0.1176475 - 20.0;
							text2 = Math.Round(value202, 3).ToString("0.000");
							text3 = "DEG";
						}
						break;
					case 86:
						text = "CALCULATED TIMING | ERROR: REQUEST F8 55 56";
						break;
					case 87:
						text = "REGULATOR VALVE CURRENT";
						if (array2.Length >= 5)
						{
							if (array3[2] != array3[0] + 1)
							{
								text += " | ERROR: REQUEST F8 57 58";
								break;
							}
							double value199 = (double)((array3[1] << 8) + array3[3]) * 1.220721752;
							text2 = Math.Round(value199, 3).ToString("0.000");
							text3 = "MILLIAMPS";
						}
						break;
					case 88:
						text = "REGULATOR VALVE CURRENT | ERROR: REQUEST F8 57 58";
						break;
					case 89:
					{
						text = "INJECTOR PUMP FUEL TEMPERATURE";
						if (array2.Length < 5)
						{
							break;
						}
						if (array3[2] != array3[0] + 1)
						{
							text += " | ERROR: REQUEST F8 59 5A";
							break;
						}
						double num113 = (double)((array3[1] << 8) + array3[3]) * 0.0625;
						double value195 = (num113 - 32.0) / 1.8;
						if (Settings.Default.Units == "imperial")
						{
							text2 = Math.Round(num113, 1).ToString("0.0");
							text3 = "°F";
						}
						else if (Settings.Default.Units == "metric")
						{
							text2 = Math.Round(value195, 1).ToString("0.0");
							text3 = "°C";
						}
						break;
					}
					case 90:
						text = "INJECTOR PUMP FUEL TEMPERATURE | ERROR: REQUEST F8 59 5A";
						break;
					case 91:
						text = "INJECTOR PUMP ENGINE SPEED";
						if (array2.Length >= 5)
						{
							if (array3[2] != array3[0] + 1)
							{
								text += " | ERROR: REQUEST F8 5B 5C";
								break;
							}
							double value193 = (double)((array3[1] << 8) + array3[3]) * 0.125;
							text2 = Math.Round(value193, 3).ToString("0.000");
							text3 = "RPM";
						}
						break;
					case 92:
						text = "INJECTOR PUMP ENGINE SPEED | ERROR: REQUEST F8 5B 5C";
						break;
					case 95:
						text = "FREEZE FRM DTC 2:";
						if (array2.Length >= 5)
						{
							if (array3[2] != array3[0] + 1)
							{
								text += " | ERROR: REQUEST F8 5F 60";
								break;
							}
							if (array3[1] == 0 && array3[3] == 0)
							{
								text += "EMPTY SLOT";
								break;
							}
							text2 = "OBD2 P" + Util.ByteToHexString(new byte[2]
							{
								array3[1],
								array3[3]
							}, 0, 2).Replace(" ", "");
						}
						break;
					case 96:
						text = "FREEZE FRAME DTC 2: | ERROR: REQUEST F8 5F 60";
						break;
					default:
					{
						for (int num109 = 0; num109 < num23; num109++)
						{
							list3.Add(array3[num109 * 2]);
							list4.Add(array3[num109 * 2 + 1]);
						}
						text = "F8 RAM TABLE | OFFSET: " + Util.ByteToHexStringSimple(list3.ToArray());
						text2 = Util.ByteToHexStringSimple(list4.ToArray());
						break;
					}
					}
				}
				else if (Year >= 2003 && CumminsSelected)
				{
					switch (array3[0])
					{
					case 6:
					{
						text = "VEHICLE SPEED";
						if (array2.Length < 5)
						{
							break;
						}
						if (array3[2] != array3[0] + 1)
						{
							text += " | ERROR: REQUEST F8 06 07";
							break;
						}
						double num125 = (double)((array3[1] << 8) + array3[3]) * (1.0 / 256.0);
						double value220 = num125 * 1.609344;
						if (Settings.Default.Units == "imperial")
						{
							text2 = Math.Round(num125, 3).ToString("0.000");
							text3 = "MPH";
						}
						else if (Settings.Default.Units == "metric")
						{
							text2 = Math.Round(value220, 3).ToString("0.000");
							text3 = "KM/H";
						}
						break;
					}
					case 7:
						text = "VEHICLE SPEED | ERROR: REQUEST F8 06 07";
						break;
					case 8:
						text = "ENGINE SPEED";
						if (array2.Length >= 5)
						{
							if (array3[2] != array3[0] + 1)
							{
								text += " | ERROR: REQUEST F8 08 09";
								break;
							}
							double value240 = (double)((array3[1] << 8) + array3[3]) * 0.125;
							text2 = Math.Round(value240, 3).ToString("0.000");
							text3 = "RPM";
						}
						break;
					case 9:
						text = "ENGINE SPEED | ERROR: REQUEST F8 08 09";
						break;
					case 10:
					{
						text = "SWITCH STATUS 1";
						List<string> list26 = new List<string>();
						if (Util.IsBitSet(array3[1], 7))
						{
							list26.Add("-7-");
						}
						if (Util.IsBitSet(array3[1], 6))
						{
							list26.Add("BRAKE");
						}
						if (Util.IsBitSet(array3[1], 5))
						{
							list26.Add("-5-");
						}
						if (Util.IsBitSet(array3[1], 4))
						{
							list26.Add("-4-");
						}
						if (Util.IsBitSet(array3[1], 3))
						{
							list26.Add("-3-");
						}
						if (Util.IsBitSet(array3[1], 2))
						{
							list26.Add("-2-");
						}
						if (Util.IsBitSet(array3[1], 1))
						{
							list26.Add("-1-");
						}
						if (Util.IsBitSet(array3[1], 0))
						{
							list26.Add("-0-");
						}
						if (list26.Count == 0)
						{
							break;
						}
						text += ": ";
						foreach (string item9 in list26)
						{
							text = text + item9 + " | ";
						}
						if (text.Length > 2)
						{
							text = text.Remove(text.Length - 3);
						}
						break;
					}
					case 11:
					{
						text = "SWITCH STATUS 2";
						List<string> list23 = new List<string>();
						if (Util.IsBitSet(array3[1], 7))
						{
							list23.Add("-7-");
						}
						if (Util.IsBitSet(array3[1], 6))
						{
							list23.Add("BRAKE");
						}
						if (Util.IsBitSet(array3[1], 5))
						{
							list23.Add("IDLE");
						}
						if (Util.IsBitSet(array3[1], 4))
						{
							list23.Add("-4-");
						}
						if (Util.IsBitSet(array3[1], 3))
						{
							list23.Add("-3-");
						}
						if (Util.IsBitSet(array3[1], 2))
						{
							list23.Add("NOT-IDLE");
						}
						if (Util.IsBitSet(array3[1], 1))
						{
							list23.Add("-1-");
						}
						if (Util.IsBitSet(array3[1], 0))
						{
							list23.Add("-0-");
						}
						if (list23.Count == 0)
						{
							break;
						}
						text += ": ";
						foreach (string item10 in list23)
						{
							text = text + item10 + " | ";
						}
						if (text.Length > 2)
						{
							text = text.Remove(text.Length - 3);
						}
						break;
					}
					case 12:
						text = "ENGINE LOAD";
						if (array2.Length >= 5)
						{
							if (array3[2] != array3[0] + 1)
							{
								text += " | ERROR: REQUEST F8 0C 0D";
								break;
							}
							double value238 = (double)((array3[1] << 8) + array3[3]) * (1.0 / 256.0);
							text2 = Math.Round(value238, 1).ToString("0.0");
							text3 = "PERCENT";
						}
						break;
					case 13:
						text = "ENGINE LOAD | ERROR: REQUEST F8 0C 0D";
						break;
					case 14:
						text = "APP SENSOR PERCENT";
						if (array2.Length >= 5)
						{
							if (array3[2] != array3[0] + 1)
							{
								text += " | ERROR: REQUEST F8 0E 0F";
								break;
							}
							double value225 = (double)((array3[1] << 8) + array3[3]) * 0.25;
							text2 = Math.Round(value225, 1).ToString("0.0");
							text3 = "PERCENT";
						}
						break;
					case 15:
						text = "APP SENSOR PERCENT | ERROR: REQUEST F8 0E 0F";
						break;
					case 16:
					{
						text = "BOOST PRESSURE";
						if (array2.Length < 5)
						{
							break;
						}
						if (array3[2] != array3[0] + 1)
						{
							text += " | ERROR: REQUEST F8 10 11";
							break;
						}
						double num128 = (double)((array3[1] << 8) + array3[3]) * (1.0 / 128.0);
						double value236 = num128 * 6.894757;
						if (Settings.Default.Units == "imperial")
						{
							text2 = Math.Round(num128, 3).ToString("0.000");
							text3 = "PSI";
						}
						else if (Settings.Default.Units == "metric")
						{
							text2 = Math.Round(value236, 3).ToString("0.000");
							text3 = "KPA";
						}
						break;
					}
					case 17:
						text = "BOOST PRESSURE | ERROR: REQUEST F8 10 11";
						break;
					case 18:
					{
						text = "ENGINE COOLANT TEMPERATURE";
						if (array2.Length < 5)
						{
							break;
						}
						if (array3[2] != array3[0] + 1)
						{
							text += " | ERROR: REQUEST F8 12 13";
							break;
						}
						double num123 = (double)((array3[1] << 8) + array3[3]) * (1.0 / 64.0);
						double value217 = (num123 - 32.0) / 1.8;
						if (Settings.Default.Units == "imperial")
						{
							text2 = Math.Round(num123, 1).ToString("0.0");
							text3 = "°F";
						}
						else if (Settings.Default.Units == "metric")
						{
							text2 = Math.Round(value217, 1).ToString("0.0");
							text3 = "°C";
						}
						break;
					}
					case 19:
						text = "ENGINE COOLANT TEMPERATURE | ERROR: REQUEST F8 12 13";
						break;
					case 20:
					{
						text = "INTAKE AIR TEMPERATURE";
						if (array2.Length < 5)
						{
							break;
						}
						if (array3[2] != array3[0] + 1)
						{
							text += " | ERROR: REQUEST F8 14 15";
							break;
						}
						double num127 = (double)((array3[1] << 8) + array3[3]) * (1.0 / 64.0);
						double value233 = (num127 - 32.0) / 1.8;
						if (Settings.Default.Units == "imperial")
						{
							text2 = Math.Round(num127, 1).ToString("0.0");
							text3 = "°F";
						}
						else if (Settings.Default.Units == "metric")
						{
							text2 = Math.Round(value233, 1).ToString("0.0");
							text3 = "°C";
						}
						break;
					}
					case 21:
						text = "INTAKE AIR TEMPERATURE | ERROR: REQUEST F8 14 15";
						break;
					case 25:
					{
						text = "SWITCH STATUS 3";
						List<string> list22 = new List<string>();
						if (Util.IsBitSet(array3[1], 7))
						{
							list22.Add("-7-");
						}
						if (Util.IsBitSet(array3[1], 6))
						{
							list22.Add("-6-");
						}
						if (Util.IsBitSet(array3[1], 5))
						{
							list22.Add("-5-");
						}
						if (Util.IsBitSet(array3[1], 4))
						{
							list22.Add("-4-");
						}
						if (Util.IsBitSet(array3[1], 3))
						{
							list22.Add("INTHEAT2");
						}
						if (Util.IsBitSet(array3[1], 2))
						{
							list22.Add("INTHEAT1");
						}
						if (Util.IsBitSet(array3[1], 1))
						{
							list22.Add("TRFPMPDR");
						}
						if (Util.IsBitSet(array3[1], 0))
						{
							list22.Add("-0-");
						}
						if (list22.Count == 0)
						{
							break;
						}
						text += ": ";
						foreach (string item11 in list22)
						{
							text = text + item11 + " | ";
						}
						if (text.Length > 2)
						{
							text = text.Remove(text.Length - 3);
						}
						break;
					}
					case 30:
						text = "FINAL FUEL STATE";
						switch (array3[1])
						{
						case 0:
							text2 = "NOT SET";
							break;
						case 1:
							text2 = "JCOM TORQUE";
							break;
						case 2:
							text2 = "JCOM SPEED";
							break;
						case 3:
							text2 = "PROGRSV SHIFT";
							break;
						case 4:
							text2 = "PTO";
							break;
						case 5:
							text2 = "USER COMMAND";
							break;
						case 6:
							text2 = "LIMP HOME";
							break;
						case 7:
							text2 = "ASG THROTTLE";
							break;
						case 8:
							text2 = "4-D FUELING";
							break;
						case 9:
							text2 = "CRUISE CONTROL";
							break;
						case 10:
							text2 = "ROAD SPEED GOV";
							break;
						case 11:
							text2 = "LOW SPEED GOV";
							break;
						case 12:
							text2 = "HIGH SPEED GOV";
							break;
						case 13:
							text2 = "TORQUE DERATE OVERRIDE";
							break;
						case 14:
							text2 = "LOW GEAR";
							break;
						case 15:
							text2 = "ALTITUDE DERATE";
							break;
						case 16:
							text2 = "AFC DERATE";
							break;
						case 17:
							text2 = "ANC DERATE";
							break;
						case 18:
							text2 = "ENGINE PROTECT";
							break;
						case 19:
							text2 = "TORQUE CRV LIMIT";
							break;
						case 20:
							text2 = "JCOM TORQUE DERATE";
							break;
						case 21:
							text2 = "OUT OF GEAR";
							break;
						case 22:
							text2 = "CRANKING";
							break;
						case 23:
							text2 = "USER OVERRIDE";
							break;
						case 24:
							text2 = "ENGINE BRAKE";
							break;
						case 25:
							text2 = "ENGINE OVERSPEED";
							break;
						case 26:
							text2 = "ENGINE STOPPED";
							break;
						case 27:
							text2 = "SHUTDOWN";
							break;
						case 28:
							text2 = "FUEL DTC DERATE";
							break;
						case 29:
							text2 = "ENGINE PROTECT";
							break;
						case 30:
							text2 = "ALL SPD GOV APP";
							break;
						case 31:
							text2 = "ALT TORQUE";
							break;
						case 32:
							text2 = "MASTER/SLAVE OVERRIDE";
							break;
						case 33:
							text2 = "STARTUP OIL LIMIT";
							break;
						case 34:
							text2 = "PTO DERATE";
							break;
						case 35:
							text2 = "TORQUE CONTROL";
							break;
						case 36:
							text2 = "POWERTRAIN PROTECT";
							break;
						case 37:
							text2 = "T2 SPEED";
							break;
						case 38:
							text2 = "T2 TORQUE DERATE";
							break;
						case 39:
							text2 = "T2 DERATE";
							break;
						case 40:
							text2 = "NO DERATE";
							break;
						case 41:
							text2 = "ANTI THEFT DERATE";
							break;
						case 42:
							text2 = "PART THROTTLE LIMIT";
							break;
						case 43:
							text2 = "STEADY-STATE AMB DERATE";
							break;
						case 44:
							text2 = "TRSNT COOLANT DERATE";
							break;
						}
						break;
					case 31:
						text = "BATTERY VOLTAGE";
						if (array2.Length >= 5)
						{
							if (array3[2] != array3[0] + 1)
							{
								text += " | ERROR: REQUEST F8 1F 20";
								break;
							}
							double value242 = (double)((array3[1] << 8) + array3[3]) * 0.0625;
							text2 = Math.Round(value242, 3).ToString("0.000");
							text3 = "V";
						}
						break;
					case 32:
						text = "BATTERY VOLTAGE | ERROR: REQUEST F8 1F 20";
						break;
					case 33:
						text = "CALCULATED FUEL";
						if (array2.Length >= 5)
						{
							if (array3[2] != array3[0] + 1)
							{
								text += " | ERROR: REQUEST F8 21 22";
								break;
							}
							double value239 = (double)((array3[1] << 8) + array3[3]) * 0.001953155;
							text2 = Math.Round(value239, 3).ToString("0.000");
							text3 = "PERCENT";
						}
						break;
					case 34:
						text = "CALCULATED FUEL | ERROR: REQUEST F8 21 22";
						break;
					case 35:
						text = "CALCULATED TIMING";
						if (array2.Length >= 5)
						{
							if (array3[2] != array3[0] + 1)
							{
								text += " | ERROR: REQUEST F8 23 24";
								break;
							}
							double value235 = (double)((array3[1] << 8) + array3[3]) * 0.1176475 - 20.0;
							text2 = Math.Round(value235, 3).ToString("0.000");
							text3 = "DEG";
						}
						break;
					case 36:
						text = "CALCULATED TIMING | ERROR: REQUEST F8 23 24";
						break;
					case 37:
						text = "REGULATOR VALVE CURRENT";
						if (array2.Length >= 5)
						{
							if (array3[2] != array3[0] + 1)
							{
								text += " | ERROR: REQUEST F8 25 26";
								break;
							}
							double value230 = (double)((array3[1] << 8) + array3[3]) * 1.220721752;
							text2 = Math.Round(value230, 3).ToString("0.000");
							text3 = "MILLIAMPS";
						}
						break;
					case 38:
						text = "REGULATOR VALVE CURRENT | ERROR: REQUEST F8 25 26";
						break;
					case 39:
						text = "DEFECT STATUS";
						switch (array3[1])
						{
						case 0:
							text2 = "OK";
							break;
						case 1:
							text2 = "CURRENT HIGH";
							break;
						case 2:
							text2 = "CURRENT LOW";
							break;
						}
						break;
					case 40:
						text = "FUEL PRESSURE STATUS";
						switch (array3[1])
						{
						case 0:
							text2 = "OK";
							break;
						case 1:
							text2 = "TOO HIGH";
							break;
						case 2:
							text2 = "LIMIT";
							break;
						case 4:
							text2 = "TOO LOW";
							break;
						case 8:
							text2 = "NEG DEV";
							break;
						case 16:
							text2 = "POS DEV";
							break;
						case 32:
							text2 = "LK MON";
							break;
						case 64:
							text2 = "LK IDLE";
							break;
						}
						break;
					case 41:
						text = "FUEL PRESSURE VOLTS";
						if (array2.Length >= 5)
						{
							if (array3[2] != array3[0] + 1)
							{
								text += " | ERROR: REQUEST F8 29 2A";
								break;
							}
							double value226 = (double)((array3[1] << 8) + array3[3]) * 0.0049;
							text2 = Math.Round(value226, 3).ToString("0.000");
							text3 = "V";
						}
						break;
					case 42:
						text = "FUEL PRESSURE VOLTS | ERROR: REQUEST F8 29 2A";
						break;
					case 45:
						text = "FUEL LEVEL PERCENT";
						if (array2.Length >= 5)
						{
							if (array3[2] != array3[0] + 1)
							{
								text += " | ERROR: REQUEST F8 2D 2E";
								break;
							}
							double value219 = (double)((array3[1] << 8) + array3[3]) * 0.3921568627;
							text2 = Math.Round(value219, 1).ToString("0.0");
							text3 = "PERCENT";
						}
						break;
					case 46:
						text = "FUEL LEVEL PERCENT | ERROR: REQUEST F8 2D 2E";
						break;
					case 49:
						text = "FREEZE FRM DTC 1:";
						if (array2.Length >= 5)
						{
							if (array3[2] != array3[0] + 1)
							{
								text += " | ERROR: REQUEST F8 31 32";
								break;
							}
							if (array3[1] == 0 && array3[3] == 0)
							{
								text += "EMPTY SLOT";
								break;
							}
							text2 = "OBD2 P" + Util.ByteToHexString(new byte[2]
							{
								array3[1],
								array3[3]
							}, 0, 2).Replace(" ", "");
						}
						break;
					case 50:
						text = "FREEZE FRAME DTC 1: | ERROR: REQUEST F8 31 32";
						break;
					case 56:
					{
						text = "VEHICLE SPEED";
						if (array2.Length < 5)
						{
							break;
						}
						if (array3[2] != array3[0] + 1)
						{
							text += " | ERROR: REQUEST F8 38 39";
							break;
						}
						double num129 = (double)((array3[1] << 8) + array3[3]) * (1.0 / 256.0);
						double value237 = num129 * 1.609344;
						if (Settings.Default.Units == "imperial")
						{
							text2 = Math.Round(num129, 3).ToString("0.000");
							text3 = "MPH";
						}
						else if (Settings.Default.Units == "metric")
						{
							text2 = Math.Round(value237, 3).ToString("0.000");
							text3 = "KM/H";
						}
						break;
					}
					case 57:
						text = "VEHICLE SPEED | ERROR: REQUEST F8 38 39";
						break;
					case 58:
						text = "ENGINE SPEED";
						if (array2.Length >= 5)
						{
							if (array3[2] != array3[0] + 1)
							{
								text += " | ERROR: REQUEST F8 3A 3B";
								break;
							}
							double value234 = (double)((array3[1] << 8) + array3[3]) * 0.125;
							text2 = Math.Round(value234, 3).ToString("0.000");
							text3 = "RPM";
						}
						break;
					case 59:
						text = "ENGINE SPEED | ERROR: REQUEST F8 3A 3B";
						break;
					case 60:
					{
						text = "SWITCH STATUS 1";
						List<string> list21 = new List<string>();
						if (Util.IsBitSet(array3[1], 7))
						{
							list21.Add("-7-");
						}
						if (Util.IsBitSet(array3[1], 6))
						{
							list21.Add("BRAKE");
						}
						if (Util.IsBitSet(array3[1], 5))
						{
							list21.Add("-5-");
						}
						if (Util.IsBitSet(array3[1], 4))
						{
							list21.Add("-4-");
						}
						if (Util.IsBitSet(array3[1], 3))
						{
							list21.Add("-3-");
						}
						if (Util.IsBitSet(array3[1], 2))
						{
							list21.Add("-2-");
						}
						if (Util.IsBitSet(array3[1], 1))
						{
							list21.Add("-1-");
						}
						if (Util.IsBitSet(array3[1], 0))
						{
							list21.Add("-0-");
						}
						if (list21.Count == 0)
						{
							break;
						}
						text += ": ";
						foreach (string item12 in list21)
						{
							text = text + item12 + " | ";
						}
						if (text.Length > 2)
						{
							text = text.Remove(text.Length - 3);
						}
						break;
					}
					case 61:
					{
						text = "SWITCH STATUS 2";
						List<string> list25 = new List<string>();
						if (Util.IsBitSet(array3[1], 7))
						{
							list25.Add("-7-");
						}
						if (Util.IsBitSet(array3[1], 6))
						{
							list25.Add("BRAKE");
						}
						if (Util.IsBitSet(array3[1], 5))
						{
							list25.Add("IDLE");
						}
						if (Util.IsBitSet(array3[1], 4))
						{
							list25.Add("-4-");
						}
						if (Util.IsBitSet(array3[1], 3))
						{
							list25.Add("-3-");
						}
						if (Util.IsBitSet(array3[1], 2))
						{
							list25.Add("NOT-IDLE");
						}
						if (Util.IsBitSet(array3[1], 1))
						{
							list25.Add("-1-");
						}
						if (Util.IsBitSet(array3[1], 0))
						{
							list25.Add("-0-");
						}
						if (list25.Count == 0)
						{
							break;
						}
						text += ": ";
						foreach (string item13 in list25)
						{
							text = text + item13 + " | ";
						}
						if (text.Length > 2)
						{
							text = text.Remove(text.Length - 3);
						}
						break;
					}
					case 62:
						text = "ENGINE LOAD";
						if (array2.Length >= 5)
						{
							if (array3[2] != array3[0] + 1)
							{
								text += " | ERROR: REQUEST F8 3E 3F";
								break;
							}
							double value232 = (double)((array3[1] << 8) + array3[3]) * (1.0 / 256.0);
							text2 = Math.Round(value232, 1).ToString("0.0");
							text3 = "PERCENT";
						}
						break;
					case 63:
						text = "ENGINE LOAD | ERROR: REQUEST F8 3E 3F";
						break;
					case 64:
						text = "APP SENSOR PERCENT";
						if (array2.Length >= 5)
						{
							if (array3[2] != array3[0] + 1)
							{
								text += " | ERROR: REQUEST F8 40 41";
								break;
							}
							double value229 = (double)((array3[1] << 8) + array3[3]) * 0.25;
							text2 = Math.Round(value229, 1).ToString("0.0");
							text3 = "PERCENT";
						}
						break;
					case 65:
						text = "APP SENSOR PERCENT | ERROR: REQUEST F8 40 41";
						break;
					case 66:
					{
						text = "BOOST PRESSURE";
						if (array2.Length < 5)
						{
							break;
						}
						if (array3[2] != array3[0] + 1)
						{
							text += " | ERROR: REQUEST F8 42 43";
							break;
						}
						double num126 = (double)((array3[1] << 8) + array3[3]) * (1.0 / 128.0);
						double value223 = num126 * 6.894757;
						if (Settings.Default.Units == "imperial")
						{
							text2 = Math.Round(num126, 3).ToString("0.000");
							text3 = "PSI";
						}
						else if (Settings.Default.Units == "metric")
						{
							text2 = Math.Round(value223, 3).ToString("0.000");
							text3 = "KPA";
						}
						break;
					}
					case 67:
						text = "BOOST PRESSURE | ERROR: REQUEST F8 42 43";
						break;
					case 68:
					{
						text = "ENGINE COOLANT TEMPERATURE";
						if (array2.Length < 5)
						{
							break;
						}
						if (array3[2] != array3[0] + 1)
						{
							text += " | ERROR: REQUEST F8 44 45";
							break;
						}
						double num124 = (double)((array3[1] << 8) + array3[3]) * (1.0 / 64.0);
						double value218 = (num124 - 32.0) / 1.8;
						if (Settings.Default.Units == "imperial")
						{
							text2 = Math.Round(num124, 1).ToString("0.0");
							text3 = "°F";
						}
						else if (Settings.Default.Units == "metric")
						{
							text2 = Math.Round(value218, 1).ToString("0.0");
							text3 = "°C";
						}
						break;
					}
					case 69:
						text = "ENGINE COOLANT TEMPERATURE | ERROR: REQUEST F8 44 45";
						break;
					case 70:
					{
						text = "INTAKE AIR TEMPERATURE";
						if (array2.Length < 5)
						{
							break;
						}
						if (array3[2] != array3[0] + 1)
						{
							text += " | ERROR: REQUEST F8 46 47";
							break;
						}
						double num130 = (double)((array3[1] << 8) + array3[3]) * (1.0 / 64.0);
						double value241 = (num130 - 32.0) / 1.8;
						if (Settings.Default.Units == "imperial")
						{
							text2 = Math.Round(num130, 1).ToString("0.0");
							text3 = "°F";
						}
						else if (Settings.Default.Units == "metric")
						{
							text2 = Math.Round(value241, 1).ToString("0.0");
							text3 = "°C";
						}
						break;
					}
					case 71:
						text = "INTAKE AIR TEMPERATURE | ERROR: REQUEST F8 46 47";
						break;
					case 75:
					{
						text = "SWITCH STATUS 3";
						List<string> list24 = new List<string>();
						if (Util.IsBitSet(array3[1], 7))
						{
							list24.Add("-7-");
						}
						if (Util.IsBitSet(array3[1], 6))
						{
							list24.Add("-6-");
						}
						if (Util.IsBitSet(array3[1], 5))
						{
							list24.Add("-5-");
						}
						if (Util.IsBitSet(array3[1], 4))
						{
							list24.Add("-4-");
						}
						if (Util.IsBitSet(array3[1], 3))
						{
							list24.Add("INTHEAT2");
						}
						if (Util.IsBitSet(array3[1], 2))
						{
							list24.Add("INTHEAT1");
						}
						if (Util.IsBitSet(array3[1], 1))
						{
							list24.Add("TRFPMPDR");
						}
						if (Util.IsBitSet(array3[1], 0))
						{
							list24.Add("-0-");
						}
						if (list24.Count == 0)
						{
							break;
						}
						text += ": ";
						foreach (string item14 in list24)
						{
							text = text + item14 + " | ";
						}
						if (text.Length > 2)
						{
							text = text.Remove(text.Length - 3);
						}
						break;
					}
					case 80:
						text = "FINAL FUEL STATE";
						switch (array3[1])
						{
						case 0:
							text2 = "NOT SET";
							break;
						case 1:
							text2 = "JCOM TORQUE";
							break;
						case 2:
							text2 = "JCOM SPEED";
							break;
						case 3:
							text2 = "PROGRSV SHIFT";
							break;
						case 4:
							text2 = "PTO";
							break;
						case 5:
							text2 = "USER COMMAND";
							break;
						case 6:
							text2 = "LIMP HOME";
							break;
						case 7:
							text2 = "ASG THROTTLE";
							break;
						case 8:
							text2 = "4-D FUELING";
							break;
						case 9:
							text2 = "CRUISE CONTROL";
							break;
						case 10:
							text2 = "ROAD SPEED GOV";
							break;
						case 11:
							text2 = "LOW SPEED GOV";
							break;
						case 12:
							text2 = "HIGH SPEED GOV";
							break;
						case 13:
							text2 = "TORQUE DERATE OVERRIDE";
							break;
						case 14:
							text2 = "LOW GEAR";
							break;
						case 15:
							text2 = "ALTITUDE DERATE";
							break;
						case 16:
							text2 = "AFC DERATE";
							break;
						case 17:
							text2 = "ANC DERATE";
							break;
						case 18:
							text2 = "ENGINE PROTECT";
							break;
						case 19:
							text2 = "TORQUE CRV LIMIT";
							break;
						case 20:
							text2 = "JCOM TORQUE DERATE";
							break;
						case 21:
							text2 = "OUT OF GEAR";
							break;
						case 22:
							text2 = "CRANKING";
							break;
						case 23:
							text2 = "USER OVERRIDE";
							break;
						case 24:
							text2 = "ENGINE BRAKE";
							break;
						case 25:
							text2 = "ENGINE OVERSPEED";
							break;
						case 26:
							text2 = "ENGINE STOPPED";
							break;
						case 27:
							text2 = "SHUTDOWN";
							break;
						case 28:
							text2 = "FUEL DTC DERATE";
							break;
						case 29:
							text2 = "ENGINE PROTECT";
							break;
						case 30:
							text2 = "ALL SPD GOV APP";
							break;
						case 31:
							text2 = "ALT TORQUE";
							break;
						case 32:
							text2 = "MASTER/SLAVE OVERRIDE";
							break;
						case 33:
							text2 = "STARTUP OIL LIMIT";
							break;
						case 34:
							text2 = "PTO DERATE";
							break;
						case 35:
							text2 = "TORQUE CONTROL";
							break;
						case 36:
							text2 = "POWERTRAIN PROTECT";
							break;
						case 37:
							text2 = "T2 SPEED";
							break;
						case 38:
							text2 = "T2 TORQUE DERATE";
							break;
						case 39:
							text2 = "T2 DERATE";
							break;
						case 40:
							text2 = "NO DERATE";
							break;
						case 41:
							text2 = "ANTI THEFT DERATE";
							break;
						case 42:
							text2 = "PART THROTTLE LIMIT";
							break;
						case 43:
							text2 = "STEADY-STATE AMB DERATE";
							break;
						case 44:
							text2 = "TRSNT COOLANT DERATE";
							break;
						}
						break;
					case 81:
						text = "BATTERY VOLTAGE";
						if (array2.Length >= 5)
						{
							if (array3[2] != array3[0] + 1)
							{
								text += " | ERROR: REQUEST F8 51 52";
								break;
							}
							double value231 = (double)((array3[1] << 8) + array3[3]) * 0.0625;
							text2 = Math.Round(value231, 3).ToString("0.000");
							text3 = "V";
						}
						break;
					case 82:
						text = "BATTERY VOLTAGE | ERROR: REQUEST F8 51 52";
						break;
					case 83:
						text = "CALCULATED FUEL";
						if (array2.Length >= 5)
						{
							if (array3[2] != array3[0] + 1)
							{
								text += " | ERROR: REQUEST F8 53 54";
								break;
							}
							double value228 = (double)((array3[1] << 8) + array3[3]) * 0.001953155;
							text2 = Math.Round(value228, 3).ToString("0.000");
							text3 = "PERCENT";
						}
						break;
					case 84:
						text = "CALCULATED FUEL | ERROR: REQUEST F8 53 54";
						break;
					case 85:
						text = "CALCULATED TIMING";
						if (array2.Length >= 5)
						{
							if (array3[2] != array3[0] + 1)
							{
								text += " | ERROR: REQUEST F8 55 56";
								break;
							}
							double value227 = (double)((array3[1] << 8) + array3[3]) * 0.1176475 - 20.0;
							text2 = Math.Round(value227, 3).ToString("0.000");
							text3 = "DEG";
						}
						break;
					case 86:
						text = "CALCULATED TIMING | ERROR: REQUEST F8 55 56";
						break;
					case 87:
						text = "REGULATOR VALVE CURRENT";
						if (array2.Length >= 5)
						{
							if (array3[2] != array3[0] + 1)
							{
								text += " | ERROR: REQUEST F8 57 58";
								break;
							}
							double value224 = (double)((array3[1] << 8) + array3[3]) * 1.220721752;
							text2 = Math.Round(value224, 3).ToString("0.000");
							text3 = "MILLIAMPS";
						}
						break;
					case 88:
						text = "REGULATOR VALVE CURRENT | ERROR: REQUEST F8 57 58";
						break;
					case 89:
						text = "DEFECT STATUS";
						switch (array3[1])
						{
						case 0:
							text2 = "OK";
							break;
						case 1:
							text2 = "CURRENT HIGH";
							break;
						case 2:
							text2 = "CURRENT LOW";
							break;
						}
						break;
					case 90:
						text = "FUEL PRESSURE STATUS";
						switch (array3[1])
						{
						case 0:
							text2 = "OK";
							break;
						case 1:
							text2 = "TOO HIGH";
							break;
						case 2:
							text2 = "LIMIT";
							break;
						case 4:
							text2 = "TOO LOW";
							break;
						case 8:
							text2 = "NEG DEV";
							break;
						case 16:
							text2 = "POS DEV";
							break;
						case 32:
							text2 = "LK MON";
							break;
						case 64:
							text2 = "LK IDLE";
							break;
						}
						break;
					case 91:
						text = "FUEL PRESSURE VOLTS";
						if (array2.Length >= 5)
						{
							if (array3[2] != array3[0] + 1)
							{
								text += " | ERROR: REQUEST F8 5B 5C";
								break;
							}
							double value222 = (double)((array3[1] << 8) + array3[3]) * 0.0049;
							text2 = Math.Round(value222, 3).ToString("0.000");
							text3 = "V";
						}
						break;
					case 92:
						text = "FUEL PRESSURE VOLTS | ERROR: REQUEST F8 5B 5C";
						break;
					case 95:
						text = "FUEL LEVEL PERCENT";
						if (array2.Length >= 5)
						{
							if (array3[2] != array3[0] + 1)
							{
								text += " | ERROR: REQUEST F8 5F 60";
								break;
							}
							double value221 = (double)((array3[1] << 8) + array3[3]) * 0.3921568627;
							text2 = Math.Round(value221, 1).ToString("0.0");
							text3 = "PERCENT";
						}
						break;
					case 96:
						text = "FUEL LEVEL PERCENT | ERROR: REQUEST F8 5F 60";
						break;
					case 99:
						text = "FREEZE FRM DTC 2:";
						if (array2.Length >= 5)
						{
							if (array3[2] != array3[0] + 1)
							{
								text += " | ERROR: REQUEST F8 63 64";
								break;
							}
							if (array3[1] == 0 && array3[3] == 0)
							{
								text += "EMPTY SLOT";
								break;
							}
							text2 = "OBD2 P" + Util.ByteToHexString(new byte[2]
							{
								array3[1],
								array3[3]
							}, 0, 2).Replace(" ", "");
						}
						break;
					case 100:
						text = "FREEZE FRAME DTC 2: | ERROR: REQUEST F8 63 64";
						break;
					default:
					{
						for (int num122 = 0; num122 < num23; num122++)
						{
							list3.Add(array3[num122 * 2]);
							list4.Add(array3[num122 * 2 + 1]);
						}
						text = "F8 RAM TABLE | OFFSET: " + Util.ByteToHexStringSimple(list3.ToArray());
						text2 = Util.ByteToHexStringSimple(list4.ToArray());
						break;
					}
					}
				}
				else
				{
					byte b3 = array3[0];
					for (int num131 = 0; num131 < num23; num131++)
					{
						list3.Add(array3[num131 * 2]);
						list4.Add(array3[num131 * 2 + 1]);
					}
					text = "F8 RAM TABLE | OFFSET: " + Util.ByteToHexStringSimple(list3.ToArray());
					text2 = Util.ByteToHexStringSimple(list4.ToArray());
				}
				break;
			case 249:
				text = "F9 RAM TABLE SELECTED";
				if (array2.Length >= 3)
				{
					byte b3 = array3[0];
					for (int num72 = 0; num72 < num23; num72++)
					{
						list3.Add(array3[num72 * 2]);
						list4.Add(array3[num72 * 2 + 1]);
					}
					text = "F9 RAM TABLE | OFFSET: " + Util.ByteToHexStringSimple(list3.ToArray());
					text2 = Util.ByteToHexStringSimple(list4.ToArray());
				}
				break;
			case 250:
				text = "FA RAM TABLE SELECTED";
				if (array2.Length >= 3)
				{
					byte b3 = array3[0];
					for (int num69 = 0; num69 < num23; num69++)
					{
						list3.Add(array3[num69 * 2]);
						list4.Add(array3[num69 * 2 + 1]);
					}
					text = "FA RAM TABLE | OFFSET: " + Util.ByteToHexStringSimple(list3.ToArray());
					text2 = Util.ByteToHexStringSimple(list4.ToArray());
				}
				break;
			case 251:
				text = "FB RAM TABLE SELECTED";
				if (array2.Length < 3)
				{
					break;
				}
				switch (array3[0])
				{
				case 1:
					text = "ENGINE SPEED";
					if (array2.Length >= 5)
					{
						if (array3[2] != array3[0] + 1)
						{
							text += " | ERROR: REQUEST FB 01 02";
							break;
						}
						double value85 = (double)((array3[1] << 8) + array3[3]) * 0.125;
						text2 = Math.Round(value85, 3).ToString("0.000");
						text3 = "RPM";
					}
					break;
				case 2:
					text = "ENGINE SPEED | ERROR: REQUEST FB 01 02";
					break;
				case 3:
				{
					text = "TRANSMISSION TEMPERATURE";
					if (array2.Length < 5)
					{
						break;
					}
					if (array3[2] != array3[0] + 1)
					{
						text += " | ERROR: REQUEST FB 03 04";
						break;
					}
					double num49 = (double)((array3[1] << 8) + array3[3]) * (1.0 / 64.0);
					double value86 = (num49 - 32.0) / 1.8;
					if (Settings.Default.Units == "imperial")
					{
						text2 = Math.Round(num49, 1).ToString("0.0");
						text3 = "°F";
					}
					else if (Settings.Default.Units == "metric")
					{
						text2 = Math.Round(value86, 1).ToString("0.0");
						text3 = "°C";
					}
					break;
				}
				case 4:
					text = "TRANSMISSION TEMPERATURE | ERROR: REQUEST FB 03 04";
					break;
				case 5:
				{
					text = "VEHICLE SPEED";
					if (array2.Length < 5)
					{
						break;
					}
					if (array3[2] != array3[0] + 1)
					{
						text += " | ERROR: REQUEST FB 05 06";
						break;
					}
					double num51 = (double)((array3[1] << 8) + array3[3]) * (1.0 / 256.0);
					double value91 = num51 * 1.609344;
					if (Settings.Default.Units == "imperial")
					{
						text2 = Math.Round(num51, 3).ToString("0.000");
						text3 = "MPH";
					}
					else if (Settings.Default.Units == "metric")
					{
						text2 = Math.Round(value91, 3).ToString("0.000");
						text3 = "KM/H";
					}
					break;
				}
				case 6:
					text = "VEHICLE SPEED | ERROR: REQUEST FB 05 06";
					break;
				case 7:
					text = "APP SENSOR PERCENT";
					if (array2.Length >= 5)
					{
						if (array3[2] != array3[0] + 1)
						{
							text += " | ERROR: REQUEST FB 07 08";
							break;
						}
						double value94 = (double)((array3[1] << 8) + array3[3]) * 0.25;
						text2 = Math.Round(value94, 1).ToString("0.0");
						text3 = "PERCENT";
					}
					break;
				case 8:
					text = "APP SENSOR PERCENT | ERROR: REQUEST FB 07 08";
					break;
				case 11:
					text = "TRANSMISSION TEMPERATURE SENSOR VOLTS";
					if (array2.Length >= 5)
					{
						if (array3[2] != array3[0] + 1)
						{
							text += " | ERROR: REQUEST FB 0B 0C";
							break;
						}
						double value90 = (double)((array3[1] << 8) + array3[3]) * 0.0049;
						text2 = Math.Round(value90, 3).ToString("0.000");
						text3 = "V";
					}
					break;
				case 12:
					text = "TRANSMISSION TEMP SENSOR VOLTS | ERROR: REQUEST FB 0B 0C";
					break;
				case 15:
				{
					text = "ENGINE COOLANT TEMPERATURE";
					if (array2.Length < 5)
					{
						break;
					}
					if (array3[2] != array3[0] + 1)
					{
						text += " | ERROR: REQUEST FB 0F 10";
						break;
					}
					double num52 = (double)((array3[1] << 8) + array3[3]) * (1.0 / 64.0);
					double value92 = (num52 - 32.0) / 1.8;
					if (Settings.Default.Units == "imperial")
					{
						text2 = Math.Round(num52, 1).ToString("0.0");
						text3 = "°F";
					}
					else if (Settings.Default.Units == "metric")
					{
						text2 = Math.Round(value92, 1).ToString("0.0");
						text3 = "°C";
					}
					break;
				}
				case 16:
					text = "ENGINE COOLANT TEMPERATURE | ERROR: REQUEST FB 0F 10";
					break;
				case 17:
					text = "ENGINE COOLANT TEMPERATURE SENSOR VOLTS";
					if (array2.Length >= 5)
					{
						if (array3[2] != array3[0] + 1)
						{
							text += " | ERROR: REQUEST FB 11 12";
							break;
						}
						double value88 = (double)((array3[1] << 8) + array3[3]) * 0.0049;
						text2 = Math.Round(value88, 3).ToString("0.000");
						text3 = "V";
					}
					break;
				case 18:
					text = "ECT SENSOR VOLTS | ERROR: REQUEST FB 11 12";
					break;
				case 19:
				{
					text = "BOOST PRESSURE";
					if (array2.Length < 5)
					{
						break;
					}
					if (array3[2] != array3[0] + 1)
					{
						text += " | ERROR: REQUEST FB 13 14";
						break;
					}
					double num53 = (double)((array3[1] << 8) + array3[3]) * (1.0 / 128.0);
					double value93 = num53 * 6.894757;
					if (Settings.Default.Units == "imperial")
					{
						text2 = Math.Round(num53, 3).ToString("0.000");
						text3 = "PSI";
					}
					else if (Settings.Default.Units == "metric")
					{
						text2 = Math.Round(value93, 3).ToString("0.000");
						text3 = "KPA";
					}
					break;
				}
				case 20:
					text = "BOOST PRESSURE | ERROR: REQUEST FB 13 14";
					break;
				case 21:
				{
					text = "INTAKE AIR TEMPERATURE";
					if (array2.Length < 5)
					{
						break;
					}
					if (array3[2] != array3[0] + 1)
					{
						text += " | ERROR: REQUEST FB 15 16";
						break;
					}
					double num50 = (double)((array3[1] << 8) + array3[3]) * (1.0 / 64.0);
					double value89 = (num50 - 32.0) / 1.8;
					if (Settings.Default.Units == "imperial")
					{
						text2 = Math.Round(num50, 1).ToString("0.0");
						text3 = "°F";
					}
					else if (Settings.Default.Units == "metric")
					{
						text2 = Math.Round(value89, 1).ToString("0.0");
						text3 = "°C";
					}
					break;
				}
				case 22:
					text = "INTAKE AIR TEMPERATURE | ERROR: REQUEST FB 15 16";
					break;
				case 23:
					text = "INTAKE AIR TEMPERATURE SENSOR VOLTS";
					if (array2.Length >= 5)
					{
						if (array3[2] != array3[0] + 1)
						{
							text += " | ERROR: REQUEST FB 17 18";
							break;
						}
						double value87 = (double)((array3[1] << 8) + array3[3]) * 0.0049;
						text2 = Math.Round(value87, 3).ToString("0.000");
						text3 = "V";
					}
					break;
				case 24:
					text = "IAT SENSOR VOLTS | ERROR: REQUEST FB 17 18";
					break;
				case 25:
					text = "BATTERY VOLTAGE";
					if (array2.Length >= 5)
					{
						if (array3[2] != array3[0] + 1)
						{
							text += " | ERROR: REQUEST FB 19 1A";
							break;
						}
						double value84 = (double)((array3[1] << 8) + array3[3]) * 0.0625;
						text2 = Math.Round(value84, 3).ToString("0.000");
						text3 = "V";
					}
					break;
				case 26:
					text = "BATTERY VOLTAGE | ERROR: REQUEST FB 19 1A";
					break;
				}
				if (Year < 2003 && CumminsSelected)
				{
					switch (array3[1])
					{
					case 27:
						text = "INJECTOR PUMP BATTERY VOLTAGE";
						if (array2.Length >= 5)
						{
							if (array3[2] != array3[0] + 1)
							{
								text += " | ERROR: REQUEST FB 1B 1C";
								break;
							}
							double value101 = (double)((array3[1] << 8) + array3[3]) * 0.0183;
							text2 = Math.Round(value101, 3).ToString("0.000");
							text3 = "V";
						}
						break;
					case 28:
						text = "INJECTOR PUMP BATTERY VOLTAGE | ERROR: REQUEST FB 1B 1C";
						break;
					case 31:
					{
						text = "INJECTOR PUMP FUEL TEMPERATURE";
						if (array2.Length < 5)
						{
							break;
						}
						if (array3[2] != array3[0] + 1)
						{
							text += " | ERROR: REQUEST FB 1F 20";
							break;
						}
						double num54 = (double)((array3[1] << 8) + array3[3]) * 0.0625;
						double value98 = (num54 - 32.0) / 1.8;
						if (Settings.Default.Units == "imperial")
						{
							text2 = Math.Round(num54, 1).ToString("0.0");
							text3 = "°F";
						}
						else if (Settings.Default.Units == "metric")
						{
							text2 = Math.Round(value98, 1).ToString("0.0");
							text3 = "°C";
						}
						break;
					}
					case 32:
						text = "INJECTOR PUMP FUEL TEMPERATURE | ERROR: REQUEST FB 1F 20";
						break;
					case 71:
					{
						text = "SWITCH STATUS";
						List<string> list11 = new List<string>();
						if (Util.IsBitSet(array3[1], 7))
						{
							list11.Add("OPSCLSD");
						}
						else
						{
							list11.Add("OPSOPEN");
						}
						if (Util.IsBitSet(array3[1], 6))
						{
							list11.Add("-6-");
						}
						if (Util.IsBitSet(array3[1], 5))
						{
							list11.Add("ODRLSD");
						}
						else
						{
							list11.Add("ODPRSD");
						}
						if (Util.IsBitSet(array3[1], 4))
						{
							list11.Add("P/N");
						}
						else
						{
							list11.Add("D/R");
						}
						if (Util.IsBitSet(array3[1], 3))
						{
							list11.Add("-3-");
						}
						if (Util.IsBitSet(array3[1], 2))
						{
							list11.Add("-2-");
						}
						if (Util.IsBitSet(array3[1], 1))
						{
							list11.Add("-1-");
						}
						if (Util.IsBitSet(array3[1], 0))
						{
							list11.Add("0-0");
						}
						if (list11.Count == 0)
						{
							break;
						}
						foreach (string item15 in list11)
						{
							text2 = text2 + item15 + " | ";
						}
						if (text2.Length > 2)
						{
							text2 = text2.Remove(text2.Length - 3);
						}
						break;
					}
					case 74:
						text = "FINAL FUEL STATE";
						switch (array3[1])
						{
						case 0:
							text2 = "NOT SET";
							break;
						case 1:
							text2 = "JCOM TORQUE";
							break;
						case 2:
							text2 = "JCOM SPEED";
							break;
						case 3:
							text2 = "PROGRSV SHIFT";
							break;
						case 4:
							text2 = "PTO";
							break;
						case 5:
							text2 = "USER COMMAND";
							break;
						case 6:
							text2 = "LIMP HOME";
							break;
						case 7:
							text2 = "ASG THROTTLE";
							break;
						case 8:
							text2 = "4-D FUELING";
							break;
						case 9:
							text2 = "CRUISE CONTROL";
							break;
						case 10:
							text2 = "ROAD SPEED GOV";
							break;
						case 11:
							text2 = "LOW SPEED GOV";
							break;
						case 12:
							text2 = "HIGH SPEED GOV";
							break;
						case 13:
							text2 = "TORQUE DERATE OVERRIDE";
							break;
						case 14:
							text2 = "LOW GEAR";
							break;
						case 15:
							text2 = "ALTITUDE DERATE";
							break;
						case 16:
							text2 = "AFC DERATE";
							break;
						case 17:
							text2 = "ANC DERATE";
							break;
						case 18:
							text2 = "ENGINE PROTECT";
							break;
						case 19:
							text2 = "TORQUE CRV LIMIT";
							break;
						case 20:
							text2 = "JCOM TORQUE DERATE";
							break;
						case 21:
							text2 = "OUT OF GEAR";
							break;
						case 22:
							text2 = "CRANKING";
							break;
						case 23:
							text2 = "USER OVERRIDE";
							break;
						case 24:
							text2 = "ENGINE BRAKE";
							break;
						case 25:
							text2 = "ENGINE OVERSPEED";
							break;
						case 26:
							text2 = "ENGINE STOPPED";
							break;
						case 27:
							text2 = "SHUTDOWN";
							break;
						case 28:
							text2 = "FUEL DTC DERATE";
							break;
						case 29:
							text2 = "ENGINE PROTECT";
							break;
						case 30:
							text2 = "ALL SPD GOV APP";
							break;
						case 31:
							text2 = "ALT TORQUE";
							break;
						case 32:
							text2 = "MASTER/SLAVE OVERRIDE";
							break;
						case 33:
							text2 = "STARTUP OIL LIMIT";
							break;
						case 34:
							text2 = "PTO DERATE";
							break;
						case 35:
							text2 = "TORQUE CONTROL";
							break;
						case 36:
							text2 = "POWERTRAIN PROTECT";
							break;
						case 37:
							text2 = "T2 SPEED";
							break;
						case 38:
							text2 = "T2 TORQUE DERATE";
							break;
						case 39:
							text2 = "T2 DERATE";
							break;
						case 40:
							text2 = "NO DERATE";
							break;
						case 41:
							text2 = "ANTI THEFT DERATE";
							break;
						case 42:
							text2 = "PART THROTTLE LIMIT";
							break;
						case 43:
							text2 = "STEADY-STATE AMB DERATE";
							break;
						case 44:
							text2 = "TRSNT COOLANT DERATE";
							break;
						}
						break;
					case 81:
						text = "BOOST VOLTS";
						if (array2.Length >= 5)
						{
							if (array3[2] != array3[0] + 1)
							{
								text += " | ERROR: REQUEST FB 51 52";
								break;
							}
							double value95 = (double)((array3[1] << 8) + array3[3]) * 0.0049;
							text2 = Math.Round(value95, 3).ToString("0.000");
							text3 = "V";
						}
						break;
					case 82:
						text = "BOOST VOLTS | ERROR: REQUEST FB 51 52";
						break;
					case 85:
						text = "WATER IN FUEL VOLTS";
						if (array2.Length >= 5)
						{
							if (array3[2] != array3[0] + 1)
							{
								text += " | ERROR: REQUEST FB 55 56";
								break;
							}
							double value103 = (double)((array3[1] << 8) + array3[3]) * 0.0049;
							text2 = Math.Round(value103, 3).ToString("0.000");
							text3 = "V";
						}
						break;
					case 86:
						text = "WATER IN FUEL VOLTS | ERROR: REQUEST FB 55 56";
						break;
					case 87:
						text = "ENGINE LOAD";
						if (array2.Length >= 5)
						{
							if (array3[2] != array3[0] + 1)
							{
								text += " | ERROR: REQUEST FB 57 58";
								break;
							}
							double value99 = (double)((array3[1] << 8) + array3[3]) * (1.0 / 256.0);
							text2 = Math.Round(value99, 1).ToString("0.0");
							text3 = "PERCENT";
						}
						break;
					case 88:
						text = "ENGINE LOAD | ERROR: REQUEST FB 57 58";
						break;
					case 185:
					{
						text = "BATTERY TEMPERATURE";
						if (array2.Length < 5)
						{
							break;
						}
						if (array3[2] != array3[0] + 1)
						{
							text += " | ERROR: REQUEST FB B9 BA";
							break;
						}
						double num56 = (double)((array3[1] << 8) + array3[3]) * 0.0048;
						double value102 = (num56 - 32.0) / 1.8;
						if (Settings.Default.Units == "imperial")
						{
							text2 = Math.Round(num56, 1).ToString("0.0");
							text3 = "°F";
						}
						else if (Settings.Default.Units == "metric")
						{
							text2 = Math.Round(value102, 1).ToString("0.0");
							text3 = "°C";
						}
						break;
					}
					case 186:
						text = "BATTERY TEMPERATURE | ERROR: REQUEST FB B9 BA";
						break;
					case 203:
						text = "KEY-ON COUNTER";
						if (array2.Length >= 5)
						{
							if (array3[2] != array3[0] + 1)
							{
								text += " | ERROR: REQUEST FB CB CC";
								break;
							}
							text2 = ((ushort)((array3[1] << 8) + array3[3])).ToString("0");
							text3 = "COUNTS";
						}
						break;
					case 204:
						text = "KEY-ON COUNTER | ERROR: REQUEST FB CB CC";
						break;
					case 205:
						text = "ENGINE SPEED CKD SENSOR";
						if (array2.Length >= 5)
						{
							if (array3[2] != array3[0] + 1)
							{
								text += " | ERROR: REQUEST FB CD CE";
								break;
							}
							double value97 = (double)((array3[1] << 8) + array3[3]) * 0.125;
							text2 = Math.Round(value97, 3).ToString("0.000");
							text3 = "RPM";
						}
						break;
					case 206:
						text = "ENGINE SPEED CKD SENSOR | ERROR: REQUEST FB CF D0";
						break;
					case 207:
						text = "ENGINE SPEED CMP SENSOR";
						if (array2.Length >= 5)
						{
							if (array3[2] != array3[0] + 1)
							{
								text += " | ERROR: REQUEST FB CD CE";
								break;
							}
							double value105 = (double)((array3[1] << 8) + array3[3]) * 0.125;
							text2 = Math.Round(value105, 3).ToString("0.000");
							text3 = "RPM";
						}
						break;
					case 208:
						text = "ENGINE SPEED CMP SENSOR | ERROR: REQUEST FB CF D0";
						break;
					case 215:
						text = "APP SENSOR VOLTS";
						if (array2.Length >= 5)
						{
							if (array3[2] != array3[0] + 1)
							{
								text += " | ERROR: REQUEST FB D7 D8";
								break;
							}
							double value104 = (double)((array3[1] << 8) + array3[3]) * 0.0049;
							text2 = Math.Round(value104, 3).ToString("0.000");
							text3 = "V";
						}
						break;
					case 216:
						text = "APP SENSOR VOLTS | ERROR: REQUEST FB D7 D8";
						break;
					case 220:
						text = "CRUISE CONTROL | DENIED REASON";
						text2 = array3[1] switch
						{
							2 => "CRUISE SWITCH DTC", 
							3 => "VSS RATIONALITY", 
							4 => "BRAKE RATIONALITY", 
							5 => "ON/OFF SWITCH", 
							6 => "BRAKE SWITCH", 
							7 => "CANCEL SWITCH", 
							8 => "SPEED SENSOR", 
							9 => "RPM LIMIT", 
							10 => "RPM/VSS RATIO", 
							11 => "CLUTCH SWITCH", 
							12 => "P/N SWITCH", 
							_ => "N/A", 
						};
						break;
					case 222:
						text = "CRUISE CONTROL | LAST CUTOUT REASON";
						text2 = array3[1] switch
						{
							2 => "CRUISE SWITCH DTC", 
							3 => "VSS RATIONALITY", 
							4 => "BRAKE RATIONALITY", 
							5 => "ON/OFF SWITCH", 
							6 => "BRAKE SWITCH", 
							7 => "CANCEL SWITCH", 
							8 => "SPEED SENSOR", 
							9 => "RPM LIMIT", 
							10 => "RPM/VSS RATIO", 
							11 => "CLUTCH SWITCH", 
							12 => "P/N SWITCH", 
							_ => "N/A", 
						};
						break;
					case 226:
						text = "CRUISE INDICATOR LAMP";
						text2 = ((!Util.IsBitSet(array3[1], 0)) ? "OFF" : "ON");
						break;
					case 228:
					{
						text = "CRUISE | BUTTON PRESSED";
						List<string> list10 = new List<string>();
						if (array3[1] == 0)
						{
							list10.Add("ON/OFF");
						}
						if (Util.IsBitSet(array3[1], 1) && Util.IsBitSet(array3[1], 0))
						{
							list10.Add("SET");
						}
						if (Util.IsBitSet(array3[1], 7))
						{
							list10.Add("-7-");
						}
						if (Util.IsBitSet(array3[1], 6))
						{
							list10.Add("-6-");
						}
						if (Util.IsBitSet(array3[1], 5))
						{
							list10.Add("-5-");
						}
						if (Util.IsBitSet(array3[1], 4))
						{
							list10.Add("-4-");
						}
						if (Util.IsBitSet(array3[1], 3))
						{
							list10.Add("-3-");
						}
						if (Util.IsBitSet(array3[1], 2))
						{
							list10.Add("ACC/RES");
						}
						if (Util.IsBitSet(array3[1], 1))
						{
							list10.Add("COAST");
						}
						if (Util.IsBitSet(array3[1], 0))
						{
							list10.Add("CANCEL");
						}
						if (list10.Count == 0)
						{
							break;
						}
						foreach (string item16 in list10)
						{
							text2 = text2 + item16 + " | ";
						}
						if (text2.Length > 2)
						{
							text2 = text2.Remove(text2.Length - 3);
						}
						break;
					}
					case 229:
					{
						text = "CRUISE SET SPEED";
						if (array2.Length < 5)
						{
							break;
						}
						if (array3[2] != array3[0] + 1)
						{
							text += " | ERROR: REQUEST FB E5 E6";
							break;
						}
						double num55 = (double)((array3[1] << 8) + array3[3]) * (1.0 / 256.0);
						double value100 = num55 * 1.609344;
						if (Settings.Default.Units == "imperial")
						{
							text2 = Math.Round(num55, 3).ToString("0.000");
							text3 = "MPH";
						}
						else if (Settings.Default.Units == "metric")
						{
							text2 = Math.Round(value100, 3).ToString("0.000");
							text3 = "KM/H";
						}
						break;
					}
					case 230:
						text = "CRUISE SET SPEED | ERROR: REQUEST FB E5 E6";
						break;
					case 231:
						text = "CRUISE SWITCH VOLTS";
						if (array2.Length >= 5)
						{
							if (array3[2] != array3[0] + 1)
							{
								text += " | ERROR: REQUEST FB E7 E8";
								break;
							}
							double value96 = (double)((array3[1] << 8) + array3[3]) * 0.0049;
							text2 = Math.Round(value96, 3).ToString("0.000");
							text3 = "V";
						}
						break;
					case 232:
						text = "CRUISE SWITCH VOLTS | ERROR: REQUEST FB E7 E8";
						break;
					default:
					{
						for (int l = 0; l < num23; l++)
						{
							list3.Add(array3[l * 2]);
							list4.Add(array3[l * 2 + 1]);
						}
						text = "FB RAM TABLE | OFFSET: " + Util.ByteToHexStringSimple(list3.ToArray());
						text2 = Util.ByteToHexStringSimple(list4.ToArray());
						break;
					}
					}
				}
				else if (Year >= 2003 && CumminsSelected)
				{
					switch (array3[1])
					{
					case 27:
						text = "OUTPUT SHAFT SPEED";
						if (array2.Length >= 5)
						{
							if (array3[2] != array3[0] + 1)
							{
								text += " | ERROR: REQUEST FB 1B 1C";
								break;
							}
							double value115 = (double)((array3[1] << 8) + array3[3]) * 0.125;
							text2 = Math.Round(value115, 3).ToString("0.000");
							text3 = "RPM";
						}
						break;
					case 28:
						text = "OUTPUT SHAFT SPEED | ERROR: REQUEST FB 1B 1C";
						break;
					case 29:
						text = "WATER IN FUEL";
						if (array2.Length >= 5)
						{
							if (array3[2] != array3[0] + 1)
							{
								text += " | ERROR: REQUEST FB 1D 1E";
								break;
							}
							text2 = ((double)((array3[1] << 8) + array3[3])).ToString("0");
							text3 = "COUNTS";
						}
						break;
					case 30:
						text = "WATER IN FUEL | ERROR: REQUEST FB 1D 1E";
						break;
					case 31:
						text = "TRANSMISSION PWM DUTY CYCLE";
						if (array2.Length >= 5)
						{
							if (array3[2] != array3[0] + 1)
							{
								text += " | ERROR: REQUEST FB 1F 20";
								break;
							}
							double value111 = (double)((array3[1] << 8) + array3[3]) * (1.0 / 256.0);
							text2 = Math.Round(value111, 1).ToString("0.0");
							text3 = "PERCENT";
						}
						break;
					case 32:
						text = "TRANSMISSION PWM DUTY CYCLE | ERROR: REQUEST FB 1F 20";
						break;
					case 35:
						text = "PRESENT DRIVE GEAR";
						switch (array3[1])
						{
						case 0:
							text2 = "NEUTRAL";
							break;
						case 1:
							text2 = "1ST";
							break;
						case 2:
							text2 = "2ND";
							break;
						case 3:
							text2 = "3RD";
							break;
						case 4:
							text2 = "4TH";
							break;
						case 5:
							text2 = "5TH";
							break;
						case 6:
							text2 = "6TH";
							break;
						case 16:
							text2 = "REVERSE";
							break;
						}
						break;
					case 47:
					{
						text = "TARGET GOVERNOR PRESSURE";
						if (array2.Length < 5)
						{
							break;
						}
						if (array3[2] != array3[0] + 1)
						{
							text += " | ERROR: REQUEST FB 2F 30";
							break;
						}
						double num60 = (double)((array3[1] << 8) + array3[3]) * (1.0 / 128.0);
						double value123 = num60 * 6.894757;
						if (Settings.Default.Units == "imperial")
						{
							text2 = Math.Round(num60, 3).ToString("0.000");
							text3 = "PSI";
						}
						else if (Settings.Default.Units == "metric")
						{
							text2 = Math.Round(value123, 3).ToString("0.000");
							text3 = "KPA";
						}
						break;
					}
					case 48:
						text = "TARGET GOVERNOR PRESSURE | ERROR: REQUEST FB 2F 30";
						break;
					case 49:
						text = "PPS 1 SENSOR PERCENT";
						if (array2.Length >= 5)
						{
							if (array3[2] != array3[0] + 1)
							{
								text += " | ERROR: REQUEST FB 31 32";
								break;
							}
							double value117 = (double)((array3[1] << 8) + array3[3]) * 0.25;
							text2 = Math.Round(value117, 3).ToString("0.000");
							text3 = "PERCENT";
						}
						break;
					case 50:
						text = "PPS 1 SENSOR PERCENT | ERROR: REQUEST FB 31 32";
						break;
					case 51:
						text = "PPS 1 SENSOR VOLTS";
						if (array2.Length >= 5)
						{
							if (array3[2] != array3[0] + 1)
							{
								text += " | ERROR: REQUEST FB 33 34";
								break;
							}
							double value112 = (double)((array3[1] << 8) + array3[3]) * 0.0049;
							text2 = Math.Round(value112, 3).ToString("0.000");
							text3 = "V";
						}
						break;
					case 52:
						text = "PPS 1 SENSOR VOLTS | ERROR: REQUEST FB 33 34";
						break;
					case 63:
						text = "PPS 2 SENSOR PERCENT";
						if (array2.Length >= 5)
						{
							if (array3[2] != array3[0] + 1)
							{
								text += " | ERROR: REQUEST FB 3F 40";
								break;
							}
							double value120 = (double)((array3[1] << 8) + array3[3]) * 0.25;
							text2 = Math.Round(value120, 3).ToString("0.000");
							text3 = "PERCENT";
						}
						break;
					case 64:
						text = "PPS 2 SENSOR PERCENT | ERROR: REQUEST FB 3F 40";
						break;
					case 65:
						text = "PPS 2 SENSOR VOLTS";
						if (array2.Length >= 5)
						{
							if (array3[2] != array3[0] + 1)
							{
								text += " | ERROR: REQUEST FB 41 42";
								break;
							}
							double value119 = (double)((array3[1] << 8) + array3[3]) * 0.0049;
							text2 = Math.Round(value119, 3).ToString("0.000");
							text3 = "V";
						}
						break;
					case 66:
						text = "PPS 2 SENSOR VOLTS | ERROR: REQUEST FB 41 42";
						break;
					case 69:
						text = "IDLE SWITCH STATUS";
						text2 = ((!Util.IsBitSet(array3[1], 1) || !Util.IsBitSet(array3[1], 0)) ? "RELEASED" : "PRESSED");
						break;
					case 70:
					{
						text = "BRAKE SWITCH PRESSED";
						double value114 = (double)(int)array3[1] * 0.25;
						text2 = Math.Round(value114, 3).ToString("0.000");
						text3 = "PERCENT";
						break;
					}
					case 71:
					{
						text = "SWITCH STATUS";
						List<string> list13 = new List<string>();
						if (Util.IsBitSet(array3[1], 7))
						{
							list13.Add("OPSCLSD");
						}
						else
						{
							list13.Add("OPSOPEN");
						}
						if (Util.IsBitSet(array3[1], 6))
						{
							list13.Add("-6-");
						}
						if (Util.IsBitSet(array3[1], 5))
						{
							list13.Add("ODRLSD");
						}
						else
						{
							list13.Add("ODPRSD");
						}
						if (Util.IsBitSet(array3[1], 4))
						{
							list13.Add("P/N");
						}
						else
						{
							list13.Add("D/R");
						}
						if (Util.IsBitSet(array3[1], 3))
						{
							list13.Add("-3-");
						}
						if (Util.IsBitSet(array3[1], 2))
						{
							list13.Add("-2-");
						}
						if (Util.IsBitSet(array3[1], 1))
						{
							list13.Add("-1-");
						}
						if (Util.IsBitSet(array3[1], 0))
						{
							list13.Add("-0-");
						}
						if (list13.Count == 0)
						{
							break;
						}
						foreach (string item17 in list13)
						{
							text2 = text2 + item17 + " | ";
						}
						if (text2.Length > 2)
						{
							text2 = text2.Remove(text2.Length - 3);
						}
						break;
					}
					case 72:
						text = "DESIRED TORQUE CONVERTER CLUTCH STATUS";
						text2 = ((!Util.IsBitSet(array3[1], 0)) ? "UNLOCKED" : "LOCKED");
						break;
					case 74:
						text = "FINAL FUEL STATE";
						switch (array3[1])
						{
						case 0:
							text2 = "NOT SET";
							break;
						case 1:
							text2 = "JCOM TORQUE";
							break;
						case 2:
							text2 = "JCOM SPEED";
							break;
						case 3:
							text2 = "PROGRSV SHIFT";
							break;
						case 4:
							text2 = "PTO";
							break;
						case 5:
							text2 = "USER COMMAND";
							break;
						case 6:
							text2 = "LIMP HOME";
							break;
						case 7:
							text2 = "ASG THROTTLE";
							break;
						case 8:
							text2 = "4-D FUELING";
							break;
						case 9:
							text2 = "CRUISE CONTROL";
							break;
						case 10:
							text2 = "ROAD SPEED GOV";
							break;
						case 11:
							text2 = "LOW SPEED GOV";
							break;
						case 12:
							text2 = "HIGH SPEED GOV";
							break;
						case 13:
							text2 = "TORQUE DERATE OVERRIDE";
							break;
						case 14:
							text2 = "LOW GEAR";
							break;
						case 15:
							text2 = "ALTITUDE DERATE";
							break;
						case 16:
							text2 = "AFC DERATE";
							break;
						case 17:
							text2 = "ANC DERATE";
							break;
						case 18:
							text2 = "ENGINE PROTECT";
							break;
						case 19:
							text2 = "TORQUE CRV LIMIT";
							break;
						case 20:
							text2 = "JCOM TORQUE DERATE";
							break;
						case 21:
							text2 = "OUT OF GEAR";
							break;
						case 22:
							text2 = "CRANKING";
							break;
						case 23:
							text2 = "USER OVERRIDE";
							break;
						case 24:
							text2 = "ENGINE BRAKE";
							break;
						case 25:
							text2 = "ENGINE OVERSPEED";
							break;
						case 26:
							text2 = "ENGINE STOPPED";
							break;
						case 27:
							text2 = "SHUTDOWN";
							break;
						case 28:
							text2 = "FUEL DTC DERATE";
							break;
						case 29:
							text2 = "ENGINE PROTECT";
							break;
						case 30:
							text2 = "ALL SPD GOV APP";
							break;
						case 31:
							text2 = "ALT TORQUE";
							break;
						case 32:
							text2 = "MASTER/SLAVE OVERRIDE";
							break;
						case 33:
							text2 = "STARTUP OIL LIMIT";
							break;
						case 34:
							text2 = "PTO DERATE";
							break;
						case 35:
							text2 = "TORQUE CONTROL";
							break;
						case 36:
							text2 = "POWERTRAIN PROTECT";
							break;
						case 37:
							text2 = "T2 SPEED";
							break;
						case 38:
							text2 = "T2 TORQUE DERATE";
							break;
						case 39:
							text2 = "T2 DERATE";
							break;
						case 40:
							text2 = "NO DERATE";
							break;
						case 41:
							text2 = "ANTI THEFT DERATE";
							break;
						case 42:
							text2 = "PART THROTTLE LIMIT";
							break;
						case 43:
							text2 = "STEADY-STATE AMB DERATE";
							break;
						case 44:
							text2 = "TRSNT COOLANT DERATE";
							break;
						}
						break;
					case 79:
						text = "WASTEGATE DUTY CYCLE";
						if (array2.Length >= 5)
						{
							if (array3[2] != array3[0] + 1)
							{
								text += " | ERROR: REQUEST FB 4F 50";
								break;
							}
							double value122 = (double)((array3[1] << 8) + array3[3]) * (1.0 / 256.0);
							text2 = Math.Round(value122, 3).ToString("0.000");
							text3 = "PERCENT";
						}
						break;
					case 80:
						text = "WASTEGATE DUTY CYCLE | ERROR: REQUEST FB 4F 50";
						break;
					case 81:
						text = "BOOST VOLTS";
						if (array2.Length >= 5)
						{
							if (array3[2] != array3[0] + 1)
							{
								text += " | ERROR: REQUEST FB 51 52";
								break;
							}
							double value118 = (double)((array3[1] << 8) + array3[3]) * 0.0049;
							text2 = Math.Round(value118, 3).ToString("0.000");
							text3 = "V";
						}
						break;
					case 82:
						text = "BOOST VOLTS | ERROR: REQUEST FB 51 52";
						break;
					case 85:
						text = "WATER IN FUEL VOLTS";
						if (array2.Length >= 5)
						{
							if (array3[2] != array3[0] + 1)
							{
								text += " | ERROR: REQUEST FB 55 56";
								break;
							}
							double value116 = (double)((array3[1] << 8) + array3[3]) * 0.0049;
							text2 = Math.Round(value116, 3).ToString("0.000");
							text3 = "V";
						}
						break;
					case 86:
						text = "WATER IN FUEL VOLTS | ERROR: REQUEST FB 55 56";
						break;
					case 87:
						text = "ENGINE LOAD";
						if (array2.Length >= 5)
						{
							if (array3[2] != array3[0] + 1)
							{
								text += " | ERROR: REQUEST FB 57 58";
								break;
							}
							double value113 = (double)((array3[1] << 8) + array3[3]) * (1.0 / 256.0);
							text2 = Math.Round(value113, 1).ToString("0.0");
							text3 = "PERCENT";
						}
						break;
					case 88:
						text = "ENGINE LOAD | ERROR: REQUEST FB 57 58";
						break;
					case 185:
					{
						text = "BATTERY TEMPERATURE";
						if (array2.Length < 5)
						{
							break;
						}
						if (array3[2] != array3[0] + 1)
						{
							text += " | ERROR: REQUEST FB B9 BA";
							break;
						}
						double num58 = (double)((array3[1] << 8) + array3[3]) * (1.0 / 64.0);
						double value109 = (num58 - 32.0) / 1.8;
						if (Settings.Default.Units == "imperial")
						{
							text2 = Math.Round(num58, 1).ToString("0.0");
							text3 = "°F";
						}
						else if (Settings.Default.Units == "metric")
						{
							text2 = Math.Round(value109, 1).ToString("0.0");
							text3 = "°C";
						}
						break;
					}
					case 186:
						text = "BATTERY TEMPERATURE | ERROR: REQUEST FB B9 BA";
						break;
					case 203:
						text = "KEY-ON COUNTER";
						if (array2.Length >= 5)
						{
							if (array3[2] != array3[0] + 1)
							{
								text += " | ERROR: REQUEST FB CB CC";
								break;
							}
							text2 = ((ushort)((array3[1] << 8) + array3[3])).ToString("0");
							text3 = "COUNTS";
						}
						break;
					case 204:
						text = "KEY-ON COUNTER | ERROR: REQUEST FB CB CC";
						break;
					case 205:
						text = "ENGINE SPEED CKD SENSOR";
						if (array2.Length >= 5)
						{
							if (array3[2] != array3[0] + 1)
							{
								text += " | ERROR: REQUEST FB CD CE";
								break;
							}
							double value124 = (double)((array3[1] << 8) + array3[3]) * 0.125;
							text2 = Math.Round(value124, 3).ToString("0.000");
							text3 = "RPM";
						}
						break;
					case 206:
						text = "ENGINE SPEED CKD SENSOR | ERROR: REQUEST FB CF D0";
						break;
					case 207:
						text = "ENGINE SPEED CMP SENSOR";
						if (array2.Length >= 5)
						{
							if (array3[2] != array3[0] + 1)
							{
								text += " | ERROR: REQUEST FB CD CE";
								break;
							}
							double value121 = (double)((array3[1] << 8) + array3[3]) * 0.125;
							text2 = Math.Round(value121, 3).ToString("0.000");
							text3 = "RPM";
						}
						break;
					case 208:
						text = "ENGINE SPEED CMP SENSOR | ERROR: REQUEST FB CF D0";
						break;
					case 210:
					{
						text = "RELAY STATUS";
						List<string> list12 = new List<string>();
						if (Util.IsBitSet(array3[1], 7))
						{
							list12.Add("-7-");
						}
						if (Util.IsBitSet(array3[1], 6))
						{
							list12.Add("-6-");
						}
						if (Util.IsBitSet(array3[1], 5))
						{
							list12.Add("CRS12V");
						}
						if (Util.IsBitSet(array3[1], 4))
						{
							list12.Add("CVACBLOCK");
						}
						else
						{
							list12.Add("CVACAPPLY");
						}
						if (Util.IsBitSet(array3[1], 3))
						{
							list12.Add("CVNTBLOCK");
						}
						else
						{
							list12.Add("CVNTBLEED");
						}
						if (Util.IsBitSet(array3[1], 2))
						{
							list12.Add("-2-");
						}
						if (Util.IsBitSet(array3[1], 1))
						{
							list12.Add("ODSOLON");
						}
						else
						{
							list12.Add("ODSOLOFF");
						}
						if (Util.IsBitSet(array3[1], 0))
						{
							list12.Add("TCM12V");
						}
						if (list12.Count == 0)
						{
							break;
						}
						text += ": ";
						foreach (string item18 in list12)
						{
							text = text + item18 + " | ";
						}
						if (text.Length > 2)
						{
							text = text.Remove(text.Length - 3);
						}
						break;
					}
					case 215:
						text = "APP SENSOR VOLTS";
						if (array2.Length >= 5)
						{
							if (array3[2] != array3[0] + 1)
							{
								text += " | ERROR: REQUEST FB D7 D8";
								break;
							}
							double value107 = (double)((array3[1] << 8) + array3[3]) * 0.0049;
							text2 = Math.Round(value107, 3).ToString("0.000");
							text3 = "V";
						}
						break;
					case 216:
						text = "APP SENSOR VOLTS | ERROR: REQUEST FB D7 D8";
						break;
					case 220:
						text = "CRUISE CONTROL | DENIED REASON";
						text2 = array3[1] switch
						{
							2 => "CRUISE SWITCH DTC", 
							3 => "VSS RATIONALITY", 
							4 => "BRAKE RATIONALITY", 
							5 => "ON/OFF SWITCH", 
							6 => "BRAKE SWITCH", 
							7 => "CANCEL SWITCH", 
							8 => "SPEED SENSOR", 
							9 => "RPM LIMIT", 
							10 => "RPM/VSS RATIO", 
							11 => "CLUTCH SWITCH", 
							12 => "P/N SWITCH", 
							_ => "N/A", 
						};
						break;
					case 222:
						text = "CRUISE CONTROL | LAST CUTOUT REASON";
						text2 = array3[1] switch
						{
							2 => "CRUISE SWITCH DTC", 
							3 => "VSS RATIONALITY", 
							4 => "BRAKE RATIONALITY", 
							5 => "ON/OFF SWITCH", 
							6 => "BRAKE SWITCH", 
							7 => "CANCEL SWITCH", 
							8 => "SPEED SENSOR", 
							9 => "RPM LIMIT", 
							10 => "RPM/VSS RATIO", 
							11 => "CLUTCH SWITCH", 
							12 => "P/N SWITCH", 
							_ => "N/A", 
						};
						break;
					case 226:
						text = "CRUISE INDICATOR LAMP";
						text2 = ((!Util.IsBitSet(array3[1], 0)) ? "OFF" : "ON");
						break;
					case 228:
					{
						text = "CRUISE | BUTTON PRESSED";
						List<string> list14 = new List<string>();
						if (array3[1] == 0)
						{
							list14.Add("ON/OFF");
						}
						if (Util.IsBitSet(array3[1], 1) && Util.IsBitSet(array3[1], 0))
						{
							list14.Add("SET");
						}
						if (Util.IsBitSet(array3[1], 7))
						{
							list14.Add("-7-");
						}
						if (Util.IsBitSet(array3[1], 6))
						{
							list14.Add("-6-");
						}
						if (Util.IsBitSet(array3[1], 5))
						{
							list14.Add("-5-");
						}
						if (Util.IsBitSet(array3[1], 4))
						{
							list14.Add("-4-");
						}
						if (Util.IsBitSet(array3[1], 3))
						{
							list14.Add("-3-");
						}
						if (Util.IsBitSet(array3[1], 2))
						{
							list14.Add("ACC/RES");
						}
						if (Util.IsBitSet(array3[1], 1))
						{
							list14.Add("COAST");
						}
						if (Util.IsBitSet(array3[1], 0))
						{
							list14.Add("CANCEL");
						}
						if (list14.Count == 0)
						{
							break;
						}
						foreach (string item19 in list14)
						{
							text2 = text2 + item19 + " | ";
						}
						if (text2.Length > 2)
						{
							text2 = text2.Remove(text2.Length - 3);
						}
						break;
					}
					case 229:
					{
						text = "CRUISE SET SPEED";
						if (array2.Length < 5)
						{
							break;
						}
						if (array3[2] != array3[0] + 1)
						{
							text += " | ERROR: REQUEST FB E5 E6";
							break;
						}
						double num59 = (double)((array3[1] << 8) + array3[3]) * (1.0 / 256.0);
						double value110 = num59 * 1.609344;
						if (Settings.Default.Units == "imperial")
						{
							text2 = Math.Round(num59, 3).ToString("0.000");
							text3 = "MPH";
						}
						else if (Settings.Default.Units == "metric")
						{
							text2 = Math.Round(value110, 3).ToString("0.000");
							text3 = "KM/H";
						}
						break;
					}
					case 230:
						text = "CRUISE SET SPEED | ERROR: REQUEST FB E5 E6";
						break;
					case 231:
						text = "CRUISE SWITCH VOLTS";
						if (array2.Length >= 5)
						{
							if (array3[2] != array3[0] + 1)
							{
								text += " | ERROR: REQUEST FB E7 E8";
								break;
							}
							double value108 = (double)((array3[1] << 8) + array3[3]) * 0.0049;
							text2 = Math.Round(value108, 3).ToString("0.000");
							text3 = "V";
						}
						break;
					case 232:
						text = "CRUISE SWITCH VOLTS | ERROR: REQUEST FB E7 E8";
						break;
					case 235:
					{
						text = "INJECTORS DISABLED VEHICLE SPEED";
						if (array2.Length < 5)
						{
							break;
						}
						if (array3[2] != array3[0] + 1)
						{
							text += " | ERROR: REQUEST F8 EB EC";
							break;
						}
						double num57 = (double)((array3[1] << 8) + array3[3]) * (1.0 / 256.0);
						double value106 = num57 * 1.609344;
						if (Settings.Default.Units == "imperial")
						{
							text2 = Math.Round(num57, 3).ToString("0.000");
							text3 = "MPH";
						}
						else if (Settings.Default.Units == "metric")
						{
							text2 = Math.Round(value106, 3).ToString("0.000");
							text3 = "KM/H";
						}
						break;
					}
					case 236:
						text = "INJECTORS DISABLED VEHICLE SPEED | ERROR: REQUEST FB EB EC";
						break;
					default:
					{
						for (int m = 0; m < num23; m++)
						{
							list3.Add(array3[m * 2]);
							list4.Add(array3[m * 2 + 1]);
						}
						text = "FB RAM TABLE | OFFSET: " + Util.ByteToHexStringSimple(list3.ToArray());
						text2 = Util.ByteToHexStringSimple(list4.ToArray());
						break;
					}
					}
				}
				else
				{
					byte b3 = array3[0];
					for (int n = 0; n < num23; n++)
					{
						list3.Add(array3[n * 2]);
						list4.Add(array3[n * 2 + 1]);
					}
					text = "FB RAM TABLE | OFFSET: " + Util.ByteToHexStringSimple(list3.ToArray());
					text2 = Util.ByteToHexStringSimple(list4.ToArray());
				}
				break;
			case 252:
				text = "FC RAM TABLE SELECTED";
				if (array2.Length < 3)
				{
					break;
				}
				switch (array3[0])
				{
				case 1:
				{
					text = "TOTAL FUEL USED";
					if (array2.Length < 9)
					{
						break;
					}
					if (array3[2] != array3[0] + 1 || array3[4] != array3[0] + 2 || array3[6] != array3[0] + 3)
					{
						text += " | ERROR: REQUEST FC 01 02 03 04";
						break;
					}
					double num37 = (double)(uint)((array3[1] << 24) | (array3[3] << 16) | (array3[5] << 8) | array3[7]) * 7.6E-05;
					double value73 = num37 * 3.785412;
					if (Settings.Default.Units == "imperial")
					{
						text2 = Math.Round(num37, 1).ToString("0.0");
						text3 = "GALLON";
					}
					else if (Settings.Default.Units == "metric")
					{
						text2 = Math.Round(value73, 1).ToString("0.0");
						text3 = "LITER";
					}
					break;
				}
				case 2:
				case 3:
				case 4:
					text = "TOTAL FUEL USED | ERROR: REQUEST FC 01 02 03 04";
					break;
				case 5:
				{
					text = "TRIP FUEL USED";
					if (array2.Length < 9)
					{
						break;
					}
					if (array3[2] != array3[0] + 1 || array3[4] != array3[0] + 2 || array3[6] != array3[0] + 3)
					{
						text += " | ERROR: REQUEST FC 05 06 07 08";
						break;
					}
					double num36 = (double)(uint)((array3[1] << 24) | (array3[3] << 16) | (array3[5] << 8) | array3[7]) * 7.6E-05;
					double value72 = num36 * 3.785412;
					if (Settings.Default.Units == "imperial")
					{
						text2 = Math.Round(num36, 1).ToString("0.0");
						text3 = "GALLON";
					}
					else if (Settings.Default.Units == "metric")
					{
						text2 = Math.Round(value72, 1).ToString("0.0");
						text3 = "LITER";
					}
					break;
				}
				case 6:
				case 7:
				case 8:
					text = "TRIP FUEL USED | ERROR: REQUEST FC 05 06 07 08";
					break;
				case 9:
					text = "TOTAL TIME";
					if (array2.Length >= 9)
					{
						if (array3[2] != array3[0] + 1 || array3[4] != array3[0] + 2 || array3[6] != array3[0] + 3)
						{
							text += " | ERROR: REQUEST FC 09 0A 0B 0C";
							break;
						}
						double num41 = (double)(uint)((array3[1] << 24) | (array3[3] << 16) | (array3[5] << 8) | array3[7]) * 2.8E-05;
						double value78 = num41 * 3600.0;
						text2 = TimeSpan.FromSeconds(value78).ToString("hh\\:mm\\:ss");
						text3 = "HH:MM:SS";
					}
					break;
				case 10:
				case 11:
				case 12:
					text = "TOTAL TIME | ERROR: REQUEST FC 09 0A 0B 0C";
					break;
				case 13:
					text = "TRIP TIME";
					if (array2.Length >= 9)
					{
						if (array3[2] != array3[0] + 1 || array3[4] != array3[0] + 2 || array3[6] != array3[0] + 3)
						{
							text += " | ERROR: REQUEST FC 0D 0E 0F 10";
							break;
						}
						double num44 = (double)(uint)((array3[1] << 24) | (array3[3] << 16) | (array3[5] << 8) | array3[7]) * 2.8E-05;
						double value81 = num44 * 3600.0;
						text2 = TimeSpan.FromSeconds(value81).ToString("hh\\:mm\\:ss");
						text3 = "HH:MM:SS";
					}
					break;
				case 14:
				case 15:
				case 16:
					text = "TRIP TIME | ERROR: REQUEST FC 0D 0E 0F 10";
					break;
				case 17:
				{
					text = "TOTAL IDLE FUEL";
					if (array2.Length < 9)
					{
						break;
					}
					if (array3[2] != array3[0] + 1 || array3[4] != array3[0] + 2 || array3[6] != array3[0] + 3)
					{
						text += " | ERROR: REQUEST FC 11 12 13 14";
						break;
					}
					double num43 = (double)(uint)((array3[1] << 24) | (array3[3] << 16) | (array3[5] << 8) | array3[7]) * 7.6E-05;
					double value80 = num43 * 3.785412;
					if (Settings.Default.Units == "imperial")
					{
						text2 = Math.Round(num43, 1).ToString("0.0");
						text3 = "GALLON";
					}
					else if (Settings.Default.Units == "metric")
					{
						text2 = Math.Round(value80, 1).ToString("0.0");
						text3 = "LITER";
					}
					break;
				}
				case 18:
				case 19:
				case 20:
					text = "TOTAL IDLE FUEL | ERROR: REQUEST FC 11 12 13 14";
					break;
				case 21:
				{
					text = "TRIP IDLE FUEL";
					if (array2.Length < 9)
					{
						break;
					}
					if (array3[2] != array3[0] + 1 || array3[4] != array3[0] + 2 || array3[6] != array3[0] + 3)
					{
						text += " | ERROR: REQUEST FC 15 16 17 18";
						break;
					}
					double num45 = (double)(uint)((array3[1] << 24) | (array3[3] << 16) | (array3[5] << 8) | array3[7]) * 7.6E-05;
					double value82 = num45 * 3.785412;
					if (Settings.Default.Units == "imperial")
					{
						text2 = Math.Round(num45, 1).ToString("0.0");
						text3 = "GALLON";
					}
					else if (Settings.Default.Units == "metric")
					{
						text2 = Math.Round(value82, 1).ToString("0.0");
						text3 = "LITER";
					}
					break;
				}
				case 22:
				case 23:
				case 24:
					text = "TRIP IDLE FUEL | ERROR: REQUEST FC 15 16 17 18";
					break;
				case 25:
					text = "TOTAL IDLE TIME";
					if (array2.Length >= 9)
					{
						if (array3[2] != array3[0] + 1 || array3[4] != array3[0] + 2 || array3[6] != array3[0] + 3)
						{
							text += " | ERROR: REQUEST FC 19 1A 1B 1C";
							break;
						}
						double num40 = (double)(uint)((array3[1] << 24) | (array3[3] << 16) | (array3[5] << 8) | array3[7]) * 2.8E-05;
						double value77 = num40 * 3600.0;
						text2 = TimeSpan.FromSeconds(value77).ToString("hh\\:mm\\:ss");
						text3 = "HH:MM:SS";
					}
					break;
				case 26:
				case 27:
				case 28:
					text = "TOTAL IDLE TIME | ERROR: REQUEST FC 19 1A 1B 1C";
					break;
				case 29:
					text = "TRIP IDLE TIME";
					if (array2.Length >= 9)
					{
						if (array3[2] != array3[0] + 1 || array3[4] != array3[0] + 2 || array3[6] != array3[0] + 3)
						{
							text += " | ERROR: REQUEST FC 1D 1E 1F 20";
							break;
						}
						double num38 = (double)(uint)((array3[1] << 24) | (array3[3] << 16) | (array3[5] << 8) | array3[7]) * 2.8E-05;
						double value75 = num38 * 3600.0;
						text2 = TimeSpan.FromSeconds(value75).ToString("hh\\:mm\\:ss");
						text3 = "HH:MM:SS";
					}
					break;
				case 30:
				case 31:
				case 32:
					text = "TRIP IDLE TIME | ERROR: REQUEST FC 1D 1E 1F 20";
					break;
				case 33:
				{
					text = "TOTAL DISTANCE";
					if (array2.Length < 9)
					{
						break;
					}
					if (array3[2] != array3[0] + 1 || array3[4] != array3[0] + 2 || array3[6] != array3[0] + 3)
					{
						text += " | ERROR: REQUEST FC 21 22 23 24";
						break;
					}
					double num39 = (double)(uint)((array3[1] << 24) | (array3[3] << 16) | (array3[5] << 8) | array3[7]) * 0.000125;
					double value76 = num39 * 1.609344;
					if (Settings.Default.Units == "imperial")
					{
						text2 = Math.Round(num39, 3).ToString("0.000");
						text3 = "MILE";
					}
					else if (Settings.Default.Units == "metric")
					{
						text2 = Math.Round(value76, 3).ToString("0.000");
						text3 = "KILOMETER";
					}
					break;
				}
				case 34:
				case 35:
				case 36:
					text = "TOTAL DISTANCE | ERROR: REQUEST FC 21 22 23 24";
					break;
				case 37:
				{
					text = "TRIP DISTANCE";
					if (array2.Length < 9)
					{
						break;
					}
					if (array3[2] != array3[0] + 1 || array3[4] != array3[0] + 2 || array3[6] != array3[0] + 3)
					{
						text += " | ERROR: REQUEST FC 25 26 27 28";
						break;
					}
					double num46 = (double)(uint)((array3[1] << 24) | (array3[3] << 16) | (array3[5] << 8) | array3[7]) * 0.000125;
					double value83 = num46 * 1.609344;
					if (Settings.Default.Units == "imperial")
					{
						text2 = Math.Round(num46, 3).ToString("0.000");
						text3 = "MILE";
					}
					else if (Settings.Default.Units == "metric")
					{
						text2 = Math.Round(value83, 3).ToString("0.000");
						text3 = "KILOMETER";
					}
					break;
				}
				case 38:
				case 39:
				case 40:
					text = "TRIP DISTANCE | ERROR: REQUEST FC 25 26 27 28";
					break;
				case 41:
				{
					text = "TRIP AVERAGE FUEL";
					if (array2.Length < 5)
					{
						break;
					}
					if (array3[2] != array3[0] + 1)
					{
						text += " | ERROR: REQUEST FC 29 2A";
						break;
					}
					double num42 = (double)((array3[1] << 8) + array3[3]) * 0.125;
					double value79 = 235.214583 / num42;
					if (Settings.Default.Units == "imperial")
					{
						text2 = Math.Round(num42, 3).ToString("0.000");
						text3 = "MPG";
					}
					else if (Settings.Default.Units == "metric")
					{
						text2 = Math.Round(value79, 3).ToString("0.000");
						text3 = "L/100KM";
					}
					break;
				}
				case 42:
					text = "TRIP AVERAGE FUEL | ERROR: REQUEST FC 29 2A";
					break;
				case 43:
					text = "ECM RUN TIME";
					if (array2.Length >= 9)
					{
						if (array3[2] != array3[0] + 1 || array3[4] != array3[0] + 2 || array3[6] != array3[0] + 3)
						{
							text += " | ERROR: REQUEST FC 2B 2C 2D 2E";
							break;
						}
						double value74 = (double)(uint)((array3[1] << 24) | (array3[3] << 16) | (array3[5] << 8) | array3[7]) * 0.2;
						text2 = TimeSpan.FromSeconds(value74).ToString("hh\\:mm\\:ss");
						text3 = "HH:MM:SS";
					}
					break;
				case 44:
				case 45:
				case 46:
					text = "ECM RUN TIME | ERROR: REQUEST FC 2B 2C 2D 2E";
					break;
				case 47:
					text = "ENGINE RUN TIME";
					if (array2.Length >= 9)
					{
						if (array3[2] != array3[0] + 1 || array3[4] != array3[0] + 2 || array3[6] != array3[0] + 3)
						{
							text += " | ERROR: REQUEST FC 2F 30 31 32";
							break;
						}
						double value71 = (double)(uint)((array3[1] << 24) | (array3[3] << 16) | (array3[5] << 8) | array3[7]) * 0.2;
						text2 = TimeSpan.FromSeconds(value71).ToString("hh\\:mm\\:ss");
						text3 = "HH:MM:SS";
					}
					break;
				case 48:
				case 49:
				case 50:
					text = "ENGINE RUN TIME | ERROR: REQUEST FC 2F 30 31 32";
					break;
				default:
				{
					for (int k = 0; k < num23; k++)
					{
						list3.Add(array3[k * 2]);
						list4.Add(array3[k * 2 + 1]);
					}
					text = "FC RAM TABLE | OFFSET: " + Util.ByteToHexStringSimple(list3.ToArray());
					text2 = Util.ByteToHexStringSimple(list4.ToArray());
					break;
				}
				}
				break;
			case 253:
				text = "FD RAM TABLE SELECTED";
				if (array2.Length < 3)
				{
					break;
				}
				switch (array3[0])
				{
				case 33:
					text = "FUEL PRESSURE REGULATOR OUTPUT";
					if (array2.Length >= 5)
					{
						if (array3[2] != array3[0] + 1)
						{
							text += " | ERROR: REQUEST FD 21 22";
							break;
						}
						double value51 = (double)((array3[1] << 8) + array3[3]) * 0.025;
						text2 = Math.Round(value51, 3).ToString("0.000");
						text3 = "PERCENT";
					}
					break;
				case 34:
					text = "FUEL PRESSURE REGULATOR OUTPUT | ERROR: REQUEST FD 21 22";
					break;
				case 35:
				{
					text = "FUEL PRESSURE";
					if (array2.Length < 5)
					{
						break;
					}
					if (array3[2] != array3[0] + 1)
					{
						text += " | ERROR: REQUEST FD 23 24";
						break;
					}
					double num35 = (double)((array3[1] << 8) + array3[3]) * 7.078;
					double value63 = num35 * 6.894757;
					if (Settings.Default.Units == "imperial")
					{
						text2 = Math.Round(num35, 3).ToString("0.000");
						text3 = "PSI";
					}
					else if (Settings.Default.Units == "metric")
					{
						text2 = Math.Round(value63, 3).ToString("0.000");
						text3 = "KPA";
					}
					break;
				}
				case 36:
					text = "FUEL PRESSURE | ERROR: REQUEST FD 23 24";
					break;
				case 39:
				{
					text = "FUEL PRESSURE SETPOINT";
					if (array2.Length < 5)
					{
						break;
					}
					if (array3[2] != array3[0] + 1)
					{
						text += " | ERROR: REQUEST FD 27 28";
						break;
					}
					double num34 = (double)((array3[1] << 8) + array3[3]) * 7.078;
					double value61 = num34 * 6.894757;
					if (Settings.Default.Units == "imperial")
					{
						text2 = Math.Round(num34, 3).ToString("0.000");
						text3 = "PSI";
					}
					else if (Settings.Default.Units == "metric")
					{
						text2 = Math.Round(value61, 3).ToString("0.000");
						text3 = "KPA";
					}
					break;
				}
				case 40:
					text = "FUEL PRESSURE SETPOINT | ERROR: REQUEST FD 27 28";
					break;
				case 41:
					text = "FUEL PRESSURE SENSOR VOLTS";
					if (array2.Length >= 5)
					{
						if (array3[2] != array3[0] + 1)
						{
							text += " | ERROR: REQUEST FD 29 2A";
							break;
						}
						double value69 = (double)((array3[1] << 8) + array3[3]) * 0.0012;
						text2 = Math.Round(value69, 3).ToString("0.000");
						text3 = "V";
					}
					break;
				case 42:
					text = "FUEL PRESSURE SENSOR VOLTS | ERROR: REQUEST FD 29 2A";
					break;
				case 124:
					text = "CVN";
					if (array2.Length >= 9)
					{
						if (array3[2] != array3[0] + 1 || array3[4] != array3[0] + 2 || array3[6] != array3[0] + 3)
						{
							text += " | ERROR: REQUEST FD 7C 7D 7E 7F";
							break;
						}
						double value56 = (double)(uint)((array3[1] << 24) | (array3[3] << 16) | (array3[5] << 8) | array3[7]) * 0.0049;
						text2 = Math.Round(value56, 3).ToString("0.000");
						text3 = "V";
					}
					break;
				case 125:
				case 126:
				case 127:
					text = "CVN | ERROR: REQUEST FD 7C 7D 7E 7F";
					break;
				case 128:
					text = "RADIATOR FAN SPEED";
					if (array2.Length >= 5)
					{
						if (array3[2] != array3[0] + 1)
						{
							text += " | ERROR: REQUEST FD 80 81";
							break;
						}
						double value66 = (double)((array3[1] << 8) + array3[3]) * 0.125;
						text2 = Math.Round(value66, 3).ToString("0.000");
						text3 = "RPM";
					}
					break;
				case 129:
					text = "RADIATOR FAN SPEED | ERROR: REQUEST FD 80 81";
					break;
				case 130:
					text = "DESIRED RADIATOR FAN PWM";
					if (array2.Length >= 5)
					{
						if (array3[2] != array3[0] + 1)
						{
							text += " | ERROR: REQUEST FD 82 83";
							break;
						}
						double value59 = (double)((array3[1] << 8) + array3[3]) * (1.0 / 256.0);
						text2 = Math.Round(value59, 3).ToString("0.000");
						text3 = "PERCENT";
					}
					break;
				case 131:
					text = "DESIRED RADIATOR FAN PWM | ERROR: REQUEST FD 82 83";
					break;
				case 132:
					text = "% OF TIME @  0-10% LOAD";
					if (array2.Length >= 5)
					{
						if (array3[2] != array3[0] + 1)
						{
							text += " | ERROR: REQUEST FD 84 85";
							break;
						}
						double value55 = (double)((array3[1] << 8) + array3[3]) * (1.0 / 256.0);
						text2 = Math.Round(value55, 3).ToString("0.000");
						text3 = "PERCENT";
					}
					break;
				case 133:
					text = "% OF TIME @  0-10% LOAD | ERROR: REQUEST FD 84 85";
					break;
				case 134:
					text = "% OF TIME @ 11-20% LOAD";
					if (array2.Length >= 5)
					{
						if (array3[2] != array3[0] + 1)
						{
							text += " | ERROR: REQUEST FD 86 87";
							break;
						}
						double value68 = (double)((array3[1] << 8) + array3[3]) * (1.0 / 256.0);
						text2 = Math.Round(value68, 3).ToString("0.000");
						text3 = "PERCENT";
					}
					break;
				case 135:
					text = "% OF TIME @ 11-20% LOAD | ERROR: REQUEST FD 86 87";
					break;
				case 136:
					text = "% OF TIME @ 21-30% LOAD";
					if (array2.Length >= 5)
					{
						if (array3[2] != array3[0] + 1)
						{
							text += " | ERROR: REQUEST FD 88 89";
							break;
						}
						double value64 = (double)((array3[1] << 8) + array3[3]) * (1.0 / 256.0);
						text2 = Math.Round(value64, 3).ToString("0.000");
						text3 = "PERCENT";
					}
					break;
				case 137:
					text = "% OF TIME @ 21-30% LOAD | ERROR: REQUEST FD 88 89";
					break;
				case 138:
					text = "% OF TIME @ 31-40% LOAD";
					if (array2.Length >= 5)
					{
						if (array3[2] != array3[0] + 1)
						{
							text += " | ERROR: REQUEST FD 8A 8B";
							break;
						}
						double value60 = (double)((array3[1] << 8) + array3[3]) * (1.0 / 256.0);
						text2 = Math.Round(value60, 3).ToString("0.000");
						text3 = "PERCENT";
					}
					break;
				case 139:
					text = "% OF TIME @ 31-40% LOAD | ERROR: REQUEST FD 8A 8B";
					break;
				case 140:
					text = "% OF TIME @ 41-50% LOAD";
					if (array2.Length >= 5)
					{
						if (array3[2] != array3[0] + 1)
						{
							text += " | ERROR: REQUEST FD 8C 8D";
							break;
						}
						double value57 = (double)((array3[1] << 8) + array3[3]) * (1.0 / 256.0);
						text2 = Math.Round(value57, 3).ToString("0.000");
						text3 = "PERCENT";
					}
					break;
				case 141:
					text = "% OF TIME @ 41-50% LOAD | ERROR: REQUEST FD 8C 8D";
					break;
				case 142:
					text = "% OF TIME @ 51-60% LOAD";
					if (array2.Length >= 5)
					{
						if (array3[2] != array3[0] + 1)
						{
							text += " | ERROR: REQUEST FD 8E 8F";
							break;
						}
						double value53 = (double)((array3[1] << 8) + array3[3]) * (1.0 / 256.0);
						text2 = Math.Round(value53, 3).ToString("0.000");
						text3 = "PERCENT";
					}
					break;
				case 143:
					text = "% OF TIME @ 51-60% LOAD | ERROR: REQUEST FD 8E 8F";
					break;
				case 144:
					text = "% OF TIME @ 61-70% LOAD";
					if (array2.Length >= 5)
					{
						if (array3[2] != array3[0] + 1)
						{
							text += " | ERROR: REQUEST FD 90 91";
							break;
						}
						double value70 = (double)((array3[1] << 8) + array3[3]) * (1.0 / 256.0);
						text2 = Math.Round(value70, 3).ToString("0.000");
						text3 = "PERCENT";
					}
					break;
				case 145:
					text = "% OF TIME @ 61-70% LOAD | ERROR: REQUEST FD 90 91";
					break;
				case 146:
					text = "% OF TIME @ 71-80% LOAD";
					if (array2.Length >= 5)
					{
						if (array3[2] != array3[0] + 1)
						{
							text += " | ERROR: REQUEST FD 92 93";
							break;
						}
						double value67 = (double)((array3[1] << 8) + array3[3]) * (1.0 / 256.0);
						text2 = Math.Round(value67, 3).ToString("0.000");
						text3 = "PERCENT";
					}
					break;
				case 147:
					text = "% OF TIME @ 71-80% LOAD | ERROR: REQUEST FD 92 93";
					break;
				case 148:
					text = "% OF TIME @ 81-90% LOAD";
					if (array2.Length >= 5)
					{
						if (array3[2] != array3[0] + 1)
						{
							text += " | ERROR: REQUEST FD 94 95";
							break;
						}
						double value65 = (double)((array3[1] << 8) + array3[3]) * (1.0 / 256.0);
						text2 = Math.Round(value65, 3).ToString("0.000");
						text3 = "PERCENT";
					}
					break;
				case 149:
					text = "% OF TIME @ 81-90% LOAD | ERROR: REQUEST FD 94 95";
					break;
				case 150:
					text = "% OF TIME @ 91-100% LOAD";
					if (array2.Length >= 5)
					{
						if (array3[2] != array3[0] + 1)
						{
							text += " | ERROR: REQUEST FD 96 97";
							break;
						}
						double value62 = (double)((array3[1] << 8) + array3[3]) * (1.0 / 256.0);
						text2 = Math.Round(value62, 3).ToString("0.000");
						text3 = "PERCENT";
					}
					break;
				case 151:
					text = "% OF TIME @ 91-100% LOAD | ERROR: REQUEST FD 96 97";
					break;
				case 160:
				{
					text = "BAROMETRIC PRESSURE";
					if (array2.Length < 5)
					{
						break;
					}
					if (array3[2] != array3[0] + 1)
					{
						text += " | ERROR: REQUEST FD A0 A1";
						break;
					}
					double num33 = (double)((array3[1] << 8) + array3[3]) * 0.0159 * 0.4911542;
					double value58 = num33 * 6.894757;
					if (Settings.Default.Units == "imperial")
					{
						text2 = Math.Round(num33, 3).ToString("0.000");
						text3 = "PSI";
					}
					else if (Settings.Default.Units == "metric")
					{
						text2 = Math.Round(value58, 3).ToString("0.000");
						text3 = "KPA";
					}
					break;
				}
				case 161:
					text = "BAROMETRIC PRESSURE | ERROR: REQUEST FD A0 A1";
					break;
				case 164:
				{
					text = "AMBIENT AIR TEMPERATURE";
					if (array2.Length < 5)
					{
						break;
					}
					if (array3[2] != array3[0] + 1)
					{
						text += " | ERROR: REQUEST FD A4 A5";
						break;
					}
					double num32 = (double)((array3[1] << 8) + array3[3]) * (1.0 / 64.0);
					double value54 = (num32 - 32.0) / 1.8;
					if (Settings.Default.Units == "imperial")
					{
						text2 = Math.Round(num32, 1).ToString("0.0");
						text3 = "°F";
					}
					else if (Settings.Default.Units == "metric")
					{
						text2 = Math.Round(value54, 1).ToString("0.0");
						text3 = "°C";
					}
					break;
				}
				case 165:
					text = "AMBIENT AIR TEMPERATURE | ERROR: REQUEST FD A4 A5";
					break;
				case 166:
					text = "AMBIENT AIR TEMPERATURE SENSOR VOLTS";
					if (array2.Length >= 5)
					{
						if (array3[2] != array3[0] + 1)
						{
							text += " | ERROR: REQUEST FD A6 A7";
							break;
						}
						double value52 = (double)((array3[1] << 8) + array3[3]) * 0.0049;
						text2 = Math.Round(value52, 3).ToString("0.000");
						text3 = "V";
					}
					break;
				case 167:
					text = "AMBIENT TEMP SENSOR VOLTS | ERROR: REQUEST FD A6 A7";
					break;
				case 225:
				{
					text = "CYLINDER 1 CONTRIBUTION";
					int num31 = array3[1];
					text2 = num31.ToString("0");
					text3 = "PERCENT";
					break;
				}
				case 226:
				{
					text = "CYLINDER 5 CONTRIBUTION";
					int num30 = array3[1];
					text2 = num30.ToString("0");
					text3 = "PERCENT";
					break;
				}
				case 227:
				{
					text = "CYLINDER 3 CONTRIBUTION";
					int num29 = array3[1];
					text2 = num29.ToString("0");
					text3 = "PERCENT";
					break;
				}
				case 228:
				{
					text = "CYLINDER 6 CONTRIBUTION";
					int num28 = array3[1];
					text2 = num28.ToString("0");
					text3 = "PERCENT";
					break;
				}
				case 229:
				{
					text = "CYLINDER 2 CONTRIBUTION";
					int num27 = array3[1];
					text2 = num27.ToString("0");
					text3 = "PERCENT";
					break;
				}
				case 230:
				{
					text = "CYLINDER 4 CONTRIBUTION";
					int num26 = array3[1];
					text2 = num26.ToString("0");
					text3 = "PERCENT";
					break;
				}
				case 231:
				{
					text = "CYLINDER 1-3 CONTRIBUTION";
					int num25 = array3[1];
					text2 = num25.ToString("0");
					text3 = "PERCENT";
					break;
				}
				case 232:
				{
					text = "CYLINDER 4-6 CONTRIBUTION";
					int num24 = array3[1];
					text2 = num24.ToString("0");
					text3 = "PERCENT";
					break;
				}
				case 234:
					text = "ENGINE SPEED";
					text2 = (array3[1] * 32).ToString("0");
					text3 = "RPM";
					break;
				case 235:
					text = "CYLINDER TEST STATUS";
					switch (array3[1])
					{
					case 0:
						text2 = "NOT RUNNING";
						break;
					case 1:
						text2 = "RUNNING";
						break;
					case 2:
						text2 = "ABORTED";
						break;
					}
					break;
				case 236:
					text = "FPO TEST STATUS";
					switch (array3[1])
					{
					case 0:
						text2 = "NOT RUNNING";
						break;
					case 1:
						text2 = "RUNNING";
						break;
					case 2:
						text2 = "ABORTED";
						break;
					}
					break;
				default:
				{
					for (int j = 0; j < num23; j++)
					{
						list3.Add(array3[j * 2]);
						list4.Add(array3[j * 2 + 1]);
					}
					text = "FD RAM TABLE | OFFSET: " + Util.ByteToHexStringSimple(list3.ToArray());
					text2 = Util.ByteToHexStringSimple(list4.ToArray());
					break;
				}
				}
				break;
			case 254:
				text = "SELECT LOW-SPEED MODE";
				break;
			case byte.MaxValue:
				text = "PCM WAKE UP";
				break;
			default:
				text = string.Empty;
				break;
			}
		}
		string text10 = ((array2.Length >= 9) ? (Util.ByteToHexString(array2, 0, 7) + " .. ") : (Util.ByteToHexString(array2, 0, array2.Length) + " "));
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
		stringBuilder.Remove(2, text10.Length);
		stringBuilder.Insert(2, text10);
		stringBuilder.Remove(28, text.Length);
		stringBuilder.Insert(28, text);
		stringBuilder.Remove(82, text2.Length);
		stringBuilder.Insert(82, text2);
		stringBuilder.Remove(108, text3.Length);
		stringBuilder.Insert(108, text3);
		ushort modifiedID;
		switch (b)
		{
		case 20:
		case 22:
		case 34:
		case 37:
		case 42:
		case 45:
		case 240:
		case 241:
		case 242:
		case 243:
		case 244:
		case 245:
		case 246:
		case 247:
		case 248:
		case 249:
		case 250:
		case 251:
		case 252:
		case 253:
			modifiedID = ((array3.Length == 0) ? ((ushort)((uint)(b << 8) & 0xFF00u)) : ((ushort)(((b << 8) & 0xFF00) + array3[0])));
			break;
		default:
			modifiedID = (ushort)((uint)(b << 8) & 0xFF00u);
			break;
		}
		Diagnostics.AddRow(modifiedID, stringBuilder.ToString());
		if (speed == "62500 baud")
		{
			Diagnostics.AddRAMTableDump(data);
		}
		UpdateHeader();
		if (Settings.Default.Timestamp)
		{
			TimeSpan value243 = TimeSpan.FromMilliseconds((array[0] << 24) | (array[1] << 16) | (array[2] << 8) | array[3]);
			string contents = DateTime.Today.Add(value243).ToString("HH:mm:ss.fff") + ",";
			File.AppendAllText(MainForm.PCMLogFilename, contents);
		}
		File.AppendAllText(MainForm.PCMLogFilename, "PCM," + Util.ByteToHexStringSimple(array2) + Environment.NewLine);
		if (!StoredFaultCodesSaved)
		{
			StringBuilder stringBuilder2 = new StringBuilder();
			if (StoredFaultCodeList.Count > 0)
			{
				stringBuilder2.Append("STORED FAULT CODE LIST:" + Environment.NewLine);
				foreach (byte storedFaultCode in StoredFaultCodeList)
				{
					if (storedFaultCode != 0)
					{
						int num133 = SBEC3EngineDTC.Rows.IndexOf(SBEC3EngineDTC.Rows.Find(storedFaultCode));
						byte[] data4 = new byte[1] { storedFaultCode };
						if (num133 > -1)
						{
							stringBuilder2.Append(Util.ByteToHexStringSimple(data4) + ": " + SBEC3EngineDTC.Rows[num133]["description"]?.ToString() + Environment.NewLine);
						}
						else
						{
							stringBuilder2.Append(Util.ByteToHexStringSimple(data4) + ": UNRECOGNIZED DTC" + Environment.NewLine);
						}
					}
				}
				stringBuilder2.Remove(stringBuilder2.Length - 1, 1);
				File.AppendAllText(MainForm.PCMLogFilename, Environment.NewLine + stringBuilder2.ToString() + Environment.NewLine);
			}
			else
			{
				stringBuilder2.Append("NO STORED FAULT CODE");
				File.AppendAllText(MainForm.PCMLogFilename, Environment.NewLine + stringBuilder2.ToString() + Environment.NewLine + Environment.NewLine);
			}
			StoredFaultCodesSaved = true;
		}
		if (!PendingFaultCodesSaved)
		{
			StringBuilder stringBuilder3 = new StringBuilder();
			if (PendingFaultCodeList.Count > 0)
			{
				stringBuilder3.Append("PENDING FAULT CODE LIST:" + Environment.NewLine);
				foreach (byte pendingFaultCode in PendingFaultCodeList)
				{
					if (pendingFaultCode != 0)
					{
						int num134 = SBEC3EngineDTC.Rows.IndexOf(SBEC3EngineDTC.Rows.Find(pendingFaultCode));
						byte[] data5 = new byte[1] { pendingFaultCode };
						if (num134 > -1)
						{
							stringBuilder3.Append(Util.ByteToHexStringSimple(data5) + ": " + SBEC3EngineDTC.Rows[num134]["description"]?.ToString() + Environment.NewLine);
						}
						else
						{
							stringBuilder3.Append(Util.ByteToHexStringSimple(data5) + ": UNRECOGNIZED DTC" + Environment.NewLine);
						}
					}
				}
				stringBuilder3.Remove(stringBuilder3.Length - 1, 1);
				File.AppendAllText(MainForm.PCMLogFilename, Environment.NewLine + stringBuilder3.ToString() + Environment.NewLine);
			}
			else
			{
				stringBuilder3.Append("NO PENDING FAULT CODE");
				File.AppendAllText(MainForm.PCMLogFilename, Environment.NewLine + stringBuilder3.ToString() + Environment.NewLine + Environment.NewLine);
			}
			PendingFaultCodesSaved = true;
		}
		if (FaultCodes1TSaved)
		{
			return;
		}
		StringBuilder stringBuilder4 = new StringBuilder();
		if (FaultCode1TList.Count > 0)
		{
			stringBuilder4.Append("ONE-TRIP FAULT CODE LIST:" + Environment.NewLine);
			foreach (byte faultCode1T in FaultCode1TList)
			{
				if (faultCode1T != 0)
				{
					int num135 = SBEC3EngineDTC.Rows.IndexOf(SBEC3EngineDTC.Rows.Find(faultCode1T));
					byte[] data6 = new byte[1] { faultCode1T };
					if (num135 > -1)
					{
						stringBuilder4.Append(Util.ByteToHexStringSimple(data6) + ": " + SBEC3EngineDTC.Rows[num135]["description"]?.ToString() + Environment.NewLine);
					}
					else
					{
						stringBuilder4.Append(Util.ByteToHexStringSimple(data6) + ": -" + Environment.NewLine);
					}
				}
			}
			stringBuilder4.Remove(stringBuilder4.Length - 1, 1);
			File.AppendAllText(MainForm.PCMLogFilename, Environment.NewLine + stringBuilder4.ToString() + Environment.NewLine);
		}
		else
		{
			stringBuilder4.Append("NO ONE-TRIP FAULT CODE");
			File.AppendAllText(MainForm.PCMLogFilename, Environment.NewLine + stringBuilder4.ToString() + Environment.NewLine + Environment.NewLine);
		}
		FaultCodes1TSaved = true;
	}
}
