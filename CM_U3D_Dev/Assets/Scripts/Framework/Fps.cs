using UnityEngine;
using UnityEngine.UI;

public class Fps : SingletonObject<Fps>
{
    const string TAG = "Fps ";

    /// <summary>
    /// 是否开始fps
    /// </summary>
    //public static bool isStartFps;

    public static Fps mySelf;

    public Text txtFps;
    public float fps;
    public float curFps { get { return fps; } }

    //实时帧计算参数
    float f_UpdateInterval = 0.5f;//统计间隔时间
    float f_LastTime;//起始时间
    float f_Frames = 0;//帧数计数

    //统计fps
    float totalFps;//一定时间内总的fps
    int countFps;//统计fps的次数
    int countFps25;//>=25的fps的次数

    void Start()
    {
        mySelf = this;
        //StartFps();
    }

    void Update()
    {
        //if (!isStartFps)
        //{
        //    return;
        //}

        f_Frames = f_Frames + 1;
        if (Time.realtimeSinceStartup > f_LastTime + f_UpdateInterval)
        {
            float fps_Temp = f_Frames / (Time.realtimeSinceStartup - f_LastTime);
            fps = TextUtil.StrToFloat(string.Format("{0:0.#}", fps_Temp));//1位小数，四舍五入

            f_Frames = 0;
            f_LastTime = Time.realtimeSinceStartup;

            totalFps += fps;
            countFps += 1;
            if (fps >= 25)
                countFps25 += 1;

            txtFps.text = fps.ToString();
            //string info_Log = "fps: " + fps;
            //CTools.writeLog(CGameSet.resFullPath, GameLog.GetLogFileName(GameLog.fileName_Log_Fps), info_Log, true);
        }
    }

    /// <summary>
    /// 开始fps
    /// </summary>
    public void StartFps()
    {
        f_LastTime = Time.realtimeSinceStartup;
        f_Frames = 0;
        totalFps = 0;
        countFps = 0;
        countFps25 = 0;
        //isStartFps = true;

        //string info_Log = "开始fps----------------------";
        //CTools.writeLog(CGameSet.resFullPath, GameLog.GetLogFileName(GameLog.fileName_Log_Fps), info_Log, true);
    }

    /// <summary>
    /// 结束fps
    /// </summary>
    public void EndFps()
    {
        //isStartFps = false;

        string info_Log = TAG + "结束fps,";
        float time = countFps * f_UpdateInterval;
        info_Log += "用时: " + time + " 秒,";
        float averageFps = totalFps * f_UpdateInterval / time;
        info_Log += "fps均值: " + averageFps + " 帧/秒,";
        float rate = countFps25 * 100 / countFps;
        info_Log += "fps>=25比率: " + rate + " %";

        //CTools.writeLog(CGameSet.resFullPath, GameLog.GetLogFileName(GameLog.fileName_Log_Fps), info_Log, true);
    }

    /*void OnGUI()
    {
        GUI.color = Color.black;
        GUI.Label(new Rect(10, 10, 100, 20), "FPS:" + fps);
        GUI.color = Color.white;
        GUI.Label(new Rect(9, 9, 100, 20), "FPS:" + fps);
    }*/

}