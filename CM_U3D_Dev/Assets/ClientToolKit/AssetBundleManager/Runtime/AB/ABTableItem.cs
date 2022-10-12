using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using MTool.AssetBundleManager.Runtime;

/// <summary>
/// 一个AB颗粒的配置
/// </summary>
[Serializable]
public sealed class ABTableItem
{
    public string Name;
    //public string AbsPath;
    //public string[] Assets;
    public string[] Dependencies;
}

/// <summary>
/// 一个热更文件的属性(服务器)
/// </summary>
//[Serializable]
//public class ABTableItemInfo
//{
//    public string N;
//    public int S;
//    public string H;
//}
/// <summary>
/// 一个热更文件的属性(客户端)
/// </summary>

