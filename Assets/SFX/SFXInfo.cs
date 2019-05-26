using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SFXInfo {

    public SFXClip clip {
        get {
            if (_override_clip == null) {
                if (default_clip == null) {
                    default_clip = SoundManager.instance.sound_bank.GetSFXClip(_default_codename);
                }
                return default_clip;
            } else {
                return _override_clip;
            }
        }
    }

    public static implicit operator SFXClip(SFXInfo info){
        return info.clip;
    }

    public SFXInfo(string default_name) {
        _default_codename = default_name;
    }

    [SerializeField] SFXClip _override_clip;
    [SerializeField] string _default_codename;
    [HideInInspector] [SerializeField] SFXClip default_clip;
}
