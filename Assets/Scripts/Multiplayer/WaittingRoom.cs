using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class WaittingRoom : MonoBehaviourPunCallbacks
{
    enum WaitCountdown
    {
        WaittingPlayers,
        WaittingStart
    }

    //Configurable properties
    [SerializeField] private string mainLevelScene = "Sandbox";
    [SerializeField] private double waitCountdown = 120.0f;
    [SerializeField] private double startCountdown = 30.0f;
    [SerializeField] private Button startGameButton;

    //UI
    [SerializeField] private Text timerText;
    [SerializeField] private Text bgText;
    
    //Players
    [SerializeField] private Text playersText;
    [SerializeField] private Text playersShadowText;

    //Room properties
    private Room _room;
    private const int MaxPlayersPerRoom = 4;

    //Control properties
    private bool _sceneIsLoading = false;
    private WaitCountdown _waitCountdown = WaitCountdown.WaittingPlayers;
    private double _time;

    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        _room = PhotonNetwork.CurrentRoom;
        //Start game
        startGameButton.onClick.AddListener(LoadMainLevel);

        if (!PhotonNetwork.IsMasterClient)
        {
            startGameButton.gameObject.SetActive(false);
        }

        if (PhotonNetwork.IsMasterClient)
        {
            _time = waitCountdown;
            Hashtable hash = new Hashtable() { { "Time", _time } };
            _room.SetCustomProperties(hash);
            _room.MaxPlayers = MaxPlayersPerRoom;
        }
        else
        {
            _time = (double)_room.CustomProperties["Time"];
        }
        
        UpdatePlayers();
    }

    private void Update()
    {
        if (PhotonNetwork.NetworkClientState == ClientState.Leaving) return;

        if (PhotonNetwork.IsMasterClient)
            UpdateTimer();

        else ReadTimer();

        SetUITimer();
    }

    //Update timer by deltaTime on master client
    void UpdateTimer()
    {
        //Update Room Time
        _time -= Time.deltaTime;

        UpdateRoomTimeProperty();

        if (!(_time <= 0f)) return;

        if (_waitCountdown.Equals(WaitCountdown.WaittingPlayers))
        {
            _time = startCountdown;
            _waitCountdown = WaitCountdown.WaittingStart;
            return;
        }

        if (!_sceneIsLoading)
            LoadMainLevel();
    }

    //Load main level method, called when timer is over
    void LoadMainLevel()
    {
        _sceneIsLoading = true;
        _room.IsOpen = false;
        PhotonNetwork.LoadLevel(mainLevelScene);
    }

    //Read timer for non-master clients
    void ReadTimer()
    {
        _time = (double)_room.CustomProperties["Time"];
    }

    //Update Timer on UI
    void SetUITimer()
    {
        timerText.text = ((int)_time).ToString();
        bgText.text = ((int)_time).ToString();
    }

    //Update timer for all clients
    void UpdateRoomTimeProperty()
    {
        Hashtable hash = _room.CustomProperties;
        hash.Remove("Time");
        hash.Add("Time", _time);
        _room.SetCustomProperties(hash);
    }

    //Called each time a new player enters room
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        base.OnPlayerEnteredRoom(newPlayer);
        UpdatePlayers();
        
        if (!PhotonNetwork.IsMasterClient) return;
        if (_room.PlayerCount < MaxPlayersPerRoom) return;

        //Close room
        _room.IsOpen = false;

        //Set new timer
        _time = startCountdown;
        _waitCountdown = WaitCountdown.WaittingStart;
        UpdateRoomTimeProperty();
    }

    //Called whenever a player leaves the room
    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log("Leaving");
        UpdatePlayers();
        
        //If is master client, set button On and open room
        if (!PhotonNetwork.IsMasterClient) return;

        startGameButton.gameObject.SetActive(true);
        _room.IsOpen = true;
    }

    public override void OnLeftRoom()
    {
        SceneManager.LoadScene("Menu");
        base.OnLeftRoom();
    }
    
    public override void OnJoinedRoom()
    {
        UpdatePlayers();
    }

    private void UpdatePlayers()
    {
        playersText.text = _room.PlayerCount + "/" + _room.MaxPlayers;
        playersShadowText.text = _room.PlayerCount + "/" + _room.MaxPlayers;
    }
}