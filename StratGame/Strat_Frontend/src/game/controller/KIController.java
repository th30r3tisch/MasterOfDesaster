package game.controller;

import game.model.map.Town;
import game.model.map.TreeNode;
import game.model.map.WorldModel;

import java.util.ArrayList;
import java.util.Random;

public class KIController {

    private int conquerRadius = 400;

    public void checkTown(Town t, WorldModel wm){
        if (t.getLife() > 20){
            ArrayList<TreeNode> conqerItems;
            ArrayList<TreeNode> enemyItems = new ArrayList<>();
            Random r = new Random();
            conqerItems = wm.getAreaContent((int)(t.getX() - conquerRadius), (int)(t.getY() - conquerRadius), (int)(t.getX() + conquerRadius), (int)(t.getY() + conquerRadius));
            for (TreeNode tn: conqerItems) {
                if (tn instanceof Town) {
                    if (((Town) tn).getOwner() == null){
                        enemyItems.add(tn);
                    } else if (!((Town) tn).getOwner().getName().equals("KI")){
                        enemyItems.add(tn);
                    }
                }
            }
            if (enemyItems.size() > 0)
                ConnectionController.attackRequest(t, enemyItems.get(r.nextInt(enemyItems.size())));
            else this.conquerRadius += 100;
        }
    }

}
