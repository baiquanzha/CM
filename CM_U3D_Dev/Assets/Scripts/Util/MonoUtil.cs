using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using XLua;

/// <summary>
/// DEV: 张柏泉
/// mono工具类
/// </summary>
public class MonoUtil : SingletonObject<MonoUtil>
{
    void Start()
    {

    }

    //等待
    public Coroutine Wait(float time, Action call, MonoBehaviour mono = null)
    {
        if (!mono || mono == null)
        {
            mono = this;
        }
        return mono.StartCoroutine(WaitTime(time, call));
    }

    IEnumerator WaitTime(float time, Action call)
    {
        yield return new WaitForSeconds(time);
        try
        {
            call?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError("GameExtend.WaitTime" + e);
        }
    }

    //等待
    public Coroutine Wait(float time, LuaFunction func, MonoBehaviour mono = null, object param = null)
    {
        if (!mono || mono == null)
        {
            mono = this;
        }
        return mono.StartCoroutine(WaitTime(time, func, param));
    }

    IEnumerator WaitTime(float time, LuaFunction func, object param)
    {
        yield return new WaitForSeconds(time);
        try
        {
            if (param != null)
            {
                func.Call(param);
            }
            else
            {
                func.Call();
            }
        }
        catch (Exception e)
        {
            Debug.LogError("GameExtend.WaitTime" + e);
        }
    }

    //停止
    public void Stop(Coroutine coroutine, MonoBehaviour mono = null)
    {
        if (!mono || mono == null)
        {
            mono = this;
        }
        mono.StopCoroutine(coroutine);
    }


    #region 异步加载场景
    public void StartSceneCoroutine(string sceneName, LuaFunction function)
    {
        StartCoroutine(LoadScene(sceneName, function));
    }

    IEnumerator LoadScene(string sceneName, LuaFunction function)
    {
        yield return SceneManager.LoadSceneAsync(sceneName);
        function.Call();
    }
    #endregion
}
