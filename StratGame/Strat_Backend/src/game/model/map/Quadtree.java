package game.model.map;

import game.model.Player;

import java.io.Serializable;
import java.util.ArrayList;
import java.util.List;
import java.util.logging.Level;
import java.util.logging.Logger;

public class Quadtree implements Serializable {
    final int MAX_CAPACITY =4;
    private int level = 0;
    private List<TreeNode> treeNodes;
    private Quadtree northWest = null;
    private Quadtree northEast = null;
    private Quadtree southWest = null;
    private Quadtree southEast = null;
    private Boundry boundry;
    private static Logger LOGGER = Logger.getLogger("InfoLogging");

    public Quadtree(int level, Boundry boundry) {
        this.level = level;
        this.treeNodes = new ArrayList<>();
        this.boundry = boundry;
    }

    void split() {
        int xOffset = this.boundry.getxMin()
                + (this.boundry.getxMax() - this.boundry.getxMin()) / 2;
        int yOffset = this.boundry.getyMin()
                + (this.boundry.getyMax() - this.boundry.getyMin()) / 2;

        northWest = new Quadtree(this.level + 1, new Boundry(
                this.boundry.getxMin(),
                this.boundry.getyMin(),
                xOffset,
                yOffset));
        northEast = new Quadtree(this.level + 1, new Boundry(
                xOffset,
                this.boundry.getyMin(),
                this.boundry.getxMax(),
                yOffset));
        southWest = new Quadtree(this.level + 1, new Boundry(
                this.boundry.getxMin(),
                yOffset,
                xOffset,
                this.boundry.getyMax()));
        southEast = new Quadtree(this.level + 1, new Boundry(
                xOffset,
                yOffset,
                this.boundry.getxMax(),
                this.boundry.getyMax()));

    }

    public void insert(TreeNode treeNode) {
        float x = treeNode.getX();
        float y = treeNode.getY();
        if (!this.boundry.inRange(x, y)) {
            return;
        }

        if (treeNodes.size() < MAX_CAPACITY) {
            treeNodes.add(treeNode);
            return;
        }
        // Exceeded the capacity so split it in FOUR
        if (northWest == null) {
            split();
        }

        // Check to which partition coordinates belong
        if (this.northWest.boundry.inRange(x, y))
            this.northWest.insert(treeNode);
        else if (this.northEast.boundry.inRange(x, y))
            this.northEast.insert(treeNode);
        else if (this.southWest.boundry.inRange(x, y))
            this.southWest.insert(treeNode);
        else if (this.southEast.boundry.inRange(x, y))
            this.southEast.insert(treeNode);
        else
            LOGGER.log(Level.SEVERE, "ERROR : Unhandled partition " + x + " " +  y);
    }

    public void getAreaContent(Quadtree tree, int startX, int startY, int endX, int endY, ArrayList<TreeNode> wholeMap) {
        if (tree == null) return;

        if( !(startX > tree.boundry.xMax) && !(endX < tree.boundry.xMin) && !(startY > tree.boundry.yMax) && !(endY < tree.boundry.yMin)){
            for (TreeNode treeNode : tree.treeNodes) {
                if (treeNode.inRange(startX, startY, endX, endY)){
                    wholeMap.add(treeNode);
                }
            }
        }
        getAreaContent(tree.northWest, startX, startY, endX, endY, wholeMap);
        getAreaContent(tree.northEast, startX, startY, endX, endY, wholeMap);
        getAreaContent(tree.southWest, startX, startY, endX, endY, wholeMap);
        getAreaContent(tree.southEast, startX, startY, endX, endY, wholeMap);
    }

    public ArrayList<TreeNode> getAllContent(Quadtree tree, int startX, int startY, int endX, int endY){
        ArrayList<TreeNode> wholeMap = new ArrayList<>();
        getAreaContent(tree, startX, startY, endX, endY, wholeMap);
        return wholeMap;
    }

