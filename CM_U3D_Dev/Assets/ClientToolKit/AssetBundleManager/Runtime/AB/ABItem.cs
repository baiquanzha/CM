using System;
using System.Collections.Generic;
using UnityEngine;
using MTool.AppUpdaterLib.Runtime;
using MTool.LoggerModule.Runtime;
using ILogger = MTool.LoggerModule.Runtime.ILogger;

/// <summary>
/// AB底层处理类
/// </summary>
public sealed class ABItem
{
    private static readonly Lazy<ILogger> s_mLogger = new Lazy<ILogger>(() =>
        LoggerManager.GetLogger("ABItem"));
    public ABItem(ABTableItem table)
    {
        Table = table;
    }

    /// <summary>
    /// 引用计数
    /// </summary>
    public int UseCount = 0;
    
    /// <summary>
    /// 添加异步任务委托
    /// </summary>
    public ABItemCallBack AddListHandle;
    
    /// <summary>
    /// AB数据配置
    /// </summary>
    public ABTableItem Table;
    
    /// <summary>
    /// 依赖项列表
    /// </summary>
    public ABItem[] Dependencies;
    
    /// <summary>
    /// AB实体
    /// </summary>
    public AssetBundle AB;
    
    /// <summary>
    /// 加载的模式，True为异步，False为同步
    /// </summary>
    public bool Async = false;
    
    /// <summary>
    /// 作为副体回调队列
    /// </summary>
    private Queue<ABItemCallBack> dependencCalls = new Queue<ABItemCallBack>();
    
    /// <summary>
    /// 作为主体回调队列
    /// </summary>
    private Queue<ABItemCallBack> mianABcalls = new Queue<ABItemCallBack>();

    /// <summary>
    /// 依赖项加载的数目
    /// </summary>
    public int dependenciesCount { get; private set; }

    
    /// <summary>
    /// 初始化依赖项状态
    /// </summary>
    //private bool initDependenciesState = true;
    
    /// <summary>
    /// 加载依赖项状态
    /// </summary>
    private LoadDependenciesState loadDependenciesState = LoadDependenciesState.Ready;
    
    /// <summary>
    /// 开始加载配置
    /// </summary>
    /// <param name="abItemCallBack"></param>
    /// <param name="async"></param>
    /// <param name="loadDepend"></param>
    public void LoadItem(ABItemCallBack abItemCallBack, bool async = false, bool isMainAB = true)
    {
        //if (initDependenciesState)
        //{
        //    initDependenciesState = false;
        //    if (Dependencies == null && Table.Dependencies.Length > 0)
        //    {
        //        Dependencies = new ABItem[Table.Dependencies.Length];
        //        for (int i = 0; i < Dependencies.Length; i++)
        //        {
        //            ABItem item = ABMgr.GetItem(Table.Dependencies[i]);
        //            if (item == null)
        //            {
        //                AbHelp.ABLog("Load dependencie fail:" + Table.Name + "," + Table.Dependencies[i], false);
        //            }
        //            Dependencies[i] = item;
        //        }
        //    }
        //}
        
        Async = async;
        if (abItemCallBack != null)
        {
            if (isMainAB)
            {
                mianABcalls.Enqueue(abItemCallBack);
            }
            else
            {
                dependencCalls.Enqueue(abItemCallBack);
            }
        }

        AddUseCount();
        beginLoadDependencies(isMainAB);
    }



    public void UpdateDependencies(ABConfigHandle handle)
    {
        if (Dependencies == null && Table.Dependencies.Length > 0)
        {
            Dependencies = new ABItem[Table.Dependencies.Length];
            for (int i = 0; i < Dependencies.Length; i++)
            {
                ABItem item = handle.GetItem(Table.Dependencies[i]);
                if (item == null)
                {
                    s_mLogger.Value?.Error("Load dependencie fail:" + Table.Name + "," + Table.Dependencies[i]);
                }
                Dependencies[i] = item;
            }
        }
    }

    /// <summary>
    /// 添加引用计数
    /// </summary>
    public void AddUseCount()
    {
        UseCount++;
    }
    
    /// <summary>
    /// 回调所有注册到该实体的函数
    /// </summary>
    private void callback()
    {
        //如果需要删除(该功能暂时不开启)
        //deleteByState();
        
        if(AB != null)
        {
            if (loadDependenciesState == LoadDependenciesState.Complete)
            {
                while (mianABcalls.Count > 0)
                {
                    mianABcalls.Dequeue()(this);
                }
            }

            while (dependencCalls.Count > 0)
            {
                dependencCalls.Dequeue()(this);
            }
        }
    }
    
    /// <summary>
    /// 加载依赖项的回调
    /// </summary>
    /// <param name="item"></param>
    public void ItemCallBack(ABItem item)
    {
        dependenciesCount++;
        if (dependenciesCount >= Table.Dependencies.Length)
        {
            loadDependenciesState = LoadDependenciesState.Complete;
            loadAB();
        }
    }
    
