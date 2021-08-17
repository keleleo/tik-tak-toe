using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Log : MonoBehaviour
{
    public static Log instance;
    [SerializeField] private Text debugText;
    static Text static_DebugText;
    private void Awake()
    {
        instance = this;
        static_DebugText = debugText;
    }
    public static void Debug(string debug)
    {
        static_DebugText.text = debug+"\n"+ static_DebugText.text;
    }
}
