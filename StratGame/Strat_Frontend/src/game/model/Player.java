package game.model;

import game.model.map.Town;

import java.awt.*;
import java.io.Serializable;
import java.util.ArrayList;

public class Player implements Serializable {

    private String name;
    private Color color;
    private ArrayList<Town> ownedTowns = new ArrayList<>();

    public ArrayList<Town> getOwnedTowns() { return ownedTowns; }
    public void setOwnedTown(Town town) { this.ownedTowns.add(town); }
    public void removeOwnedTown(Town town){
        for(Town t : ownedTowns){
            if(t.isNode(town.getX(), town.getY())){
                ownedTowns.remove(t);
                break;
            }
        }
    }

    public Color getColor() {
        return this.color;
    }
    public javafx.scene.paint.Color getFXColor() {
        javafx.scene.paint.Color fxColor = javafx.scene.paint.Color.rgb(this.color.getRed(), this.color.getGreen(), this.color.getBlue());
        return fxColor;
    }
    public void setColor(Color color) { this.color = color; }

    public String getName() {
        return name;
    }
    public void setName(String name) {
        this.name = name;
    }
}
