

namespace ServerModel.Models
{
    public class SFTPUpLoadFilesInfo
    {
        public string[] SourceFiles { set; get; }

        public string[] TargetFiles { set; get; }


        public bool IsEmpty() 
        {
            bool isNotEmpty = ((this.SourceFiles != null) && (this.SourceFiles.Length > 0)) &&
                ((this.TargetFiles != null) && (this.TargetFiles.Length > 0));
            return !isNotEmpty;
        }


        public bool IsValidated() 
        {
            if (!this.IsEmpty()) 
            {
                if (this.SourceFiles.Length == this.TargetFiles.Length) 
                {
                    return true;
                }

                return false;
            }

            return false;
        }
    }
}
