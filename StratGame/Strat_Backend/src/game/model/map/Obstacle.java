package game.model.map;

import java.io.Serializable;

public class Obstacle extends TreeNode implements Serializable {

    private int width;
    private int height;
    private boolean horizontal;
    private boolean vertical;
    private int obstacleSize = 20;

    // orientation 0 is horizontal, 1 is vertical
    public Obstacle(int x, int y, int orientation, int obstacleLength) {
        this.x = x;
        this.y = y;
        createObstacle(orientation, obstacleLength);
    }

    private void createObstacle(int orientation, int obstacleLength) {
        if (orientation == 0){
            this.height = obstacleSize;
            this.width = obstacleLength;
            this.horizontal = true;
            this.vertical = false;
        } else if (orientation == 1){
            this.height = obstacleLength;
            this.width = obstacleSize;
            this.vertical = true;
            this.horizontal = false;
        }
    }

    public int getWidth() {
        return width;
    }

    public int getHeight() {
        return height;
    }

    public boolean isHorizontal() {
        return horizontal;
    }

    public boolean isVertical() {
        return vertical;
    }

}
