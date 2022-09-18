using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class NetworkingManager : MonoBehaviourPunCallbacks
{
    [SerializeField] MenuManager menuManager;
    [SerializeField]
    GameObject lobbyPanel, mainPanel, usernamePanel,
    usernameInput, playerFoundUI, playerFoundHolder, idRoomText;
    string userName;

    public void ConnectToTheServer()
    {
        Debug.Log("Connecting to the server");
        PhotonNetwork.ConnectUsingSettings();

    }
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
        Debug.Log("Connecting to the lobby");
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Joined lobby");
        PhotonNetwork.JoinRandomRoom();
    }
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        CreateRoom();
    }
    void CreateRoom()
    {
        int roomID = Random.Range(0, 5000);
        RoomOptions roomOptions = new RoomOptions()
        {
            IsVisible = true,
            IsOpen = true,
            PublishUserId = true,
            MaxPlayers = 6
        };

        if (PhotonNetwork.CreateRoom($"Room_{roomID}", roomOptions))
            Debug.Log($"Room created with id: {roomID}");


    }
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log($"Room creation failed return code: {returnCode}");
    }
    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        Debug.Log("Loading game");
        OpenLobbyScreen();

    }
    public void OpenLobbyScreen()
    {

        usernamePanel.SetActive(false);

        lobbyPanel.SetActive(true);
        idRoomText.GetComponent<TextMeshProUGUI>().text = PhotonNetwork.CurrentRoom.Name;

        Player[] players = PhotonNetwork.PlayerList;

        foreach (Transform child in playerFoundHolder.transform)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < players.Length; i++)
        {
            Instantiate(playerFoundUI, playerFoundHolder.transform).
            GetComponent<PlayerFoundUI>().SetUserName(players[i].NickName);
            Debug.Log("Nickname: " + players[i].NickName);
        }


    }
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (Transform trans in playerFoundHolder.transform)
        {
            Destroy(trans);
        }
        foreach (var player in roomList)
        {
            Instantiate(playerFoundUI, playerFoundHolder.transform)
            .GetComponent<PlayerFoundUI>().SetUserName(player.Name);
        }
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Instantiate(playerFoundUI, playerFoundHolder.transform)
        .GetComponent<PlayerFoundUI>().SetUserName(newPlayer.NickName);
        Debug.Log(newPlayer.NickName);

    }

    public void StartOnlineGame()
    {
        PhotonNetwork.LoadLevel("Online");

    }
    public void InputUserName()
    {
        mainPanel.SetActive(false);
        usernamePanel.SetActive(true);
    }
    public void SubmitUserName()
    {
        TMP_InputField usernameInputField = usernameInput.GetComponent<TMP_InputField>();
        usernameInputField.characterLimit = 10;
        PhotonNetwork.NickName = usernameInputField.text;

        ConnectToTheServer();
    }

}
