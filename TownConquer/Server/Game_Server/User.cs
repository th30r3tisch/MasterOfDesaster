using SharedLibrary;
using SharedLibrary.Models;
using System.Drawing;
using System.Numerics;

namespace Game_Server {
    abstract class User {
        public Game game;
        public Player player;
        public int id;

        public void SetupUser(string playerName, Color color, long timestamp) {
            player = new Player(id, playerName, color, timestamp);
            game.gm.CreateTown(player);
        }

        public void InteractWithTown(Vector3 atkTown, Vector3 deffTown) {
            if (!game.gm.IsIntersecting(atkTown, deffTown)) {
                game.gm.AddActionToTown(atkTown, deffTown);
                if (Constants.TRAININGS_MODE == false) {
                    foreach (Client client in game.clients.Values) {
                        if (client.player != null) {
                            ServerSend.GrantedAction(client.id, atkTown, deffTown);
                        }
                    }
                }
            }
        }

        public void RetreatFromTown(Vector3 atkTown, Vector3 deffTown) {
            game.gm.RemoveActionFromTown(atkTown, deffTown);
            if (Constants.TRAININGS_MODE == false) {
                foreach (Client client in game.clients.Values) {
                    if (client.player != null) {
                        ServerSend.GrantedRetreat(client.id, atkTown, deffTown);
                    }
                }
            }
        }

        public void ConquerTown(Vector3 deffTown) {
            Town town = game.tree.SearchTown(game.tree, deffTown);
            game.gm.ConquerTown(this, town);
            if (Constants.TRAININGS_MODE == false) {
                foreach (Client client in game.clients.Values) {
                    if (client.player != null) {
                        ServerSend.GrantedConquer(client.id, player, town.position, town.livingTime);
                    }
                }
            }
        }

        abstract public void Disconnect();
    }
}
