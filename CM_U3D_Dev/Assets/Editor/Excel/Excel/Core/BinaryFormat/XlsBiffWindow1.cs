namespace Excel.Core.BinaryFormat
{
    using System;

    internal class XlsBiffWindow1 : XlsBiffRecord
    {
        internal XlsBiffWindow1(byte[] bytes) : this(bytes, 0)
        {
        }

        internal XlsBiffWindow1(byte[] bytes, uint offset) : base(bytes, offset)
        {
        }

        public ushort ActiveTab =>
            base.ReadUInt16(10);

        public ushort FirstVisibleTab =>
            base.ReadUInt16(12);

        public Window1Flags Flags =>
            ((Window1Flags) base.ReadUInt16(8));

        public ushort Height =>
            base.ReadUInt16(6);

        public ushort Left =>
            base.ReadUInt16(0);

        public ushort SelectedTabCount =>
            base.ReadUInt16(14);

        public ushort TabRatio =>
            base.ReadUInt16(0x10);

        public ushort Top =>
            base.ReadUInt16(2);

        public ushort Width =>
            base.ReadUInt16(4);

        [Flags]
        public enum Window1Flags : ushort
        {
            Hidden = 1,
            HScrollVisible = 8,
            Minimized = 2,
            VScrollVisible = 0x10,
            WorkbookTabs = 0x20
        }
    }
}

