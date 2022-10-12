namespace Excel.Core.BinaryFormat
{
    using System;

    internal class XlsBiffRow : XlsBiffRecord
    {
        internal XlsBiffRow(byte[] bytes) : this(bytes, 0)
        {
        }

        internal XlsBiffRow(byte[] bytes, uint offset) : base(bytes, offset)
        {
        }

        public ushort FirstDefinedColumn =>
            base.ReadUInt16(2);

        public ushort Flags =>
            base.ReadUInt16(12);

        public ushort LastDefinedColumn =>
            base.ReadUInt16(4);

        public uint RowHeight =>
            base.ReadUInt16(6);

        public ushort RowIndex =>
            base.ReadUInt16(0);

        public ushort XFormat =>
            base.ReadUInt16(14);
    }
}

