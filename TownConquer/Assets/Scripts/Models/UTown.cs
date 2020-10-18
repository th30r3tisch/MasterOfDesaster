
using SharedLibrary.Models;
using System.Collections.Generic;
using UnityEngine;

public class UTown : Town {

    public GameObject go;
    public List<GameObject> incommingAttacks = new List<GameObject>();

    public UTown(System.Numerics.Vector3 _pos): base(_pos) { }

    public GameObject GetAttackGameObject(Vector3 _startPos) {
        foreach (var _atk in incommingAttacks) {
            if (_atk.GetComponent<AttackManager>().start == _startPos) {
                return _atk;
            }
        }
        return null;
    }
}