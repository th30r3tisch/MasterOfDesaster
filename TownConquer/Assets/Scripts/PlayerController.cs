using SharedLibrary;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public LayerMask mask;
    public int lineWidth;
    public Material material;

    private Vector3 lineStart;
    private Vector3 lineEnd;
    

    void Update() {
        AttackTown();
    }


    /// <summary>
    /// drags the camera through the world
    /// </summary>
    /// <param name="_pos">current position of the camera</param>
    /// <returns>new position of the camera</returns>
    void AttackTown() {
        int? _startTownId = null;
        if (!Input.GetKey(KeyCode.LeftAlt)) {
            if (Input.GetMouseButtonDown(0)) {
                RaycastHit _hitInfo = GetRayCastHitInfo();
                if (_hitInfo.collider.gameObject.name.StartsWith("Town")) {
                    lineStart = _hitInfo.collider.gameObject.transform.position;
                    _startTownId = _hitInfo.collider.gameObject.GetInstanceID();
                }
            }
            if (Input.GetMouseButtonUp(0)) {
                RaycastHit _hitInfo = GetRayCastHitInfo();
                if (_hitInfo.collider.gameObject.name.StartsWith("Town") &&
                    _hitInfo.collider.gameObject.GetInstanceID() != _startTownId) {
                    lineEnd = _hitInfo.collider.gameObject.transform.position;
                    CreateLineMesh(_hitInfo);
                }
            }
        }
    }

    private void CreateLineMesh(RaycastHit _hitInfo) {
        GameObject _atkLine = new GameObject();
        MeshRenderer _meshRenderer = _atkLine.AddComponent<MeshRenderer>();
        _meshRenderer.sharedMaterial = new Material(Shader.Find("Standard"));
        MeshFilter _meshFilter = _atkLine.AddComponent<MeshFilter>();
        Mesh _mesh = new Mesh();

        Vector3[] _vertices = new Vector3[4]
            {
                            new Vector3(lineStart.x - Constants.ATTACK_LINE_WIDTH, lineStart.y, lineStart.z),
                            new Vector3(lineStart.x + Constants.ATTACK_LINE_WIDTH, lineStart.y, lineStart.z),
                            new Vector3(lineEnd.x - Constants.ATTACK_LINE_WIDTH, lineEnd.y, lineEnd.z),
                            new Vector3(lineEnd.x + Constants.ATTACK_LINE_WIDTH, lineEnd.y, lineEnd.z)
            };
        int[] _tris = new int[6]
            {
                // lower left triangle
                0, 2, 1,
                // upper right triangle
                2, 3, 1
            };
        Vector3[] normals = new Vector3[4]
            {
                -Vector3.forward,
                -Vector3.forward,
                -Vector3.forward,
                -Vector3.forward
            };
        Vector2[] uv = new Vector2[4]
        {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1)
        };

        _mesh.vertices = _vertices;
        _mesh.triangles = _tris;
        _mesh.normals = normals;
        _mesh.uv = uv;
        _meshFilter.mesh = _mesh;
        _atkLine.name = "atk";
        _atkLine.transform.parent = _hitInfo.collider.gameObject.transform;
    }

    private RaycastHit GetRayCastHitInfo() {
        RaycastHit _hitInfo;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Physics.Raycast(ray, out _hitInfo, Mathf.Infinity, mask);
        return _hitInfo;
    }
}
