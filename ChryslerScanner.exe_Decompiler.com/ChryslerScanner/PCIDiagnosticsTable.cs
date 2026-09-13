using System;
using System.Collections.Generic;
using ChryslerScanner.Properties;

namespace ChryslerScanner;

public class PCIDiagnosticsTable
{
	public delegate void TableUpdatedEventHandler(object sender, EventArgs e);

	public List<string> Table = new List<string>();

	public List<ushort> IDByteList = new List<ushort>();

	public List<byte> UniqueIDByteList = new List<byte>();

	public List<byte> IDByte2426List = new List<byte>(2);

	public int Row24 = 5;

	public int Row26 = 6;

	public int Row48 = 7;

	public int Row68 = 8;

	public const int ListStart = 10;

	public int LastUpdatedLine = 1;

	public event TableUpdatedEventHandler TableUpdated;

	public PCIDiagnosticsTable()
	{
		InitPCITable();
		OnTableUpdated(EventArgs.Empty);
	}

	public void InitPCITable()
	{
		Table.Clear();
		Table.Add("┌─────────────────────────┐                                                                                              ");
		Table.Add("│ PCI-BUS (SAE J1850 VPW) │ STATE: N/A                                                                                   ");
		Table.Add("├─────────────────────────┼─────────────────────────────────────────────────────┬─────────────────────────┬─────────────┐");
		Table.Add("│ MESSAGE [HEX]           │ DESCRIPTION                                         │ VALUE                   │ UNIT        │");
		Table.Add("╞═════════════════════════╪═════════════════════════════════════════════════════╪═════════════════════════╪═════════════╡");
		Table.Add("│ 24 -- -- -- -- -- --    │ REQUEST  |                                          │                         │             │");
		Table.Add("│ 26 -- -- -- -- -- --    │ RESPONSE |                                          │                         │             │");
		Table.Add("│ 48 -- -- -- -- -- --    │ REQUEST  |                                          │                         │             │");
		Table.Add("│ 68 -- -- -- -- --       │ RESPONSE |                                          │                         │             │");
		Table.Add("├─────────────────────────┼─────────────────────────────────────────────────────┼─────────────────────────┼─────────────┤");
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
		if (!IDByteList.Contains(modifiedID) && modifiedID >> 8 != 36 && modifiedID >> 8 != 38 && modifiedID >> 8 != 72 && modifiedID >> 8 != 104)
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
				Table.RemoveAt(10);
				Table.Insert(10, row);
			}
			else
			{
				Table.Insert(10 + num, row);
			}
			LastUpdatedLine = 10 + num;
		}
		else if (IDByteList.Contains(modifiedID) && modifiedID >> 8 != 36 && modifiedID >> 8 != 38 && modifiedID >> 8 != 72 && modifiedID >> 8 != 104)
		{
			num = IDByteList.FindIndex((ushort x) => x == modifiedID);
			Table.RemoveAt(10 + num);
			Table.Insert(10 + num, row);
			LastUpdatedLine = 10 + num;
		}
		switch (modifiedID >> 8)
		{
		case 36:
			if (!IDByte2426List.Contains(36))
			{
				IDByte2426List.Add(36);
			}
			IDByte2426List.Sort();
			Table.RemoveAt(Row24);
			Table.Insert(Row24, row);
			LastUpdatedLine = Row24;
			break;
		case 38:
			if (!IDByte2426List.Contains(38))
			{
				IDByte2426List.Add(38);
			}
			IDByte2426List.Sort();
			Table.RemoveAt(Row26);
			Table.Insert(Row26, row);
			LastUpdatedLine = Row26;
			break;
		case 72:
			if (!IDByte2426List.Contains(72))
			{
				IDByte2426List.Add(72);
			}
			IDByte2426List.Sort();
			Table.RemoveAt(Row48);
			Table.Insert(Row48, row);
			LastUpdatedLine = Row48;
			break;
		case 104:
			if (!IDByte2426List.Contains(104))
			{
				IDByte2426List.Add(104);
			}
			IDByte2426List.Sort();
			Table.RemoveAt(Row68);
			Table.Insert(Row68, row);
			LastUpdatedLine = Row68;
			break;
		}
	}

	public virtual void OnTableUpdated(EventArgs e)
	{
		this.TableUpdated?.Invoke(this, e);
	}
}
