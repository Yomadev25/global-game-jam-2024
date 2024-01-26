using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class Menu : MonoBehaviourPunCallbacks
{
    [SerializeField]
    private GameObject _loadingPanel;

    [Header("Title HUD")]
    [SerializeField]
    private CanvasGroup _titleHud;
    [SerializeField]
    private Button _playButton;
    [SerializeField]
    private Button _exitButton;

    [Header("Name HUD")]
    [SerializeField]
    private CanvasGroup _nameHud;
    [SerializeField]
    private TMP_InputField _nameInput;
    [SerializeField]
    private Button _nameConfirmButton;
    [SerializeField]
    private Button _backToTitleButton;

    [Header("Lobby HUD")]
    [SerializeField]
    private CanvasGroup _lobbyHud;
    [SerializeField]
    private Button _createRoomButton;
    [SerializeField]
    private Button _backToNameButton;
    [SerializeField]
    private Transform _roomListRoot;
    [SerializeField]
    private GameObject _roomTemplate;

    [Header("Room HUD")]
    [SerializeField]
    private CanvasGroup _roomHud;
    [SerializeField]
    private TextMeshProUGUI _roomNameText;
    [SerializeField]
    private Button _leaveRoomButton;
    [SerializeField]
    private Button _startButton;
    [SerializeField]
    private Button _readyButton;
    [SerializeField]
    private Sprite _readySprite;
    [SerializeField]
    private Sprite _waitSprite;
    [SerializeField]
    private Transform _playerListRoot;
    [SerializeField]
    private GameObject _playerItemTemplate;
    [SerializeField]
    private GameObject _readyAlert;

    private RoomOptions _roomOption = new RoomOptions();
    private List<RoomInfo> _roomList = new List<RoomInfo>();
    private Hashtable playerProperties = new Hashtable();
    private float nextUpdateTime;

    private void Start()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        _exitButton.gameObject.SetActive(false);
#endif

        _playButton.onClick.AddListener(Play);
        _exitButton.onClick.AddListener(Exit);

        _nameConfirmButton.onClick.AddListener(NameConfirm);
        _backToTitleButton.onClick.AddListener(BackToTitle);

        _createRoomButton.onClick.AddListener(CreateRoom);
        _backToNameButton.onClick.AddListener(LeaveLobby);

        _startButton.onClick.AddListener(GameStart);
        _readyButton.onClick.AddListener(Ready);
        _leaveRoomButton.onClick.AddListener(LeaveRoom);

        _titleHud.gameObject.SetActive(true);
        _nameHud.gameObject.SetActive(false);
        _lobbyHud.gameObject.SetActive(false);
        _roomHud.gameObject.SetActive(false);
    }

    private void Play()
    {
        _titleHud.gameObject.SetActive(false);
        _nameHud.gameObject.SetActive(true);

        _loadingPanel.SetActive(true);
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        _loadingPanel.SetActive(false);
    }

    #region NAME HUD
    private void NameConfirm()
    {
        if (string.IsNullOrEmpty(_nameInput.text)) return;
        PhotonNetwork.NickName = _nameInput.text;

        _nameHud.gameObject.SetActive(false);
        _lobbyHud.gameObject.SetActive(true);

        PhotonNetwork.JoinLobby();
        _loadingPanel.SetActive(true);
    }

    private void BackToTitle()
    {
        PhotonNetwork.Disconnect();
        _nameHud.gameObject.SetActive(false);
        _titleHud.gameObject.SetActive(true);
    }
    #endregion

    #region LOBBY HUD
    public override void OnJoinedLobby()
    {
        base.OnJoinedLobby();
        Debug.LogWarning("Joined Lobby : " + PhotonNetwork.CurrentLobby.Name);

        PhotonNetwork.AutomaticallySyncScene = true;
        _loadingPanel.SetActive(false);
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        base.OnRoomListUpdate(roomList);
        Debug.LogFormat("There're {0} rooms.", roomList.Count);

        if (Time.time >= nextUpdateTime)
        {
            UpdateRoomList(roomList);
            nextUpdateTime = Time.time + 1.5f;
        }
    }

    private void UpdateRoomList(List<RoomInfo> roomList)
    {
        foreach (Transform item in _roomListRoot)
        {
            Destroy(item.gameObject);
        }
        _roomList.Clear();

        foreach (var room in roomList)
        {
            if (room.RemovedFromList) continue;

            GameObject GO = Instantiate(_roomTemplate, _roomListRoot);

            var roomCodeText = GO.transform.Find("(text) room name").GetComponent<TMP_Text>();
            if (roomCodeText != null)
            {
                roomCodeText.text = room.Name;
            }

            var playerCount = GO.transform.Find("(text) count").GetComponent<TMP_Text>();
            if (playerCount != null)
            {
                playerCount.text = $"{room.PlayerCount} / {room.MaxPlayers}";
            }

            var joinButton = GO.GetComponent<Button>();
            if (joinButton != null)
            {
                joinButton.onClick.AddListener(() =>
                {
                    if (room.PlayerCount == room.MaxPlayers) return;
                    JoinRoom(room.Name);
                });

                joinButton.interactable = room.PlayerCount != room.MaxPlayers;
            }

            _roomList.Add(room);
            GO.SetActive(true);
        }
    }

    private void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
        _loadingPanel.SetActive(true);
    }

    private void CreateRoom()
    {
        _roomOption.MaxPlayers = 4;
        _roomOption.IsVisible = true;
        string roomName = PhotonNetwork.NickName;

        PhotonNetwork.CreateRoom(roomName, _roomOption);
        _loadingPanel.SetActive(true);
    }

    private void LeaveLobby()
    {
        if (PhotonNetwork.InLobby)
        {
            PhotonNetwork.LeaveLobby();
        }

        _lobbyHud.gameObject.SetActive(false);
        _nameHud.gameObject.SetActive(true);
    }

    public override void OnLeftLobby()
    {
        base.OnLeftLobby();
        Debug.LogWarning("Left Lobby");
    }
    #endregion

    #region ROOM HUD
    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();
        Debug.Log("Joined Room");

        playerProperties["ready"] = false;
        _readyButton.image.sprite = _readySprite;
        PhotonNetwork.SetPlayerCustomProperties(playerProperties);

        InitRoomProperty();
        _loadingPanel.SetActive(false);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        base.OnJoinRandomFailed(returnCode, message);
        Debug.LogErrorFormat("OnJoinRandomFailed {0} : {1}", returnCode, message);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);
        Debug.LogErrorFormat("OnJoinRoomFailed {0} : {1}", returnCode, message);
    }

    public override void OnLeftRoom()
    {
        base.OnLeftRoom();
        Debug.LogWarningFormat("Left room.");

        playerProperties["ready"] = false;
        _readyButton.image.sprite = _readySprite;
        PhotonNetwork.SetPlayerCustomProperties(playerProperties);

        _lobbyHud.gameObject.SetActive(true);
        _roomHud.gameObject.SetActive(false);

        _loadingPanel.SetActive(false);
    }

    private void InitRoomProperty()
    {
        _roomHud.gameObject.SetActive(true);
        _lobbyHud.gameObject.SetActive(false);

        _roomNameText.text = PhotonNetwork.CurrentRoom.Name + "'s room";

        UpdatePlayerList();
    }

    private void UpdatePlayerList()
    {
        foreach (Transform item in _playerListRoot)
        {
            Destroy(item.gameObject);
        }

        foreach (var player in PhotonNetwork.PlayerList)
        {
            GameObject GO = Instantiate(_playerItemTemplate, _playerListRoot);

            var nameText = GO.transform.Find("(text) Name").gameObject.GetComponent<TMP_Text>();
            if (nameText != null)
            {
                nameText.text = player.NickName;
            }

            var hostIcon = GO.transform.Find("(image) host").gameObject;
            if (hostIcon != null)
            {
                hostIcon.SetActive(player.IsMasterClient);
            }

            var readyIcon = GO.transform.Find("(image) Ready").gameObject;
            {
                if (player.CustomProperties.ContainsKey("ready"))
                {
                    readyIcon.SetActive((bool)player.CustomProperties["ready"]);
                }
            }

            GO.SetActive(true);
        }

        HostPlayButtonUpdate();
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        base.OnPlayerPropertiesUpdate(targetPlayer, changedProps);
        UpdatePlayerList();
        if (PhotonNetwork.PlayerList.Count(x => (bool)x.CustomProperties["ready"]) >= PhotonNetwork.PlayerList.Length - 1)
        {
            _startButton.interactable = true;
        }
        else
        {
            _startButton.interactable = false;
        }
    }

    private void HostPlayButtonUpdate()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            playerProperties["ready"] = false;
            PhotonNetwork.SetPlayerCustomProperties(playerProperties);

            _startButton.gameObject.SetActive(true);
            _readyButton.gameObject.SetActive(false);
        }
        else
        {
            _startButton.gameObject.SetActive(false);
            _readyButton.gameObject.SetActive(true);
        }
    }

    private void Ready()
    {
        if (PhotonNetwork.IsMasterClient) return;

        if (!(bool)playerProperties["ready"])
        {
            playerProperties["ready"] = true;
            _readyButton.image.sprite = _waitSprite;
        }
        else
        {
            playerProperties["ready"] = false;
            _readyButton.image.sprite = _readySprite;
        }
        PhotonNetwork.SetPlayerCustomProperties(playerProperties);
    }

    private void GameStart()
    {
#if !UNITY_EDITOR
            if (PhotonNetwork.PlayerList.Length <= 1) return;
#endif

        if (PhotonNetwork.PlayerList.Count(x => (bool)x.CustomProperties["ready"]) >= PhotonNetwork.PlayerList.Length - 1)
        {
            PhotonNetwork.CurrentRoom.IsVisible = false;
            PhotonNetwork.CurrentRoom.IsOpen = false;

            PhotonNetwork.LoadLevel("Game");
        }
        else
        {
            StopCoroutine(ReadyAlert());
            StartCoroutine(ReadyAlert());
        }
    }

    private IEnumerator ReadyAlert()
    {
        _readyAlert.SetActive(true);
        yield return new WaitForSeconds(3f);
        _readyAlert.SetActive(false);
    }

    private void LeaveRoom()
    {
        PhotonNetwork.AutomaticallySyncScene = false;
        PhotonNetwork.LeaveRoom();

        _loadingPanel.SetActive(true);
    }
    #endregion

    private void Exit()
    {
        Application.Quit();
    }
}
