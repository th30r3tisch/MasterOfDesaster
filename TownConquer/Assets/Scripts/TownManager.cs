using SharedLibrary;
using UnityEngine;

public class TownManager : MonoBehaviour {
    public int id;
    public int ownerid;
    public string ownerName;
    public float life;
    public UTown town;

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
            life += town.GetSupportCount() - town.GetAttackCount() - town.outgoing.Count;
            if (life < 0) {
                life = 0;
                if (town.GetAttackCount() > 0) {
                    ConquerTownRequest();
                }
                else {
                    RequestRetreatOfAllTroops();
                }
            }
        }
    }

    public void RequestRetreatOfAllTroops() {
        foreach (GameObject _target in town.outgoing) {
            ClientSend.RetreatRequest(gameObject.transform.position, _target.GetComponent<AttackManager>().end);
        }
    }

    public void RetreatTroopsFromTown(UTown _targetTown) {
        GameObject _outgoing = null;
        int i = 0;
        while (i < town.outgoing.Count && _outgoing == null) {
            if (town.outgoing[i].GetComponent<AttackManager>().end == ConversionManager.ToUnityVector(_targetTown.position)) {
                _outgoing = town.outgoing[i];
            }
            i++;
        }
        town.outgoing.Remove(_outgoing);
        _targetTown.go.GetComponent<TownManager>().RemoveIncomingRef(town);
        DestroyImmediate(_outgoing);
    }

    public void RemoveIncomingRef(UTown _originTown) {
        GameObject _incoming = null;
        int i = 0;
        while (i < town.incoming.Count && _incoming == null) {
            if (town.incoming[i].GetComponent<AttackManager>().start == ConversionManager.ToUnityVector(_originTown.position)) {
                _incoming = town.incoming[i];
            }
            i++;
        }
        town.incoming.Remove(_incoming);
    }


    private void ConquerTownRequest() {
        if (town.GetFirstAttackOwnerID() == Client.instance.myId) {
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

