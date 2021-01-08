using SharedLibrary;
using System;
using UnityEngine;

public class ClientSend : MonoBehaviour
{
    private static void SendTCPData(Packet packet) {
        packet.WriteLength();
        Client.instance.tcp.SendData(packet);
    }

    private static void SendUDPData(Packet packet) {
        packet.WriteLength();
        Client.instance.udp.SendData(packet);
    }

    public static void WelcomeReceived() {
        using (Packet packet = new Packet((int)ClientPackets.welcomeReceived)) {
            packet.Write(Client.instance.myId);
            packet.Write(SetupUIManager.instance.usernameField.text);
            if (SetupUIManager.instance.redColor.isOn) {
                packet.Write(System.Drawing.Color.FromArgb(1, 255, 0, 0));
            }
            else if (SetupUIManager.instance.blueColor.isOn) {
                packet.Write(System.Drawing.Color.FromArgb(1, 0, 0, 255));
            }
            else if (SetupUIManager.instance.yellowColor.isOn) {
                packet.Write(System.Drawing.Color.FromArgb(1, 255, 238, 0));
            }
            else if (SetupUIManager.instance.lightBlueColor.isOn) {
                packet.Write(System.Drawing.Color.FromArgb(1, 0, 218, 255));
            }
            else if(SetupUIManager.instance.greenColor.isOn) {
                packet.Write(System.Drawing.Color.FromArgb(1, 0, 255, 0 ));
            }

            SendTCPData(packet);
        }
    }

    public static void AttackRequest(Vector3 atkTown, Vector3 deffTown) {
        using (Packet packet = new Packet((int)ClientPackets.attackRequest)) {
            packet.Write(Client.instance.myId);
            packet.Write(ConversionManager.ToNumericVector(atkTown));
            packet.Write(ConversionManager.ToNumericVector(deffTown));
            packet.Write(DateTime.Now.ToBinary());

            SendTCPData(packet);
        }
    }

    public static void RetreatRequest(Vector3 atkTown, Vector3 deffTown) {
        using (Packet packet = new Packet((int)ClientPackets.retreatRequest)) {
            packet.Write(Client.instance.myId);
            packet.Write(ConversionManager.ToNumericVector(atkTown));
            packet.Write(ConversionManager.ToNumericVector(deffTown));
            packet.Write(DateTime.Now.ToBinary());

            SendTCPData(packet);
        }
    }

    public static void ConquerRequest(Vector3 deffTown) {
        using (Packet packet = new Packet((int)ClientPackets.conquerRequest)) {
            packet.Write(Client.instance.myId);
            packet.Write(ConversionManager.ToNumericVector(deffTown));
            packet.Write(DateTime.Now.ToBinary());

            SendTCPData(packet);
        }
    }
}
