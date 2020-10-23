using SharedLibrary;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TownManager : MonoBehaviour {
    public int id;
    public int ownerid;
    public string ownerName;
    public float life;
    public int supporter = 0;
    public List<UTown> attacker = new List<UTown>();

    private float elapsed;

    void Update() {
        GrowLife();
    }

    private void GrowLife() {
        elapsed += Time.deltaTime;
        if (elapsed >= Constants.TOWN_GROTH_SECONDS) {
            elapsed = 0;
            if (ownerid >= 0) {
                life += 1;
            }
            life += supporter - attacker.Count;
            if (life <= 0) {
                ConquerTown();
            }
        }
    }

    private void ConquerTown() {
        UTown _town = attacker.First();
        if (_town.player.id == Client.instance.myId) {
            ClientSend.ConquerRequest(gameObject.transform.position);
        }
    }

    private void OnMouseEnter() {
        GetComponent<Outline>().OutlineWidth = 3;
    }

    private void OnMouseExit() {
        GetComponent<Outline>().OutlineWidth = 0;
    }
}

