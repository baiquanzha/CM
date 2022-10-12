using System.IO;

namespace MTool.Framework.Network
{
    public interface IMsgProcesser
    {
        /// <summary>
        /// 处理发送数据
        /// </summary>
        /// <param name="data">发送数据包的字节数组</param>
        /// <param name="stream">发送流</param>
        void WriteSendBytes(byte[] data, Stream stream);

        /// <summary>
        /// 处理读取数据
        /// </summary>
        /// <param name="stream">读取流</param>
        /// <param name="msgData">读取数据包的字节数组</param>
        /// <returns>读取结果</returns>
        bool ReadRecvBytes(Stream stream, out byte[] msgData);

        /// <summary>
        /// 数据包分发处理
        /// </summary>
        /// <param name="data">数据包</param>
        void DispatchMsg(byte[] data);
    }
}