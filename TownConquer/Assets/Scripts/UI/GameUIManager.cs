using SharedLibrary.Models;
using System;
using UnityEngine;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour {

    public Text playerName;
    public Text townNumber;
    //towninfo
    public Text owner;
    public Text life;
    public Text ownedSince;

    private Player _me;

    public void Init() {
        _me = Client.instance.me;
        playerName.text = _me.username;
        playerName.color = new Color(_me.color.R, _me.color.G, _me.color.B, _me.color.A);
    }

    void Update() {
        townNumber.text = _me.towns.Count + " towns";
    }

    public void Close() {
        Application.Quit();
    }

    public void DisplayTownInfo(string name, double life, long creation) {
        owner.text = name;
        this.life.text = life.ToString();
        ownedSince.text = creation.ToString("HH:mm");
    }
}
