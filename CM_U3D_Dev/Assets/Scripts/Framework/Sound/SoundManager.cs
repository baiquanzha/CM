using MTool.AssetBundleManager.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 音效管理
/// </summary>
public class SoundManager : SingletonObject<SoundManager> {
    public Dictionary<string, AudioClip> clipDic = new Dictionary<string, AudioClip>();
    /// <summary>
    /// 音乐大小
    /// </summary>
    public float musicVol = 0.5f;
    /// <summary>
    /// 音乐大小
    /// </summary>
    public float soundVol = 0.5f;
    /// <summary>
    /// 音效组件缓存数量
    /// </summary>
    public int chacheAudioCount = 5;

    /// <summary>
    /// 背景音乐(只能存在一个，并且是循环的，播放新的会自动替换旧的)
    /// </summary>
    public AudioSource bgmSource;

    /// <summary>
    /// 播放中的音频字典
    /// </summary>
    private List<Sound> soundList = new List<Sound>();

    //缓存音效组件 5个
    private List<Sound> cacheSounds = new List<Sound>();

    protected override void Awake() {
        base.Awake();
        bgmSource = GetComponent<AudioSource>();
    }

    void Update() {
        for (int i = soundList.Count - 1; i >= 0; i--) {
            Sound sound = soundList[i];
            if (sound.isClear || sound.IsFinished()) {
                soundList.RemoveAt(i);

                if (cacheSounds.Count < chacheAudioCount) {
                    sound.Clear();
                    cacheSounds.Add(sound);
                } else {
                    sound.Destroy();
                }
            }
        }
    }

    public void PlayBGM(string path, bool isLoop) {
        AudioClip clip = null;
        if (clipDic.ContainsKey(path)) {
            clip = clipDic[path];
        } else {
            clip = AssetBundleManager.Load<AudioClip>("AudioClip/" + path);
            clipDic.Add(path, clip);
        }
        PlayBGM(clip, isLoop);
    }

    /// <summary>
    /// 播放背景音乐
    /// </summary>
    /// <param name="clip"></param>
    /// <param name="isLoop"></param>
    public void PlayBGM(AudioClip clip, bool isLoop) {
        bgmSource.clip = clip;
        bgmSource.loop = isLoop;
        bgmSource.volume = musicVol;
        bgmSource.Play();
    }

    public void SetMusicVolume(float _musicVol) {
        this.musicVol = _musicVol;
        if (bgmSource != null) {
            bgmSource.volume = _musicVol;
        }
    }

    public void SetSoundVolume(float _soundVol) {
        this.soundVol = _soundVol;
        AudioSource[] sources = this.gameObject.GetComponentsInChildren<AudioSource>();
        for (int i = 1; i < sources.Length; i++) {
            if (sources[i] != null) {
                sources[i].volume = _soundVol;
            }
        }
    }

    /// <summary>
    /// 暂停/恢复背景音乐
    /// </summary>
    public void PauseBGM() {
        if (bgmSource.clip == null) {
            Debug.LogWarning("没有设置背景音乐");
            return;
        }
        if (bgmSource.isPlaying) {
            bgmSource.Pause();
        } else {
            bgmSource.UnPause();
        }
    }

    /// <summary>
    /// 停止背景音乐
    /// </summary>
    public void StopBGM() {
        bgmSource.Stop();
    }

    public Sound PlaySound(string path, bool isLoop) {
        AudioClip clip = null;
        if (clipDic.ContainsKey(path)) {
            clip = clipDic[path];
        } else {
            clip = AssetBundleManager.Load<AudioClip>("AudioClip/" + path);
            clipDic.Add(path, clip);
        }
        return PlaySound(clip, isLoop);
    }

    /// <summary>
    /// 播放新音乐
    /// </summary>
    /// <param name="musicName"></param>
    /// <param name="clip"></param>
    /// <param name="isLoop"></param>
    /// <returns></returns>
    public Sound PlaySound(AudioClip clip, bool isLoop) {
        Sound sound = null;
        AudioSource source;
        if (cacheSounds.Count > 0) {
            sound = cacheSounds[0];
            source = sound.source;
            cacheSounds.RemoveAt(0);
        } else {
            source = this.gameObject.AddComponent<AudioSource>();
            sound = new Sound();

        }
        source.volume = soundVol;
        sound.Init(clip, source, isLoop);
        soundList.Add(sound);
        return sound;
    }

    /// <summary>
    /// 音乐停止
    /// </summary>
    /// <param name="musicName"></param>
    public void StopSound(Sound sound) {
        for (int i = 0; i < soundList.Count; i++) {
            if (soundList[i] == sound) {
                soundList.RemoveAt(i);
                CacheSound(sound);
                break;
            }
        }
    }

    public void CacheSound(Sound sound) {
        if (cacheSounds.Count < chacheAudioCount) {
            sound.Clear();
            cacheSounds.Add(sound);
        } else {
            sound.Destroy();
        }
    }
}
