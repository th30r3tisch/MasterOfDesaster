package game.model.units;

import javafx.scene.Node;

import java.awt.geom.Point2D;
import java.io.Serializable;
import java.util.ArrayList;

public abstract class Unit implements Serializable {

    private int life;

    private Node view;
    private Point2D velocity;
    private ArrayList path;

    public Integer getLife() {
        return life;
    }

    public void setLife(Integer life) {
        this.life = life;
    }

    private void calculatePath(){
        //TODO implement
    }

    //update method for updating position
    public void update(){
        view.setTranslateX(view.getTranslateX() + velocity.getX());
        view.setTranslateX(view.getTranslateY() + velocity.getY());
    }
}
