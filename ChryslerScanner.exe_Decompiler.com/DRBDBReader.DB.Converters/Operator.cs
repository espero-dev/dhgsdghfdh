namespace DRBDBReader.DB.Converters;

public enum Operator : byte
{
	EQUAL = 61,
	NOT_EQUAL = 33,
	GREATER = 62,
	LESS = 60,
	MASK_ZERO = 48,
	MASK_NOT_ZERO = 57
}
