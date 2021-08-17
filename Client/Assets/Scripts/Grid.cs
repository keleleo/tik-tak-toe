using server;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public static Grid instance;
    [SerializeField] private GameObject squarePrefab;
    [SerializeField] private Square[] squares = new Square[9];
    // Start is called before the first frame update
    private void Awake()
    {
        Grid.instance = this;
    }
    void Start()
    {
        RenderGrid();

        Client.instance.ReceivedGameState += (json) =>
        {
            try
            {
                Grid.instance.UpdateSquares(Client.instance.gameState.gameState);
            }
            catch (Exception ex)
            {
                Debug.LogError(ex);
            }
        };
        //StartCoroutine(RequestGameState());
    }
    IEnumerator RequestGameState()
    {
        yield return new WaitForSeconds(1.5f);
        Client.instance.RequestGameStates();
    }
    private void RenderGrid()
    {
        for (int i = 0; i < 9; i++)
        {
            GameObject square = Instantiate(squarePrefab, this.transform);
            square.GetComponent<Square>().StartSquare(i, this);
            squares[i] = square.GetComponent<Square>();
        }

    }
    public void UpdateSquares(int[] state)
    {
        if (state.Length != 9)
        {

            return;
        }
        for (int i = 0; i < 9; i++)
        {
            Grid.instance.squares[i].SetValue(state[i]);
        }
    }
    public void SquareButton(int index)
    {
        Client.instance.GameAction(index);
    }
}
