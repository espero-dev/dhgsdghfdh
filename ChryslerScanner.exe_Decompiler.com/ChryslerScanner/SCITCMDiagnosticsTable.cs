using System;
using System.Collections.Generic;
using ChryslerScanner.Properties;

namespace ChryslerScanner;

public class SCITCMDiagnosticsTable
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

	public SCITCMDiagnosticsTable()
	{
		InitSCITCMTable();
		OnTableUpdated(EventArgs.Empty);
	}

	public void InitSCITCMTable()
	{
		Table.Clear();
		Table.Add("┌─────────────────────────┐                                                                                              ");
		Table.Add("│ SCI-BUS (SAE J2610) TCM │ STATE: N/A                                                                                   ");
		Table.Add("├─────────────────────────┼─────────────────────────────────────────────────────┬─────────────────────────┬─────────────┐");
		Table.Add("│ MESSAGE [HEX]           │ DESCRIPTION                                         │ VALUE                   │ UNIT        │");
		Table.Add("╞═════════════════════════╪═════════════════════════════════════════════════════╪═════════════════════════╪═════════════╡");
		Table.Add("│                         │                                                     │                         │             │");
		Table.Add("└─────────────────────────┴─────────────────────────────────────────────────────┴─────────────────────────┴─────────────┘");
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

	public virtual void OnTableUpdated(EventArgs e)
	{
		this.TableUpdated?.Invoke(this, e);
	}
}
