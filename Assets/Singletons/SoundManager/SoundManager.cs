using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundManager : Singleton<SoundManager> {

    public SoundBank sound_bank { get { return _sound_bank; } }

    [SerializeField] AudioSource main, fade_in, sfx;
    [SerializeField] SoundBank _sound_bank;
    [Range(0,1)][SerializeField] float volume = 0.1f;

    Coroutine fade_routine;

    List<SFXClip> clips_played_this_frame;

    public static void PlaySong(AudioClip clip) {
        if (instance) {
            instance.LocalPlaySong(clip);
        }
    }
    public static void SetVolume(float volume) {
        if (instance) {
            instance.volume = volume;
            instance.SetAllVolumes(volume);
        }
    }
    public static float GetVolume() {
        if (instance) {
            return instance.volume;
        }
        return 0;
    }
    public static void Pause() {
        if (instance) {
            instance.main.Pause();
        }
    }
    public static void UnPause() {
        if (instance) {
            instance.main.UnPause();
        }
    }

    public void LocalPlaySong(AudioClip clip) {
        instance.main.UnPause();
        if (main.clip == null || !main.isPlaying) {
            main.clip = clip;
            if (fade_routine != null) {
                StopCoroutine(fade_routine);
            }
            fade_routine = StartCoroutine(FadeInMain(2f));
        } else {
            if (fade_routine != null) {
                StopCoroutine(fade_routine);
            }
            fade_routine = StartCoroutine(TradeOutMain(2f, clip));
        }
    }

    public void LocalPlaySfx(SFXInfo info, bool is_once_per_frame = true) {
        if (info == null || info.clip == null) return;
        if (is_once_per_frame && clips_played_this_frame.Contains(info.clip)) return;

        info.clip.PlaySound(sfx);
        clips_played_this_frame.Add(info.clip);
    }

    public void FadeOut() {
        if (fade_routine != null) StopCoroutine(fade_routine);  
        StartCoroutine(FadeOutMain(2f));
    }

    protected override void OnAwake() {
        sound_bank.ReloadDictionary();
        SetAllVolumes(volume);
        clips_played_this_frame = new List<SFXClip>();
    }

    private void LateUpdate() {
        clips_played_this_frame.Clear();
    }

    void SetAllVolumes(float volume) {
        main.volume = fade_in.volume = sfx.volume = volume;
    }

    IEnumerator FadeOutMain(float fade_length) {
        float timer = fade_length;
        while (timer > 0) {
            timer -= Time.unscaledDeltaTime;
            main.volume = Mathf.Pow((timer / fade_length), 4f) * volume;
            yield return null;
        }
        main.volume = 0;
        main.clip = null;
        main.Stop();

        fade_routine = null;
    }

    IEnumerator FadeInMain(float fade_length) {
        float timer = 0;
        main.Play();
        while (timer < fade_length) {
            timer += Time.unscaledDeltaTime;
            main.volume = (timer / fade_length) * volume;
            yield return null;
        }
        main.volume = volume;

        fade_routine = null;
    }

    IEnumerator TradeOutMain(float fade_length, AudioClip new_clip) {
        float timer = fade_length;
        fade_in.clip = new_clip;
        fade_in.Play();
        fade_in.volume = 0;
        while (timer > 0) {
            timer -= Time.unscaledDeltaTime;
            main.volume = Mathf.Pow(timer / fade_length, 4f) * volume;
            fade_in.volume = Mathf.Pow(1 - (timer / fade_length), 4f) * volume;
            yield return null;
        }
        main.volume = 0;
        main.clip = null;
        main.Stop();
        fade_in.volume = volume;
        AudioSource temp = main;
        main = fade_in;
        fade_in = temp;

        fade_routine = null;
    }
}
