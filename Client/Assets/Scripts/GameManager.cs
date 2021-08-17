using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    private static readonly Queue<Action> _executionQueue = new Queue<Action>();
    private static readonly Queue<Action<string>> _executionQueueS = new Queue<Action<string>>();
    private static readonly Queue<string> _executionQueueSValue = new Queue<string>();

    private static readonly Queue<Action<bool>> _executionQueueBool = new Queue<Action<bool>>();
    private static readonly Queue<bool> _executionQueueBoolValue = new Queue<bool>();
    private void Awake()
    {
        if (GameManager.instance == null)
        {
            GameManager.instance = this;
            DontDestroyOnLoad(instance);
        }
        else if (GameManager.instance != this)
            Destroy(this);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
            LoadGameScene();
        lock (_executionQueue)
        {
            while (_executionQueue.Count > 0)
            {
                _executionQueue.Dequeue().Invoke();
            }
        }

        lock (_executionQueueS)
        {
            while (_executionQueueS.Count > 0)
            {
                _executionQueueS.Dequeue().Invoke(_executionQueueSValue.Dequeue());
            }
        }
        lock (_executionQueueBool)
        {
            while(_executionQueueBool.Count > 0)
            {
                _executionQueueBool.Dequeue().Invoke(_executionQueueBoolValue.Dequeue());
            }
        }
    }
    public void LoadGameScene()
    {
        SceneManager.LoadScene("Game");

    }
    public void LoadHomeScene()
    {
        SceneManager.LoadScene("Home");

    }
    public void EnqueueBool(Action<bool> a, bool v)
    {
        _executionQueueBool.Enqueue((x) =>
        {
            a(x);
        });
        _executionQueueBoolValue.Enqueue(v);
    }
    public void EnqueueS(Action<string> a, string v)
    {
        _executionQueueS.Enqueue((x) =>
        {
            a(x);
        });
        _executionQueueSValue.Enqueue(v);
    }
    public void Enqueue(IEnumerator action)
    {
        lock (_executionQueue)
        {
            _executionQueue.Enqueue(() =>
            {
                StartCoroutine(action);
            });
        }
    }
    public void Enqueue(Action action)
    {
        Enqueue(ActionWrapper(action));
    }
    IEnumerator ActionWrapper(Action a)
    {
        a();
        yield return null;

    }

}
