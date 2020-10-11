using SharedLibrary;
using UnityEngine;

public class TownManager : MonoBehaviour
{
    public int id;
    public int ownerid;
    public string ownerName;
    public float life;

    private float elapsed;

    void Update() {
        elapsed += Time.deltaTime;
        if (elapsed >= Constants.TOWN_GROTH_SECONDS) {
            elapsed = 0;
            life += 1;
        }
    }
}

