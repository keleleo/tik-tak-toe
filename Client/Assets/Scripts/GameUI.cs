using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameUI : MonoBehaviour
{
    public static GameUI instance;
    private void Awake()
    {
        instance = this;
    }
    [SerializeField] private GameObject waitingAction;
    [SerializeField] private GameObject readyButton;
    [SerializeField] private GameObject readyButtonCancel;
    [SerializeField] private GameObject waiting;
    [SerializeField] private GameObject lose;
    [SerializeField] private GameObject won;
    [SerializeField] private GameObject noWinner;
    // Start is called before the first frame update
    void Start()
    {
        try
        {
            Client.instance.startGame += Instance_startGame;
            Client.instance.lose += Instance_lose;
            Client.instance.won += Instance_won;
            Client.instance.noWinner += Instance_noWinner;
            Client.instance.ReceivedGameState += Instance_ReceivedGameState;
        }
        catch (Exception _ex)
        {
            Debug.Log(_ex);
        }
    }

    private void Instance_ReceivedGameState(string obj)
    {
        server.GameState gameState = JsonUtility.FromJson<server.GameState>(obj);
        if(gameState.whoIsMe == gameState._c)
        {
            GameManager.instance.EnqueueBool(GameUI.instance.waitingAction.SetActive,false);
        }
        else
        {
            GameManager.instance.EnqueueBool(GameUI.instance.waitingAction.SetActive, true);
        }
    }

    private void Instance_noWinner()
    {
        try
        {
            GameManager.instance.EnqueueBool(GameUI.instance.lose.SetActive, false);
            GameManager.instance.EnqueueBool(GameUI.instance.won.SetActive, false);
            GameManager.instance.EnqueueBool(GameUI.instance.noWinner.SetActive, true);
            GameManager.instance.EnqueueBool(GameUI.instance.readyButton.SetActive, true);
            GameManager.instance.EnqueueBool(GameUI.instance.readyButtonCancel.SetActive, false);
        }
        catch (Exception _ex)
        {
            Debug.Log(_ex);
        }
    }
    
    private void Instance_won()
    {
        try
        {
            Debug.Log("you win 1");
            GameManager.instance.EnqueueBool(GameUI.instance.lose.SetActive, false);
            GameManager.instance.EnqueueBool(GameUI.instance.won.SetActive, true);
            GameManager.instance.EnqueueBool(GameUI.instance.noWinner.SetActive, false);
            GameManager.instance.EnqueueBool(GameUI.instance.readyButton.SetActive, true);
            GameManager.instance.EnqueueBool(GameUI.instance.readyButtonCancel.SetActive, false);
            Debug.Log("you win 2");
        }
        catch (Exception _ex)
        {
            Debug.Log(_ex);
        }

    }

    private void Instance_lose()
    {
        try
        {
            Debug.Log("you lose 1");
            GameManager.instance.EnqueueBool(GameUI.instance.lose.SetActive, true);
            GameManager.instance.EnqueueBool(GameUI.instance.won.SetActive, false);
            GameManager.instance.EnqueueBool(GameUI.instance.noWinner.SetActive, false);
            GameManager.instance.EnqueueBool(GameUI.instance.readyButton.SetActive, true);
            GameManager.instance.EnqueueBool(GameUI.instance.readyButtonCancel.SetActive, false);
            Debug.Log("you lose 2");
        }
        catch (Exception _ex)
        {
            Debug.Log(_ex);
        }
    }

    private void Instance_startGame()
    {
        try
        {
            Debug.Log("start 1");
            GameManager.instance.EnqueueBool(GameUI.instance.waiting.SetActive, false);
            GameManager.instance.EnqueueBool(GameUI.instance.lose.SetActive, false);
            GameManager.instance.EnqueueBool(GameUI.instance.won.SetActive, false);
            GameManager.instance.EnqueueBool(GameUI.instance.noWinner.SetActive, false);
            GameManager.instance.EnqueueBool(GameUI.instance.readyButton.SetActive, false);
            GameManager.instance.EnqueueBool(GameUI.instance.readyButtonCancel.SetActive, false);
            Debug.Log("start 2");
        }
        catch (Exception ex)
        {
            Debug.Log(ex);
        }
    }
    public void ReadyButton()
    {
        Client.instance.PlayerReady();
    }
    public void CancelButton()
    {
        Client.instance.PlayerReadyCancel();

    }
}
