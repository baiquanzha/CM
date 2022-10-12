using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeUtil {
    public static long m_serverTimeOffset = 0;  //客户端于服务器毫秒时间差值

    /// <summary>
    /// 获得服务器毫秒时间
    /// </summary>
    /// <returns>13位时间戳</returns>
    public static long GetServerMSTime() {
        return GetTimeMillStamp() + m_serverTimeOffset;
    }

    //获取服务器时间结构体
    public static TimeSpan GetServerTimeSpan() {
        DateTime tempSysDT = DateTime.UtcNow.AddMilliseconds(m_serverTimeOffset);
        return tempSysDT - new DateTime(1970, 1, 1);
    }

    /// <summary>
    /// 返回服务器时间
    /// </summary>
    /// <returns>10位时间戳</returns>
    public static int GetServerTime() {
        return (int)(GetServerMSTime() / 1000);
    }

    /// 获取当前时间戳
    /// </summary>
    /// <returns></returns>
    public static int GetTimeStamp() {
        TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return (int)Convert.ToInt64(ts.TotalSeconds);
    }

    //获得本地毫秒时间戳
    public static long GetTimeMillStamp() {
        TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return Convert.ToInt64(ts.TotalMilliseconds);
    }

    //获取当前时间结构体
    public static TimeSpan GetCurTimeSpan() {
        return DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
    }

    public static DateTime TimeToDateTime(long time) {
        DateTime tempSysDT = new DateTime(1970, 1, 1);
        tempSysDT = tempSysDT.AddMilliseconds(time);
        return tempSysDT;
    }

    public static bool IsAfterTime(long time, int hour, int minute) {
        DateTime tempSysDT = new DateTime(1970, 1, 1);
        tempSysDT = tempSysDT.AddMilliseconds(time);
        DateTime curTime = DateTime.UtcNow;
        //TimeSpan ts = curTime - tempSysDT;
        //if (ts.Days > 0) {
        //    return true;
        //}
        int tempDays = Mathf.FloorToInt(time / 1000f / 3600 / 24);
        int curDays = Mathf.FloorToInt(GetServerMSTime() / 1000f / 3600 / 24);
        if (curDays > tempDays) {
            return true;
        }
        //时间已在节点之后
        if (tempSysDT.Hour > hour) {
            return false;
        } else if (tempSysDT.Hour == hour && tempSysDT.Minute >= minute) {
            return false;
        }

        //当前时间在节点之后
        if (curTime.Hour > hour) {
            return true;
        } else if (curTime.Hour == hour && curTime.Minute >= minute) {
            return true;
        }

        return false;
    }
}
