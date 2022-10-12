namespace Excel.Core.BinaryFormat
{
    using Excel.Exceptions;
    using System;
    using System.Text;

    internal class XlsDirectoryEntry
    {
        public const int Length = 0x80;
        private readonly byte[] m_bytes;
        private XlsDirectoryEntry m_child;
        private XlsHeader m_header;
        private XlsDirectoryEntry m_leftSibling;
        private XlsDirectoryEntry m_rightSibling;

        public XlsDirectoryEntry(byte[] bytes, XlsHeader header)
        {
            if (bytes.Length < 0x80)
            {
                throw new BiffRecordException("Directory Entry error: Array is too small.");
            }
            this.m_bytes = bytes;
            this.m_header = header;
        }

        public XlsDirectoryEntry Child
        {
            get => 
                this.m_child;
            set
            {
                if (this.m_child == null)
                {
                    this.m_child = value;
                }
            }
        }

        public uint ChildSid =>
            BitConverter.ToUInt32(this.m_bytes, 0x4c);

        public Guid ClassId
        {
            get
            {
                byte[] dst = new byte[0x10];
                Buffer.BlockCopy(this.m_bytes, 80, dst, 0, 0x10);
                return new Guid(dst);
            }
        }

        public DateTime CreationTime =>
            DateTime.FromFileTime(BitConverter.ToInt64(this.m_bytes, 100));

        public DECOLOR EntryColor =>
            ((DECOLOR) Buffer.GetByte(this.m_bytes, 0x43));

        public ushort EntryLength =>
            BitConverter.ToUInt16(this.m_bytes, 0x40);

        public string EntryName =>
            Encoding.Unicode.GetString(this.m_bytes, 0, this.EntryLength).TrimEnd(new char[1]);

        public STGTY EntryType =>
            ((STGTY) Buffer.GetByte(this.m_bytes, 0x42));

        public bool IsEntryMiniStream =>
            (this.StreamSize < this.m_header.MiniStreamCutoff);

        public DateTime LastWriteTime =>
            DateTime.FromFileTime(BitConverter.ToInt64(this.m_bytes, 0x6c));

        public XlsDirectoryEntry LeftSibling
        {
            get => 
                this.m_leftSibling;
            set
            {
                if (this.m_leftSibling == null)
                {
                    this.m_leftSibling = value;
                }
            }
        }

        public uint LeftSiblingSid =>
            BitConverter.ToUInt32(this.m_bytes, 0x44);

        public uint PropType =>
            BitConverter.ToUInt32(this.m_bytes, 0x7c);

        public XlsDirectoryEntry RightSibling
        {
            get => 
                this.m_rightSibling;
            set
            {
                if (this.m_rightSibling == null)
                {
                    this.m_rightSibling = value;
                }
            }
        }

        public uint RightSiblingSid =>
            BitConverter.ToUInt32(this.m_bytes, 0x48);

        public uint StreamFirstSector =>
            BitConverter.ToUInt32(this.m_bytes, 0x74);

        public uint StreamSize =>
            BitConverter.ToUInt32(this.m_bytes, 120);

        public uint UserFlags =>
            BitConverter.ToUInt32(this.m_bytes, 0x60);
    }
}

