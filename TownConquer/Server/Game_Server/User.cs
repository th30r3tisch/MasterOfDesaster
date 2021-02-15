using SharedLibrary;
using SharedLibrary.Models;
using System;
using System.Drawing;
using System.Numerics;

namespace Game_Server {
    abstract class User {
        protected Game game { get; set; }
        public Player player;
        public int id;

        public void SetupUser(string playerName, Color color) {
            player = new Player(id, playerName, color, DateTime.Now);
            game.gm.CreateTown(player);
        }

        public void InteractWithTown(Vector3 atkTown, Vector3 deffTown, DateTime timeStamp) {
            if (!game.gm.IsIntersecting(atkTown, deffTown)) {
                game.gm.AddActionToTown(atkTown, deffTown, timeStamp);
                if (Constants.TRAININGS_MODE == false) {
                    foreach (Client client in game.clients.Values) {
                        if (client.player != null) {
                            ServerSend.GrantedAction(client.id, atkTown, deffTown);
                        }
                    }
                }
            }
        }

        public void RetreatFromTown(Vector3 atkTown, Vector3 deffTown, DateTime timeStamp) {
            game.gm.RemoveActionFromTown(atkTown, deffTown, timeStamp);
            if (Constants.TRAININGS_MODE == false) {
                foreach (Client client in game.clients.Values) {
                    if (client.player != null) {
                        ServerSend.GrantedRetreat(client.id, atkTown, deffTown);
                    }
                }
            }
        }

        public void ConquerTown(Player conquerer, Vector3 deffTown, DateTime timeStamp) {
            game.gm.ConquerTown(conquerer, deffTown, timeStamp);
            if (Constants.TRAININGS_MODE == false) {
                foreach (Client client in game.clients.Values) {
                    if (client.player != null) {
                        ServerSend.GrantedConquer(client.id, conquerer, deffTown);
                    }
                }
            }
        }

        abstract public void Disconnect();
    }
}
