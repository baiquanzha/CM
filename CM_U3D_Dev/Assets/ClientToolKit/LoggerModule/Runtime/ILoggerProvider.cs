namespace MTool.LoggerModule.Runtime
{
    public interface ILoggerProvider
    {
        /// <summary>
        /// Logger名字
        /// </summary>
        string Name { get; }

        /// <summary>
        /// 根据名字获取Logger
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        ILogger GetLogger(string name);

        /// <summary>
        /// 终止Logger服务
        /// </summary>
        void Shutdown();
    }
}
