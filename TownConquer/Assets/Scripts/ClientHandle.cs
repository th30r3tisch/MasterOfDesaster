using SharedLibrary;
using SharedLibrary.Models;
using System;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class ClientHandle : MonoBehaviour
{
    public static void Welcome(Packet packet) {
        string msg = packet.ReadString();
        int myId = packet.ReadInt();

        Debug.Log($"Message from server: {msg}");
        Client.instance.myId = myId;
        Client.instance.towns = new List<Town>();
        Client.instance.enemies = new List<Player>();
        ClientSend.WelcomeReceived();

        //udp
        Client.instance.udp.Connect(((IPEndPoint)Client.instance.tcp.socket.Client.LocalEndPoint).Port);
    }

    public static void CreateWorld(Packet packet) {
        int myId = packet.ReadInt();
        string username = packet.ReadString();
        System.Drawing.Color color = packet.ReadColor();
        long creationTime = packet.ReadLong();
        Client.instance.me = new Player(myId, username, color, creationTime);
        int seed = packet.ReadInt();

        System.Numerics.Vector3 v = packet.ReadVector3();
        Vector3 townPos = new Vector3(v.X, v.Y, v.Z);

        GameManager.instance.InitMap(seed, townPos, Client.instance.me, creationTime);
    }

    public static void UpdateWorld(Packet packet) {
        List<Vector3> towns = new List<Vector3>();
        int enemyId = packet.ReadInt();
        string enemyname = packet.ReadString();
        System.Drawing.Color enemyColor = packet.ReadColor();
        long creationTime = packet.ReadLong();
        int townNumber = packet.ReadInt();
        for (int i = 0; i < townNumber; i++) {
            System.Numerics.Vector3 v = packet.ReadVector3();
            Vector3 townPos = new Vector3(v.X, v.Y, v.Z);
            towns.Add(townPos); 
        }
        Player enemy = new Player(enemyId, enemyname, enemyColor, creationTime);
        GameManager.instance.AddEnemies(enemy, towns);
    }

    public static void GrantedInteraction(Packet packet) {
        System.Numerics.Vector3 v1 = packet.ReadVector3();
        Vector3 atkTown = new Vector3(v1.X, v1.Y, v1.Z);
        System.Numerics.Vector3 v2 = packet.ReadVector3();
        Vector3 deffTown = new Vector3(v2.X, v2.Y, v2.Z);
        GameManager.instance.AddInteractionToTown(atkTown, deffTown);
    }

    public static void GrantedRetreat(Packet packet) {
        System.Numerics.Vector3 v1 = packet.ReadVector3();
        Vector3 atkTown = new Vector3(v1.X, v1.Y, v1.Z);
        System.Numerics.Vector3 v2 = packet.ReadVector3();
        Vector3 deffTown = new Vector3(v2.X, v2.Y, v2.Z);
        GameManager.instance.RetreatTroops(atkTown, deffTown);
    }

    public static void GrantedConquer(Packet packet) {
        int conquererId = packet.ReadInt();
        System.Numerics.Vector3 v2 = packet.ReadVector3();
        long time = packet.ReadLong();
        Vector3 deffTown = new Vector3(v2.X, v2.Y, v2.Z);
        GameManager.instance.ConquerTown(conquererId, deffTown, time);
    }

    public static void PlayerDisconnected(Packet packet) {
        int playerId = packet.ReadInt();
        List<Player> player = Client.instance.enemies;
        for (int i = 0; i < player.Count; i++) {
            if (player[i].id == playerId) {
                player.Remove(player[i]);
            }
        }
    }
}
