using SharedLibrary;
using UnityEngine;

public class ClientSend : MonoBehaviour
{
    private static void SendTCPData(Packet _packet) {
        _packet.WriteLength();
        Client.instance.tcp.SendData(_packet);
    }

    private static void SendUDPData(Packet _packet) {
        _packet.WriteLength();
        Client.instance.udp.SendData(_packet);
    }

    public static void WelcomeReceived() {
        using (Packet _packet = new Packet((int)ClientPackets.welcomeReceived)) {
            _packet.Write(Client.instance.myId);
            _packet.Write(UIManager.instance.usernameField.text);
            if (UIManager.instance.redColor.isOn) {
                _packet.Write(System.Drawing.Color.FromArgb(1, 255, 0, 0));
            }
            else if (UIManager.instance.blueColor.isOn) {
                _packet.Write(System.Drawing.Color.FromArgb(1, 0, 0, 255));
            }
            else if (UIManager.instance.yellowColor.isOn) {
                _packet.Write(System.Drawing.Color.FromArgb(1, 255, 238, 0));
            }
            else if (UIManager.instance.lightBlueColor.isOn) {
                _packet.Write(System.Drawing.Color.FromArgb(1, 0, 218, 255));
            }
            else if(UIManager.instance.greenColor.isOn) {
                _packet.Write(System.Drawing.Color.FromArgb(1, 0, 255, 0 ));
            }

            SendTCPData(_packet);
        }
    }

    public static void AttackRequest(Vector3 atkTown, Vector3 deffTown) {
        using (Packet _packet = new Packet((int)ClientPackets.attackRequest)) {
            _packet.Write(Client.instance.myId);
            _packet.Write(new System.Numerics.Vector3(atkTown.x, atkTown.y, atkTown.z));
            _packet.Write(new System.Numerics.Vector3(deffTown.x, deffTown.y, deffTown.z));

            SendTCPData(_packet);
        }
    }

    public static void RetreatRequest(Vector3 atkTown, Vector3 deffTown) {
        using (Packet _packet = new Packet((int)ClientPackets.retreatRequest)) {
            _packet.Write(Client.instance.myId);
            _packet.Write(new System.Numerics.Vector3(atkTown.x, atkTown.y, atkTown.z));
            _packet.Write(new System.Numerics.Vector3(deffTown.x, deffTown.y, deffTown.z));

            SendTCPData(_packet);
        }
    }
}
