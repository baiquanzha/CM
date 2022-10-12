using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sound {
    public AudioClip clip;
    public AudioSource source;
    public bool isLoop = false;
    public bool isClear;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="clip"></param>
    public Sound() {

    }

    public void Init(AudioClip clip, AudioSource source, bool isLoop) {
        isClear = false;
        this.clip = clip;
        this.source = source;
        this.source.clip = clip;
        this.isLoop = isLoop;
        this.source.loop = isLoop;
        this.source.Play();
    }


    /// <summary>
    /// 播放进度
    /// </summary>
    public float progress {
        get {
            if (source == null || clip == null)
                return 0f;
            return (float)source.timeSamples / (float)clip.samples;
        }
    }

    public void Play() {
        if (!source.isPlaying) {
            source.UnPause();
        }
    }

    public void Pause() {
        if (source.isPlaying) {
            source.Pause();
        }
    }

    public bool IsPlaying() {
        return source != null && source.isPlaying;
    }

    /// <summary>
    /// 判断是否完成播放
    /// </summary>
    public bool IsFinished() {
        if (isLoop) {
            return false;
        }
        return progress >= 1f || !IsPlaying();
    }

    public void Clear() {
        clip = null;
        source.clip = null;
    }

    public void Destroy() {
        clip = null;
        source.clip = null;
        GameObject.Destroy(source);
        source = null;
    }
}
