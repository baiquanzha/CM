using System.Text;
using UnityEngine;

namespace MTool.Framework.Utility
{
    public static class Util
    {
        static StringBuilder stringBuilder = new StringBuilder();

        /// <summary>
        /// 查找Transform在指定根节点下的路径
        /// </summary>
        /// <param name="t">要查找的节点</param>
        /// <param name="root">指定的根节点，可以为空，为空时返回在场景中的路径</param>
        /// <returns></returns>
        public static string GetTransformPath(Transform t, Transform root)
        {
            if (t == null)
                return string.Empty;
            if (t.parent == null)
                return t.name;
            stringBuilder.Length = 0;
            stringBuilder.Insert(0, t.name);
            Transform tmp = t;
            string sep = "/";
            while (tmp.parent != root && tmp.parent != null)
            {
                tmp = tmp.parent;
                stringBuilder.Insert(0, string.Concat(tmp.name, sep));
            }
            return stringBuilder.ToString();
        }
    }
}