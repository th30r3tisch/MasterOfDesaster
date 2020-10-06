using SharedLibrary;
using System.Net;
using UnityEngine;

public class ClientHandle : MonoBehaviour
{
    public static void Welcome(Packet _packet) {
        string _msg = _packet.ReadString();
        int _myId = _packet.ReadInt();

        Debug.Log($"Message from server: {_msg}");
        Client.instance.myId = _myId;
        ClientSend.WelcomeReceived();

        //udp
        Client.instance.udp.Connect(((IPEndPoint)Client.instance.tcp.socket.Client.LocalEndPoint).Port);
    }

    public static void CreateWorld(Packet _packet) {
        Client.instance.myId = _packet.ReadInt();
        Client.instance.username = _packet.ReadString();
        System.Drawing.Color c = _packet.ReadColor();
        Client.instance.color = new Color( c.R, c.G, c.B, c.A);
        int _seed = _packet.ReadInt();

        GameManager.instance.InitMap(_seed);
    }
}
