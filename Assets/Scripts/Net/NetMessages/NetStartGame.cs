using Unity.Networking.Transport;

public class NetStartGame : NetMessage
{
    public NetStartGame()
    {
        Code = OpCode.START_GAME;
    }
    public NetStartGame(DataStreamReader reader)
    {
        Code = OpCode.START_GAME;
        Deserialize(reader);
    }
    public override void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte)Code);
    }
    public override void Deserialize(DataStreamReader reader)
    {
    }
    public override void RecievedOnClient()
    {
        NetUtility.C_START_GAME?.Invoke(this);
    }
    public override void RecieviedOnServer(NetworkConnection connection)
    {
        NetUtility.S_START_GAME?.Invoke(this, connection);
    }
}
