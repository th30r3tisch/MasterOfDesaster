package game.model.map;

import game.model.Player;

import java.awt.*;
import java.io.Serializable;
import java.util.AbstractMap;
import java.util.ArrayList;

public class Town extends TreeNode implements Serializable {

    private Player owner;
    private int life;
    private Color color;
    //private ArrayList<Town> conqueredByTowns = new ArrayList<>();
    private ArrayList<AbstractMap.SimpleEntry<Town, Long>> conqueredByTowns = new ArrayList<>();

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

    public void removeAllConquerors(){
        conqueredByTowns = new ArrayList<>();
    }

    public void addConqueredByTown(Town town){
        if(conqueredByTowns == null) conqueredByTowns = new ArrayList<>();
        this.conqueredByTowns.add(new AbstractMap.SimpleEntry(town, System.currentTimeMillis()));
    }

    public void removeConqueredByTown(Town town) {
        this.conqueredByTowns.removeIf(entry -> (entry.getKey().isNode(town.getX(), town.getY())));
        this.conqueredByTowns.trimToSize();
    }

    public Town(int x, int y) {
        this.life = 10;
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
}
