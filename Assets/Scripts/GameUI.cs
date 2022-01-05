using System;
using TMPro;
using UnityEngine;

public enum CameraAngle
{
    menu = 0,
    white = 1,
    black = 2
}

public class GameUI : MonoBehaviour
{
    //singleton
    public static GameUI Instance { set; get; }
    
    public Server server;
    public Client client;
    [SerializeField] private Animator menuAnimator;
    [SerializeField] private TMP_InputField addressInput;
    [SerializeField] private GameObject[] cameraAngles;
    public Action<bool> SetLocalGame;
    private void Awake()
    {
        Instance = this;
        RegisterEvents();
    }

    // zmiana k¹tu kamery
    public void ChangeCamera(CameraAngle index) {
        for (int i = 0; i < cameraAngles.Length; i++)
        {
            cameraAngles[i].SetActive(false);
        }
        cameraAngles[(int)index].SetActive(true);
    }

    // przyciski
    public void OnLocalGameButton()
    {
        menuAnimator.SetTrigger("InGame");
        SetLocalGame?.Invoke(true);
        server.Init(8005);
        client.Init("127.0.0.1", 8005);
    }
    public void OnOnlineGameButton()
    {
        menuAnimator.SetTrigger("OnlineMenu");
    }
    public void OnOnlineHostButton()
    {
        SetLocalGame?.Invoke(false);
        server.Init(8005);
        client.Init("127.0.0.1", 8005);
        menuAnimator.SetTrigger("HostMenu");
    }
    public void OnOnlineConnectButton()
    {

        SetLocalGame?.Invoke(false);
        client.Init(addressInput.text, 8005);
    }
    public void OnOnlineBackButton()
    {

        menuAnimator.SetTrigger("StartMenu");
    }
    public void OnHostBackButton() {
        server.Shutdown();
        client.Shutdown();
        menuAnimator.SetTrigger("OnlineMenu");
    }
    public void OnLeaveFromGameMenu() {
        ChangeCamera(CameraAngle.menu);
        menuAnimator.SetTrigger("StartMenu");
        
    }
    // wydarzenia
    private void RegisterEvents()
    {

        NetUtility.C_START_GAME += OnStartGameClient;
    }
    private void UnRegisterEvents()
    {

        NetUtility.C_START_GAME -= OnStartGameClient;
    }
    private void OnStartGameClient(NetMessage obj)
    {
        menuAnimator.SetTrigger("InGame");
    }
}
