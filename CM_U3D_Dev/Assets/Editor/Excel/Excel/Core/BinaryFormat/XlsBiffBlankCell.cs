namespace Excel.Core.BinaryFormat
{
    using System;

    internal class XlsBiffBlankCell : XlsBiffRecord
    {
        internal XlsBiffBlankCell(byte[] bytes) : this(bytes, 0)
        {
        }

        internal XlsBiffBlankCell(byte[] bytes, uint offset) : base(bytes, offset)
        {
        }

        public ushort ColumnIndex =>
            base.ReadUInt16(2);

        public ushort RowIndex =>
            base.ReadUInt16(0);

        public ushort XFormat =>
            base.ReadUInt16(4);
    }
}

