using System;
using System.Collections.Generic;
using System.Text;
using MTool.AssetBundleManager.Runtime;
using MTool.Core.Functional;
using UnityEngine;

/// <summary>
/// AB系统的配置基础操作类
/// </summary>
public sealed class ABConfigHandle
{
    public ABConfigHandle(ResManifest conf)
    {
        this.Initialize(conf);
    }
    public Dictionary<string, ABItem> ABItemList = null;
    public Dictionary<string, ABRequest> Cache = new Dictionary<string, ABRequest>(StringComparer.OrdinalIgnoreCase);
    public List<ABItem> updateList = new List<ABItem>();
    private Queue<ABItemAsyncAction> itemActionList = new Queue<ABItemAsyncAction>();
    public ResManifest config;
    private List<ABRequest> asyncReqList = new List<ABRequest>();


    /// <summary>
    /// 用于临时拼接字符串
    /// </summary>
    private readonly StringBuilder mTempSb = new StringBuilder(4);

    private void Initialize(ResManifest conf)
    {
        Load(conf);

        //if (AbHelp.IsDebug)
        //{
        //    this.mAbDebugView = new ABDebugView(this);
        //}
    }

    /// <summary>
    /// 装载AB配置表
    /// </summary>
    /// <param name="conf"></param>
    public void Load(ResManifest conf)
    {
        config = conf;
        if (config == null || config.Tables == null)
            return;
        ABItemList = new Dictionary<string, ABItem>(StringComparer.OrdinalIgnoreCase);
        config.Tables.ForCall((v, idx) =>
        {
            ABItem item = new ABItem(v);
            item.AddListHandle = addUpdateList;
            ABItemList[item.Table.Name] = item;
        });

        ABItemList.ForeachCall(x =>
        {
            x.Value.UpdateDependencies(this);
        });

    }
    private void addUpdateList(ABItem item)
    {
        updateList.Add(item);
    }
    private void addUploadListList(ABItemAsyncAction item)
    {
        itemActionList.Enqueue(item);
    }

    private void addAsyncRequest(ABRequest req)
    {
        asyncReqList.Add(req);
    }
    
    /// <summary>
    /// 获取一个AB底层处理类
    /// </summary>
    /// <param name="pathName"></param>
    /// <returns></returns>
    public ABItem GetItem(string pathName)
    {
        if (ABItemList.ContainsKey(pathName))
            return ABItemList[pathName];
        return null;
    }
    
    /// <summary>
    /// 判断文件路径是否存在
    /// </summary>
    /// <param name="pathName"></param>
    /// <returns></returns>
    public bool RequestExist(string pathName)
    {
        return Cache.ContainsKey(pathName);
    }
    
    /// <summary>
    /// 获取一个请求句柄
    /// </summary>
    /// <param name="pathName"></param>
    /// <returns></returns>
    internal ABRequest GetRequest(string pathName, bool isScene = false)
    {
        if (Cache.ContainsKey(pathName))
        {
            return Cache[pathName];
        }

        if (ABItemList.ContainsKey(pathName))
        {
            return CreateAndAdd(ABItemList[pathName],pathName, isScene);
        }

        ABRequest request = TestAbPath(pathName, isScene);

        return request;
    }


    public ABRequest GetRequestFormLoaded(string pathName) 
    {
        ABRequest request = null;
        Cache.TryGetValue(pathName,out request);

        return request;
    }

    private ABRequest TestAbPath(string pathName, bool isScene = false)
    {
        string bundleName = pathName;
        while (true)
        {
            int lastIdx = bundleName.LastIndexOf('/');
            if (lastIdx != -1)
            {
                bundleName = bundleName.Substring(0, lastIdx);
                if (ABItemList.ContainsKey(bundleName))
                {
                    return CreateAndAdd(ABItemList[bundleName], pathName, isScene);
                }
            }
            else
            {
                break;
            }
        }
        return null;
        /*
        string[] pathSplit = pathName.Split('/');
        int splitLength = pathSplit.Length;
        if (splitLength != 0)
        {
            int lastIdx = 0;
            string abName = string.Empty;
            while (lastIdx < splitLength)
            {
                this.mTempSb.Clear();
                for (int i = 0; i <= lastIdx; i++)
                {
                    if (i == lastIdx)
                    {
                        this.mTempSb.Append(pathSplit[i]);
                    }
                    else
                    {
                        this.mTempSb.Append(pathSplit[i]).Append('/');
                    }
                }
                abName = this.mTempSb.ToString();
                if (ABItemList.ContainsKey(abName))
                {
                    return CreateAndAdd(ABItemList[abName], pathName, isScene);
                }

                lastIdx++;
            }
        }
        return null;*/
    }


    private ABRequest CreateAndAdd(ABItem abItem, string path , bool isScene = false)
    {
        ABRequest req = new ABRequest(path , abItem , isScene);
        req.AddListHandle = addAsyncRequest;
        Cache[req.Path] = req;

        return req;
    }
    
