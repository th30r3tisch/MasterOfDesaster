package game.model.map;

import game.model.Player;
import javafx.scene.shape.Shape;

import java.io.Serializable;
import java.util.ArrayList;

public class WorldModel implements Serializable {

    private ArrayList<TreeNode> treeNodes = new ArrayList<>();
    private ArrayList<Shape> shapes = new ArrayList<>();
    private Quadtree quadtree;

    public ArrayList<Shape> getCreateShapes() {
        ArrayList<Shape> shapes = new ArrayList<>();
        for (TreeNode treeNode: treeNodes) {
           shapes.add(treeNode.create());
        }
        return shapes;
    }

    public ArrayList<TreeNode> getTreeNodes(){
        return treeNodes;
    }

    public TreeNode getTreeNode(TreeNode toFindNode){
        for(int i = 0; i< treeNodes.size(); i++){
            if(treeNodes.get(i).isNode(toFindNode.getX(), toFindNode.getY()))
                return treeNodes.get(i);
        }
        return null;
    }

    public ArrayList<TreeNode> getAreaContent(int startX, int startY, int endX, int endY) {
        return this.quadtree.getConquerContent(startX, startY, endX, endY);
    }

    public void addNode(TreeNode shape) {
        this.treeNodes.add(shape);
    }
    public void setNodes(ArrayList<TreeNode> nodes){
        treeNodes = nodes;
    }

    public void setShapes(ArrayList<Shape> shapes){
        this.shapes = shapes;
    }
    public ArrayList<Shape> getShapes() {
        return shapes;
    }

    public void setQuadtree(Quadtree quadtree) {
        this.quadtree = quadtree;
        this.treeNodes = this.quadtree.getAllContent(quadtree, 0, 0, 4000, 2000);
    }

    public void atkupdateQuadtree(ArrayList<TreeNode> nodes){
        this.quadtree.addUpdateNode(nodes);
    }

    public void rmupdateQuadtree(ArrayList<TreeNode> nodes){
        this.quadtree.rmUpdateNode(nodes);
    }

    public void updateTownOwnership(Player player, TreeNode treeNode){
        this.quadtree.updateTownOwnership(player, treeNode);
    }
    
    public void updateTreeNodes(){
        if (quadtree != null)
            this.treeNodes = this.quadtree.getAllContent(this.quadtree, 0, 0, 4000, 2000);
    }
}
