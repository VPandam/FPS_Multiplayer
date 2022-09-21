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

    override public void OnEnable()
    {
        SceneManager.sceneLoaded += InstantiatePlayer;
    }
    override public void OnDisable()
    {
        SceneManager.sceneLoaded -= InstantiatePlayer;
    }
    void OnDestroy()
    {
        SceneManager.sceneLoaded -= InstantiatePlayer;
    }

    void InstantiatePlayer(Scene scene, LoadSceneMode loadSceneMode)
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
