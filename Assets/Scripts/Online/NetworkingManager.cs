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

    bool onUserNameScreen;

    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.Return) && onUserNameScreen)
        {
            SubmitUserName();
        }
    }
    public void ExitGame()
    {
        Application.Quit();
    }
    public void StartOnlineGame()
    {
        PhotonNetwork.LoadLevel("Main");
    }
    public void MainToUserName()
    {
        mainPanel.SetActive(false);
        usernamePanel.SetActive(true);
        onUserNameScreen = true;
    }
    public void LobbyToMain()
    {
        mainPanel.SetActive(true);
        lobbyPanel.SetActive(false);
        PhotonNetwork.Disconnect();
        onUserNameScreen = false;
    }
    public void UserNameToMain()
    {
        mainPanel.SetActive(true);
        usernamePanel.SetActive(false);
        onUserNameScreen = false;
    }
    public void SubmitUserName()
    {
        TMP_InputField usernameInputField = usernameInput.GetComponent<TMP_InputField>();
        usernameInputField.characterLimit = 10;
        PhotonNetwork.NickName = usernameInputField.text;

        ConnectToTheServer();
    }


    public void ConnectToTheServer()
    {
        Debug.Log("Connecting to the server");
        PhotonNetwork.ConnectUsingSettings();

    }
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
        Debug.Log("Connecting to the lobby");
        PhotonNetwork.AutomaticallySyncScene = true;
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
        onUserNameScreen = false;

        idRoomText.GetComponent<TextMeshProUGUI>().text = PhotonNetwork.CurrentRoom.Name;
        UpdatePlayersListUI();

    }
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        UpdatePlayersListUI();
    }
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdatePlayersListUI();
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Instantiate(playerFoundUI, playerFoundHolder.transform)
        .GetComponent<PlayerFoundUI>().SetUserName(newPlayer.NickName);

    }

    void UpdatePlayersListUI()
    {
        Player[] playerList = PhotonNetwork.PlayerList;

        foreach (Transform playerFound in playerFoundHolder.transform)
        {
            Destroy(playerFound.gameObject);
        }

        for (int i = 0; i < playerList.Length; i++)
        {
            Instantiate(playerFoundUI, playerFoundHolder.transform).
            GetComponent<PlayerFoundUI>().SetUserName(playerList[i].NickName);
        }
    }
}
