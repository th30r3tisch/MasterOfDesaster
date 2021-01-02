using Game_Server.KI;
using SharedLibrary;
using System;
using System.Threading;

namespace Game_Server {
    class Program {

        private static bool isRunning = false;

        static void Main(string[] args) {
            Console.Title = "GameServer";
            isRunning = true;

            Thread mainThread = new Thread(MainThread);
            mainThread.Start("Main");
            Server server = new Server();

            Server.Start(Constants.MAX_PLAYERS, Constants.SERVER_PORT);

        }

        private static void MainThread(object data) {
            Console.WriteLine($"{data} thread started. Running at {Constants.TICKS_PER_SEC} ticks per second.");
            DateTime _nextLoop = DateTime.Now;

            while (isRunning) {
                while (_nextLoop < DateTime.Now) {
                    GameManager.Update();
                    _nextLoop = _nextLoop.AddMilliseconds(Constants.MS_PER_TICK);

                    if (_nextLoop > DateTime.Now) {
                        Thread.Sleep(_nextLoop - DateTime.Now);
                    }
                }
            }
        }
    }
}
