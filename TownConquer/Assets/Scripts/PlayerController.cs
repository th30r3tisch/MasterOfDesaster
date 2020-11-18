using System;
using System.Linq;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public LayerMask mask;
    public int lineWidth;
    public Material material;

    DateTime _timeOne = DateTime.Now;
    DateTime _timeTwo;
    Vector3 _lineStart = Vector3.one;
    Vector3 _lineEnd;
    bool _startCondition = false;
    GameObject _startTown = null;


    void Update() {
        CheckIfAttackIsHappening();
        CheckIfAttackIsAborted();
    }

    private void CheckIfAttackIsHappening() {
        if (!Input.GetKey(KeyCode.LeftAlt)) {

            if (Input.GetMouseButtonDown(0)) {
                RaycastHit _hitInfo = GetRayCastHitInfo();
                GameObject _go = _hitInfo.collider.gameObject;
                if (_go.name.StartsWith("Town") &&
                    _go.GetComponent<TownManager>().ownerid == Client.instance.myId) {
                    _lineStart = _go.transform.position;
                    _startTown = _go;
                    _startCondition = true;
                }
            }
            if (Input.GetMouseButtonUp(0) && _startCondition) {
                RaycastHit _hitInfo = GetRayCastHitInfo();
                GameObject _go = _hitInfo.collider.gameObject;
                if (_go.name.StartsWith("Town") &&
                    _go.GetInstanceID() != _startTown.GetInstanceID() &&
                    !_go.GetComponent<TownManager>().town.attackerTowns.Contains(_startTown.GetComponent<TownManager>().town) &&
                    !_go.GetComponent<TownManager>().town.supporterTowns.Contains(_startTown.GetComponent<TownManager>().town)) {
                    _lineEnd = _go.transform.position;
                    ClientSend.AttackRequest(_lineStart, _lineEnd);
                }
            }
        }
    }

    private void CheckIfAttackIsAborted() {
        if (!Input.GetKey(KeyCode.LeftAlt)) {
            if (Input.GetMouseButtonUp(1)) {
                RaycastHit _hitInfo = GetRayCastHitInfo();
                GameObject go = _hitInfo.collider.gameObject;
                AttackManager atm = go.GetComponent<AttackManager>();
                if (go.name.StartsWith("at") && atm.ownerid == Client.instance.myId) {
                    ClientSend.RetreatRequest(
                        ConversionManager.ToUnityVector(atm.start.position),
                        ConversionManager.ToUnityVector(atm.end.position));
                }
            }
        }
    }

    private RaycastHit GetRayCastHitInfo() {
        RaycastHit _hitInfo;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Physics.Raycast(ray, out _hitInfo, Mathf.Infinity, mask);
        return _hitInfo;
    }
}
