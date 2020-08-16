package game.view;

import game.controller.LoginController;
import javafx.geometry.Insets;
import javafx.geometry.Pos;
import javafx.scene.Scene;
import javafx.scene.control.Button;
import javafx.scene.control.Label;
import javafx.scene.control.TextField;
import javafx.scene.layout.GridPane;
import javafx.scene.layout.HBox;
import javafx.scene.text.Font;
import javafx.scene.text.FontWeight;
import javafx.scene.text.Text;

public class Login implements ViewInterface {

    private Scene login;
    private LoginController controller;
    private int windowWidth = 300;
    private int windowHeight = 200;

    public Login(LoginController lc) {
        this.controller = lc;
    }

    @Override
    public Scene getScene() {

        GridPane grid = new GridPane();
        grid.setAlignment(Pos.CENTER);
        grid.setHgap(10);
        grid.setVgap(10);
        grid.setPadding(new Insets(25, 25, 25, 25));

        Text scenetitle = new Text("Welcome");
        scenetitle.setFont(Font.font("Tahoma", FontWeight.NORMAL, 20));
        grid.add(scenetitle, 0, 0, 2, 1);

        Label userName = new Label("Nickname:");
        grid.add(userName, 0, 1);
        TextField userTextField = new TextField();
        grid.add(userTextField, 1, 1);

        Label serverIP = new Label("Server:");
        grid.add(serverIP, 0, 2);
        TextField serverIPInput = new TextField();
        grid.add(serverIPInput, 1, 2);

        Button startbtn = new Button("Start");
        HBox hbBtn = new HBox(10);
        hbBtn.setAlignment(Pos.BOTTOM_RIGHT);
        hbBtn.getChildren().add(startbtn);
        grid.add(hbBtn, 1, 4);
        startbtn.setOnAction(e -> controller.submitName(
                e,
                userTextField.getCharacters(),
                serverIPInput.getCharacters()
                )
        );

        login = new Scene(grid, windowWidth, windowHeight);

        return login;
    }

}
