using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// DEV: 张柏泉
/// 游戏适配
/// </summary>
public class GameAdapt {
    public const int DESIGN_SCREEN_WIDTH = 1920;
    public const int DESIGN_SCREEN_HEIGHT = 1080;

    public static int gameWidth = 1280;
    public static int gameHeight = 720;

    public static float standardRate = 1;
    public static float screenRate = 1;
    public static float pixelScale = 1;
    public static float fullScale = 1;
    public static int matchWidthOrHeight = 1;

    public static int level = 1;
    public static int adjustLeft = 0;
    public static int adjustRight = 0;

    public static void Init(CanvasScaler canvasScaler) {
        standardRate = DESIGN_SCREEN_WIDTH  * 1f / DESIGN_SCREEN_HEIGHT;
        screenRate = Screen.width * 1f / Screen.height;
        
        if (screenRate >= standardRate) {
            canvasScaler.matchWidthOrHeight = 1;
            matchWidthOrHeight = 1;

            pixelScale = Screen.height * 1f / DESIGN_SCREEN_HEIGHT;
            fullScale = Screen.width * 1f / (DESIGN_SCREEN_WIDTH * pixelScale);
            gameWidth = (int)(DESIGN_SCREEN_HEIGHT * 1f / Screen.height * Screen.width);
            gameHeight = DESIGN_SCREEN_HEIGHT;
            //Debug.Log("-----111 pixelScale = " + pixelScale + " Screen.height = " + Screen.height+ " DESIGN_SCREEN_HEIGHT = "+ DESIGN_SCREEN_HEIGHT);
        } else {
            canvasScaler.matchWidthOrHeight = 0;
            matchWidthOrHeight = 0;

            pixelScale = Screen.width * 1f / DESIGN_SCREEN_WIDTH;
            fullScale = Screen.height * 1f / (DESIGN_SCREEN_HEIGHT * pixelScale);
            gameWidth = DESIGN_SCREEN_WIDTH;
            gameHeight = (int)(DESIGN_SCREEN_WIDTH * 1f / Screen.width * Screen.height);
            //Debug.Log("-----222 pixelScale = " + pixelScale + " Screen.width = " + Screen.width + " DESIGN_SCREEN_WIDTH = " + DESIGN_SCREEN_WIDTH);
        }
    }

    string[] iphoneModels = new string[] {
            "iPhone10,3",//iPhone X Global
            "iPhone10,6",//iPhone X GSM
            "iPhone11,2",//iPhone XS
            "iPhone11,4",//iPhone XS Max China
            "iPhone11,6",//iPhone XS Max
            "iPhone11,8",//iPhone XR
            "iPhone12,1",//iPhone 11
            "iPhone12,3",//iPhone 11 Pro
            "iPhone12,5",//iPhone 11 Pro Max
            "iPhone12,8",//iPhone SE 2nd Gen
            "iPhone13,1",//iPhone 12 Mini
            "iPhone13,2",//iPhone 12
            "iPhone13,3",//iPhone 12 Pro
            "iPhone13,4"//iPhone 12 Pro Max
        };

    /// <summary>
    /// 适配安卓苹果刘海屏水滴屏
    /// </summary>
    /// <param name="fitNode">适配节点</param>
    public void OpeniPhoneX(RectTransform fitNode) {
        Debug.Log("-------" + SystemInfo.deviceModel + "-------");
#if UNITY_IOS && !UNITY_EDITOR
            string model = SystemInfo.deviceModel;
            bool fit = false;
            foreach (var item in iphoneModels) {
                if(model.CompareTo(item) == 0) {
                    fitNode.offsetMax = new Vector2(-88f, 0f);
                    fitNode.offsetMin = new Vector2(44f, 0f);
                    fit = true;
                    break;
                }
            }
#elif UNITY_ANDROID && !UNITY_EDITOR
            //try {
            //    int[] offset = NativeReceiver.Get().jo.Call<int[]>("GetNotchHeight");
            //    fitNode.offsetMin = new Vector2(offset[1], 0f);
            //    //顶部高度
            //    fitNode.offsetMax = new Vector2(-offset[0], 0f);
                
            //    Debug.Log($"GetNotchHeight :: {offset[0]}, {offset[1]}");
            //} catch (Exception e) { }
            fitNode.offsetMax = new Vector2(GameWorld.Get().adjustLeft, 0);
#endif
        Debug.Log($"fitNode :: {fitNode.offsetMin}, {fitNode.offsetMax}");
    }
}
