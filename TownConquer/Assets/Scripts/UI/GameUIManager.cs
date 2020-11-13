using SharedLibrary.Models;
using UnityEngine;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour {

    public Text playerName;
    public Text townNumber;

    private Player me;

    public void Init() {
        me = Client.instance.me;
        playerName.text = me.username;
        playerName.color = new Color(me.color.R, me.color.G, me.color.B, me.color.A);
    }

    void Update() {
        townNumber.text = me.towns.Count + " towns";
    }

    public void Close() {
        Application.Quit();
    }
}
