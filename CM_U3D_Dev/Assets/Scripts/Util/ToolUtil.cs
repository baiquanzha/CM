using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using XLua;

public class ToolUtil {
    public static byte[] StringToBytes(string str) {
        return Encoding.UTF8.GetBytes(str);
    }

    public static byte[] HexStringToByteArray(string hexString) {
        //将16进制秘钥转成字节数组
        var byteArray = new byte[hexString.Length / 2];
        for (var x = 0; x < byteArray.Length; x++) {
            var i = Convert.ToInt32(hexString.Substring(x * 2, 2), 16);
            byteArray[x] = (byte)i;
        }
        return byteArray;
    }

    public static string GetMD5HashFromString(string str) {
        if (str != null) {
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(str);
            return GetMD5HashFromBytes(bytes);
        } else {
            return null;
        }
    }

    public static string GetMD5HashFromBytes(byte[] bytes) {
        try {
            MemoryStream ms = new MemoryStream(bytes);
            //FileStream file = new FileStream(fileName, FileMode.Open);
            System.Security.Cryptography.MD5 md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] retVal = md5.ComputeHash(ms);
            //file.Close();

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < retVal.Length; i++) {
                sb.Append(retVal[i].ToString("x2"));
            }
            return sb.ToString();
        } catch (Exception ex) {
            Debug.LogError("获取bytes hash异常" + ex.Message);
            return null;
            //throw new Exception("GetMD5HashFromFile() fail, fileName" + fileName + ",error: " + ex.Message);
        }
    }

    #region 加解密
    public const string PrivateKey = "WOKEemnQ52JrWcYR";
    public const string IV = "1234567890123456";

    /// <summary>
    ///  AES 加密
    /// </summary>
    /// <param name="str">明文（待加密）</param>
    /// <param name="key">密文</param>
    /// <returns></returns>

    public static string AesEncrypt(string str) {
        if (string.IsNullOrEmpty(str)) return null;
        byte[] toEncryptArray = Encoding.UTF8.GetBytes(str);

        RijndaelManaged rm = new RijndaelManaged();
        rm.Key = Encoding.UTF8.GetBytes(PrivateKey);
        rm.IV = Encoding.UTF8.GetBytes(IV);
        rm.Mode = CipherMode.CBC;
        rm.Padding = PaddingMode.PKCS7;
        ICryptoTransform cTransform = rm.CreateEncryptor();
        byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

        return Convert.ToBase64String(resultArray, 0, resultArray.Length);
    }
    /// <summary>
    ///  AES 解密
    /// </summary>
    /// <param name="str">明文（待解密）</param>
    /// <param name="key">密文</param>
    /// <param name="iv">向量</param>
    /// <returns></returns>
    public static string AesDecrypt(string str) {
        if (string.IsNullOrEmpty(str)) return null;
        byte[] toEncryptArray;
        try {
            toEncryptArray = Convert.FromBase64String(str);
        } catch { return null; }

        RijndaelManaged rm = new RijndaelManaged();
        rm.Key = Encoding.UTF8.GetBytes(PrivateKey);
        rm.IV = Encoding.UTF8.GetBytes(IV);
        rm.Mode = CipherMode.CBC;
        rm.Padding = PaddingMode.PKCS7;

        ICryptoTransform cTransform = rm.CreateDecryptor();
        byte[] resultArray = cTransform.TransformFinalBlock(toEncryptArray, 0, toEncryptArray.Length);

        return Encoding.UTF8.GetString(resultArray);
    }
    #endregion

    //射线
    public static bool Raycast(Ray ray, out RaycastHit hit) {
        return Physics.Raycast(ray, out hit);
    }

    //射线
    public static bool Raycast(Ray ray, out RaycastHit hit, float maxDistance, int layerMask) {
        return Physics.Raycast(ray, out hit, maxDistance, layerMask);
    }

    public static TweenerCore<Vector2, Vector2, VectorOptions> DOTweenTo(RectTransform objRect, Vector2 targetPos, float time) {
        return DOTween.To(() => { return objRect.anchoredPosition; },
                    (v) => { objRect.anchoredPosition = v; }, targetPos, time);
    }

    public static TweenerCore<float, float, FloatOptions> DOTweenTime(float time) {
        float count = 0;
        return DOTween.To(() => count, a => count = a, 1, time);
    }

    public static TweenerCore<Color, Color, ColorOptions> DOSpriteFade(SpriteRenderer target, float endValue, float duration) {
        return target.DOFade(endValue, duration);
    }

    public static TweenerCore<float, float, FloatOptions> DOTweenTo(float value, LuaFunction luaFun, float targetValue, float time) {
        return DOTween.To(() => { return value; }, (v) => {
            value = v;
            luaFun.Call(v);
        }, targetValue, time);
    }

    public static Tweener SetTweenerComplete(Tweener tween, LuaFunction fun) {
        return tween.OnComplete(() => {
            if (fun != null) {
                fun.Call();
                fun.Dispose();
            }
        });
    }

    //点是否在三角形内
    public static bool PointAndTriangleCollision(Vector2 point, Vector2 pos1, Vector2 pos2, Vector2 pos3) {
        // 利用几何学中点和直线的关系。几何学中表示直线的方程式ax + by + c = 0，如果某个点（x1，y1）在直线上，
        // 将坐标点的值带入方程会得到d = ax1 + by1 + c，如果：
        // d = 0：点在直线上；
        // d > 0：点在直线下方（右方）；
        // d< 0：点在直线上方（左方）；
        // 根据这个性质，要判断点P是否在三角形内部，我们就需要判断点P是否在三条直线的同一个方向上。
        // 首先需要根据两个顶点求出参数a，b，c，假如我们需要求出顶点A和顶点B所在的直线方程，那么
        // a = yb - ya;
        // b = xa - xb;
        // c = xb* ya - xa* yb;
        // 在求出a，b，c之后，就可以使用直线方程求d。
        // 不过在求出参数之前，我们需要注意一点就是直线没有方向，直线方程可以在两边同时乘以负数，
        // 直线方程的意义不会变化，但是这种不确定性对于我们求d值会造成影响。
        // 为了规避这种影响，我们需要保证在求三条直线方程的时候，
        // 三个顶点以顺时针（或者逆时针）的一个方向进行计算，
        // 也就是说在求每条直线方程的两个顶点统一用终点减去起点（或者用起点减去终点）求出斜率。
        // 这样就可以保证d的一致性，在求出三个d值（d1，d2，d3）后，如果其中任意两个d相乘的结果小于0，即表示P点不在三角形内部。
        float a = 0;
        float b = 0;
        float c = 0;

        float d1 = 0;
        float d2 = 0;
        float d3 = 0;

        // 顶点1和顶点2的直线方程
        a = pos2.y - pos1.y;
        b = pos1.x - pos2.x;
        c = pos2.x * pos1.y - pos1.x * pos2.y;
        d1 = a * point.x + b * point.y + c;

        // 顶点2和顶点3的直线方程
        a = pos3.y - pos2.y;
        b = pos2.x - pos3.x;
        c = pos3.x * pos2.y - pos2.x * pos3.y;
        d2 = a * point.x + b * point.y + c;

        if (d1 * d2 < 0)
            return false;

        // 顶点3和顶点1的直线方程
        a = pos1.y - pos3.y;
        b = pos3.x - pos1.x;
        c = pos1.x * pos3.y - pos3.x * pos1.y;
        d3 = a * point.x + b * point.y + c;

        if (d2 * d3 < 0)
            return false;
        return true;
    }

    public static bool IsPointInTriangle(Vector3 point, Vector3 pos1, Vector3 pos2, Vector3 pos3) {
        float a = 0;
        float b = 0;
        float c = 0;

        float d1 = 0;
        float d2 = 0;
        float d3 = 0;

        // 顶点1和顶点2的直线方程
        a = pos2.y - pos1.y;
        b = pos1.x - pos2.x;
        c = pos2.x * pos1.y - pos1.x * pos2.y;
        d1 = a * point.x + b * point.y + c;

        // 顶点2和顶点3的直线方程
        a = pos3.y - pos2.y;
        b = pos2.x - pos3.x;
        c = pos3.x * pos2.y - pos2.x * pos3.y;
        d2 = a * point.x + b * point.y + c;

        if (d1 * d2 < 0)
            return false;

        // 顶点3和顶点1的直线方程
        a = pos1.y - pos3.y;
        b = pos3.x - pos1.x;
        c = pos1.x * pos3.y - pos3.x * pos1.y;
        d3 = a * point.x + b * point.y + c;

        if (d2 * d3 < 0)
            return false;
        return true;
    }

    // 圆与矩形碰撞检测// 圆心(x, y), 半径r, 矩形中心(x0, y0), 矩形上边中心(x1, y1), 矩形右边中心(x2, y2)
    public static bool IsCircleIntersectRectangle(float x, float y, float r, float x0, float y0, float x1, float y1, float x2, float y2) {
        float w1 = DistanceBetweenTwoPoints(x0, y0, x2, y2);
        float h1 = DistanceBetweenTwoPoints(x0, y0, x1, y1);
        float w2 = DistanceFromPointToLine(x, y, x0, y0, x1, y1);
        float h2 = DistanceFromPointToLine(x, y, x0, y0, x2, y2);
        if (w2 > w1 + r)
            return false;
        if (h2 > h1 + r)
            return false;
        if (w2 <= w1)
            return true;
        if (h2 <= h1)
            return true;
        return (w2 - w1) * (w2 - w1) + (h2 - h1) * (h2 - h1) <= r * r;
    }

    // 计算两点之间的距离
    public static float DistanceBetweenTwoPoints(float x1, float y1, float x2, float y2) {
        return Mathf.Sqrt((x2 - x1) * (x2 - x1) + (y2 - y1) * (y2 - y1));
    }

    // 计算点(x, y)到经过两点(x1, y1)和(x2, y2)的直线的距离
    public static float DistanceFromPointToLine(float x, float y, float x1, float y1, float x2, float y2) {
        float a = y2 - y1;
        float b = x1 - x2;
        float c = x2 * y1 - x1 * y2;
        //assert(fabs(a) > 0.00001f || fabs(b) > 0.00001f);	
        return Mathf.Abs(a * x + b * y + c) / Mathf.Sqrt(a * a + b * b);
    }

    public static float GetPosZ(float x, float y) {
        RaycastHit hitInfo;
        Vector3 origin = new Vector3(x, y, -500);
        if (Physics.Raycast(origin, new Vector3(0, 0, 1), out hitInfo)) {
            return hitInfo.point.z;
        }
        return 0;
    }
}
