using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using XLua;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Spine.Unity;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using MTool.Core.Security.Cryptography;
using MTool.Framework.Network;
using MTool.Framework;

public static class CustomWrapConfig
{
    [LuaCallCSharp]
    public static List<Type> WrapTypeList = new List<Type>()
    {
        typeof(Transform),
        typeof(RectTransform),
        typeof(Image),
        typeof(Sprite),
        typeof(Canvas),
        typeof(Renderer),
        typeof(SpriteRenderer),
        typeof(SortingGroup),
        typeof(SkeletonAnimation),
        typeof(MaterialPropertyBlock),
        typeof(TimeSpan),
        typeof(DateTime),
        typeof(Application),
        typeof(RenderTexture),
        typeof(Camera),
        typeof(MeshRenderer),
        typeof(Convert),
        typeof(Animation),
        typeof(Animator),
        typeof(EventSystem),
        typeof(Vector3),
        typeof(Vector2),
        typeof(Vector4),
        typeof(ScrollRect),
        typeof(LayoutUtility),
        typeof(TextAnchor),
        typeof(Input),
        typeof(Tilemap),
        typeof(PlayerPrefs),

        typeof(IGlobe),
        typeof(SoundManager),
        typeof(GameLauncher),

        typeof(ProtokitHelper.ProtokitHttpClient),
        typeof(ProtokitHelper.ProtokitClient),
        typeof(ProtokitHelper.DelegateRecvProto),
        typeof(X25519),
        typeof(X25519KeyAgreement),
        typeof(Aes),
        typeof(Trace),
    };

    [LuaCallCSharp]
    [ReflectionUse]
    public static List<Type> DOTweenTypeList = new List<Type>()
    {
        typeof(DG.Tweening.AutoPlay),
        typeof(DG.Tweening.AxisConstraint),
        typeof(DG.Tweening.Ease),
        typeof(DG.Tweening.LogBehaviour),
        typeof(DG.Tweening.LoopType),
        typeof(DG.Tweening.PathMode),
        typeof(DG.Tweening.PathType),
        typeof(DG.Tweening.RotateMode),
        typeof(DG.Tweening.ScrambleMode),
        typeof(DG.Tweening.TweenType),
        typeof(DG.Tweening.UpdateType),
        typeof(DG.Tweening.PathType),

        typeof(DG.Tweening.DOTween),
        typeof(DG.Tweening.DOTweenModuleUI),
        typeof(DG.Tweening.DOTweenModuleSprite),
        typeof(DG.Tweening.DOVirtual),
        typeof(DG.Tweening.EaseFactory),
        typeof(DG.Tweening.Tweener),
        typeof(DG.Tweening.Tween),
        typeof(DG.Tweening.Sequence),
        typeof(DG.Tweening.TweenParams),
        typeof(DG.Tweening.Core.ABSSequentiable),

        typeof(DG.Tweening.Core.TweenerCore<Vector3, Vector3, DG.Tweening.Plugins.Options.VectorOptions>),

        typeof(DG.Tweening.TweenCallback),
        typeof(DG.Tweening.TweenExtensions),
        typeof(DG.Tweening.TweenSettingsExtensions),
        typeof(DG.Tweening.ShortcutExtensions),
    };

    [CSharpCallLua]
    //一些委托类型 Xlua不能直接识别 需要在此添加
    public static List<Type> CSharpCallLuaTypeList = new List<Type>()
    {
        typeof(UnityAction<bool>),
        typeof(UnityAction<string>),
        typeof(UnityAction<float>),
        typeof(UnityAction<int>),
        typeof(UnityAction<GameObject, int>),
        typeof(UnityAction<float, float>),
        typeof(UnityAction<float, float, float, float>),
        typeof(UnityAction<Vector2>),

        typeof(Action),
        typeof(Func<double, double, double>),
        typeof(Action<string>),
        typeof(Action<double>),
        typeof(Action<int>),
        typeof(Action<LuaTable, bool>),
        typeof(UnityEngine.Events.UnityAction),
        typeof(System.Collections.IEnumerator),
        typeof(ProtokitHelper.DelegateRecvProto),
        typeof(ProtokitHelper.DelegateRecvPacketFinish),
        typeof(ProtokitHelper.DelegateHttpResponseFailed),
        typeof(ProtokitHelper.DelegateRecvProtoBytes),
        typeof(ProtokitHelper.DelegateRecvCommonError),
        typeof(ABObjectCallBack),
        typeof(ObjectCallBack),
    };
}