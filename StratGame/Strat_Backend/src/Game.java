import game.model.Player;
import game.model.map.*;

import java.awt.geom.Line2D;
import java.util.ArrayList;
import java.util.Random;
import java.util.logging.Level;
import java.util.logging.Logger;


class Game {
    private WorldModel world;
    private KI ki;
    private static Logger LOGGER = Logger.getLogger("InfoLogging");
    private int obstacles = 10;
    private int towns = 20;
    private int gameMapHeight = 2000; // has to match with the gameMapHeight in frontend
    private int gameMapWidth = 4000; // has to match with the gameMapWidth in frontend
    private int distanceToEdge = 100;
    private int townMinDist = 100;
    private int obstacleMinLength = 50;
    private int obstacleMaxLength = 400;
    private ArrayList<TreeNode> areaContent;

    public Game() {
        this.world = new WorldModel(0, 0, gameMapWidth, gameMapHeight);
        LOGGER.log(Level.INFO,"Creating map");
        genereateInitialMap();
        createKI();
    }

    private void genereateInitialMap() {
        createObstacles();
        createTowns();
    }

     private void createTowns(){
         for (int i = 0; i < towns; i++){
             createTown();
         }
     }

     private void createKI(){
        this.ki = new KI();
        Town t = this.createTown();
        t.setOwner(ki.getKi());
        ki.getKi().setOwnedTown(t);
        Thread thread = new Thread(() -> {
            while (true) {
                try {
                    Thread.sleep(5000);
                } catch (InterruptedException ex) {
                    LOGGER.log(Level.SEVERE, "KI Error", ex);
                }
                for (Town town: ki.getKi().getOwnedTowns()) {
                    town.getLife();
                }
            }
        });
        thread.setDaemon(true);
        thread.start();
     }

     public Town createTown(){
         Town t = null;
         while(t == null){
             int x = randomNumber(distanceToEdge, gameMapWidth - distanceToEdge);
             int y = randomNumber(distanceToEdge, gameMapHeight - distanceToEdge);
             if (this.getAreaContent((x - townMinDist), (y - townMinDist), (x + townMinDist), (y + townMinDist)).size() == 0) { // check for overlapping towns
                 if (this.getAreaContent((x - obstacleMaxLength), (y - obstacleMaxLength), (x + townMinDist), (y + townMinDist)).size() == 0) { // check for overlapping obstacles
                     t = new Town(x, y);
                 }
             }
         }
         this.world.insert(t);
         return t;
     }

     private void createObstacles(){
         for (int i = 0; i < obstacles; i++){
             this.world.insert(new Obstacle(
                     randomNumber(distanceToEdge, gameMapWidth - distanceToEdge),
                     randomNumber(distanceToEdge, gameMapHeight - distanceToEdge),
                     randomNumber(0, 1),
                     randomNumber(obstacleMinLength, obstacleMaxLength)));
         }
     }

     public boolean isIntersecting(ArrayList<TreeNode> towns){
        ArrayList<TreeNode> intersectionObjs = new ArrayList<>();
        int t1x = (int) towns.get(0).getX();
        int t1y = (int) towns.get(0).getY();
        int t2x = (int) towns.get(1).getX();
        int t2y = (int) towns.get(1).getY();
        int startX = Math.min(t1x, t2x);
        int startY = Math.min(t1y, t2y);
        int endX = Math.max(t1x, t2x);
        int endY = Math.max(t1y, t2y);
        //rectangle between towns
        intersectionObjs.addAll(this.getAreaContent(startX, startY, endX, endY));
        //rectangle around town one
        intersectionObjs.addAll(this.getAreaContent(t1x - obstacleMaxLength, t1y - obstacleMaxLength, t1x + obstacleMaxLength, t1y + obstacleMaxLength));
        //rectangle around town two
        intersectionObjs.addAll(this.getAreaContent(t2x - obstacleMaxLength, t2y - obstacleMaxLength, t2x + obstacleMaxLength, t2y + obstacleMaxLength));
        if (intersectionObjs.size() != 0) {
            for (TreeNode node: intersectionObjs ) {
               if(node instanceof Obstacle){
                   boolean intersecting;
                   intersecting = Line2D.linesIntersect(towns.get(0).getX(),towns.get(0).getY(),towns.get(1).getX(),towns.get(1).getY(), node.getX(), node.getY(), node.getX() + ((Obstacle) node).getWidth(), node.getY() + ((Obstacle) node).getHeight());
                   if (intersecting) return true;
               }
            }
        }
        return false;
     }

     private int randomNumber(int min, int max){
         Random r = new Random();
         return r.nextInt(max - min + 1) + min;
     }

     public ArrayList<TreeNode> getAreaContent(int startX, int startY, int endX, int endY){
         return this.world.getAreaContent(startX, startY, endX, endY);
     }

     public Quadtree getInitialMap(){
        return this.world.getQuadtree();
     }

     public void updateTownOwner (Player player, Town town){
        world.getQuadtree().updateOwner(player, town);
     }

     public void addAttackTown(Town atk, Town def){
        world.getQuadtree().addUpdateNode(atk, def);
     }

    public void removeAttackTown(ArrayList<TreeNode> nodes){
        world.getQuadtree().rmUpdateNode(nodes);
    }
}
