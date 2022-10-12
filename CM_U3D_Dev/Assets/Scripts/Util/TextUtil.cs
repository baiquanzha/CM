using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

/// <summary>
/// DEV: 张柏泉
/// 文本工具类
/// </summary>
public class TextUtil  {
    //字符串转化为整形数组
    public static int[] StrToIntArr(string str, char splitStr = ',') {
        if (str == null || str == "") {
            return null;
        }
        string[] paramStrs = str.Split(splitStr);
        int[] value = new int[paramStrs.Length];
        for (int i = 0; i < value.Length; i++) {
            value[i] = Convert.ToInt32(paramStrs[i]);
        }
        return value;
    }

    //字符串转化为2维整形数组
    public static int[][] StrToIntArr2(string str, char splitStr1 = '|', char splitStr2 = ',') {
        if (str == null || str == "") {
            return null;
        }
        string[] paramStrs = str.Split(splitStr1);
        int[][] value = new int[paramStrs.Length][];
        for (int i = 0; i < value.Length; i++) {
            value[i] = StrToIntArr(paramStrs[i], splitStr2);
        }
        return value;
    }

    public static float[] StrToFloatArr(string str, char splitStr = ',') {
        if (str == null || str == "") {
            return null;
        }
        string[] paramStrs = str.Split(splitStr);
        float[] value = new float[paramStrs.Length];
        for (int i = 0; i < value.Length; i++) {
            value[i] = Convert.ToSingle(paramStrs[i]);
        }
        return value;
    }

    public static float[][] StrToFloatArr2(string str, char splitStr1 = '|', char splitStr2 = ',') {
        if (str == null || str == "") {
            return null;
        }
        string[] paramStrs = str.Split(splitStr1);
        float[][] value = new float[paramStrs.Length][];
        for (int i = 0; i < value.Length; i++) {
            value[i] = StrToFloatArr(paramStrs[i], splitStr2);
        }
        return value;
    }

    /// <summary>
    /// 将(秒)转换成时分秒 格式：00:00:00
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    public static string GetTimeStr(int time, bool isHour=false) {
        if (time > 0) {
            int s = (int)(time % 60);// 剩余秒数
            int m = (int)(time / 60) % 60;// 剩余分钟
            int h = (int)(time / 60 / 60) % 24;// 剩余小时
            if (h > 0 || isHour) {
                return string.Format("{0:D2}:{1:D2}:{2:D2}", h, m, s);
            } else {
                return string.Format("{0:D2}:{1:D2}", m, s);
            }
        } else {
            if (isHour) {
                return "00:00:00";
            } else {
                return "00:00";
            }
        }
    }

    /// <summary>
    /// 字符串转float
    /// </summary>
    /// <param name="str">字符串</param>
    /// <returns></returns>
    public static float StrToFloat(string str) {
        //if (str == null || str.Equals(""))
        if (str == null || str.Length <= 0) {
            return 0;
        }
        float returnValue = 0;
        try {
            if (str.StartsWith("0x") || str.StartsWith("0X")) {
                returnValue = (float)Convert.ToDouble(str.Substring(2));
            } else
                if (str.IndexOf(".") != -1) {
                returnValue = (float)Convert.ToDouble(str);
            } else {
                returnValue = (float)Convert.ToDouble(str);
            }
        } catch (Exception ex) {
        }
        return returnValue;
    }
}
