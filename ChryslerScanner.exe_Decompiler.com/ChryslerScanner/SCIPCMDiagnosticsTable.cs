using System;
using System.Collections.Generic;
using ChryslerScanner.Helpers;
using ChryslerScanner.Properties;

namespace ChryslerScanner;

public class SCIPCMDiagnosticsTable
{
	public delegate void TableUpdatedEventHandler(object sender, EventArgs e);

	public List<string> Table = new List<string>();

	public List<string> RAMDumpTable = new List<string>();

	public bool RAMDumpTableVisible;

	public byte RAMTableAddress;

	public List<ushort> IDByteList = new List<ushort>();

	public List<byte> UniqueIDByteList = new List<byte>();

	public const int ListStart = 5;

	public int LastUpdatedLine = 1;

	public event TableUpdatedEventHandler TableUpdated;

	public SCIPCMDiagnosticsTable()
	{
		InitSCIPCMTable();
		InitRAMDumpTable();
		OnTableUpdated(EventArgs.Empty);
	}

	public void InitSCIPCMTable()
	{
		Table.Clear();
		Table.Add("┌─────────────────────────┐                                                                                              ");
		Table.Add("│ SCI-BUS (SAE J2610) PCM │ STATE: N/A                                                                                   ");
		Table.Add("├─────────────────────────┼─────────────────────────────────────────────────────┬─────────────────────────┬─────────────┐");
		Table.Add("│ MESSAGE [HEX]           │ DESCRIPTION                                         │ VALUE                   │ UNIT        │");
		Table.Add("╞═════════════════════════╪═════════════════════════════════════════════════════╪═════════════════════════╪═════════════╡");
		Table.Add("│                         │                                                     │                         │             │");
		Table.Add("└─────────────────────────┴─────────────────────────────────────────────────────┴─────────────────────────┴─────────────┘");
	}

	public void InitRAMDumpTable()
	{
		RAMDumpTable.Clear();
		RAMDumpTable.Add("                                                        ");
		RAMDumpTable.Add("┌────┬─────────────────────────────────────────────────┐");
		RAMDumpTable.Add("│ FX │ 00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F │");
		RAMDumpTable.Add("├────┼─────────────────────────────────────────────────┤");
		RAMDumpTable.Add("│ 00 │                                                 │");
		RAMDumpTable.Add("│ 10 │                                                 │");
		RAMDumpTable.Add("│ 20 │                                                 │");
		RAMDumpTable.Add("│ 30 │                                                 │");
		RAMDumpTable.Add("│ 40 │                                                 │");
		RAMDumpTable.Add("│ 50 │                                                 │");
		RAMDumpTable.Add("│ 60 │                                                 │");
		RAMDumpTable.Add("│ 70 │                                                 │");
		RAMDumpTable.Add("│ 80 │                                                 │");
		RAMDumpTable.Add("│ 90 │                                                 │");
		RAMDumpTable.Add("│ A0 │                                                 │");
		RAMDumpTable.Add("│ B0 │                                                 │");
		RAMDumpTable.Add("│ C0 │                                                 │");
		RAMDumpTable.Add("│ D0 │                                                 │");
		RAMDumpTable.Add("│ E0 │                                                 │");
		RAMDumpTable.Add("├────┴─────────────────────────────────────────────────┤");
		RAMDumpTable.Add("│                              TIMESTAMP: 00:00:00.000 │");
		RAMDumpTable.Add("└──────────────────────────────────────────────────────┘");
	}

	public void UpdateHeader(string row)
	{
		Table.RemoveAt(1);
		Table.Insert(1, row);
		OnTableUpdated(EventArgs.Empty);
	}

	public void AddRow(ushort modifiedID, string row)
	{
		int num = 0;
		if (!IDByteList.Contains(modifiedID))
		{
			byte item = (byte)((uint)(modifiedID >> 8) & 0xFFu);
			if (!UniqueIDByteList.Contains(item))
			{
				UniqueIDByteList.Add(item);
			}
			IDByteList.Add(modifiedID);
			if (Settings.Default.SortByID)
			{
				IDByteList.Sort();
			}
			num = IDByteList.FindIndex((ushort x) => x == modifiedID);
			if (IDByteList.Count == 1)
			{
				Table.RemoveAt(5);
				Table.Insert(5, row);
			}
			else
			{
				Table.Insert(5 + num, row);
			}
			LastUpdatedLine = 5 + num;
		}
		else
		{
			num = IDByteList.FindIndex((ushort x) => x == modifiedID);
			Table.RemoveAt(5 + num);
			Table.Insert(5 + num, row);
			LastUpdatedLine = 5 + num;
		}
	}

	public void AddRAMTableDump(byte[] data)
	{
		if (data[4] != 254 && data[4] >= 240)
		{
			if (data[4] != RAMTableAddress)
			{
				InitRAMDumpTable();
			}
			RAMTableAddress = data[4];
			RAMDumpTable[2] = RAMDumpTable[2].Remove(2, 2).Insert(2, Util.ByteToHexString(data, 4));
			TimeSpan value = TimeSpan.FromMilliseconds((data[0] << 24) + (data[1] << 16) + (data[2] << 8) + data[3]);
			string value2 = DateTime.Today.Add(value).ToString("HH:mm:ss.fff");
			RAMDumpTable[20] = RAMDumpTable[20].Remove(42, 12).Insert(42, value2);
			byte[] array = new byte[data.Length - 5];
			Array.Copy(data, 5, array, 0, array.Length);
			for (int i = 0; i < array.Length / 2; i++)
			{
				int index = 4 + (array[i * 2] >> 4);
				int startIndex = 7 + 3 * (array[i * 2] & 0xF);
				RAMDumpTable[index] = RAMDumpTable[index].Remove(startIndex, 2).Insert(startIndex, Util.ByteToHexString(array, i * 2 + 1));
			}
			if (RAMDumpTableVisible)
			{
				Table.RemoveRange(Table.Count - 22, 22);
			}
			Table.AddRange(RAMDumpTable);
			RAMDumpTableVisible = true;
		}
	}

	public virtual void OnTableUpdated(EventArgs e)
	{
		this.TableUpdated?.Invoke(this, e);
	}
}
