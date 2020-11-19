using SharedLibrary.Models;
using System.Collections.Generic;
using UnityEngine;

public class UTown : Town {

    public GameObject go;
    public List<GameObject> outgoingActions = new List<GameObject>();

    public UTown(System.Numerics.Vector3 _pos): base(_pos) { }

}