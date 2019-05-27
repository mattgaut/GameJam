using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : Singleton<GameManager> {

    public bool input_active { get; private set; }

    [SerializeField] Character player_prefab;
    [SerializeField] bool start_on_awake;

    [SerializeField] Image fade_image;

    [SerializeField] Canvas ui;

    SFXInfo pause = new SFXInfo("sfx_pause");

    public Character player { get; private set; }
    public Level current_level { get; private set; }

    Dictionary<string, List<GameObject>> dropped_objects;

    protected override void OnAwake() {
        dropped_objects = new Dictionary<string, List<GameObject>>();
        input_active = true;
        if (start_on_awake) StartGame();
    }

    public void StartGame() {
        player = Instantiate(player_prefab);
        DontDestroyOnLoad(player);
        LoadScene("HubScene");
    }

    public void LoadScene(string name) {
        if (input_active) StartCoroutine(LoadSceneRoutine(name));
    }

    IEnumerator LoadSceneRoutine(string name) {
        input_active = false;
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
        input_active = true;
    }

    public void KillPlayer() {
        StartCoroutine(Respawn());
    }

    public void Quit() {
        Application.Quit();
    }

    private void Update() {
        if (Input.GetButtonDown("Pause")) {
            SoundManager.instance.PlaySfx(pause);
            if (Time.timeScale == 0) {
                Time.timeScale = 1;
                ui.gameObject.SetActive(false);
            } else {
                Time.timeScale = 0;
                ui.gameObject.SetActive(true);
            }
        }
    }

    IEnumerator Respawn() {
        input_active = false;
        int lk = player.LockInvincibility();

        yield return FadeToBlack(2f);

        input_active = true;
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
