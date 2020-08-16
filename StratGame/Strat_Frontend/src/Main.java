import game.Game;
import javafx.application.Application;

public class Main {

    public Main() {    }

    public static void main(String[] args) {
        if (args.length != 1) {
            System.err.println("Pass the server IP as command line argument");
            return;
        }
        Application.launch(Game.class, args);
    }
}