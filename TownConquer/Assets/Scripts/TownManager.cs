using SharedLibrary;
using System.Linq;
using UnityEngine;

public class TownManager : MonoBehaviour {
    public int id;
    public int ownerid;
    public string ownerName;
    public float life;
    public UTown town;

    private float elapsed;
    private GameUIManager ui;

    private void Start() {
        ui = GameObject.Find("UI").GetComponentInChildren<GameUIManager>();
    }

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
            life += town.supporterTowns.Count - town.attackerTowns.Count - town.outgoing.Count;
            if (life < 0) {
                life = 0;
                if (town.attackerTowns.Count > 0) {
                    ConquerTownRequest();
                }
                else {
                    RequestRetreatOfAllTroops();
                }
            }
        }
    }

    public void RequestRetreatOfAllTroops() {
        foreach (GameObject _target in town.outgoingActions) {
            ClientSend.RetreatRequest(
                gameObject.transform.position, 
                ConversionManager.ToUnityVector(_target.GetComponent<AttackManager>().end.position));
        }
    }

    public void RetreatTroopsFromTown(UTown _targetTown) {
        GameObject _outgoing = null;
        int i = 0;
        while (i < town.outgoingActions.Count && _outgoing == null) {
            if (town.outgoingActions[i].GetComponent<AttackManager>().end.position == _targetTown.position) {
                _outgoing = town.outgoingActions[i];
            }
            i++;
        }
        town.outgoingActions.Remove(_outgoing);
        DestroyImmediate(_outgoing);
    }


    private void ConquerTownRequest() {
        if (town.attackerTowns.First().player.id == Client.instance.myId) {
            ClientSend.ConquerRequest(gameObject.transform.position);
        }
    }

    private void OnMouseEnter() {
        GetComponent<Outline>().OutlineWidth = 3;
        ui.DisplayTownInfo(ownerName, life, town.creationTime);
    }

    private void OnMouseExit() {
        GetComponent<Outline>().OutlineWidth = 0;
    }
}

