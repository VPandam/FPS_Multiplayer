using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public enum GameState
{
    inGame, pause, gameOver, menu
}
public class GameManager : MonoBehaviour
{
    [SerializeField]
    int maxFrames = 90;
    [SerializeField]
    GameObject[] spawners;
    int currentRound;

    [SerializeField]
    TextMeshProUGUI roundText;
    [SerializeField]
    TextMeshProUGUI roundsSurvivedText;

    string stringRoundText;

    [SerializeField]
    GameObject zombiePrefab;

    [SerializeField]
    GameObject gameOverPanel;

    //Depending on whitch platform we are on, we load one or other scene.
    [SerializeField]
    string menuScene, mainScene;

    bool gamePaused;
    [SerializeField] GameObject pausePanel;
    GameState currentGameState;
    public GameState CurrentGameState { get => currentGameState; }


    // Start is called before the first frame update
    void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = maxFrames;
        StartGame();
    }
    void StartGame()
    {
        currentGameState = GameState.inGame;
        StartCoroutine(LookForEnemies());
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Pause"))
        {
            if (currentGameState == GameState.pause)
                Resume();
            else
                Pause();
        }
    }

    void SetRound(int round)
    {
        currentRound = round;
        roundText.text = $"Round: {currentRound}";
        Debug.Log("SetRound");
    }

    void StartNextRound()
    {
        SetRound(currentRound + 1);
        for (int i = 0; i < currentRound; i++)
        {
            int randomSpawnIndex = Random.Range(0, spawners.Length);

            GameObject enemy = Instantiate(zombiePrefab, spawners[randomSpawnIndex].transform.position,
            Quaternion.identity);
            enemy.GetComponent<ZombieManager>().gameManager = this;
        }
    }

    public IEnumerator LookForEnemies()
    {
        yield return new WaitForSeconds(3.2f);
        GameObject[] enemiesAlive = GameObject.FindGameObjectsWithTag("Enemy");

        if (enemiesAlive.Length == 0)
        {
            StartNextRound();
        }
    }

    public void GameOver()
    {
        currentGameState = GameState.gameOver;
        roundsSurvivedText.text = $"ROUNDS SURVIVED: {currentRound}";
        StartCoroutine(FadeInGameOverPanel());
    }

    IEnumerator FadeInGameOverPanel()
    {
        CanvasGroup cg = gameOverPanel.GetComponent<CanvasGroup>();
        cg.gameObject.SetActive(true);
        while (cg.alpha < 1)
        {
            cg.alpha += Time.deltaTime / 3;
            yield return null;
        }
        Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.None;
    }

    public void Restart()
    {

        Time.timeScale = 1;
        currentGameState = GameState.pause;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);

    }

    public void GoToMenu()
    {
        Time.timeScale = 1;
        currentGameState = GameState.menu;
        SceneManager.LoadScene(menuScene);
    }

    void Pause()
    {
        Cursor.lockState = CursorLockMode.None;
        currentGameState = GameState.pause;
        Time.timeScale = 0;
        pausePanel.SetActive(true);
    }
    public void Resume()
    {
        Cursor.lockState = CursorLockMode.Locked;
        currentGameState = GameState.inGame;
        Time.timeScale = 1;
        pausePanel.SetActive(false);
    }

}
