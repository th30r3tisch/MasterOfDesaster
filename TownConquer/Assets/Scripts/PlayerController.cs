using UnityEngine;

public class PlayerController : MonoBehaviour {

    public LayerMask mask;
    public int lineWidth;
    public Material material;

    private Vector3 _lineStart = Vector3.one;
    private Vector3 _lineEnd;
    private bool _startCondition = false;
    private GameObject _startTown = null;

    private void Update() {
        CheckIfInteractionIsHappening();
        CheckIfInteractionIsAborted();
    }

    private void CheckIfInteractionIsHappening() {
        if (!Input.GetKey(KeyCode.LeftAlt)) {

            if (Input.GetMouseButtonDown(0)) {
                RaycastHit hitInfo = GetRayCastHitInfo();
                GameObject go = hitInfo.collider.gameObject;
                if (go.name.StartsWith("Town") &&
                    go.GetComponent<TownManager>().ownerid == Client.instance.myId) {
                    _lineStart = go.transform.position;
                    _startTown = go;
                    _startCondition = true;
                }
            }
            if (Input.GetMouseButtonUp(0) && _startCondition) {
                RaycastHit hitInfo = GetRayCastHitInfo();
                GameObject go = hitInfo.collider.gameObject;
                if (go.name.StartsWith("Town") &&
                    go.GetInstanceID() != _startTown.GetInstanceID() &&
                    !go.GetComponent<TownManager>().town.outgoing.Contains(_startTown.GetComponent<TownManager>().town)) {
                    _lineEnd = go.transform.position;
                    ClientSend.InteractionRequest(_lineStart, _lineEnd);
                }
            }
        }
    }

    private void CheckIfInteractionIsAborted() {
        if (!Input.GetKey(KeyCode.LeftAlt)) {
            if (Input.GetMouseButtonUp(1)) {
                RaycastHit hitInfo = GetRayCastHitInfo();
                GameObject go = hitInfo.collider.gameObject;
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
        RaycastHit hitInfo;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Physics.Raycast(ray, out hitInfo, Mathf.Infinity, mask);
        return hitInfo;
    }
}
