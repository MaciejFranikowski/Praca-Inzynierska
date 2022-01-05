using Unity.Networking.Transport;

public class NetWelcome : NetMessage { 
    public int AssignedTeam { set; get; }
    public NetWelcome()
    {
        Code = OpCode.WELCOME;
    }
    public NetWelcome(DataStreamReader reader)
    {
        Code = OpCode.WELCOME;
        Deserialize(reader);
    }
    public override void Serialize(ref DataStreamWriter writer)
    {
        writer.WriteByte((byte)Code);
        writer.WriteInt(AssignedTeam);
    }
    public override void Deserialize(DataStreamReader reader)
    {
        AssignedTeam = reader.ReadInt();
    }
    public override void RecievedOnClient()
    {
        NetUtility.C_WELCOME?.Invoke(this);
    }
    public override void RecieviedOnServer(NetworkConnection connection)
    {
        NetUtility.S_WELCOME?.Invoke(this, connection);
    }
}
