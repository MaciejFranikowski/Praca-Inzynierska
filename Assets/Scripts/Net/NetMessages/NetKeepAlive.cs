using Unity.Networking.Transport;

public class NetKeepAlive : NetMessage
{
    public NetKeepAlive()
    {
        Code = OpCode.KEEP_ALIVE;
    }
    public NetKeepAlive(DataStreamReader reader) 
    {
        Code = OpCode.KEEP_ALIVE;
        Deserialize(reader);
    }
    public override void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte)Code);
    }
    public override void Deserialize(DataStreamReader reader) // Op code juz przeczytany, nic nie trzeb czytaæ
    {
    }

    public override void RecievedOnClient()
    {
        NetUtility.C_KEEP_ALIVE?.Invoke(this);
    }
    public override void RecieviedOnServer(NetworkConnection connection)
    {
        NetUtility.S_KEEP_ALIVE?.Invoke(this, connection);
    }
}
