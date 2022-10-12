
namespace ServerModel.Models
{
    public class YAMLABTableItemInfo
    {
        public string N { set; get; }
        public int S { set; get; }
        public string H { set; get; }
    }

    public class YAMLABConfigInfo
    {
        public string Ver { set; get; }
        public YAMLABTableItemInfo[] List { set; get; }
    }
}
