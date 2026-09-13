namespace DRBDBReader.DB.Records;

public class DADRecord : Record
{
	private const byte FIELD_ID = 0;

	private const byte FIELD_REQUEST_LENGTH = 1;

	private const byte FIELD_RESPONSE_LENGTH = 3;

	private const byte FIELD_EXTRACT_OFFSET = 5;

	private const byte FIELD_EXTRACT_SIZE = 6;

	private const byte FIELD_EMPTY_ONE = 7;

	private const byte FIELD_EMPTY_TWO = 8;

	private const byte FIELD_PROTOCOL = 10;

	public ushort id;

	public byte requestLength;

	public byte responseLength;

	public byte extractOffset;

	public byte extractSize;

	public ushort protocolid;

	public DADRecord(Table table, byte[] record)
		: base(table, record)
	{
		id = (ushort)base.table.readField(this, 0);
		requestLength = (byte)base.table.readField(this, 1);
		responseLength = (byte)base.table.readField(this, 3);
		extractOffset = (byte)base.table.readField(this, 5);
		extractSize = (byte)base.table.readField(this, 6);
		protocolid = (ushort)base.table.readField(this, 10);
	}
}
