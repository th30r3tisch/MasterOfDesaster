using SharedLibrary;
using System;
using UnityEngine;

/// <summary>
/// BASIERT AUF DEM CODE VON TOM WEILAND SIEHE:
/// Weiland, Tom, 8 Dec 2019, https://github.com/tom-weiland/tcp-udp-networking/blob/682f3fe3dd0b9a7d74e13896eabbfa48d3c95e20/GameClient/Assets/Scripts/ClientSend.cs [23.03.2021]
/// Der Code wurde bearbeitet und Anpassungen vorgenommen.
/// Dieser Code spielt im Rahmen der Arbeit nur eine geringe Rolle.
/// </summary>

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

    public static void InteractionRequest(Vector3 atkTown, Vector3 deffTown) {
        using (Packet packet = new Packet((int)ClientPackets.interactionRequest)) {
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
