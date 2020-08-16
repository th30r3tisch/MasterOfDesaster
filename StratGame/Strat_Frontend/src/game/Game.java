package game;

import game.controller.LoginController;
import game.controller.WorldController;
import javafx.application.Application;
import javafx.stage.Stage;
import game.view.Login;

public class Game extends Application {

    @Override
    public void start(Stage primaryStage) {
        WorldController wc = new WorldController();
        LoginController lc = new LoginController(primaryStage, wc);

        primaryStage.setTitle("Conquer all towns");
        primaryStage.setResizable(false);
        primaryStage.setOnCloseRequest(e -> System.exit(0));
        primaryStage.setScene(new Login(lc).getScene());
        primaryStage.show();
    }
}