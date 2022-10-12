namespace Excel.Core.BinaryFormat
{
    using System;

    internal class XlsBiffSimpleValueRecord : XlsBiffRecord
    {
        internal XlsBiffSimpleValueRecord(byte[] bytes) : this(bytes, 0)
        {
        }

        internal XlsBiffSimpleValueRecord(byte[] bytes, uint offset) : base(bytes, offset)
        {
        }

        public ushort Value =>
            base.ReadUInt16(0);
    }
}

