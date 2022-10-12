namespace Excel.Core.BinaryFormat
{
    using System;

    internal class XlsBiffLabelSSTCell : XlsBiffBlankCell
    {
        internal XlsBiffLabelSSTCell(byte[] bytes) : this(bytes, 0)
        {
        }

        internal XlsBiffLabelSSTCell(byte[] bytes, uint offset) : base(bytes, offset)
        {
        }

        public string Text(XlsBiffSST sst) => 
            sst.GetString(this.SSTIndex);

        public uint SSTIndex =>
            base.ReadUInt32(6);
    }
}

