using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>
/// GameObject工具类
/// From:PandaQ
/// </summary>
public class GameObjectUtil
{
    public static GameObject Instantiate(GameObject obj, Transform parent) {
        if (obj) {
            return GameObject.Instantiate(obj, parent);
        }
        return null;
    }

    public static bool IsNull(GameObject obj) {
        if (obj) {
            return false;
        } else {
            return true;
        }
    }

    public static Component SafeGetComponent(GameObject go, string path, string componentType) {
        if (go == null ||
            string.IsNullOrEmpty(componentType))
            return null;

        Transform findTrans = go.transform;
        if (string.IsNullOrEmpty(path) == false)
            findTrans = go.transform.Find(path);
        if (findTrans == null)
            return null;

        return findTrans.GetComponent(componentType);
    }

    public static Component[] GetComponents(GameObject go, string componentType) {
        if (go == null || string.IsNullOrEmpty(componentType))
            return null;
        Type type = Type.GetType(componentType);
        return go.GetComponentsInChildren(type);
    }

    public static Component[] GetComponentsType(GameObject go, Type _type) {
        if (go == null || _type == null)
            return null;
        return go.GetComponentsInChildren(_type);
    }

    public static void SetPosition(GameObject obj, float x, float y, float z) {
        obj.transform.position = new Vector3(x, y, z);
    }

    public static void SetTransPos(Transform trans, float x, float y, float z) {
        trans.position = new Vector3(x, y, z);
    }

    //设置物体的缩放
    public static void SetLocalScale(GameObject go, float scaleX, float scaleY, float scaleZ)
    {
        if (go)
        {
            go.transform.localScale = new Vector3(scaleX, scaleY, scaleZ);
        }
        else
        {
            Debug.LogError("设置物体的缩放失败 传入了空的GameObject");
        }
    }

    //设置物体的坐标
    public static void SetLocalPosition(GameObject go, float posX, float posY, float posZ = 0f)
    {
        if (go)
        {
            go.transform.localPosition = new Vector3(posX, posY, posZ);
        }
        else
        {
            Debug.LogError("设置物体的坐标失败 传入了空的GameObject");
        }
    }

    //设置物体的旋转
    public static void SetLocalRotation(GameObject go, float rotX, float rotY, float rotZ)
    {
        if (go)
        {
            go.transform.localRotation = Quaternion.Euler(rotX, rotY, rotZ);
        }
        else
        {
            Debug.LogError("设置物体的旋转失败 传入了空的GameObject");
        }
    }

    #region RectTransform
    public static RectTransform GetRectTransformComp(GameObject go)
    {
        if (go)
        {
            RectTransform rectTrans = go.transform as RectTransform;
            if (rectTrans != null)
            {
                return rectTrans;
            }
            else
            {
                Debug.LogErrorFormat("组件中没有RectTransform [{0}]", go.name);
            }
        }
        else
        {
            Debug.LogError("获取RectTransfrom失败 传入了空的GameObject");
        }
        return null;
    }

    //设置组件的尺寸
    public static void SetRectSize(GameObject go, float width, float height)
    {
        if (go) {
            RectTransform rectTrans = GetRectTransformComp(go);
            if (rectTrans != null) {
                rectTrans.sizeDelta = new Vector2(width, height);
            } else {
                Debug.LogError("设置组件的尺寸失败");
            }
        }
    }

    //获取组件的尺寸
    public static Vector2 GetRectSize(GameObject go)
    {
        if (go) {
            RectTransform rectTrans = GetRectTransformComp(go);
            if (rectTrans != null) {
                return rectTrans.sizeDelta;
            } else {
                Debug.Log("获取组件的尺寸失败");
                return Vector2.zero;
            }
        } else {
            return Vector2.zero;
        }
    }

    //获取组件的宽度
    public static float GetRectWidth(GameObject go)
    {
        if (go)
        {
            return GetRectSize(go).x;
        }
        return 0;
    }

    //获取组件的高度
    public static float GetRectHeight(GameObject go)
    {
        if (go)
        {
            return GetRectSize(go).y;
        }
        return 0;
    }

    //设置物体的坐标
    public static void SetAnchoredPosition(GameObject go, float posX, float posY)
    {
        if (go)
        {
            RectTransform rectTrans = GetRectTransformComp(go);
            if (rectTrans != null)
            {
                rectTrans.anchoredPosition = new Vector2(posX, posY);
            }
            else
            {
                Debug.LogError("设置物体的坐标失败");
            }
        }
    }

    //设置锚点
    public static void SetAnchors(GameObject go, float minX, float minY, float maxX, float maxY)
    {
        if (go) {
            RectTransform rectTrans = GetRectTransformComp(go);
            if (rectTrans != null) {
                rectTrans.anchorMin = new Vector2(minX, minY);
                rectTrans.anchorMax = new Vector2(maxX, maxY);
            } else {
                Debug.LogError("设置锚点失败");
            }
        }
    }
    #endregion

