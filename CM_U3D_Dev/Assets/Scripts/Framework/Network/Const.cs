namespace MTool.Framework.Network
{
    public static class Const
    {
        /// <summary>
        /// 数据包最大长度
        /// </summary>
        public static readonly int Max_Package_Size = 64 * 1024;

        /// <summary>
        /// 数据头长度
        /// </summary>
        public static readonly int Head_Size = 4;

        /// <summary>
        /// 缓冲区长度
        /// </summary>
        public static readonly int Recv_Buffer_Size = Max_Package_Size + Head_Size;

        /// <summary>
        /// 网络消息流的最大长度
        /// </summary>
        public static readonly int Stream_Max_Length = 2 * Recv_Buffer_Size;

        /// <summary>
        /// 网络操作消息队列容量
        /// </summary>
        public static readonly int Max_OptionMsg_Capacity = 16;

        /// <summary>
        /// 消息队列容量
        /// </summary>
        public static readonly int Max_Msg_Capacity = 128;

        /// <summary>
        /// 消息处理字典容量
        /// </summary>
        public static readonly int Msg_Handler_Capacity = 1024;

        /// <summary>
        /// 唯一消息处理字典容量
        /// </summary>
        public static readonly int Msg_Unique_Handler_Capacity = 32;
    }
}