using System;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

public class Client : MonoBehaviour
{
    #region Singleton
    // Singleton implementation (doesn't provide protetcion against creating a newsingleton, but whatever)
    public static Client Instance { set; get; }

    private void Awake()
    {
        Instance = this;
    }
    #endregion

    public NetworkDriver driver;
    private NetworkConnection connection;

    private bool isActive = false;
    public Action connectionDroped;

    // W przeciwienstwie do serwera jest IP
    public void Init(string ip, ushort port)
    {
        driver = NetworkDriver.Create();
        NetworkEndPoint endpoint = NetworkEndPoint.Parse(ip, port);

        connection = driver.Connect(endpoint);
        Debug.Log("Proba polaczenia z serwerem na adresie: " + endpoint.Address);
        isActive = true;

        // Rejestrowanie do wydarzen
        RegisterToEvent();
    }
    public void Shutdown()
    {
        if (isActive)
        {
            UnRegisterToEvent();
            driver.Dispose();
            isActive = false;
            connection = default(NetworkConnection);
        }
    }
    public void OnDestroy()
    {
        Shutdown();
    }
    public void Update()
    {
        if (!isActive)
        {
            return;
        }

        driver.ScheduleUpdate().Complete();
        // Sprawdzenie polaczenia
        CheckAlive();
        // Obs³uga wiadomosci
        UpdateMessagePump();
    }
    private void CheckAlive()
    {
        if(!connection.IsCreated && isActive)
        {
            Debug.Log("Straco po³¹czenie z serwerem");
            connectionDroped?.Invoke();
            Shutdown();
        }
    }
    private void UpdateMessagePump()
    {
        DataStreamReader stream;
        NetworkEvent.Type cmd;
        while ((cmd = connection.PopEvent(driver, out stream)) != NetworkEvent.Type.Empty)
        { 
            if (cmd == NetworkEvent.Type.Connect)
            {
                // Przypisanie druzyny itd
                SendToServer(new NetWelcome());
            }
            else if (cmd == NetworkEvent.Type.Data)
            {
                // Parsowanie
                NetUtility.OnData(stream, default(NetworkConnection));
            }
            else if (cmd == NetworkEvent.Type.Disconnect)
            {
                connection = default(NetworkConnection);
                connectionDroped?.Invoke();
                Shutdown();
            }
        }
        
    }
    public void SendToServer(NetMessage msg)
    {
        DataStreamWriter writer;
        driver.BeginSend(connection, out writer);
        msg.Serialize(ref writer);
        driver.EndSend(writer);
    }

    // Parsowanie wydarzeñ
    private void RegisterToEvent()
    {
        // Subskrypcja do onkeepalive eventu i pod³¹czenie metody OnKeepAlive
        NetUtility.C_KEEP_ALIVE += OnKeepAlive;
    }
    private void UnRegisterToEvent()
    {
        NetUtility.C_KEEP_ALIVE -= OnKeepAlive;
    }
    private void OnKeepAlive(NetMessage msg)
    {
        // Odes³anie wiadomosci do serwera
        SendToServer(msg);
    }
}
