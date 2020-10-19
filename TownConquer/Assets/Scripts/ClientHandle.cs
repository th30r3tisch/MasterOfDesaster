using SharedLibrary;
using SharedLibrary.Models;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class ClientHandle : MonoBehaviour
{
    public static void Welcome(Packet _packet) {
        string _msg = _packet.ReadString();
        int _myId = _packet.ReadInt();

        Debug.Log($"Message from server: {_msg}");
        Client.instance.myId = _myId;
        Client.instance.towns = new List<Town>();
        Client.instance.enemies = new List<Player>();
        ClientSend.WelcomeReceived();

        //udp
        Client.instance.udp.Connect(((IPEndPoint)Client.instance.tcp.socket.Client.LocalEndPoint).Port);
    }

    public static void CreateWorld(Packet _packet) {
        int _myId = _packet.ReadInt();
        string _username = _packet.ReadString();
        System.Drawing.Color _color = _packet.ReadColor();
        Client.instance.me = new Player(_myId, _username, _color);
        int _seed = _packet.ReadInt();

        System.Numerics.Vector3 v = _packet.ReadVector3();
        Vector3 _townPos = new Vector3(v.X, v.Y, v.Z);

        GameManager.instance.InitMap(_seed, _townPos, Client.instance.me);
    }

    public static void UpdateWorld(Packet _packet) {
        int _enemyId = _packet.ReadInt();
        string _enemyname = _packet.ReadString();
        System.Drawing.Color _enemyColor = _packet.ReadColor();
        System.Numerics.Vector3 _v = _packet.ReadVector3();

        Player _enemy = new Player(_enemyId, _enemyname, _enemyColor);
        Vector3 _townPos = new Vector3(_v.X, _v.Y, _v.Z);
        GameManager.instance.AddEnemies(_enemy, _townPos);
    }

    public static void GrantedAttack(Packet _packet) {
        System.Numerics.Vector3 _v1 = _packet.ReadVector3();
        Vector3 _atkTown = new Vector3(_v1.X, _v1.Y, _v1.Z);
        System.Numerics.Vector3 _v2 = _packet.ReadVector3();
        Vector3 _deffTown = new Vector3(_v2.X, _v2.Y, _v2.Z);
        GameManager.instance.AttackTown(_atkTown, _deffTown);
    }

    public static void GrantedRetreat(Packet _packet) {
        System.Numerics.Vector3 _v1 = _packet.ReadVector3();
        Vector3 _atkTown = new Vector3(_v1.X, _v1.Y, _v1.Z);
        System.Numerics.Vector3 _v2 = _packet.ReadVector3();
        Vector3 _deffTown = new Vector3(_v2.X, _v2.Y, _v2.Z);
        GameManager.instance.RetreatTroops(_atkTown, _deffTown);
    }

    public static void GrantedConquer(Packet _packet) {
        int _conquererId = _packet.ReadInt();
        System.Numerics.Vector3 _v2 = _packet.ReadVector3();
        Vector3 _deffTown = new Vector3(_v2.X, _v2.Y, _v2.Z);
        GameManager.instance.ConquerTown(_conquererId, _deffTown);
    }
}
