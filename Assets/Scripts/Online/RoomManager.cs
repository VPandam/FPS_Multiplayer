using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class RoomManager : MonoBehaviourPunCallbacks
{

    public static RoomManager roomManager;
    SceneManager sceneManager;

    private void Awake()
    {
        if (roomManager == null)
        {
            roomManager = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        Vector3 playerSpawnPosition = new Vector3(Random.Range(-3, 3), 2, Random.Range(-3, 3));

        if (PhotonNetwork.InRoom)
        {
            PhotonNetwork.Instantiate("Player", playerSpawnPosition, Quaternion.identity);
        }
        else
        {
            Instantiate(Resources.Load("Player"), playerSpawnPosition, Quaternion.identity);
        }
    }
}
