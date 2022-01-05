using System;
using Unity.Collections;
using Unity.Networking.Transport;
using UnityEngine;

public class Server : MonoBehaviour
{
    // singleton
    public static Server Instance { set; get;  }

    private void Awake()
    {
        Instance = this;
    }

    public NetworkDriver driver;
    // Po��czenia do serwera
    private NativeList<NetworkConnection> connections;

    private bool isActive = false;
    // Interwa� wysy�ania keepAliveMessage, musi byc bo inaczej timeout i connection si� zerwie
    private const float keepAliveTickRate = 20.0f;
    private float lastKeepAlive;

    // jak klient si� roz��czy
    public Action connectionDroped;


    public void Init(ushort port)
    {
        // Inicjalizacja drivera
        driver = NetworkDriver.Create();
        // tworzenie endpointu do kt�rego klienci si� pod��czaj�, mo�e to byc dowolny Ipv4
        NetworkEndPoint endpoint = NetworkEndPoint.AnyIpv4;
        endpoint.Port = port;

        // Nas�uchiwanie na porcie, jesli zero to sukces
        if(driver.Bind(endpoint) != 0)
        {
            // Np je�li port zabrany np 80
            Debug.Log("Port unable to be bound" + endpoint.Port);
            return;
        } 
        else
        {
            driver.Listen();
            Debug.Log("Currently listening on port" + endpoint.Port);
        }
        // lista po��cze�, max 2
        connections = new NativeList<NetworkConnection>(2, Allocator.Persistent);
        isActive = true;
    }
    public void Shutdown() {
        if (isActive)
        {
            driver.Dispose();
            connections.Dispose();
            isActive = false;
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

        // Wysylanie keepAlive co 20s
        KeepAlive();

        // Wyczyszczenie kolejki wiadomosci przychodzacych
        driver.ScheduleUpdate().Complete();
        // Jesli jest ktos podlaczony ale nei aktywny to usuwamy referencje
        CleanupConnections();
        AcceptNewConnections();
        // Obs�uga wiadomo�ci
        UpdateMessagePump();
    }
    private void KeepAlive()
    {
        if(Time.time - lastKeepAlive > keepAliveTickRate)
        {
            lastKeepAlive = Time.time;
            Broadcast(new NetKeepAlive());
        }
    }
    private void CleanupConnections()
    {
        for (int i = 0; i < connections.Length; i++)
        {
            // Jak jest nie created to �e jest nie active
            if (!connections[i].IsCreated)
            {
                connections.RemoveAtSwapBack(i);
                // Zeby nie zepsuc petli
                --i;
            }
        }
    }
    private void AcceptNewConnections() {
        NetworkConnection c;
        while((c = driver.Accept()) != default(NetworkConnection))
        {
            connections.Add(c);
        }
    }
    private void UpdateMessagePump()
    {
        // do czytania wiadomosci
        DataStreamReader stream;
        for (int i = 0; i < connections.Length; i++)
        {
            NetworkEvent.Type cmd;
            // Jesli typ wiadomosci to data lub disccoennct
            while((cmd = driver.PopEventForConnection(connections[i], out stream)) != NetworkEvent.Type.Empty){
                if(cmd == NetworkEvent.Type.Data)
                {
                    // Parsowanie
                    NetUtility.OnData(stream, connections[i], this);
                } else if (cmd == NetworkEvent.Type.Disconnect)
                {
                    Debug.Log("Klient od��czy� si�o od serwera");
                    connections[i] = default(NetworkConnection);
                    connectionDroped?.Invoke();
                    // Jesli ktos sie od��czy�
                    Shutdown();
                }
            }
        }
    }
    public void SendToClient(NetworkConnection connection, NetMessage msg)
    {
        // Pude�ko na wiadomosc
        DataStreamWriter writer;
        // Zapis adresu
        driver.BeginSend(connection, out writer);
        // Pakowanie zawartosci
        msg.Serialize(ref writer);
        // Wyslanie
        driver.EndSend(writer);
    }
    public void Broadcast(NetMessage msg)
    {
        for (int i = 0; i < connections.Length; i++)
        {
            if (connections[i].IsCreated) {
                SendToClient(connections[i], msg);
            }
        }
    }
}
