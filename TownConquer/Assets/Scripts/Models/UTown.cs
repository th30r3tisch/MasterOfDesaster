using SharedLibrary.Models;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class UTown : Town {

    public GameObject go;
    public List<GameObject> incoming = new List<GameObject>();
    public List<GameObject> outgoingGO = new List<GameObject>();

    public UTown(System.Numerics.Vector3 _pos): base(_pos) { }

    /// <summary>
    /// counts the incoming supports
    /// </summary>
    /// <returns>returns the number of supports incoming at this town</returns>
    public int GetSupportCount() {
        int _supCount = 0;
        foreach (GameObject _incoming in incoming) {
            string _incomingType = _incoming.GetComponent<AttackManager>().type;
            if (_incomingType.Equals("sup")) {
                _supCount++;
            }
        }
        return _supCount;
    }

    /// <summary>
    /// counts the incoming attacks
    /// </summary>
    /// <returns>returns the number of attacks incoming at this town</returns>
    public int GetAttackCount() {
        int _atkCount = 0;
        foreach (GameObject _incoming in incoming) {
            string _incomingType = _incoming.GetComponent<AttackManager>().type;
            if (_incomingType.Equals("atk")) {
                _atkCount++;
            }
        }
        return _atkCount;
    }

    public int GetFirstAttackOwnerID() {
        return incoming.First().GetComponent<AttackManager>().ownerid;
    }

}