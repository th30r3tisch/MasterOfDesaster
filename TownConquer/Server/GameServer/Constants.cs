using System.ComponentModel;

namespace GameServer {
    class Constants {
        public const int TICKS_PER_SEC = 30;
        public const int MS_PER_TICK = 1000 / TICKS_PER_SEC;

        public const int TOWN_INITIAL_LIFE = 10;
        public const int OBSTACLE_WIDTH = 20;
        public const int OBSTACLE_NUMBER = 10;
        public const int TOWN_NUMBER = 20;
        public const int MAP_HEIGHT = 2000;
        public const int MAP_WIDTH = 4000;
        public const int DISTANCE_TO_EDGES = 100;
        public const int TOWN_MIN_DISTANCE = 100;
        public const int OBSTACLE_MIN_LENGTH = 50;
        public const int OBSTACLE_MAX_LENGTH = 400;
    }
}
