using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager> {

    [SerializeField] Character player_prefab;
    [SerializeField] bool start_on_awake;

    public Character player { get; private set; }
    public Level current_level { get; private set; }

    protected override void OnAwake() {
        if (start_on_awake) StartGame();
    }

    public void StartGame() {
        player = Instantiate(player_prefab);
        DontDestroyOnLoad(player);
        LoadScene("HubScene");
    }

    public void LoadScene(string name) {
        StartCoroutine(LoadSceneRoutine(name));
    }

    IEnumerator LoadSceneRoutine(string name) {
        SceneManager.LoadScene(name);
        yield return null;

        current_level = FindObjectOfType<Level>();
        player.transform.position = current_level.spawn_point.position;
    }
}
