using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySongOnAwake : MonoBehaviour {

    [SerializeField] AudioClip song;
    void Start() {
        SoundManager.instance.PlaySong(song);
    }
}
