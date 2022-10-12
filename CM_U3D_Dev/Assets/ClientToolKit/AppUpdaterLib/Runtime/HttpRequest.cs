using MTool.LoggerModule.Runtime;
using System;
using UnityEngine;
using UnityEngine.Networking;
using ILogger = MTool.LoggerModule.Runtime.ILogger;

public class HttpRequest
{
    private static readonly Lazy<ILogger> s_mLogger = new Lazy<ILogger>(()=> 
        LoggerManager.GetLogger("HttpRequest"));

    public string CurrentUrl = null;
    private UnityWebRequest req = null;
    private float lastProcess = 0.0f, duration = 0.0f, allDuration = 4.0f;
    private byte retryCount = 0, retryAllCount = 3;
    private byte loadState = 0;
    private byte[] Data;
    private Action<byte[]> callBackHandle;
    public void Load(string url, Action<byte[]> callback)
    {
        if (loadState != 0) return;
        callBackHandle = callback;
        loadState = 1;
        retryCount = 0;
        CurrentUrl = url;
        Update();
    }

    public void Update()
    {
        switch(loadState)
        {
            case 1://初始化
                {
                    duration = 0;
                    lastProcess = 0;
                    s_mLogger.Value.Debug($"Downloading {CurrentUrl}");
                    req = UnityWebRequest.Get(CurrentUrl);
                    req.SendWebRequest();
                    loadState = 2;
                    break;
                }
            case 2://下载
                {
                    if (req.isDone)
                    {
                        if (req.isNetworkError || req.isHttpError)
                        {
                            s_mLogger.Value.Error(req.responseCode.ToString() + ":" + req.error + ":" + req.url);
                            loadState = 3;
                        }
                        else
                        {
                            s_mLogger.Value.Debug($"responseCode : {req.responseCode}");
                            if (req.responseCode == 200 || req.responseCode == 304)
                            {
                                loadState = 4;
                            }
                            else
                            {
                                loadState = 3;
                            }
                        }
                    }
                    else
                    {
                        if(lastProcess == req.downloadProgress)
                        {
                            duration += Time.deltaTime;
                            if(duration >= allDuration)
                            {
                                loadState = 3;
                            }
                        }
                        else
                        {
                            lastProcess = req.downloadProgress;
                            duration = 0;
                        }
                    }
                    break;
                }
            case 3://失败处理
                {
                    req.Dispose();
                    req = null;
                    retryCount++;
                    if(retryCount >= retryAllCount)
                    {
                        loadState = 5;
                    }
                    else
                    {
                        //重试
                        loadState = 1;
                        s_mLogger.Value.Info("retry:" +retryCount + ",url："+ CurrentUrl);
                    }
                    break;
                }
            case 4://成功回调
                {
                    loadState = 0;
                    Data = req.downloadHandler.data;
                    req.Dispose();
                    req = null;
                    callBackHandle(Data);
                    Data = null;
                    break;
                }
            case 5://失败回调
                {
                    loadState = 0;
                    if(req != null)
                    {
                        req.Dispose();
                        req = null;
                    }
                    callBackHandle(null);
                    break;
                }
        }
    }
    private ulong lastSize = 0;
    public ulong LoadSize()
    {
        if (req != null)
        {
            lastSize = req.downloadedBytes;
        }
        return lastSize;
    }
    public void Clear()
    {
        if(req != null)
        {
            req.Dispose();
            req = null;
        }
        Data = null;
    }
}
