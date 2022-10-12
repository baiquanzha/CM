using System;
using System.Collections.Generic;
using MTool.AssetBundleManager.Runtime;
using MTool.LoggerModule.Runtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using ILogger = MTool.LoggerModule.Runtime.ILogger;
#if UNITY_EDITOR
using UnityEditor;
#endif
using Object = UnityEngine.Object;

/// <summary>
/// AB系统请求(封装)类
/// </summary>
public sealed class ABRequest
{

    private static readonly Lazy<ILogger> s_mLogger = new Lazy<ILogger>(() =>
        LoggerManager.GetLogger("ABItem"));

    
    public ABRequest(string path , ABItem abItem, bool isScene = false) 
    {
        Path = path;
        ABHandle = abItem;
        if (isScene) 
        {
            this.IsScene = true;
        }
        else
        {
            int lastSlashIdx = path.LastIndexOf('/');
            if (lastSlashIdx != -1)
                AssetPath = path.Substring(lastSlashIdx + 1);
            else
                AssetPath = path;
        }
    }

    /// <summary>
    /// 初始化请求函数
    /// </summary>
    /// <param name="path"></param>
    /// <param name="assetPath"></param>
    public ABRequest(string path, string assetPath)
    {
        Path = path;
        AssetPath = assetPath;
    }

//#if UNITY_EDITOR
//    public List<string> StackStr = new List<string>();
//#endif

    private Queue<ABRequestCallBack> calls = new Queue<ABRequestCallBack>();

    private static Dictionary<int, ABRequest> objCache = new Dictionary<int, ABRequest>();

    /// <summary>
    /// AB的路径（相对路径）
    /// </summary>
    public string Path;
    
    /// <summary>
    /// AB文件在Unity上的真实路径
    /// </summary>
    public string AssetPath;
    
    /// <summary>
    /// 该请求对应的AB底层处理类
    /// </summary>
    public ABItem ABHandle;
    
    /// <summary>
    /// 请求的引用计数
    /// </summary>
    public int UseCount = 0;

    /// <summary>
    /// 请求是否是异步
    /// </summary>
    public bool Async = false;
    
    /// <summary>
    /// 请求是否完成
    /// </summary>
    public bool IsDone = false;

    /// <summary>
    /// 添加异步任务委托
    /// </summary>
    public ABRequestCallBack AddListHandle;

    /// <summary>
    /// 加载Asset的异步可等待操作
    /// </summary>
    public AssetBundleRequest AsyncRequest;

    /// <summary>
    /// 异步回调队列
    /// </summary>
    public Queue<ObjectCallBack> AsyncLoadTasks = new Queue<ObjectCallBack>();

    /// <summary>
    /// 返回请求主资源
    /// </summary>
    public UnityEngine.Object MainAsset
    {
        get
        {
            return this.Load();
        }
    }

    /// <summary>
    /// 该请求的作用是否是加载一个场景
    /// </summary>
    public bool IsScene { get; private set; } = false;

    /// <summary>
    /// 场景名，如果当前的请求属于加载场景。
    /// 注意，场景名严格要求要小写
    /// </summary>
    //public string SceneName { get; private set; } = string.Empty;
    public LoadSceneMode LoadSceneMode { get; private set; } = LoadSceneMode.Single;


    /// <summary>
    /// 按照类型加载对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public T Load<T>() where T : UnityEngine.Object
    {
        return this.Load(typeof(T)) as T;
    }
    /// <summary>
    /// 加载并Init GameObject对象
    /// </summary>
    /// <param name="callbcak"></param>
    /// <param name="pos"></param>
    /// <param name="rot"></param>
    /// <param name="useDefaultPos"></param>
    /// <param name="async"></param>
    public void LoadABObject(ABObjectCallBack callbcak, Vector3 pos, Vector3 rot, bool useDefaultPos = false, bool async = true)
    {
        GameObject obj = InitObj(this, pos, rot, useDefaultPos);
        callbcak(obj);
    }
    
    /// <summary>
    /// 加载对象
    /// </summary>
    /// <param name="type"></param>
    /// <param name="addCache"></param>
    /// <returns></returns>
    public Object Load(System.Type type = null)
    {
        UnityEngine.Object obj = null;
        if (!IsScene && IsDone)
        {
            obj = LoadByType(type);
        }
        return obj;
    }

    public void LoadAsync(ObjectCallBack callback, System.Type type = null, bool addCache = true)
    {
        AsyncRequest = LoadByTypeAsync(type);
        AsyncLoadTasks.Enqueue(callback);
    }

    /// <summary>
    /// 开始加载物体请求
    /// </summary>
    /// <param name="req"></param>
    /// <param name="async"></param>
    public void LoadAsset(ABRequestCallBack req , bool async)
    {
        Async = async;
//#if UNITY_EDITOR
//        StackStr.Add(AbHelp.GetStackTraceModelName());
//        if (StackStr.Count > 20)
//        {
//            StackStr.RemoveAt(0);
//        }
//#endif
        AddUseCount();
        if (req != null)
        {
            calls.Enqueue(req);
        }
        ABHandle.LoadItem(requestCallBack, Async);
    }


    //public void LoadABOnly(ABRequestCallBack req, bool async)
    //{
    //    Async = async;

    //    if (req != null)
    //    {
    //        calls.Enqueue(req);
    //    }
    //    ABHandle.LoadItem(requestCallBack, Async);
    //}


    /// <summary>
    /// 开始加载物体请求
    /// </summary>
    /// <param name="req"></param>
    /// <param name="async"></param>
    public void LoadScene(ABRequestCallBack req, bool async, LoadSceneMode mode = LoadSceneMode.Single)
    {
        this.LoadSceneMode = mode;
        this.LoadAsset(req,async);
    }


