using System;
using System.IO;
using System.Collections.Generic;

namespace MTool.AppUpdaterLib.Runtime
{
    [Serializable]
    public class UpdateResItem
    {
        public string ResName;
        public string FileName;
        public string MD5;
    }

    [Serializable]
    public class UpdateResMap
    {
        public Dictionary<string, UpdateResItem> ResMap = new Dictionary<string, UpdateResItem>();

        public void SetUpdateResItem(FileDesc fileDesc, string localFileName)
        {
            if (ResMap.ContainsKey(fileDesc.N))
            {
                ResMap[fileDesc.N].FileName = localFileName;
                ResMap[fileDesc.N].MD5 = fileDesc.H;
            }
            else
            {
                UpdateResItem resItem = new UpdateResItem
                {
                    ResName = fileDesc.N,
                    FileName = localFileName,
                    MD5 = fileDesc.H
                };
                ResMap.Add(resItem.ResName, resItem);
            }
        }

        public string GetResLocalFileName(string ResName)
        {
            string path = string.Empty;
            if (ResMap.ContainsKey(ResName))
                path = ResMap[ResName].FileName;
            return path;
        }

        public static UpdateResMap RegenUpdateResMap(VersionManifest baseRes, VersionManifest currRes, bool overwrite = true)
        {
            UpdateResMap updateResMap = new UpdateResMap();
            if (baseRes != null && currRes != null)
            {
                List<FileDesc> resDiff = baseRes.CalculateDifference(currRes, ResSyncMode.FULL);
                if (resDiff != null && resDiff.Count > 0)
                {
                    foreach (FileDesc fileDesc in resDiff)
                    {
                        string localPath = AppUpdaterContext.GetUpdateFileRelativeLocalPath(fileDesc);
                        updateResMap.SetUpdateResItem(fileDesc, localPath);
                    }
                }
            }
            
            if (overwrite && updateResMap.ResMap.Count > 0)
            {
                string content = UpdateResMapParser.Serialize(updateResMap);
                File.WriteAllText(AssetsFileSystem.GetWritePath(AssetsFileSystem.UpdateResMap), content, new System.Text.UTF8Encoding(false, true));
                string path = AssetsFileSystem.GetWritePath(AssetsFileSystem.UpdateResMap);
            }
            return updateResMap;
        }
    }

    public class UpdateResMapParser
    {
        public static UpdateResMap Parse(string content)
        {
            Dictionary<string, UpdateResItem> resMap = new Dictionary<string, UpdateResItem>();
            var doc = new JSONObject(content);
            var keys = doc.keys;
            const char wellNumChar = '#';
            foreach (var key in keys)
            {
                string resName = key;
                string fileName = doc[key].str;
                var splitStrs = fileName.Split(wellNumChar);
                string md5 = splitStrs[1];
                UpdateResItem resItem = new UpdateResItem
                {
                    ResName = resName,
                    FileName = fileName,
                    MD5 = md5
                };
                resMap.Add(resName, resItem);
            }
            UpdateResMap map = new UpdateResMap
            {
                ResMap = resMap
            };
            return map;
        }

        public static string Serialize(UpdateResMap resMap)
        {
            var doc = new JSONObject();
            
            var e = resMap.ResMap.GetEnumerator();
            while (e.MoveNext())
            {
                doc.AddField(e.Current.Key, e.Current.Value.FileName);
            }

            return doc.ToString();
        }
    }
}