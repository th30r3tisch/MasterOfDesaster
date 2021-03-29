using SharedLibrary;
using System.Linq;
using UnityEngine;

public class TownManager : MonoBehaviour {
    public int id;
    public int ownerid;
    public string ownerName;
    public double life;
    public UTown town;

    private float _elapsed;
    private GameUIManager _ui;

    public System.Diagnostics.Stopwatch sw;

    private void Start() {
        _ui = GameObject.Find("UI").GetComponentInChildren<GameUIManager>();
        sw = System.Diagnostics.Stopwatch.StartNew();
    }

    void Update() {
        GrowLife();
    }

    private void GrowLife() {
        town.CalculateLife(sw.ElapsedMilliseconds);
        life = town.life;

        if (life < 0) {
            life = 0;
            if (town.incomingAttackerTowns.Count > 0) {
                ConquerTownRequest();
            }
            else {
                RequestRetreatOfAllTroops();
            }
        }
    }

    public void RequestRetreatOfAllTroops() {
        foreach (GameObject target in town.outgoingActions) {
            ClientSend.RetreatRequest(
                gameObject.transform.position,
                ConversionManager.ToUnityVector(target.GetComponent<AttackManager>().end.position));
        }
    }

    public void RetreatTroopsFromTown(UTown targetTown) {
        GameObject outgoing = null;
        int i = 0;
        while (i < town.outgoingActions.Count && outgoing == null) {
            if (town.outgoingActions[i].GetComponent<AttackManager>().end.position == targetTown.position) {
                outgoing = town.outgoingActions[i];
            }
            i++;
        }
        town.outgoingActions.Remove(outgoing);
        DestroyImmediate(outgoing);
    }


    private void ConquerTownRequest() {
        if (town.incomingAttackerTowns.First().owner.id == Client.instance.myId) {
            ClientSend.ConquerRequest(gameObject.transform.position);
        }
    }

    private void OnMouseEnter() {
        GetComponent<Outline>().OutlineWidth = 3;
        _ui.DisplayTownInfo(ownerName, life, town.livingTime);
    }

    private void OnMouseExit() {
        GetComponent<Outline>().OutlineWidth = 0;
    }
}

