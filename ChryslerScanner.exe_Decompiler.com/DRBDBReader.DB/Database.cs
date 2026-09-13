using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using DRBDBReader.DB.Records;

namespace DRBDBReader.DB;

public class Database
{
	public const ushort TABLE_MODULE = 0;

	public const ushort TABLE_DES_INFO = 1;

	public const ushort TABLE_BINARY_DATA_SPECIFIER = 2;

	public const ushort TABLE_UNKNOWN_3 = 3;

	public const ushort TABLE_CONVERTERS_STATE = 4;

	public const ushort TABLE_CONVERTERS_NUMERIC = 5;

	public const ushort TABLE_SERIVCE_CAT_STUFFS = 6;

	public const ushort TABLE_QUALIFIER = 7;

	public const ushort TABLE_DATA_ACQUISITION_DESCRIPTION = 8;

	public const ushort TABLE_DRB_MENU = 9;

	public const ushort TABLE_MODULE_DATAELEMENT = 10;

	public const ushort TABLE_UNKNOWN_11 = 11;

	public const ushort TABLE_EMPTY_12 = 12;

	public const ushort TABLE_STATE_DATA_SPECIFIER = 13;

	public const ushort TABLE_UNKNOWN_14 = 14;

	public const ushort TABLE_STATE_ENTRY = 15;

	public const ushort TABLE_STRINGS = 16;

	public const ushort TABLE_NUMERIC_DATA_SPECIFIER = 17;

	public const ushort TABLE_DATAELEMENT_QUALIFIER = 18;

	public const ushort TABLE_UNKNOWN_19 = 19;

	public const ushort TABLE_UNKNOWN_20 = 20;

	public const ushort TABLE_UNKNOWN_21 = 21;

	public const ushort TABLE_UNKNOWN_22 = 22;

	public const ushort TABLE_TRANSMIT = 23;

	public const ushort TABLE_EMPTY_24 = 24;

	public const ushort TABLE_EMPTY_25 = 25;

	public const ushort TABLE_DBTEXT_1 = 26;

	public const ushort TABLE_DBTEXT_2 = 27;

	private FileInfo dbFile;

	public SimpleBinaryReader dbReader;

	public Table[] tables;

	private static ushort[] syncedTableReadOrder = new ushort[28]
	{
		26, 27, 16, 12, 24, 25, 3, 11, 14, 19,
		20, 21, 22, 13, 2, 17, 4, 5, 15, 8,
		7, 18, 9, 1, 6, 0, 23, 10
	};

	private static ushort[] primaryTableReadOrder = new ushort[10] { 3, 21, 13, 2, 17, 15, 9, 1, 6, 0 };

	private static ushort[] secondaryTableReadOrder = new ushort[5] { 18, 7, 4, 5, 8 };

	private static ushort[] finalTableReadOrder = new ushort[2] { 23, 10 };

	private static ushort[] noDependencyTables = new ushort[8] { 12, 24, 25, 11, 14, 19, 20, 22 };

	public bool isStarScanDB;

	public Database(FileInfo dbFile)
	{
		this.dbFile = dbFile;
		using (FileStream fileStream = new FileStream(this.dbFile.FullName, FileMode.Open, FileAccess.Read))
		{
			dbReader = new SimpleBinaryReader(fileStream);
		}
		isStarScanDB = checkStarScan();
		makeTables();
	}

	private bool checkStarScan()
	{
		int offset = dbReader.rawDB.Length - 23;
		return dbReader.ReadBytes(ref offset, 8).SequenceEqual(new byte[8] { 83, 116, 97, 114, 83, 67, 65, 78 });
	}

