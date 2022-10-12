using MTool.AppUpdaterLib.Runtime;
using MTool.AssetBundleManager.Runtime;
using MTool.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using XLua;

public class LuaManager : FrameworkModule {
    private const string TAG = "LuaBehaviour ";
    //资源加密解密常量定义
    private const int Encrypt_Len = 256;
    private const string Encrypt_Key = "9A33B313F0485A5448080B99C1576CD4";
    
    private AssetBundle luaAb = null;

    private LuaEnv lua;
    public LuaEnv env {
        get {
            if (lua == null) {
                Debug.Log(TAG + "创建LuaEnv");
                lua = new LuaEnv();
                lua.AddLoader(LoadLuaBytes);
            }
            return lua;
        }
    }

    internal override void Init() {
    }

    internal override void Update(float elapseTime, float realElapseTime) {
    }

    internal override void LateUpdate() {
    }

    internal override void Shutdown() {
    }

    /// <summary>
    /// 从资源目录下读取lua字节
    /// </summary>
    /// <param name="fname"></param>
    /// <returns></returns>
    public byte[] LoadLuaBytes(ref string fname) {
#if UNITY_EDITOR
        bool isLoadFromBundle = false;
        if (!AssetBundleManager.SimulateAssetBundleInEditor) {
            isLoadFromBundle = true;
        }
#else
        bool isLoadFromBundle = true;
#endif
        if (fname.EndsWith(".lua")) {
            fname = fname.Substring(0, fname.Length - 4);
        }
        fname = fname.Replace('.', '/');
        if (!isLoadFromBundle) {
            if (!fname.EndsWith(".lua")) {
                fname = fname + ".lua";
            }
            string path_Lua = "LuaProject/" + fname;
            string srcFilePath = Application.dataPath + "/.."+ Path.DirectorySeparatorChar + path_Lua;
            byte[] fileBytes = File.ReadAllBytes(srcFilePath);
            if (fileBytes != null) {
                if (fileBytes[0] == 0xEF && fileBytes[1] == 0xBB && fileBytes[2] == 0xBF) {
                    byte[] fileBytes2 = new byte[fileBytes.Length - 3];
                    Array.Copy(fileBytes, 3, fileBytes2, 0, fileBytes2.Length);
                    return fileBytes2;
                }
            } else {
                Debug.LogError(TAG + "【直接】载入lua脚本失败 " + path_Lua);
            }
            return fileBytes;
        } else {
            Debug.Log(TAG + "准备【从bundle中】载入lua脚本 " + fname);
            if (luaAb == null) {
                string abPath = AssetsFileSystem.GetWritePath($"lua.x");

                if (!File.Exists(abPath)) {
#if APPEND_PLATFORM_NAME
                            abPath = $"{Application.streamingAssetsPath}/{Utility.GetPlatformName()}/lua.x";
#else
                    abPath = $"{Application.streamingAssetsPath}/lua.x";
#endif
                }
                luaAb = AssetBundle.LoadFromFile(abPath);
            }
            var path = $"lua/{fname.Replace('.', '/')}";
            var textAssets = luaAb.LoadAsset<TextAsset>($"Assets/{path}.bytes");
            var bytes = textAssets.bytes;
            if (IGlobe.IsLuaEncrypt) {
                EncryptAll(ref bytes);
            }
            return bytes;
        }
    }

    //RC4 字符串
    public string RC4(string input, string key) {
        StringBuilder result = new StringBuilder();
        int x, y, j = 0;
        int[] box = new int[256];

        for (int i = 0; i < 256; i++) {
            box[i] = i;
        }

        for (int i = 0; i < 256; i++) {
            j = (key[i % key.Length] + box[i] + j) % 256;
            x = box[i];
            box[i] = box[j];
            box[j] = x;
        }

        for (int i = 0; i < input.Length; i++) {
            y = i % 256;
            j = (box[y] + j) % 256;
            x = box[y];
            box[y] = box[j];
            box[j] = x;

            result.Append((char)(input[i] ^ box[(box[y] + box[j]) % 256]));
        }
        return result.ToString();
    }

    //RC4 byte 数组
    public byte[] RC4(byte[] input) {
        byte[] result = new byte[input.Length];
        int x, y, j = 0;
        int[] box = new int[256];

        for (int i = 0; i < 256; i++) {
            box[i] = i;
        }

        for (int i = 0; i < 256; i++) {
            j = (Encrypt_Key[i % Encrypt_Key.Length] + box[i] + j) % 256;
            x = box[i];
            box[i] = box[j];
            box[j] = x;
        }

        for (int i = 0; i < input.Length; i++) {
            y = i % 256;
            j = (box[y] + j) % 256;
            x = box[y];
            box[y] = box[j];
            box[j] = x;

            result[i] = (byte)(input[i] ^ box[(box[y] + box[j]) % 256]);
        }
        return result;
    }

    //局部加密解密
    public void Encrypt(ref byte[] input) {
        if (IGlobe.IsLuaEncrypt) //使用加密
        {
            if (input != null && input.Length > Encrypt_Len) {
                byte[] tmp = new byte[Encrypt_Len];
                Array.Copy(input, 0, tmp, 0, Encrypt_Len);
                byte[] de = RC4(tmp);
                for (int i = 0; i < Encrypt_Len; i++) {
                    input[i] = de[i];
                }
            }
        }
    }
    //整个文件加密
    public void EncryptAll(ref byte[] input) {
        if (IGlobe.IsLuaEncrypt) //使用加密
        {
            byte[] tmp = new byte[input.LongLength];
            Array.Copy(input, 0, tmp, 0, input.LongLength);
            byte[] de = RC4(tmp);
            Array.Copy(de, 0, input, 0, input.LongLength);
            tmp = null;
            de = null;
        }
    }
}
