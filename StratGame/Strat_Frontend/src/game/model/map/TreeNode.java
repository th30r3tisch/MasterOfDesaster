package game.model.map;

import javafx.scene.shape.Shape;

import java.io.Serializable;

public abstract class TreeNode implements Serializable {
    private static final long serialVersionUID = 5912386326094196679L;
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

    public abstract Shape create();
}
