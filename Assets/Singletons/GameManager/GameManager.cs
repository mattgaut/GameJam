using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : Singleton<GameManager> {

    [SerializeField] Character player_prefab;
   
    public Character player { get; private set; }
    public Level current_level;

    protected override void OnAwake() {
        StartGame();
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
