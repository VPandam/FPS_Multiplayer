using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public enum GameState
{
    inGame, pause, gameOver, menu, shop
}
public class GameManager : MonoBehaviourPunCallbacks
{
    [SerializeField] int maxFrames = 90;
    [SerializeField] GameObject[] spawners;
    int currentRound;

    [SerializeField] TextMeshProUGUI roundText;
    [SerializeField] TextMeshProUGUI roundsSurvivedText;

    string stringRoundText;

    [SerializeField] GameObject zombiePrefab;

    [SerializeField] GameObject gameOverPanel;

    //Depending on whitch platform we are on, we load one or other scene.
    [SerializeField] string menuScene, mainScene;
    [SerializeField] GameObject pausePanel;

    //Black panel used for fade in when the game starts
    [SerializeField] GameObject fadeInGamePanel;
    GameState currentLocalGameState;
    public GameState CurrentLocalGameState { get => currentLocalGameState; }
    public GameState currentOnlineGameState;
    [HideInInspector] public VendingMachine vendingMachine;

    [SerializeField] PhotonView _photonView;
    bool isOnlineMasterAndMine;
    // Start is called before the first frame update
    void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = maxFrames;
        spawners = GameObject.FindGameObjectsWithTag("Spawner");
        StartGame();
    }
    void StartGame()
    {
        currentLocalGameState = GameState.inGame;
        StartCoroutine(FadeInOrOutPanel(fadeInGamePanel, 3, false, false));
        isOnlineMasterAndMine = PhotonNetwork.InRoom && _photonView.IsMine && PhotonNetwork.IsMasterClient;
        if (!PhotonNetwork.InRoom || isOnlineMasterAndMine)
        {
            if (!PhotonNetwork.InRoom)
                PhotonNetwork.OfflineMode = true;
            StartCoroutine(StartNextRound());
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (PhotonNetwork.InRoom && !_photonView.IsMine)
        {
            return;
        }
        if (Input.GetButtonDown("Pause"))
        {
            switch (currentLocalGameState)
            {
                case GameState.pause:
                    Resume(); break;
                case GameState.inGame:
                    Pause(); break;
                case GameState.shop:
                    vendingMachine.ExitShop(); break;
            }

        }
    }

    void SetRound(int round)
    {
        currentRound = round;
        roundText.text = $"Round: {currentRound}";
        Debug.Log("SetRound");
    }

    public IEnumerator StartNextRound()
    {
        //TODO: Animation and sound changing roung
        Debug.Log("StartNextRound");
        if (PhotonNetwork.InRoom)
        {
            Hashtable localPlayerOptions = new Hashtable();
            localPlayerOptions.Add("currentRound", currentRound + 1);
            PhotonNetwork.LocalPlayer.SetCustomProperties(localPlayerOptions);
        }
        else
            SetRound(currentRound + 1);
        yield return new WaitForSeconds(2);

        for (int i = 0; i < currentRound; i++)
        {
            Debug.Log("i = " + i);
            int randomSpawnIndex = Random.Range(0, spawners.Length);
            Debug.Log(randomSpawnIndex + " RandomSpawnIndex " + spawners.Length);

            //Photon network instantiation or normal one
            if (PhotonNetwork.InRoom)
                InstantiateZombie(true, randomSpawnIndex);
            else
                InstantiateZombie(false, randomSpawnIndex);
        }
    }

    public void InstantiateZombie(bool isOnline, int spawnIndex)
    {
        if (isOnline)
        {
            //Instantiate a zombie in photon network
            //Add this game manager to the zombie
            GameObject onlineEnemy = PhotonNetwork.Instantiate("Zombie", spawners[spawnIndex].transform.position,
            Quaternion.identity);
            if (onlineEnemy != null)
                onlineEnemy.GetComponent<ZombieManager>().gameManager = this;
        }
        else
        {
            //Normal instantiation of the zombie if we are not online
            GameObject enemy = Instantiate(Resources.Load("Zombie"), spawners[spawnIndex].transform.position,
            Quaternion.identity) as GameObject;
            if (enemy != null)
                enemy.GetComponent<ZombieManager>().gameManager = this;
        }
    }

    //TODO: Change round animation
    public void LookForEnemies()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        bool areEnemiesAlive = false;
        foreach (GameObject enemy in enemies)
        {
            ZombieManager zombieManager = enemy.GetComponent<ZombieManager>();
            if (zombieManager != null && zombieManager.isAlive)
                areEnemiesAlive = true;
        }

        if (!areEnemiesAlive)
        {
            Debug.Log("No more enemies alive " + isOnlineMasterAndMine + "inRoom: " + PhotonNetwork.InRoom
            + " isMaster: " + PhotonNetwork.IsMasterClient + " isMine " + photonView.IsMine);
            if (!PhotonNetwork.InRoom || (isOnlineMasterAndMine))
                StartCoroutine(StartNextRound());
        }

    }

    public void GameOver()
    {
        if (!PhotonNetwork.InRoom)
        {
            currentLocalGameState = GameState.gameOver;
            StartCoroutine(FadeInOrOutPanel(gameOverPanel, 2, true, true));

        }
        StartCoroutine(FadeInOrOutPanel(gameOverPanel, 2, true, true));
        roundsSurvivedText.text = $"ROUNDS SURVIVED: {currentRound}";
    }

    IEnumerator FadeInOrOutPanel(GameObject panel, float time, bool fadeIn, bool stopTime)
    {
        Debug.Log("FadeInOrOut");
        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
        cg.gameObject.SetActive(true);

        if (fadeIn)
        {
            while (cg.alpha < 1)
            {
                cg.alpha += Time.deltaTime / time;
                yield return null;
            }
        }
        if (!fadeIn)
        {
            while (cg.alpha > 0)
            {
                cg.alpha -= Time.deltaTime / time;
                yield return null;
            }
        }

        if (stopTime)
        {
            if (!PhotonNetwork.InRoom)
                Time.timeScale = 0;
            Cursor.lockState = CursorLockMode.None;
        }
    }

    public void Restart()
    {

        Time.timeScale = 1;
        currentLocalGameState = GameState.pause;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void GoToMenu()
    {
        if (!PhotonNetwork.InRoom)
        {
            Time.timeScale = 1;
        }
        PhotonNetwork.Disconnect();
        currentLocalGameState = GameState.menu;
        SceneManager.LoadScene(menuScene);
    }

    void Pause()
    {
        Cursor.lockState = CursorLockMode.None;
        currentLocalGameState = GameState.pause;
        if (!PhotonNetwork.InRoom)
        {
            Time.timeScale = 0;
        }
        pausePanel.SetActive(true);
    }
    public void Shop()
    {
        Cursor.lockState = CursorLockMode.None;
        currentLocalGameState = GameState.shop;
        if (!PhotonNetwork.InRoom)
        {
            Time.timeScale = 0;
        }
    }
    public void Resume()
    {
        Cursor.lockState = CursorLockMode.Locked;
        currentLocalGameState = GameState.inGame;
        Time.timeScale = 1;
        pausePanel.SetActive(false);
    }
    public void QuitGame()
    {
        Application.Quit();
    }

    [PunRPC]
    void DestroyPlayerGO()
    {
        Destroy(this.gameObject);
    }
    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        if (photonView.IsMine && changedProps["currentRound"] != null)
            SetRound((int)changedProps["currentRound"]);
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (otherPlayer.IsLocal)
            photonView.RPC("DestroyPlayerGO", RpcTarget.All);
    }


    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        if (PhotonNetwork.IsMasterClient)
        {
            LookForEnemies();
        }
    }
}


