import game.model.Player;

import java.awt.*;

public class KI {

    private Player ki;

    public KI(){
        ki = new Player();
        ki.setName("KI");
        ki.setColor(new Color(255,0,0));
    }

    public Player getKi() {
        return ki;
    }
}
