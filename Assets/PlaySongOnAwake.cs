using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySongOnAwake : MonoBehaviour {

    [SerializeField] SFXClip song;
    void Start() {
        SoundManager.instance.PlaySong(song);
    }
}
