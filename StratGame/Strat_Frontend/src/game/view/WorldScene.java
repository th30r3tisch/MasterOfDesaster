package game.view;

import game.controller.WorldController;
import javafx.collections.ObservableList;
import javafx.geometry.Orientation;
import javafx.scene.*;
import javafx.scene.control.*;
import javafx.scene.image.Image;
import javafx.scene.image.ImageView;
import javafx.scene.layout.AnchorPane;
import javafx.scene.layout.BorderPane;
import javafx.scene.layout.StackPane;
import javafx.scene.layout.VBox;
import javafx.scene.paint.Color;
import javafx.scene.paint.Paint;
import javafx.scene.shape.Circle;
import javafx.scene.shape.Rectangle;
import javafx.scene.text.Text;

public class WorldScene extends Scene {

    private int gameMapHeight = 2000;
    private int gameMapWidth = 4000;
    private WorldController controller;
    private Image backgroundImage;

    public WorldScene(Parent parent) {
        super(parent);
    }

    public WorldScene(Parent parent, double v, double v1) {
        super(parent, v, v1);

    }

    public WorldScene(Parent parent, Paint paint) {
        super(parent, paint);
    }

    public WorldScene(Parent parent, double v, double v1, Paint paint) {
        super(parent, v, v1, paint);

    }

    public WorldScene(Parent parent, double v, double v1, boolean b) {
        super(parent, v, v1, b);
    }

    public WorldScene(Parent parent, double v, double v1, boolean b, SceneAntialiasing sceneAntialiasing) {
        super(parent, v, v1, b, sceneAntialiasing);
    }

    //infobox of current player status
    //todo get these number from server eventually
    private Text villageAmountText;
    private Text numSoldierText;
    private int numOfVillages = 0;
    private int numOfSoldiers = 0;
    private AnchorPane getInfoBox(){
        AnchorPane parent = new AnchorPane();
        VBox infoBox = new VBox();
        villageAmountText = new Text();
        //exampleText
        villageAmountText.setText("Villages: " + numOfVillages);

        numSoldierText = new Text();
        numSoldierText.setText("Number of Soldiers: " + numOfSoldiers);

        infoBox.getChildren().addAll(
                villageAmountText,
                numSoldierText
        );
        parent.getChildren().add(infoBox);

        return  parent;
    }

    //infobox of villages
    //should be called on click
    private Text villageHpText;
    private Text villageSoldierText;
    private int villageHp = 0;
    private int numOfVillageSoldiers = 0;
    private VBox detailInfoBox(){
        VBox infoBox = new VBox();

        villageHpText = new Text();
        villageHpText.setText("Village Hp: " + villageHp);

        villageSoldierText = new Text();
        villageSoldierText.setText("Number of Soldiers: " + numOfVillageSoldiers);

        infoBox.getChildren().addAll(
                villageHpText,
                villageSoldierText
        );
        return infoBox;
    }


    private ScrollPane getGameMap(){
        ScrollPane scrollPane = new ScrollPane();
        Text hitPointText = new Text();
        //exampleText
        hitPointText.setText("hitPoint: 10");

        Text numSoldierText = new Text();
        numSoldierText.setText("Number of Soldiers: 10");
        return scrollPane;
    }

    //todo maybe add eventhandlers
    private MenuBar getMenu(){
        Menu menu = new Menu("Menu");
        MenuItem menuItem1 = new MenuItem();
        menuItem1.setText("test");
        menu.getItems().add(menuItem1);
        MenuBar menuBar = new MenuBar();
        menuBar.getMenus().add(menu);

        return  menuBar;
    }


    public void update(){

    }
}
