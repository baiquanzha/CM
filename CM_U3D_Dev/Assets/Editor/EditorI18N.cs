using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Data;
using Excel;
using OfficeOpenXml;
using XLua;
using LitJson;
using System.Reflection;

/// <summary>
/// 本地化工具:zbq
/// </summary>
public class EditorI18N {
    class MI18n {
        public string key;
        public string en;
        public string notes;
    }

    [MenuItem("工具/1.生成Json配置文件")]
    public static void CreateConfigJson() {
        string exportDir = Application.dataPath + "/../../Data/export";
        DirectoryInfo root = new DirectoryInfo(exportDir);
        FileInfo[] resFiles = root.GetFiles("*.xlsx", SearchOption.TopDirectoryOnly);
        foreach (FileInfo resFile in resFiles) {
            //string path = resFile.FullName.Replace('\\', '/');
            string path = resFile.FullName;
            Debug.Log("----------file name = " + path);
            FileStream fs = File.Open(path, FileMode.OpenOrCreate, FileAccess.Read);
            IExcelDataReader excelDataReader = ExcelReaderFactory.CreateOpenXmlReader(fs);
            DataSet result = excelDataReader.AsDataSet();
            for (int i = 0; i < result.Tables.Count; i++) {
                DataTable dataTable = result.Tables[i];
                string cofName = dataTable.TableName.Split('@')[0];
                int rowCount = dataTable.Rows.Count;
                int colCount = dataTable.Columns.Count;

                JsonData jdList = new JsonData();
                string[] names = new string[colCount];
                string[] types = new string[colCount];
                //1-属性名，2-数据类型，3-描述，
                DataRow nameRow = dataTable.Rows[0];
                DataRow typeRow = dataTable.Rows[1];
                for (int c = 0; c < colCount; c++) {
                    string name = nameRow[c].ToString().Trim();
                    types[c] = typeRow[c].ToString().Trim();

                    string[] splits = name.Split('@');
                    //注释的列
                    if (splits.Length > 1 && splits[1].Equals("pm")) {
                        names[c] = null;
                    } else {
                        names[c] = splits[0];
                    }
                }

                for (int r = 3; r < rowCount; r++) {
                    DataRow row = dataTable.Rows[r];
                    string value = row[0].ToString().Trim();
                    JsonData jd = new JsonData();
                    if (value != null && value != "") {
                        for (int c = 0; c < colCount; c++) {
                            if (names[c] == null) {
                                continue;
                            }
                            SetJsonData(jd, names[c], types[c], row[c].ToString().Trim());
                        }
                    }
                    jdList.Add(jd);
                }
                excelDataReader.Close();
                excelDataReader.Dispose();
                fs.Close();

                JsonWriter jw = new JsonWriter();
                jw.PrettyPrint = true;
                JsonMapper.ToJson(jdList, jw);
                string jsonPath = string.Format("{0}/ResourcesAB/Config/{1}Conf.json", Application.dataPath, cofName);
                Debug.Log("----------create jsonPath = " + jsonPath);
                File.WriteAllText(jsonPath, jw.TextWriter.ToString());
            }
        }
        AssetDatabase.Refresh();
        Debug.Log("----------生成Json配置文件完成");
    }

    static void ParseJsonArray(int index, string value, JsonData jd, string type) {
        if (index == 1) {
            string[] strs = value.Split(',');
            for (int i = 0; i < strs.Length; i++) {
                value = strs[i];
                switch (type) {
                    case "int32":
                    case "int64":
                        if (value == null || value == "") {
                            jd.Add(0);
                        } else {
                            jd.Add(int.Parse(value));
                        }
                        break;
                    case "float":
                        if (value == null || value == "") {
                            jd.Add(0);
                        } else {
                            jd.Add(float.Parse(value));
                        }
                        break;
                    case "bool":
                        if (value == null || value == "") {
                            jd.Add(false);
                        } else {
                            jd.Add(bool.Parse(value));
                        }
                        break;
                    default:
                        if (value == null) {
                            jd.Add("");
                        } else {
                            jd.Add(value);
                        }
                        break;
                }
            }
        } else {
            int count = index;
            value = value.Substring(index, value.Length - index * 2);
            string startStr = "";
            string endStr = "";
            for (int i = 0; i < count - 1; i++) {
                startStr = startStr + "]";
                endStr = endStr + "[";
            }
            string matchStr = startStr + "," + endStr;
            string[] strs = value.Split(new string[] { matchStr }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < strs.Length; i++) {
                JsonData json = new JsonData();
                ParseJsonArray(index - 1, strs[i], json, type);
                jd.Add(json);
            }
        }
    }

    static void SetJsonData(JsonData jd, string key, string type, string value) {
        string[] strs = type.Split(' ');
        if (strs.Length > 1) {
            type = strs[strs.Length - 1];
            JsonData json = new JsonData();
            ParseJsonArray(strs.Length - 1, value, json, type);
            jd[key] = json;
        } else {
            switch (type) {
                case "int32":
                case "int64":
                    if (value == null || value == "") {
                        jd[key] = 0;
                    } else {
                        jd[key] = int.Parse(value);
                    }
                    break;
                case "float":
                    if (value == null || value == "") {
                        jd[key] = 0;
                    } else {
                        jd[key] = float.Parse(value);
                    }
                    break;
                case "bool":
                    if (value == null || value == "") {
                        jd[key] = false;
                    } else {
                        jd[key] = bool.Parse(value);
                    }
                    break;
                default:
                    if (value == null) {
                        jd[key] = "";
                    } else {
                        jd[key] = value;
                    }
                    break;
            }
        }
    }

    static void WriteCsv(string[] strs, string path) {
        if (File.Exists(path)) {
            File.Delete(path);
        }
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < strs.Length; i++) {
            sb.Append(strs[i]);
            if (i <strs.Length - 1) {
                sb.Append("\r\n");
            }
        }
        File.WriteAllText(path, sb.ToString());
    }

    static bool IsFilter(string str) {
        MatchCollection match = Regex.Matches(str, "[a-zA-Z]");
        if (match.Count > 1) {
            return true;
        }
        return false;
        //return Regex.IsMatch(str, ".*[a-zA-Z]+.*");
    }

    static string GetAssetPath(string path) {
        string[] seperator = { "Assets" };
        string p = "Assets" + path.Split(seperator, StringSplitOptions.RemoveEmptyEntries)[1];
        return p;
    }
    
}
