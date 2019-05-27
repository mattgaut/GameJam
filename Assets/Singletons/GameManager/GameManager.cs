using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : Singleton<GameManager> {

    [SerializeField] Character player_prefab;
    [SerializeField] bool start_on_awake;

    [SerializeField] Image fade_image;

    public Character player { get; private set; }
    public Level current_level { get; private set; }

    Dictionary<string, List<GameObject>> dropped_objects;

    protected override void OnAwake() {
        dropped_objects = new Dictionary<string, List<GameObject>>();
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
        if (dropped_objects.ContainsKey(SceneManager.GetActiveScene().name)) {
            foreach (GameObject go in dropped_objects[SceneManager.GetActiveScene().name]) {
                go.SetActive(false);
            }
        }
        if (!dropped_objects.ContainsKey(name)) {
            dropped_objects.Add(name, new List<GameObject>());
        }

        if (fade_image.color != Color.black) {
            yield return FadeToBlack(1f);
        }

        SceneManager.LoadScene(name);

        yield return null;

        current_level = FindObjectOfType<Level>();
        player.transform.position = current_level.spawn_point.position;

        foreach (GameObject go in dropped_objects[name]) {
            go.SetActive(true);
        }

        yield return FadeFromBlack(1f);
    }

    public void KillPlayer() {
        StartCoroutine(Respawn());
    }

    IEnumerator Respawn() {
        int lk = player.LockInvincibility();

        yield return FadeToBlack(2f);

        LoadScene("HubScene");

        player.RestoreHealth(player.health.max);

        player.UnlockInvincibility(lk);
    }

    public void AddDroppedObjects(GameObject obj) {
        dropped_objects[SceneManager.GetActiveScene().name].Add(obj);
        DontDestroyOnLoad(obj);
    }

    public void RemoveDroppedObjects(GameObject obj) {
        dropped_objects[SceneManager.GetActiveScene().name].Remove(obj);
    }

    IEnumerator FadeToBlack(float length) {
        float time = 0;

        fade_image.color = new Color(0, 0, 0, 0);
        while (time < length) {
            yield return null;
            time += Time.deltaTime;
            fade_image.color = new Color(0,0,0, (time/length));            
        }
        fade_image.color = Color.black;
    }
    IEnumerator FadeFromBlack(float length) {
        float time = length;

        fade_image.color = new Color(0, 0, 0, 1);
        while (time > 0) {
            yield return null;
            time -= Time.deltaTime;
            fade_image.color = new Color(0, 0, 0, (time / length));
        }
    }
}