    private void addTownAtk(Quadtree tree, Town atk, Town deff){
        if (tree == null) return;

        if (!(deff.x > tree.boundry.xMax) && !(deff.x < tree.boundry.xMin) && !(deff.y > tree.boundry.yMax) && !(deff.y < tree.boundry.yMin)) {
            for (int i = 0; i < tree.treeNodes.size(); i++) {
                if (tree.treeNodes.get(i).isNode(deff.x, deff.y)) {
                    Town t = (Town) tree.treeNodes.get(i);
                    if (t.getConqueredByTowns().size() == 0) {
                        ((Town) tree.treeNodes.get(i)).addConqueredByTown(atk);
                        ((Town) tree.treeNodes.get(i)).setLife(deff.getLife());
                    }
                    else {
                        for (Town tt : deff.getConqueredByTowns()) {
                            if (tt == atk) return;
                            else {
                                ((Town) tree.treeNodes.get(i)).addConqueredByTown(atk);
                                ((Town) tree.treeNodes.get(i)).setLife(deff.getLife());
                            }
                        }
                    }
                }
            }
        }
        addTownAtk(tree.northWest, deff, atk);
        addTownAtk(tree.northEast, deff, atk);
        addTownAtk(tree.southWest, deff, atk);
        addTownAtk(tree.southEast, deff, atk);
    }

    private void rmTownAtk(Quadtree tree, Town deff, Town atk) {
        if (tree == null) return;

        if (!(deff.x > tree.boundry.xMax) && !(deff.x < tree.boundry.xMin) && !(deff.y > tree.boundry.yMax) && !(deff.y < tree.boundry.yMin)) {
            for (int i = 0; i < tree.treeNodes.size(); i++) {
                if (tree.treeNodes.get(i).isNode(deff.x, deff.y)) {
                    for (Town tt : deff.getConqueredByTowns()) {
                        if (tt == atk) {
                            ((Town) tree.treeNodes.get(i)).removeConqueredByTown(atk);
                            ((Town) tree.treeNodes.get(i)).setLife(deff.getLife());
                        }
                        else return;
                    }
                }
            }
        }
        rmTownAtk(tree.northWest, deff, atk);
        rmTownAtk(tree.northEast, deff, atk);
        rmTownAtk(tree.southWest, deff, atk);
        rmTownAtk(tree.southEast, deff, atk);
    }

    

    private void updateOwner(Quadtree tree, Player player, TreeNode treeNode){
        if (tree == null) return;

        if( !(treeNode.getX() > tree.boundry.xMax) && !(treeNode.getX() < tree.boundry.xMin) && !(treeNode.getY() > tree.boundry.yMax) && !(treeNode.getY() < tree.boundry.yMin)){
            for (int i = 0; i < tree.treeNodes.size(); i++) {
                if (tree.treeNodes.get(i).isNode(treeNode.getX(), treeNode.getX())){
                    System.out.println("update owner " + player.getName());
                    ((Town) tree.treeNodes.get(i)).removeAllConquerors();
                    ((Town) tree.treeNodes.get(i)).setOwner(player);
                    ((Town) tree.treeNodes.get(i)).setLife(((Town)treeNode).getLife());

                    return;
                }
            }
        }
        updateOwner(tree.northWest, player, treeNode);
        updateOwner(tree.northEast, player, treeNode);
        updateOwner(tree.southWest, player, treeNode);
        updateOwner(tree.southEast, player, treeNode);
    }

    public void addUpdateNode(Town attacker, Town defender){
        addTownAtk(this, attacker, defender);
    }

    public void rmUpdateNode(ArrayList<TreeNode> nodes) {
        Town inComingAtk = (Town) nodes.get(0);
        Town inComingDeff = (Town) nodes.get(1);
        rmTownAtk(this, inComingDeff, inComingAtk);
    }

    public void updateOwner(Player player,TreeNode treeNode){
        updateOwner(this, player, treeNode);
    }
}