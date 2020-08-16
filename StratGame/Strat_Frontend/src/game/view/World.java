package game.view;
import game.controller.WorldController;
import game.model.Player;
import game.model.map.Town;
import javafx.application.Platform;
import javafx.geometry.Insets;
import javafx.geometry.Orientation;
import javafx.scene.Node;
import javafx.scene.Scene;
import javafx.scene.control.*;
import javafx.scene.control.Menu;
import javafx.scene.control.MenuBar;
import javafx.scene.control.MenuItem;
import javafx.scene.control.ScrollPane;
import javafx.scene.image.Image;
import javafx.scene.image.ImageView;
import javafx.scene.layout.BorderPane;

import java.util.logging.Logger;
import javafx.scene.layout.*;
import javafx.scene.paint.Color;
import javafx.scene.text.Text;

public class World implements ViewInterface {

    private Scene world;
    private WorldController controller;
    private int windowWidth = 1200;
    private int windowHeight = 800;
    private int gameMapHeight = 2000;
    private int gameMapWidth = 4000;
    private Image backgroundImage;
    private static Logger LOGGER = Logger.getLogger("InfoLogging");

    public World(WorldController wc) {
        this.controller = wc;
        backgroundImage = new Image("assets/map.jpg");
    }

    @Override
    public Scene getScene() {

        controller.getObjects();

        BorderPane borderPane = new BorderPane();
        borderPane.setTop(getMenu());

        ImageView imageView = new ImageView(backgroundImage);
        imageView.setFitHeight(gameMapHeight);
        imageView.setFitWidth(gameMapWidth);

        SplitPane splitPane = new SplitPane();
        splitPane.setOrientation(Orientation.HORIZONTAL);
        Platform.runLater(()->{splitPane.setDividerPositions(0.1f);});
        splitPane.getItems().add(getInfoBox());

        for (Node n : controller.group.getChildren()){

        }
        controller.stack.getChildren().setAll(
                imageView,
                controller.group
        );

        // wrap the scene contents in a pannable scroll pane.
        ScrollPane scroll = controller.createScrollPane(controller.stack);
        splitPane.getItems().add(scroll);

        AnchorPane rootAnchor = new AnchorPane();
        VBox vBox = new VBox();
        vBox.getChildren().addAll(
                borderPane,
                splitPane
        );

        rootAnchor.getChildren().addAll(
                vBox
        );

        world = new Scene(rootAnchor, windowWidth, windowHeight);

        // bind the preferred size of the scroll area to the size of the scene.
        scroll.prefWidthProperty().bind(world.widthProperty());
        scroll.prefHeightProperty().bind(world.heightProperty());

        // center the scroll contents.
        scroll.setHvalue(scroll.getHmin() + (scroll.getHmax() - scroll.getHmin()) / 2);
        scroll.setVvalue(scroll.getVmin() + (scroll.getVmax() - scroll.getVmin()) / 2);

        return world;
    }

    //infobox of current player status
    //todo get these number from server eventually
    private Text townAmountText;
    private Text numSoldierText;
    private Text ownerText;
    private int numOfTowns = 0;
    private int numOfSoldiers = 0;
    private Text playerName;
    private AnchorPane getInfoBox(){
        AnchorPane parent = new AnchorPane();
        VBox infoBox = new VBox();
        playerName = new Text();
        townAmountText = new Text();
        //exampleText
        townAmountText.setText("Your Towns: " + numOfTowns);

        numSoldierText = new Text();
        ownerText = new Text();

        VBox detailBox = new VBox();
        //not working yet
        detailBox.setStyle("-fx-background-color: #FFFAAAA;");
        detailBox.setBackground(new Background(new BackgroundFill(Color.RED,CornerRadii.EMPTY, Insets.EMPTY)));
        infoBox.setPadding(new Insets(5, 0, 0, 5));
        infoBox.setMinWidth(150);
        infoBox.getChildren().addAll(
                playerName,
                townAmountText,
                ownerText,
                numSoldierText
        );


        parent.getChildren().add(infoBox);

        return  parent;
    }

    public MenuBar getMenu(){
        Menu menu = new Menu("Menu");
        MenuItem menuItem1 = new MenuItem();
        menuItem1.setText("test");
        menu.getItems().add(menuItem1);
        MenuBar menuBar = new MenuBar();
        menuBar.getMenus().add(menu);
        return  menuBar;
    }

    public void updatePlayerStat(Player player){
        playerName.setText(player.getName());
        playerName.setFill(player.getFXColor());
        townAmountText.setText("Your Towns: " +   player.getOwnedTowns().size());
    }

    public void updateTownDisplay(Town town){
        if (town == null) {
            ownerText.setText("");
            numSoldierText.setText("");
            return;
        }
        String text = "Not conquered";
        if(town.getOwner() != null){
            text = town.getOwner().getName();
        }
        ownerText.setText("Owner: " +  text);
        numSoldierText.setText("Town health: " + town.getLife());
    }

    public void update(){

    }
}
