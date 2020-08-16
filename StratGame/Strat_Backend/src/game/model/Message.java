package game.model;

import game.model.map.Quadtree;
import game.model.map.TreeNode;

import java.io.Serializable;
import java.util.ArrayList;

public class Message implements Serializable {

    private MessageType type;
    private Player player;
    private String note;
    private ArrayList<TreeNode> treeNodes;
    private Quadtree quadtree;

    public MessageType getType() {
        return type;
    }
    public void setType(MessageType type) {
        this.type = type;
    }

    public Player getPlayer() {
        return player;
    }
    public void setPlayer(Player p) {
        this.player = p;
    }

    public String getNote() {
        return note;
    }
    public void setNote(String note) {
        this.note = note;
    }

    public ArrayList<TreeNode> getTreeNodes() { return treeNodes; }
    public void setTreeNodes(ArrayList<TreeNode> treeNodes) { this.treeNodes = treeNodes; }

    public Quadtree getQuadtree() { return quadtree; }
    public void setQuadtree(Quadtree quadtree) { this.quadtree = quadtree;}
}
