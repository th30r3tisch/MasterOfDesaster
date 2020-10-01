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

    public static void SpawnPlayer(Packet _packet) {
        int _id = _packet.ReadInt();
        string _username = _packet.ReadString();
        System.Numerics.Vector3 _postition = _packet.ReadVector3();
        Vector3 _postitionUnity = new Vector3(_postition.X, _postition.Y, _postition.Z);
        System.Numerics.Quaternion _rotation = _packet.ReadQuaternion();
        Quaternion _rotationUnity = new Quaternion(_rotation.X, _rotation.Y, _rotation.Z, _rotation.W);

        GameManager.instance.SpawnPlayer(_id, _username, _postitionUnity, _rotationUnity);
    }
}
