using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SoundManager : Singleton<SoundManager> {

    public SoundBank sound_bank { get { return _sound_bank; } }

    [SerializeField] AudioSource main, fade_in, sfx;
    [SerializeField] SoundBank _sound_bank;
    [Range(0,1)][SerializeField] float music_volume = 0.1f, sfx_volume = 0.1f;

    Coroutine fade_routine;

    List<SFXClip> clips_played_this_frame;

    Dictionary<SFXInfo, Coroutine> repeating_sounds;

    public static void SetVolume(float music_volume, float sfx_volume) {
        if (instance) {
            instance.music_volume = music_volume;
            instance.sfx_volume = sfx_volume;
            instance.SetAllVolumes(music_volume, sfx_volume);
        }
    }
    public static float GetVolume() {
        if (instance) {
            return instance.music_volume;
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

    public void PlaySong(SFXClip clip, bool loop = true) {
        instance.main.UnPause();
        main.loop = loop;
        main.volume = clip.volume;
        if (main.clip == null || !main.isPlaying) {
            main.clip = clip.clip;
            if (fade_routine != null) {
                StopCoroutine(fade_routine);
            }
            fade_routine = StartCoroutine(FadeInMain(2f));
        } else {
            if (fade_routine != null) {
                StopCoroutine(fade_routine);
            }
            fade_routine = StartCoroutine(TradeOutMain(2f, clip.clip));
        }
    }

    public void PlaySfx(SFXInfo info, bool is_once_per_frame = true) {
        if (info == null || info.clip == null) return;
        if (is_once_per_frame && clips_played_this_frame.Contains(info.clip)) return;
        info.clip.PlaySound(sfx);
        clips_played_this_frame.Add(info.clip);
    }

    public void PlayRepeating(SFXInfo info, float delay = 0) {
        if (info == null || info.clip == null) return;
        if (repeating_sounds.ContainsKey(info)) {
            return;
        }
        repeating_sounds.Add(info, StartCoroutine(RepeatSound(info, delay)));
    }

    public void StopRepeating(SFXInfo info) {
        if (repeating_sounds.ContainsKey(info)) {
            StopCoroutine(repeating_sounds[info]);
            repeating_sounds.Remove(info);
        }
    }

    public void FadeOut() {
        if (fade_routine != null) StopCoroutine(fade_routine);  
        StartCoroutine(FadeOutMain(2f));
    }

    protected override void OnAwake() {
        sound_bank.ReloadDictionary();
        SetAllVolumes(music_volume, sfx_volume);
        clips_played_this_frame = new List<SFXClip>();
        repeating_sounds = new Dictionary<SFXInfo, Coroutine>();
    }

    private void LateUpdate() {
        clips_played_this_frame.Clear();
    }

    void SetAllVolumes(float music_volume, float sfx_volume) {
        main.volume = fade_in.volume = music_volume;
        sfx.volume = sfx_volume;
    }

    IEnumerator RepeatSound(SFXInfo sound, float delay) {
        while (true) {
            sound.clip.PlaySound(sfx);
            yield return new WaitForSeconds(sound.clip.clip.length + delay);
        }
    }

    IEnumerator FadeOutMain(float fade_length) {
        float timer = fade_length;
        while (timer > 0) {
            timer -= Time.unscaledDeltaTime;
            main.volume = Mathf.Pow((timer / fade_length), 4f) * music_volume;
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
            main.volume = (timer / fade_length) * music_volume;
            yield return null;
        }
        main.volume = music_volume;

        fade_routine = null;
    }

    IEnumerator TradeOutMain(float fade_length, AudioClip new_clip) {
        float timer = fade_length;
        fade_in.clip = new_clip;
        fade_in.Play();
        fade_in.volume = 0;
        while (timer > 0) {
            timer -= Time.unscaledDeltaTime;
            main.volume = Mathf.Pow(timer / fade_length, 4f) * music_volume;
            fade_in.volume = Mathf.Pow(1 - (timer / fade_length), 4f) * music_volume;
            yield return null;
        }
        main.volume = 0;
        main.clip = null;
        main.Stop();
        fade_in.volume = music_volume;
        AudioSource temp = main;
        main = fade_in;
        fade_in = temp;

        fade_routine = null;
    }
}
