using System;
using System.Collections.Generic;
using MTool.Core.ResourcePools;

namespace MTool.AppUpdaterLib.Runtime.ResManifestParser
{
    public  static class VersionManifestParser
    {

        private static ResourcePool<FileDesc> s_mPools = null;

        public static ResourcePool<FileDesc> Pools
        {
            get
            {
                Init();
                return s_mPools;
            }
        }

        private static bool mInitialize = false;

        public static void Init()
        {
            if (mInitialize)
                return;
            mInitialize = true;
            s_mPools = new ResourcePool<FileDesc>(()=>new FileDesc(),null,null);
        }

        public static VersionManifest Parse(string manifestContent,string gen = "")
        {
            List<FileDesc> list = new List<FileDesc>();

            var doc = new JSONObject(manifestContent);

            const char wellNumChar = '#';
            var keys = doc.keys;
            foreach (var key in keys)
            {
                string name = string.Empty;
                name = key.Trim();
                //if (string.IsNullOrEmpty(gen))
                //{
                //    name = key.Trim();
                //}
                //else
                //{
                //    name = $"lua/{gen}/{key.Trim()}";
                //}

                var val = doc[key].str;
                var splitStrs = val.Split(wellNumChar);

                //var remoteName = splitStrs[0];
                var size = Convert.ToInt32(splitStrs[1]);
                var hash = splitStrs[2];
                
                FileDesc desc = Pools.Obtain();
                desc.N = name;
                desc.H = hash;
                desc.S = size;
                desc.RN = val;
                list.Add(desc);
            }

            VersionManifest manifest = new VersionManifest()
            {
                Datas = list
            };
            return manifest;
        }

        public static string Serialize(VersionManifest manifest)
        {
            var doc = new JSONObject();

            for (int i = 0; i < manifest.Datas.Count; i++)
            {
                var desc = manifest.Datas[i];
                doc.AddField(desc.N, desc.RN);
            }

            return doc.ToString();
        }

    }
}
