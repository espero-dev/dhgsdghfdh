using System;
using DRBDBReader.DB.Records;

namespace DRBDBReader.DB.Converters;

public class BinaryStateConverter : StateConverter
{
	public BDSRecord bdsRecord;

	public BinaryStateConverter(Database db, byte[] record, ushort cfid, ushort dsid)
		: base(db, record, cfid, dsid)
	{
	}

	protected override void buildStateList()
	{
		Table table = db.tables[2];
		dsRecord = table.getRecord(dsid, 0);
		bdsRecord = (BDSRecord)dsRecord;
		entries.Add(0, bdsRecord.falseString);
		entries.Add(1, bdsRecord.trueString);
	}

	protected override ushort getEntryID(ushort val)
	{
		return scRecord.op switch
		{
			Operator.GREATER => (val > scRecord.mask) ? ((ushort)1) : ((ushort)0), 
			Operator.LESS => (val < scRecord.mask) ? ((ushort)1) : ((ushort)0), 
			Operator.MASK_ZERO => ((val & scRecord.mask) == 0) ? ((ushort)1) : ((ushort)0), 
			Operator.MASK_NOT_ZERO => ((val & scRecord.mask) != 0) ? ((ushort)1) : ((ushort)0), 
			Operator.NOT_EQUAL => (val != scRecord.mask) ? ((ushort)1) : ((ushort)0), 
			_ => (val == scRecord.mask) ? ((ushort)1) : ((ushort)0), 
		};
	}

	public override string ToString()
	{
		return string.Concat((base.ToString() + Environment.NewLine).Replace(Environment.NewLine + "0x00: ", Environment.NewLine + "FALSE: ").Replace(Environment.NewLine + "0x01: ", Environment.NewLine + "TRUE:  ") + Environment.NewLine + "MASK:  0x" + scRecord.mask.ToString("X2"), Environment.NewLine, "OP:    ", scRecord.op.ToString());
	}
}
