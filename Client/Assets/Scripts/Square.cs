using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Square : MonoBehaviour
{
    [SerializeField] private Text text;
    private int index;
    private Grid grid;

    public void StartSquare(int index, Grid grid)
    {
        text.text = "E";
        this.index = index;
        this.grid = grid;
        GetComponent<Button>().onClick.AddListener(() =>
        {
            grid.SquareButton(index);
        });
    }
    public void SetValue(int value)
    {
        switch (value)
        {
            case 0:
                text.text = "";
                break;
            case 1:
                text.text = "X";
                break;
            case 2:
                text.text = "O";
                break;
        }
    }
}
