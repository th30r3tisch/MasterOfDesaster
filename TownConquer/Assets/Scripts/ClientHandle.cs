using SharedLibrary;
using SharedLibrary.Models;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
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
        int _id = _packet.ReadInt();
        string _username = _packet.ReadString();
        int _seed = _packet.ReadInt();

        //System.Numerics.Vector3 _postition = _packet.ReadVector3();
        //Vector3 _postitionUnity = new Vector3(_postition.X, _postition.Y, _postition.Z);

        GameManager.instance.InitTowns(_id, _seed);
    }
}
