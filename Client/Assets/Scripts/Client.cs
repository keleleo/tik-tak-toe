using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using server;
using System;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

public class Client : MonoBehaviour
{
    public static Client instance;
    private void Awake()
    {
        if (Client.instance == null)
        {
            Client.instance = this;
            DontDestroyOnLoad(Client.instance);
        }
        else
            Destroy(this);
    }

    [SerializeField] public GameState gameState = new GameState();
    public Message message2;
    public Login loginInfos;
    private bool isSearchingParty = false;
    [SerializeField] private string lastRequestJson;
    private int maxResquestRepeat = 10;
    private int requestRepeatCount = 0;
    [SerializeField] private bool serverConnected;
    public bool ServerConnected { get => serverConnected; }

    #region Events
    public event Action logged;
    public event Action PartyFound;
    public event Action PartyLosted;

    public event Action<string> ReceivedGameState;
    public event Action startGame;
    public event Action lose;
    public event Action won;
    public event Action noWinner;
    public event Action errorJson;
    #endregion

    private void Start()
    {
        try
        {
            NetworkSystem.instance.connected += Instance_connected;
            NetworkSystem.instance.disConnected += Instance_disConnected;
            NetworkSystem.instance.messageReceived += Instance_messageReceived;
            NetworkSystem.instance.connectionErro += Instance_connectionErro;
        }
        catch (Exception _ex)
        {
            Debug.Log(_ex);
        }
    }
    private void Instance_connected()
    {
        Client.instance.serverConnected = true;
    }
    private void Instance_disConnected()
    {
        Client.instance.serverConnected = false;
    }

    private void Instance_connectionErro()
    {
        throw new NotImplementedException();
    }

    private void Instance_messageReceived(string messageJson)
    {
        try
        {
            message2 = JsonUtility.FromJson<Message>(messageJson);
            Message message = JsonUtility.FromJson<Message>(messageJson);
            switch (message.type)
            {
                case Communication.Type.login:
                    loginInfos = JsonUtility.FromJson<Login>(message.messageObj);
                    logged?.Invoke();
                    break;

                case Communication.Type.gameFoundParty:
                    FoundParty();
                    break;

                case Communication.Type.gameState:
                    ReceivedGameStates(message.messageObj);
                    break;
                case Communication.Type.gameStartGame:
                    Debug.Log("client -> receive: start game");
                    StartGame();
                    break;
                case Communication.Type.gameYouWon:
                    Debug.Log("client -> receive: you won");
                    won?.Invoke();
                    break;
                case Communication.Type.gameYouLose:
                    Debug.Log("client -> receive: you lose");
                    lose?.Invoke();
                    break;
                case Communication.Type.gamePartyLost:
                    PartyLost();
                    break;
                case Communication.Type.gameNoWinner:
                    Debug.Log("client -> receive: no winner");
                    noWinner?.Invoke();
                    break;
                case Communication.Type.errorJson:
                    errorJson?.Invoke();
                    if (requestRepeatCount < maxResquestRepeat)
                    {
                        requestRepeatCount++;
                        RepeatRequest();
                    }
                    break;
            }
        }
        catch (Exception _ex)
        {
            Debug.Log(_ex);
        }
    }

    private void PartyLost()
    {
        try
        {
            GameManager.instance.Enqueue(GameManager.instance.LoadHomeScene);
        }
        catch (Exception ex)
        {

        }
    }
    public void PlayerReady()
    {
        Send(Communication.Type.gamePlayerReady);
    }
    public void PlayerReadyCancel()
    {
        Send(Communication.Type.gamePlayerReadyCancel);
    }

    private void StartGame()
    {
        startGame?.Invoke();
    }

    public void RequestGameStates()
    {
        Send(Communication.Type.gameState);
    }
    private void ReceivedGameStates(string json)
    {
        try
        {
            gameState = JsonUtility.FromJson<GameState>(json);
            if (Client.instance.ReceivedGameState != null)
                GameManager.instance.EnqueueS(
                    Client.instance.ReceivedGameState.Invoke,
                    json
                );
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }

    public void SearchParty()
    {
        if (!isSearchingParty)
            Send(Communication.Type.gameFindParty);
        isSearchingParty = true;
    }
    public void StopSerachParty()
    {
        if (isSearchingParty)
            Send(Communication.Type.gameStopFindParty);
        isSearchingParty = false;
    }

    public void GameAction(int square)
    {
        if (gameState.whoIsMe == gameState._c)
        {
            GameAction gameAction = new GameAction();
            gameAction.square = square;
            Send(Communication.Type.gameAction, gameAction);
        }
    }

    private void Send(Communication.Type type)
    {
        requestRepeatCount = 0;
        Message message = new Message();
        message.token = loginInfos.token;
        message.type = type;
        message.messageObj = "";

        string json = JsonUtility.ToJson(message);
        lastRequestJson = json;

        NetworkSystem.instance.Write(json);
    }
    private void Send<t>(Communication.Type type, t obj)
    {
        requestRepeatCount = 0;
        Message message = new Message();
        message.token = loginInfos.token;
        message.type = type;
        message.messageObj = JsonUtility.ToJson(obj);

        string json = JsonUtility.ToJson(message);
        lastRequestJson = json;

        NetworkSystem.instance.Write(json);
    }

    private void RepeatRequest()
    {
        NetworkSystem.instance.Write(lastRequestJson);
    }
    public void Login(string user, string pass)
    {

        Login _login = new Login();
        _login.user = user;

        Send(Communication.Type.login, _login);

    }
    private void FoundParty()
    {
        isSearchingParty = false;
        try
        {
            GameManager.instance.Enqueue(GameManager.instance.LoadGameScene);
        }
        catch (Exception ex)
        {
            Debug.LogError(ex);
        }
    }
}
