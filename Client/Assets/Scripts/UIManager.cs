using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class UIManager : MonoBehaviour
{
    [SerializeField]
    public static UIManager instance;

    [Header("Menu Objects")]
    [SerializeField] private GameObject loadingMenu;
    [SerializeField] private GameObject homeMenu;
    [SerializeField] private GameObject registerMenu;
    [SerializeField] private GameObject loginMenu;
    [Header("Login")]
    [SerializeField] private InputField login;
    [SerializeField] private InputField passwordlogin;
    public Menu _menu;
    public enum Menu
    {
        Loading = 1,
        Home,
        Login,
        Register
    }

    private void Awake()
    {
        instance = this;
        _menu = Menu.Loading;
    }
    private void Start()
    {
        server.NetworkSystem.instance.connected += () =>
        {
            UIManager.instance.ChangeMenu((int)Menu.Login);
        };
        Client.instance.logged += () =>
        {
            UIManager.instance.ChangeMenu((int)Menu.Home);
        };
        //StartCoroutine(LoginLooping());
        if (Client.instance.ServerConnected)
        {
            UIManager.instance.ChangeMenu((int)Menu.Home);
        }

    }
    IEnumerator LoginLooping()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.5f);
            Login();
        }
    }
    void Update()
    {
        UpdateMenu();
    }

    public void Login()
    {
        string login = this.login.text;
        string pass = this.passwordlogin.text;
        if (login.Length > 4 &&
            pass.Length > 4)
        {
            Client.instance.Login(login, pass);

        }
    }
    public void SearchParty()
    {
        Client.instance.SearchParty();
    }
    public void StopSearchParty()
    {
        Client.instance.StopSerachParty();
    }
    private void UpdateMenu()
    {
        switch (_menu)
        {
            case Menu.Loading:
                loadingMenu.SetActive(true);
                homeMenu.SetActive(false);
                registerMenu.SetActive(false);
                loginMenu.SetActive(false);
                break;
            case Menu.Home:
                loadingMenu.SetActive(false);
                homeMenu.SetActive(true);
                registerMenu.SetActive(false);
                loginMenu.SetActive(false);
                break;
            case Menu.Register:
                loadingMenu.SetActive(false);
                homeMenu.SetActive(false);
                registerMenu.SetActive(true);
                loginMenu.SetActive(false);
                break;
            case Menu.Login:
                loadingMenu.SetActive(false);
                homeMenu.SetActive(false);
                registerMenu.SetActive(false);
                loginMenu.SetActive(true);
                break;
        }
    }
    public void ChangeMenu(int menu)
    {
        _menu = (Menu)menu;
    }

}