    /// <summary>
    /// 请求引用计数
    /// </summary>
    public void AddUseCount()
    {
        UseCount++;
    }
    
    /// <summary>
    /// 请求回调处理
    /// </summary>
    /// <param name="item"></param>
    private void requestCallBack(ABItem item)
    {
        if (!IsDone)
        {
            IsDone = true;
        }
        
        //if (this.IsScene)
        //{
        //    if (this.Async)
        //        SceneManager.LoadSceneAsync(this.SceneName,new LoadSceneParameters(this.LoadSceneMode));
        //    else
        //        SceneManager.LoadScene(this.SceneName,new LoadSceneParameters(LoadSceneMode));

        //    this.LoadSceneMode = LoadSceneMode.Single;//assignment default
        //}

        if (!deleteState())
        {
            while (calls.Count > 0)
            {
                calls.Dequeue()(this);
            }
        }
    }

    /// <summary>
    /// 删除一次请求
    /// </summary>
    public void Delete()
    {
        UseCount--;
        if (UseCount < 0)
        {
            UseCount = 0;
            s_mLogger.Value?.Error("ABRequest:无效的删除操作 " + Path);
        }

#if UNITY_EDITOR
        if (!AssetBundleManager.SimulateAssetBundleInEditor)
        {
            delete();
        }
#else
        delete();
#endif
    }
    private void delete()
    {
        if (deleteState())
        {
            IsDone = false;
//#if UNITY_EDITOR
//            StackStr.Clear();
//#endif
        }
        if (ABHandle != null)
            ABHandle.Delete();
    }

    private bool deleteState()
    {
        return IsDone && UseCount <= 0;
    }
    

    /// <summary>
    /// 删除一个物体
    /// </summary>
    /// <param name="g"></param>
    public static void DeleteObj(UnityEngine.Object g)
    {
        int id = g.GetInstanceID();
        ABRequest req = null;
        bool isGameobj = false;
        if (g is GameObject)
        {
            isGameobj = true;
            GameObject.Destroy(g);
        }
        if (objCache.TryGetValue(id, out req))
        {
            req.Delete();
            if (req.UseCount <= 0)
            {
                if (!isGameobj)
                {
                    Resources.UnloadAsset(g);
                }
                objCache.Remove(id);
            }
        }
    }

    /// <summary>
    /// 初始化一个物体
    /// </summary>
    /// <param name="req"></param>
    /// <param name="pos"></param>
    /// <param name="rot"></param>
    /// <param name="usePos"></param>
    /// <returns></returns>
    public static GameObject InitObj(ABRequest req, Vector3 pos, Vector3 rot, bool usePos = false)
    {
        return InitObj(req, pos, rot, usePos, null);
    }
    
    /// <summary>
    /// 初始化一个物体
    /// </summary>
    /// <param name="req"></param>
    /// <param name="pos"></param>
    /// <param name="rot"></param>
    /// <param name="usePos"></param>
    /// <param name="asset"></param>
    /// <returns></returns>
    public static GameObject InitObj(ABRequest req, Vector3 pos, Vector3 rot, bool usePos = false, UnityEngine.Object asset = null)
    {
        GameObject obj = null;
        if (asset == null)
        {
            asset = req.Load(typeof(UnityEngine.Object));
        }
        if (usePos)
        {
            obj = GameObject.Instantiate(asset) as GameObject;
        }
        else
        {
            obj = GameObject.Instantiate(asset, pos, Quaternion.Euler(rot)) as GameObject;
        }
        objCache[obj.GetInstanceID()] = req;
        return obj;
    }


    private Object LoadByType(System.Type type = null)
    {
        Object result = null;
#if UNITY_EDITOR
        if (!AssetBundleManager.SimulateAssetBundleInEditor)
        {
            if (type == null)
            {
                result = ABHandle.AB.LoadAsset(AssetPath);
            }
            else
            {
                result = ABHandle.AB.LoadAsset(AssetPath, type);
            }
        }
        else
        {
            if (type == null)
            {
                result = AssetDatabase.LoadMainAssetAtPath(AssetPath);
            }
            else
            {
                result = AssetDatabase.LoadAssetAtPath(AssetPath, type);
            }
        }
#else
            if (type == null)
            {
                result = ABHandle.AB.LoadAsset(AssetPath);
            }
            else
            {
                result = ABHandle.AB.LoadAsset(AssetPath, type);
            }
#endif
        return result;
    }

    private AssetBundleRequest LoadByTypeAsync(System.Type type = null)
    {
        AddListHandle(this);
        if (type == null)
        {
            return ABHandle.AB.LoadAssetAsync(AssetPath);
        }
        else
        {
            return ABHandle.AB.LoadAssetAsync(AssetPath, type);
        }
    }

    public void Update()
    {
        if (AsyncRequest != null && AsyncRequest.isDone)
        {
            if (AsyncRequest.asset == null)
                s_mLogger.Value.Error($"load asset async return a null, Path = {Path}");
            foreach (ObjectCallBack task in AsyncLoadTasks)
            {
                try
                {
                    var asset = AsyncRequest.asset;
                    if (asset == null)
                    {
                        s_mLogger.Value.Error($"Async load asset failure , asset path is \"{this.Path}\" .");
                    }

                    task?.Invoke(asset);
                }
                catch (Exception e)
                {
                    s_mLogger.Value.Fatal($"Load asset exception , error message : {e.Message} \n  stackTrace : {e.StackTrace}");
                }
            }
            AsyncLoadTasks.Clear();
            AsyncRequest = null;
        }
    }
}
public delegate void ABRequestCallBack(ABRequest ab);
public delegate void ObjectCallBack(UnityEngine.Object obj);
public delegate void BundleLoadCompletedCallBack(bool success);