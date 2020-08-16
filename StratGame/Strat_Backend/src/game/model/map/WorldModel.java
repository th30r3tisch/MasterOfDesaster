package game.model.map;

import java.io.Serializable;
import java.util.ArrayList;

public class WorldModel implements Serializable {

    private Quadtree quadtree;

    public WorldModel(int startX, int startY, int endX, int endY){
        this.quadtree = new Quadtree(1, new Boundry(startX, startY, endX, endY));
    }

    public void insert(TreeNode treeNode){
        this.quadtree.insert(treeNode);
    }

    public ArrayList<TreeNode> getAreaContent(int startX, int startY, int endX, int endY) {
        return this.quadtree.getAllContent(quadtree, startX, startY, endX, endY);
    }

    public Quadtree getQuadtree(){
        return this.quadtree;
    }
}