    /// <summary>
    /// 用于AB底层加载的状态更新
    /// </summary>
    public void UpdateItem()
    {
        for (int i = 0; i < updateList.Count; i++)
        {
            ABItem item = updateList[i];
            item.Update();
            if (item.AB != null)
            {
                updateList.RemoveAt(i);
                i--;
            }
        }
        
        long lastTick = DateTime.Now.Ticks;
        long allTick = 0;
        while (itemActionList.Count > 0)
        {
            itemActionList.Dequeue()();
            allTick = DateTime.Now.Ticks - lastTick;
            
            //大于5毫秒
            if (allTick > 50000)
            {
                break;
            }
        }

        for (int i = 0; i < asyncReqList.Count; i++)
        {
            ABRequest req = asyncReqList[i];
            req.Update();
            if (req.AsyncRequest == null)
            {
                asyncReqList.RemoveAt(i);
                i--;
            }
        }
    }

    private Vector2 scroll = Vector2.zero, scroll1 = Vector2.zero, scroll2 = Vector2.zero;
    //private string lastSelectKey = "", selectKey = "";
    //private List<ABRequest> reqUIList = new List<ABRequest>();
    //private int reqIndex = -1;
#if UNITY_EDITOR
    private ABRequest tmpReq;
#endif


    //private ABDebugView mAbDebugView = null;


//    public void DrawItemUI()
//    {
//        //if (AbHelp.IsDebug)
//        //    mAbDebugView?.DrawGUI();
//        return;
//        /*
//        if (ABItemList != null)
//        {
//            GUILayout.BeginHorizontal();
//            scroll = GUILayout.BeginScrollView(scroll, GUILayout.Width(260));
//            foreach (KeyValuePair<string, ABItem> k in ABItemList)
//            {
//                AbHelp.TmpSB.Length = 0;
//                AbHelp.TmpSB.Append(k.Key);
//                AbHelp.TmpSB.Append(":");
//                AbHelp.TmpSB.Append(k.Value.UseCount);
//                if (string.Equals(selectKey, k.Key))
//                {
//                    GUILayout.Label(AbHelp.TmpSB.ToString());
//                }
//                else if (k.Value.UseCount == 0)
//                {
//                    GUILayout.Label(AbHelp.TmpSB.ToString(), "box");
//                }
//                else
//                {
//                    if (GUILayout.Button(AbHelp.TmpSB.ToString()))
//                    {
//                        selectKey = k.Key;
//                        reqIndex = -1;
//                    }
//                }
//            }
//            GUILayout.EndScrollView();

//            if (string.CompareOrdinal(lastSelectKey, selectKey) != 0)
//            {
//                lastSelectKey = selectKey;
//                reqUIList.Clear();
//                List<ABRequest> selectList = new List<ABRequest>();
//                List<ABRequest> noSelectList = new List<ABRequest>();
//                foreach (KeyValuePair<string, ABRequest> k in Cache)
//                {
//                    if (string.CompareOrdinal(k.Value.ABHandle.Table.Name, selectKey) == 0)
//                    {
//                        if (k.Value.UseCount > 0)
//                        {
//                            selectList.Add(k.Value);
//                        }
//                        else
//                        {
//                            noSelectList.Add(k.Value);
//                        }

//                    }
//                }
//                for (int i = 0; i < selectList.Count; i++)
//                {
//                    reqUIList.Add(selectList[i]);
//                }
//                for (int i = 0; i < noSelectList.Count; i++)
//                {
//                    reqUIList.Add(noSelectList[i]);
//                }
//            }
//            if (selectKey.Length > 0)
//            {
//                scroll1 = GUILayout.BeginScrollView(scroll1, GUILayout.Width(400));
//                for (int i = 0; i < reqUIList.Count; i++)
//                {
//                    ABRequest k = reqUIList[i];
//                    AbHelp.TmpSB.Length = 0;
//                    AbHelp.TmpSB.Append(k.Path);
//                    AbHelp.TmpSB.Append(":");
//                    AbHelp.TmpSB.Append(k.UseCount);

//                    if (reqIndex == i)
//                    {
//                        GUILayout.Label(AbHelp.TmpSB.ToString());
//                    }
//                    else if (k.UseCount == 0)
//                    {
//                        GUILayout.Label(AbHelp.TmpSB.ToString(), "box");
//                    }
//                    else
//                    {

//                        if (GUILayout.Button(AbHelp.TmpSB.ToString()))
//                        {
//                            reqIndex = i;
//#if UNITY_EDITOR
//                            tmpReq = k;
//#endif
//                        }
//                    }
//                }
//                GUILayout.EndScrollView();
//            }

//#if UNITY_EDITOR
//            if (reqIndex > -1 && tmpReq != null)
//            {
//                scroll2 = GUILayout.BeginScrollView(scroll2, GUILayout.Width(400));
//                for (int i = 0; i < tmpReq.StackStr.Count; i++)
//                {
//                    GUILayout.Label(tmpReq.StackStr[i], UnityEditor.EditorStyles.helpBox);
//                }
//                GUILayout.EndScrollView();
//            }
//#endif
//            GUILayout.EndHorizontal();
//        }

//    */
//    }

}