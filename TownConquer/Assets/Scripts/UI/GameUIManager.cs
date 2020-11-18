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

    public void DisplayTownInfo(string _name, float _life, DateTime creation) {
        owner.text = _name;
        life.text = _life.ToString();
        ownedSince.text = creation.ToString("HH:mm");
    }
}
