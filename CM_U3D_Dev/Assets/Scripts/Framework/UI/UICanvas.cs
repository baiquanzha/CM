using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI画布相关
/// </summary>
public class UICanvas : SingletonObject<UICanvas> {
    public Camera uiCamera;
    public Canvas canvas;
    public CanvasScaler canvasScaler;
    public Transform baseTrans;
    public Transform normalTrans;
    public Transform guideTrans;
    public Transform topTrans;
    public Transform cacheTrans;

    void Start() {
        //自适应初始化
        GameAdapt.Init(canvasScaler);
    }
    
}
