using System;
using System.Collections.Generic;
using ChryslerScanner.Properties;

namespace ChryslerScanner;

public class CCDDiagnosticsTable
{
	public delegate void TableUpdatedEventHandler(object sender, EventArgs e);

	public List<string> Table = new List<string>();

	public List<ushort> IDByteList = new List<ushort>();

	public List<byte> UniqueIDByteList = new List<byte>();

	public List<byte> B2F2IDByteList = new List<byte>(2);

	public int B2Row = 5;

	public int F2Row = 6;

	public const int ListStart = 8;

	public int LastUpdatedLine = 1;

	public event TableUpdatedEventHandler TableUpdated;

	public CCDDiagnosticsTable()
	{
		InitCCDTable();
		OnTableUpdated(EventArgs.Empty);
	}

	public void InitCCDTable()
	{
		Table.Clear();
		Table.Add("┌─────────────────────────┐                                                                                              ");
		Table.Add("│ CCD-BUS (SAE J1567)     │ STATE: N/A                                                                                   ");
		Table.Add("├─────────────────────────┼─────────────────────────────────────────────────────┬─────────────────────────┬─────────────┐");
		Table.Add("│ MESSAGE [HEX]           │ DESCRIPTION                                         │ VALUE                   │ UNIT        │");
		Table.Add("╞═════════════════════════╪═════════════════════════════════════════════════════╪═════════════════════════╪═════════════╡");
		Table.Add("│ B2 -- -- -- -- --       │ REQUEST  |                                          │                         │             │");
		Table.Add("│ F2 -- -- -- -- --       │ RESPONSE |                                          │                         │             │");
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
		if (!IDByteList.Contains(modifiedID) && modifiedID >> 8 != 178 && modifiedID >> 8 != 242)
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
				Table.RemoveAt(8);
				Table.Insert(8, row);
			}
			else
			{
				Table.Insert(8 + num, row);
			}
			LastUpdatedLine = 8 + num;
		}
		else if (IDByteList.Contains(modifiedID) && modifiedID >> 8 != 178 && modifiedID >> 8 != 242)
		{
			num = IDByteList.FindIndex((ushort x) => x == modifiedID);
			Table.RemoveAt(8 + num);
			Table.Insert(8 + num, row);
			LastUpdatedLine = 8 + num;
		}
		switch (modifiedID >> 8)
		{
		case 178:
			if (!B2F2IDByteList.Contains(178))
			{
				B2F2IDByteList.Add(178);
			}
			B2F2IDByteList.Sort();
			Table.RemoveAt(B2Row);
			Table.Insert(B2Row, row);
			LastUpdatedLine = B2Row;
			break;
		case 242:
			if (!B2F2IDByteList.Contains(242))
			{
				B2F2IDByteList.Add(242);
			}
			B2F2IDByteList.Sort();
			Table.RemoveAt(F2Row);
			Table.Insert(F2Row, row);
			LastUpdatedLine = F2Row;
			break;
		}
	}

	public virtual void OnTableUpdated(EventArgs e)
	{
		this.TableUpdated?.Invoke(this, e);
	}
}