    #region Text
    public static Text GetTextComp(GameObject go)
    {
        if (go)
        {
            Text text = go.GetComponent<Text>();
            if (text != null)
            {
                return text;
            }
            else
            {
                Debug.LogErrorFormat("组件中没有Text [{0}]", go.name);
            }
        }
        else
        {
            Debug.LogError("获取Text失败 传入了空的GameObject");
        }
        return null;
    }

    //修改文字
    public static void SetText(GameObject go, string str)
    {
        if (str != null)
        {
            Text text = GetTextComp(go);
            if (text != null)
            {
                text.text = str;
            }
            else
            {
                Debug.LogError("修改文字失败");
            }
        }
        else
        {
            Debug.LogError("修改文字失败 string为空");
        }
    }

    //获取文字
    public static string GetText(GameObject go)
    {
        Text text = GetTextComp(go);
        if (text != null)
        {
            return text.text;
        }
        else
        {
            Debug.LogError("获取文字失败");
            return "";
        }
    }

    //设置文字颜色[float]
    public static void SetTextColor(GameObject go, float r, float g, float b, float a = 1f)
    {
        Text text = GetTextComp(go);
        if (text != null)
        {
            text.color = new Color(r, g, b, a);
        }
        else
        {
            Debug.LogError("设置文字颜色[float]失败");
        }
    }
    #endregion

    #region Button

    public static Button GetButtonComp(GameObject go)
    {
        if (go)
        {
            Button button = go.GetComponent<Button>();
            if (button != null)
            {
                return button;
            }
            else
            {
                Debug.LogErrorFormat("组件中没有Button [{0}]", go.name);
            }
        }
        else
        {
            Debug.LogError("获取Button失败 传入了空的GameObject");
        }
        return null;
    }

    //设置按钮是否可交互
    public static void SetButtonInteractable(GameObject go, bool interactable)
    {
        Button button = GetButtonComp(go);
        if (button != null)
        {
            button.interactable = interactable;
        }
        else
        {
            Debug.LogError("设置按钮是否可交互失败");
        }
    }

