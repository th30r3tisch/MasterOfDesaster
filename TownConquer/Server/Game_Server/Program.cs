using SharedLibrary;
using System;
using System.Threading;

namespace Game_Server {
    class Program {

        private static bool _isRunning = false;

        static void Main(string[] args) {
            Console.Title = "GameServer";
            _isRunning = true;

            Thread mainThread = new Thread(MainThread);
            mainThread.Start("Main");
            Server server = new Server();

            Server.Start(Constants.MAX_PLAYERS, Constants.SERVER_PORT);

        }

        private static void MainThread(object data) {
            Console.WriteLine($"{data} thread started. Running at {Constants.TICKS_PER_SEC} ticks per second.");
            if (Constants.TRAININGS_MODE == true) {
                GameManager.Update();
            }
            else {
                DateTime nextLoop = DateTime.Now;

                while (_isRunning) {
                    while (nextLoop < DateTime.Now) {
                        GameManager.Update();
                        nextLoop = nextLoop.AddMilliseconds(Constants.MS_PER_TICK);

                        if (nextLoop > DateTime.Now) {
                            Thread.Sleep(nextLoop - DateTime.Now);
                        }
                    }
                }
            }
        }
    }
}
