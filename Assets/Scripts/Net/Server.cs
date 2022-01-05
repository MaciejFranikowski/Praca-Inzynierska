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
    // Po³¹czenia do serwera
    private NativeList<NetworkConnection> connections;

    private bool isActive = false;
    // Interwa³ wysy³ania keepAliveMessage, musi byc bo inaczej timeout i connection siê zerwie
    private const float keepAliveTickRate = 20.0f;
    private float lastKeepAlive;

    // jak klient siê roz³¹czy
    public Action connectionDroped;


    public void Init(ushort port)
    {
        // Inicjalizacja drivera
        driver = NetworkDriver.Create();
        // tworzenie endpointu do którego klienci siê pod³¹czaj¹, mo¿e to byc dowolny Ipv4
        NetworkEndPoint endpoint = NetworkEndPoint.AnyIpv4;
        endpoint.Port = port;

        // Nas³uchiwanie na porcie, jesli zero to sukces
        if(driver.Bind(endpoint) != 0)
        {
            // Np jeœli port zabrany np 80
            Debug.Log("Port unable to be bound" + endpoint.Port);
            return;
        } 
        else
        {
            driver.Listen();
            Debug.Log("Currently listening on port" + endpoint.Port);
        }
        // lista po³¹czeñ, max 2
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
        // Obs³uga wiadomoœci
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
            // Jak jest nie created to ¿e jest nie active
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
                    Debug.Log("Klient od³¹czy³ siêo od serwera");
                    connections[i] = default(NetworkConnection);
                    connectionDroped?.Invoke();
                    // Jesli ktos sie od³¹czy³
                    Shutdown();
                }
            }
        }
    }
    public void SendToClient(NetworkConnection connection, NetMessage msg)
    {
        // Pude³ko na wiadomosc
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