    //添加按钮点击方法
    public static void AddClickFunc(GameObject go, UnityAction clickFunc)
    {
        if (clickFunc != null)
        {
            Button button = GetButtonComp(go);
            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(clickFunc);
            }
            else
            {
                Debug.LogError("添加按钮点击方法失败");
            }
        }
        else
        {
            Debug.LogError("添加按钮点击方法失败 UnityAction为空");
        }
    }

    public static void AddClickFuncMul(GameObject go, UnityAction clickFunc) {
        if (clickFunc != null) {
            Button button = GetButtonComp(go);
            if (button != null) {
                button.onClick.AddListener(clickFunc);
            } else {
                Debug.LogError("添加按钮点击方法失败");
            }
        } else {
            Debug.LogError("添加按钮点击方法失败 UnityAction为空");
        }
    }

    public static void RemoveClickFunc(GameObject go, UnityAction clickFunc) {
        if (clickFunc != null) {
            Button button = GetButtonComp(go);
            if (button != null) {
                button.onClick.RemoveListener(clickFunc);
            } else {
                Debug.LogError("移除按钮点击方法失败");
            }
        } else {
            Debug.LogError("移除按钮点击方法失败 UnityAction为空");
        }
    }

    //清除按钮点击方法
    public static void ClearClickFunc(GameObject go)
    {
        Button button = GetButtonComp(go);
        if (button != null)
        {
            button.onClick.RemoveAllListeners();
        }
        else
        {
            Debug.LogError("清除按钮点击方法失败");
        }
    }

    public static void AddScrollChangeFunc(GameObject go, UnityAction<Vector2> clickFunc) {
        if (clickFunc != null) {
            ScrollRect button = go.GetComponent<ScrollRect>();
            if (button != null) {
                button.onValueChanged.RemoveAllListeners();
                button.onValueChanged.AddListener(clickFunc);
            } else {
                Debug.LogError("添加滚动方法失败");
            }
        } else {
            Debug.LogError("添加滚动方法失败 UnityAction为空");
        }
    }

    public static void ClearScrollFunc(GameObject go) {
        ScrollRect button = go.GetComponent<ScrollRect>();
        if (button != null) {
            button.onValueChanged.RemoveAllListeners();
        }
    }

    #endregion

    #region Shadow
    public static Shadow GetShadowComp(GameObject go)
    {
        if (go)
        {
            Shadow[] shadows = go.GetComponents<Shadow>();
            Shadow shadow = null;
            for (int i = 0; i < shadows.Length; i++)
            {
                if (shadows[i].GetType() == typeof(Shadow))
                {
                    shadow = shadows[i];
                    break;
                }
            }
            if (shadow != null)
            {
                return shadow;
            }
            else
            {
                Debug.LogErrorFormat("组件中没有Shadow [{0}]", go.name);
            }
        }
        else
        {
            Debug.LogError("获取Outline失败 传入了空的GameObject");
        }
        return null;
    }

    //设置阴影颜色[float]
    public static void SetShadowColor(GameObject go, float r, float g, float b, float a = 1f)
    {
        Shadow shadow = GetShadowComp(go);
        if (shadow != null)
        {
            shadow.effectColor = new Color(r, g, b, a);
        }
        else
        {
            Debug.LogError("设置阴影颜色[float]失败");
        }
    }

    //设置阴影距离
    public static void SetShadowDistance(GameObject go, float x, float y)
    {
        Shadow shadow = GetShadowComp(go);
        if (shadow != null)
        {
            shadow.effectDistance = new Vector2(x, y);
        }
        else
        {
            Debug.LogError("设置阴影距离失败");
        }
    }
    #endregion

    #region Image
    public static Image GetImageComp(GameObject go)
    {
        if (go)
        {
            Image image = go.GetComponent<Image>();
            if (image != null)
            {
                return image;
            }
            else
            {
                Debug.LogErrorFormat("组件中没有Image [{0}]", go.name);
            }
        }
        else
        {
            Debug.LogError("获取Image失败 传入了空的GameObject");
        }
        return null;
    }

    //设置图片颜色[float]
    public static void SetImageColor(GameObject go, float r, float g, float b, float a = 1f)
    {
        Image image = GetImageComp(go);
        if (image != null)
        {
            image.color = new Color(r, g, b, a);
        }
        else
        {
            Debug.LogError("设置图片颜色[float]失败");
        }
    }

    //设置图片填充进度
    public static void SetImageFillAmount(GameObject go, float value)
    {
        Image image = GetImageComp(go);
        if (image != null)
        {
            image.fillAmount = value;
        }
        else
        {
            Debug.LogError("设置图片填充进度失败");
        }
    }
    #endregion

    #region Slider
    public static Slider GetSliderComp(GameObject go)
    {
        if (go)
        {
            Slider slider = go.GetComponent<Slider>();
            if (slider != null)
            {
                return slider;
            }
            else
            {
                Debug.LogErrorFormat("组件中没有Slider [{0}]", go.name);
            }
        }
        else
        {
            Debug.LogError("获取Slider失败 传入了空的GameObject");
        }
        return null;
    }

    //设置Slider进度
    public static void SetSliderValue(GameObject go, float value)
    {
        Slider slider = GetSliderComp(go);
        if (slider != null)
        {
            slider.value = value;
        }
        else
        {
            Debug.LogError("设置Slider进度失败");
        }
    }

    //添加进度变化方法
    public static void AddSliderValueChangedFunc(GameObject go, UnityAction<float> changedFunc)
    {
        if (changedFunc != null)
        {
            Slider slider = GetSliderComp(go);
            if (slider != null)
            {
                slider.onValueChanged.RemoveAllListeners();
                slider.onValueChanged.AddListener(changedFunc);
            }
            else
            {
                Debug.LogError("添加进度变化方法失败");
            }
        }
        else
        {
            Debug.LogError("添加进度变化方法失败 UnityAction<float>为空");
        }
    }

    //清除进度变化方法
    public static void ClearSliderValueChangedFunc(GameObject go)
    {
        Slider slider = GetSliderComp(go);
        if (slider != null)
        {
            slider.onValueChanged.RemoveAllListeners();
        }
        else
        {
            Debug.LogError("清除进度变化方法失败");
        }
    }
    #endregion

    #region Particle
    public static ParticleSystemRenderer GetParticleSystemRendererComp(GameObject go, bool isSilence = false)
    {
        if (go)
        {
            ParticleSystemRenderer particleSystemRenderer = go.GetComponent<ParticleSystemRenderer>();
            if (particleSystemRenderer != null)
            {
                return particleSystemRenderer;
            }
            else
            {
                if (!isSilence)
                {
                    Debug.LogErrorFormat("组件中没有ParticleSystem [{0}]", go.name);
                }
            }
        }
        else
        {
            Debug.LogError("获取ParticleSystem失败 传入了空的GameObject");
        }
        return null;
    }

    //设置粒子系统的蒙版类型
    public static void SetParticleSystemMasking(GameObject go, int maskType, bool isChild = true)
    {
        ParticleSystemRenderer psr = GetParticleSystemRendererComp(go, true);
        if (psr != null)
        {
            psr.maskInteraction = (SpriteMaskInteraction)maskType;
        }

        if (isChild)
        {
            Transform trans;
            for (int i = 0; i < go.transform.childCount; i++)
            {
                trans = go.transform.GetChild(i);
                SetParticleSystemMasking(trans.gameObject, maskType, true);
            }
        }
    }
    #endregion
}
