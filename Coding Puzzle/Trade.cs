namespace Coding_Puzzle
{
    using FileHelpers;

    [DelimitedRecord(",")]
    [IgnoreEmptyLines]
    [IgnoreFirst]
    public class Trade
    {
        public string? TradeID;

        public string? ISIN;

        // <summary>
        // Converts Notional to int 32
        // </summary>
        [FieldConverter(ConverterKind.Int32)]
        public int Notional;
    }
}