    /// <summary>
    /// 开始加载依赖项
    /// </summary>
    /// <param name="isMainAB">是否是主体AB</param>
    private void beginLoadDependencies(bool isMainAB)
    {
        //如果时主体AB
        if(isMainAB)
        {
            loadDependencies();
            
            if (loadDependenciesState == LoadDependenciesState.Complete)
            {
                loadAB();
            }
        }
        else
        {
            loadAB();
        }
    }
    
    
    /// <summary>
    /// 加载依赖项
    /// </summary>
    private void loadDependencies()
    {
        if (Dependencies != null)
        {
            if (loadDependenciesState == LoadDependenciesState.Ready)
            {
                loadDependenciesState = LoadDependenciesState.Loading;
                
                for (int i = 0; i < Dependencies.Length; i++)
                {
                    ABItem item = Dependencies[i];
                    item.LoadItem(ItemCallBack, Async);
                }
            }
            else
            {
                for (int i = 0; i < Dependencies.Length; i++)
                {
                    ABItem item = Dependencies[i];
                    item.AddUseCount();
                }
            }
        }
        else
        {
            loadDependenciesState = LoadDependenciesState.Complete;
        }
    }

    /// <summary>
    /// 在异步加载时，需要主线程读取状态
    /// </summary>
    public void Update()
    {
        if (abReq != null && abReq.isDone)
        {
            AB = abReq.assetBundle;
            abReq = null;
            callback();
        }
    }
    
    /// <summary>
    /// AB的删除操作
    /// </summary>
    /// <param name="deleteDepend"></param>
    public void Delete(bool deleteDepend = true)
    {
        UseCount--;
        if (UseCount < 0)
        {
            UseCount = 0;
            s_mLogger.Value?.Warn("ABItem:无效的删除操作:" + Table.Name);
        }
        delete(deleteDepend);
    }
    
    private void delete(bool deleteDepend)
    {
        if (deleteDepend)
        {
            if (Dependencies != null)
            {
                for (int i = 0; i < Dependencies.Length; i++)
                {
                    Dependencies[i].Delete(false);
                }
            }
        }
        if (deleteState)
        {
            //AddUploadHandle(DeleteByState);
            deleteByState();
        }
    }

    private void deleteByState()
    {
        if (deleteState)
        {
            if (AB != null)
            {
                AB.Unload(true);
                //s_mLogger.Value?.Debug($"Unload ABItem:{Table.Name}");
                AB = null;
            }
            
            mianABcalls.Clear();
            dependencCalls.Clear();
            
            dependenciesCount = 0;
            loadDependenciesState = LoadDependenciesState.Ready;
        }
    }
    
    /// <summary>
    /// 判断可以删除的时刻
    /// </summary>
    private bool deleteState
    {
        get
        {
            return UseCount <= 0;
        }
    }
    
    /// <summary>
    /// 判断是否可以加载AB
    /// </summary>
    private bool canLoadAB
    {
        get
        {
            return AB == null && abReq == null;
        }
    }
    
    private AssetBundleCreateRequest abReq;
    /// <summary>
    /// 加载AB包
    /// </summary>
    private void loadAB()
    {
        if (canLoadAB)
        {
            //string path = AssetsFileSystem.GetWritePath(Table.Name, false, AbHelp.AbFileExt);
            string resFileName = AbHelp.ResLoadMap.GetResLocalFileName(Table.Name + AbHelp.AbFileExt);
            string path = AssetsFileSystem.GetWritePath(resFileName);
            if (string.IsNullOrEmpty(resFileName) || !System.IO.File.Exists(path))
            {
                path = AssetsFileSystem.GetStreamingAssetsPath(Table.Name, AbHelp.AbFileExt);
            }
            //s_mLogger.Value?.Debug($"Load ABItem:{path}");
            if (Async)
            {
                abReq = AssetBundle.LoadFromFileAsync(path);
                AddListHandle(this);
            }
            else
            {
                
                AB = AssetBundle.LoadFromFile(path);
            }
        }
        callback();
    }
    
    /// <summary>
    /// 检查一个AB包的完整性
    /// </summary>
    public void CheckDependencies()
    {
        int count = 0;
        if (Dependencies != null)
        {
            for (int i = 0; i < Dependencies.Length; i++)
            {
                ABItem item = Dependencies[i];
                if (item.AB != null)
                {
                    count++;
                }
            }
            if (Dependencies.Length != count)
            {
                s_mLogger.Value?.Error("ABItem: checkDependencies error:" + Dependencies.Length + "," + count);
            }
        }

    }

    /// <summary>
    /// 依赖项加载状态
    /// </summary>
    public enum LoadDependenciesState
    {
        /// <summary>
        /// 未开始状态
        /// </summary>
        Ready,
        /// <summary>
        /// 正在加载状态
        /// </summary>
        Loading,
        /// <summary>
        /// 加载完成状态
        /// </summary>
        Complete
    }
}


public delegate void ABItemCallBack(ABItem ab);
public delegate void ABItemAsyncAction();
public delegate void ABItemAsyncActionHandle(ABItemAsyncAction action);
