using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace MTool.AppBuilder.Editor.Builds.Primitives
{
    public class Arguments
    {
        //--------------------------------------------------------------
        #region Fields
        //--------------------------------------------------------------

        private Dictionary<string, string> args = new Dictionary<string, string>();

        #endregion

        //--------------------------------------------------------------
        #region Properties & Events
        //--------------------------------------------------------------

        public static Arguments Empty { get { return new Arguments(); } }


        #endregion

        //--------------------------------------------------------------
        #region Creation & Cleanup
        //--------------------------------------------------------------

        public Arguments(params string[] src)
        {
            if (src == null)
            {
                return;
            }
            var regex = new Regex("([^=]+)=(.*)");
            foreach (var s in src)
            {
                var m = regex.Match(s);
                if (!m.Success)
                    continue;

                args[m.Groups[1].Value] = m.Groups[2].Value;
            }
        }

        #endregion

        //--------------------------------------------------------------
        #region Methods
        //--------------------------------------------------------------

        public bool Contains(string key) { return args.ContainsKey(key); }
        public string GetValue(string key, string defaultValue) { return Contains(key) ? args[key] : defaultValue; }
        public void ReplaceValue(string key, string value) { args[key] = value; }


        public override string ToString()
        {
            StringBuilder sb = new StringBuilder(50);

            sb.AppendLine("Arguments info : ");
            foreach (var kvp in args)
            {
                sb.AppendLine($"{kvp.Key}={kvp.Value}");
            }
            return sb.ToString();
        }

        #endregion
    }
}
