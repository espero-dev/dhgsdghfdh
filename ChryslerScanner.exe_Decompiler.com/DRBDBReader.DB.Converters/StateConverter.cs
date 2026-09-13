using System;
using System.Collections.Generic;
using DRBDBReader.DB.Records;

namespace DRBDBReader.DB.Converters;

public class StateConverter : Converter
{
	public SCRecord scRecord;

	public SDSRecord sdsRecord;

	public SortedDictionary<ushort, string> entries;

	public StateConverter(Database db, byte[] record, ushort cfid, ushort dsid)
		: base(db, record, cfid, dsid)
	{
		Table table = base.db.tables[4];
		scRecord = (SCRecord)table.getRecord(base.cfid, 0);
		entries = new SortedDictionary<ushort, string>();
		buildStateList();
	}

	protected virtual void buildStateList()
	{
		Table table = db.tables[13];
		dsRecord = table.getRecord(dsid, 0);
		sdsRecord = (SDSRecord)dsRecord;
		Record[] records = db.tables[15].records;
		for (int i = 0; i < records.Length; i++)
		{
			StateEntryRecord stateEntryRecord = (StateEntryRecord)records[i];
			if (stateEntryRecord.dsidThing == dsid)
			{
				entries.Add(stateEntryRecord.value, stateEntryRecord.nameString);
			}
		}
	}

	public override string processData(long data, bool outputMetric = false)
	{
		ushort entryID = getEntryID((ushort)data);
		if (entries.ContainsKey(entryID))
		{
			return entries[entryID];
		}
		return sdsRecord.defaultString;
	}

	protected virtual ushort getEntryID(ushort val)
	{
		if (scRecord.mask != 0)
		{
			val &= scRecord.mask;
		}
		return val;
	}

	public override string ToString()
	{
		string text = base.ToString() + Environment.NewLine;
		if (sdsRecord != null)
		{
			text = text + Environment.NewLine + "DFLT: " + sdsRecord.defaultString;
		}
		foreach (KeyValuePair<ushort, string> entry in entries)
		{
			text = text + Environment.NewLine + "0x" + entry.Key.ToString("X2") + ": " + entry.Value;
		}
		return text;
	}
}