	private void makeTables()
	{
		int offset = 0;
		dbReader.ReadUInt32(ref offset);
		dbReader.ReadUInt16(ref offset);
		ushort num = dbReader.ReadUInt16(ref offset);
		tables = new Table[num];
		for (ushort num2 = 0; num2 < num; num2++)
		{
			uint offset2 = dbReader.ReadUInt32(ref offset);
			ushort rowCount = dbReader.ReadUInt16(ref offset);
			ushort rowSize = dbReader.ReadUInt16(ref offset);
			byte b = dbReader.ReadUInt8(ref offset);
			byte[] array = dbReader.ReadBytes(ref offset, b);
			offset += 27 - b;
			List<byte> list = new List<byte>();
			for (byte b2 = 0; b2 < b; b2++)
			{
				if (array[b2] != 0)
				{
					list.Add(array[b2]);
				}
			}
			tables[num2] = new Table(this, num2, offset2, rowCount, rowSize, (byte)list.Count, Enumerable.ToArray(list));
		}
		Thread thread = new Thread(readTables);
		thread.Start(noDependencyTables);
		Thread thread2 = new Thread(readTables);
		Thread thread3 = new Thread(readTables);
		Thread thread4 = new Thread(readTables);
		thread2.Start(new ushort[1] { 26 });
		thread3.Start(new ushort[1] { 27 });
		thread3.Join();
		thread2.Join();
		thread4.Start(new ushort[1] { 16 });
		thread4.Join();
		Thread thread5 = new Thread(readTables);
		Thread thread6 = new Thread(readTables);
		thread5.Start(primaryTableReadOrder);
		thread6.Start(secondaryTableReadOrder);
		thread6.Join();
		thread5.Join();
		Thread thread7 = new Thread(readTables);
		thread7.Start(finalTableReadOrder);
		thread7.Join();
		thread.Join();
	}

	private void readTables(object tableReadOrder)
	{
		ushort[] array = (ushort[])tableReadOrder;
		foreach (ushort num in array)
		{
			tables[num].readRecords();
		}
	}

	public string getString(ushort id)
	{
		Record record = tables[16].getRecord(id, 0);
		if (record == null)
		{
			return "(null)";
		}
		return ((StringRecord)record).text;
	}

	public string getServiceCatString(ushort id)
	{
		Record record = tables[6].getRecord(id, 3);
		if (record == null)
		{
			return "(null)";
		}
		return ((ServiceCatRecord)record).name;
	}

	public string getDESString(ushort id)
	{
		Record record = tables[1].getRecord(id, 0);
		if (record == null)
		{
			return "(null)";
		}
		return ((DESRecord)record).name;
	}

	public string getProtocolText(ushort id)
	{
		return id switch
		{
			1 => "J1850", 
			53 => "CCD", 
			60 => "SCI", 
			103 => "ISO", 
			159 => "Multimeter", 
			160 => "J2190?", 
			_ => "P" + id, 
		};
	}

	public string getTX(uint id)
	{
		Record record = tables[23].getRecord(id, 0);
		if (record == null)
		{
			return null;
		}
		TXRecord tXRecord = (TXRecord)record;
		string text = getProtocolText(tXRecord.dadRecord.protocolid) + "; ";
		return tXRecord.name + ": " + text + "xmit: " + tXRecord.xmitstring + "; sc: " + tXRecord.scname;
	}

	public string getDetailedTX(uint id)
	{
		Record record = tables[23].getRecord(id, 0);
		if (record == null)
		{
			return null;
		}
		TXRecord tXRecord = (TXRecord)record;
		string text = getProtocolText(tXRecord.dadRecord.protocolid) + "; ";
		string text2 = "";
		text2 = text2 + Environment.NewLine + "dadreqlen: " + tXRecord.dadRecord.requestLength + "; dadresplen: " + tXRecord.dadRecord.responseLength + ";";
		text2 = text2 + Environment.NewLine + "dadextroff: " + tXRecord.dadRecord.extractOffset + "; dadextrsize: " + tXRecord.dadRecord.extractSize + ";";
		text2 = text2 + Environment.NewLine + "desid: " + tXRecord.dataelemsetid + "; desname: " + getDESString(tXRecord.dataelemsetid) + ";";
		text2 = text2 + Environment.NewLine + "record: " + BitConverter.ToString(tXRecord.record) + ";";
		return tXRecord.name + ": " + text + "xmit: " + tXRecord.xmitstring + "; sc: " + tXRecord.scname + ";" + text2;
	}

	public string getModule(ushort id)
	{
		Record record = tables[0].getRecord(id, 0);
		if (record == null)
		{
			return null;
		}
		ModuleRecord moduleRecord = (ModuleRecord)record;
		return moduleRecord.name + "; sc: " + moduleRecord.scname;
	}
}
