
namespace ServerModel.Models
{
    public class SFTPServerInfo
    {
        public string ServerName { set; get; }

        public int Port { set; get; }

        public string UserName { set; get; }

        public string Password { set; get; }

        public string SshHostKeyFingerprint { set; get; }

        public string SourceDir { set; get; }

        public string DesDir { set; get; }

        public string FileMask { set; get; }

        public string SourceDir2 { set; get; }

        public string[] ExtraFiles { set; get; }

    }
}
