package game.model.map;

import game.model.Player;
import javafx.scene.shape.Circle;
import javafx.scene.shape.Shape;

import java.awt.*;
import java.io.Serializable;
import java.util.AbstractMap;
import java.util.ArrayList;

public class Town extends TreeNode implements Serializable {
    private static final long serialVersionUID = -5426891661570097468L;

    private Player owner;
    private int life;
    private Color color;
    //changed array type to check for accurate attack ticks later
    private ArrayList<AbstractMap.SimpleEntry<Town, Long>> conqueredByTowns = new ArrayList<>();

    public ArrayList<AbstractMap.SimpleEntry<Town, Long>> getConqueredByTownEntries(){
        return conqueredByTowns;
    }

    public ArrayList<Town> getConqueredByTowns() {
        ArrayList<Town> towns = new ArrayList<>();
        if(conqueredByTowns != null && conqueredByTowns.size() > 0) {
            for (AbstractMap.SimpleEntry<Town, Long> entry : conqueredByTowns) {
                Town town = entry.getKey();
                towns.add(town);
            }
        }
        return towns;
    }

    public ArrayList<Long> getAttackTimes(){
        ArrayList<Long> time = new ArrayList<>();
        for (AbstractMap.SimpleEntry<Town, Long> entry : conqueredByTowns ) {
            long town = entry.getValue();
            time.add(town);
        }
        return time;
    }

    public void changeOwnership(Player player){
        owner = player;
    }

    public void addConqueredByTown(Town town){
        if(conqueredByTowns == null) conqueredByTowns = new ArrayList<>();
        //set start time for attack, to check for attack ticks later
        this.conqueredByTowns.add(new AbstractMap.SimpleEntry(town, System.currentTimeMillis()));
    }

    public void removeConqueredByTown(Town town) {
        this.conqueredByTowns.removeIf(entry -> (entry.getKey().isNode(town.getX(), town.getY())));
        this.conqueredByTowns.trimToSize();
    }

    public void removeAllConquerors(){
        conqueredByTowns = new ArrayList<>();
    }

    public Town(Player conqueror, int x, int y) {
        this.life = 20;
        this.owner = conqueror;
        this.color = conqueror.getColor();
        this.x = x;
        this.y = y;
    }

    public Player getOwner() { return owner; }

    public void setOwner(Player conqueror) {
        this.owner = conqueror;
        this.color = conqueror.getColor();
    }

    public int getLife() { return life; }
    public void setLife(int life) { this.life = life; }

    public Color getColor() { return color; }

    @Override
    public Shape create(){
        int radius = 0;
        if (life < 100){
            radius = 30;
        } else if (life > 100 && life < 1000){
            radius = 50;
        }
        Circle c = new Circle(this.x,this.y, radius);
        if(owner == null)
            c.setFill(javafx.scene.paint.Color.GRAY);
        else
            c.setFill(owner.getFXColor());
        return c;
    }
}