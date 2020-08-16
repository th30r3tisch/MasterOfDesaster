package game.model.map;

import java.io.Serializable;

public class TreeNode implements Serializable {
    float x,y;

    public float getX() {
        return x;
    }

    public float getY() {
        return y;
    }

    public boolean inRange(int startX, int startY, int endX, int endY){
        return (this.x >= startX  && this.x <= endX
                && this.y >= startY && this.y <= endY);
    }

    public boolean isNode(float x, float y){
        return (this.x == x && this.y == y);
    }

    TreeNode(){ }
}
