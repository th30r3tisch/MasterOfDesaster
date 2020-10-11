using SharedLibrary;
using SharedLibrary.Models;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public static GameManager instance;
    public static Dictionary<int, TownManager> towns = new Dictionary<int, TownManager>();

    public GameObject townPrefab;
    public GameObject landPrefab;
    public GameObject obstaclePrefab;

    private static World world;
    private static Player game;
    private Quaternion horizontalOrientation = new Quaternion(0, 0, 0, 0);

    private void Awake() {
        if (instance == null) {
            instance = this;
        }
        else if (instance != this) {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    public void AddEnemies( Player _enemy, Vector3 _townPos) {
        CreateTown(towns.Count, _townPos, _enemy);
        Client.instance.enemies.Add(_enemy);
    }

    public void InitMap(int _seed, Vector3 _townPos, Player _player) {
        GameObject _ground;
        Random.InitState(_seed);

        world = new World(0, 0, Constants.MAP_WIDTH, Constants.MAP_HEIGHT);
        game = new Player(-1, "game", System.Drawing.Color.FromArgb(100,100,100));
        _ground = Instantiate(landPrefab, new Vector3(Constants.MAP_WIDTH/2, 0, Constants.MAP_HEIGHT/2), horizontalOrientation);
        _ground.transform.localScale = new Vector3(Constants.MAP_WIDTH/10, 1, Constants.MAP_HEIGHT/10);

        CreateObstacles();
        CreateTowns();

        CreateTown(towns.Count, _townPos, _player);
    }

    private void CreateTowns() {
        for (int _i = 0; _i < Constants.TOWN_NUMBER; _i++) {
            SearchTownPos(_i);
        }
    }

    private void SearchTownPos(int _i) {
        bool flag = false;
        while (flag == false) {
            int _x = Random.Range(Constants.DISTANCE_TO_EDGES, Constants.MAP_WIDTH - Constants.DISTANCE_TO_EDGES);
            int _z = Random.Range(Constants.DISTANCE_TO_EDGES, Constants.MAP_HEIGHT - Constants.DISTANCE_TO_EDGES);
            if (GetAreaContent(
                (_x - Constants.TOWN_MIN_DISTANCE),
                (_z - Constants.TOWN_MIN_DISTANCE),
                (_x + Constants.TOWN_MIN_DISTANCE),
                (_z + Constants.TOWN_MIN_DISTANCE)).Count == 0) { // check for overlapping towns
                if (GetAreaContent(
                    (_x - Constants.OBSTACLE_MAX_LENGTH),
                    (_z - Constants.OBSTACLE_MAX_LENGTH),
                    (_x + Constants.TOWN_MIN_DISTANCE),
                    (_z + Constants.TOWN_MIN_DISTANCE)).Count == 0) { // check for overlapping obstacles
                    CreateTown(_i, new Vector3(_x, 0, _z), game);
                    flag = true;
                }
            }
        }
    }

    private void CreateTown(int _i, Vector3 _position, Player owner) {
        GameObject _town;
        Town _t;

        _t = new Town(new System.Numerics.Vector3(_position.x, _position.y, _position.z));
        _t.player = owner;
        owner.addTown(_t);
        world.Insert(_t);

        _town = Instantiate(townPrefab, _position, horizontalOrientation);
        _town.GetComponent<TownManager>().id = _i;
        _town.GetComponent<TownManager>().ownerName = owner.username;
        _town.GetComponent<TownManager>().ownerid = owner.id;
        _town.GetComponent<TownManager>().life = _t.life;
        towns.Add(_i, _town.GetComponent<TownManager>());
    }

    private void CreateObstacles() {
        GameObject _obstacle;
        Obstacle _o;
        for (int i = 0; i < Constants.OBSTACLE_NUMBER; i++) {
            System.Numerics.Vector3 _position = new System.Numerics.Vector3(
                        Random.Range(Constants.DISTANCE_TO_EDGES, Constants.MAP_WIDTH - Constants.DISTANCE_TO_EDGES),
                        0,
                        Random.Range(Constants.DISTANCE_TO_EDGES, Constants.MAP_HEIGHT - Constants.DISTANCE_TO_EDGES));
            int _orientation = Random.Range(0, 2);
            int _length = Random.Range(Constants.OBSTACLE_MIN_LENGTH, Constants.OBSTACLE_MAX_LENGTH);
            _o = new Obstacle(_position, _orientation, _length);
            world.Insert(_o);
            _obstacle = Instantiate(obstaclePrefab, new Vector3(_position.X, _position.Y, _position.Z), transform.rotation * Quaternion.Euler(0f, _o.orientation, 0f));
            _obstacle.transform.localScale = new Vector3(_o.length, 8, _o.width);
        }
    }

    public bool IsIntersecting(List<TreeNode> _towns) {
        List<TreeNode> intersectionObjs = new List<TreeNode>();
        int t1x = (int)_towns[0].position.X;
        int t1z = (int)_towns[0].position.Z;
        int t2x = (int)_towns[1].position.X;
        int t2z = (int)_towns[1].position.Z;
        int startX = System.Math.Min(t1x, t2x);
        int startZ = System.Math.Min(t1z, t2z);
        int endX = System.Math.Max(t1x, t2x);
        int endZ = System.Math.Max(t1z, t2z);
        //rectangle between towns
        intersectionObjs.AddRange(GetAreaContent(startX, startZ, endX, endZ));
        //rectangle around town one
        intersectionObjs.AddRange(GetAreaContent(
            t1x - Constants.OBSTACLE_MAX_LENGTH,
            t1z - Constants.OBSTACLE_MAX_LENGTH,
            t1x + Constants.OBSTACLE_MAX_LENGTH,
            t1z + Constants.OBSTACLE_MAX_LENGTH));
        //rectangle around town two
        intersectionObjs.AddRange(GetAreaContent(
            t2x - Constants.OBSTACLE_MAX_LENGTH,
            t2z - Constants.OBSTACLE_MAX_LENGTH,
            t2x + Constants.OBSTACLE_MAX_LENGTH,
            t2z + Constants.OBSTACLE_MAX_LENGTH));
        if (intersectionObjs.Count != 0) {
            foreach (TreeNode _node in intersectionObjs) {
                if (_node.GetType() == typeof(Obstacle)) {
                    bool? _isIntersecting = LineSegmentsIntersection(
                        new Vector2(_towns[0].position.X, _towns[0].position.Z),
                        new Vector2(_towns[1].position.X, _towns[0].position.Z),
                        new Vector2(_node.position.X, _node.position.Z),
                        new Vector2(_node.position.X + ((Obstacle)_node).width, _node.position.Z + ((Obstacle)_node).length),
                        out Vector2 intersection);
                    if (_isIntersecting != null) return true;
                }
            }
        }
        return false;
    }

    private List<TreeNode> GetAreaContent(int _startX, int _startY, int _endX, int _endY) {
        return world.GetAreaContent(_startX, _startY, _endX, _endY);
    }

    // https://github.com/setchi/Unity-LineSegmentsIntersection/blob/master/Assets/LineSegmentIntersection/Scripts/Math2d.cs
    private bool LineSegmentsIntersection(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4, out Vector2 intersection) {
        intersection = Vector2.zero;

        var d = (p2.x - p1.x) * (p4.y - p3.y) - (p2.y - p1.y) * (p4.x - p3.x);

        if (d == 0.0f) {
            return false;
        }

        var u = ((p3.x - p1.x) * (p4.y - p3.y) - (p3.y - p1.y) * (p4.x - p3.x)) / d;
        var v = ((p3.x - p1.x) * (p2.y - p1.y) - (p3.y - p1.y) * (p2.x - p1.x)) / d;

        if (u < 0.0f || u > 1.0f || v < 0.0f || v > 1.0f) {
            return false;
        }

        intersection.x = p1.x + u * (p2.x - p1.x);
        intersection.y = p1.y + u * (p2.y - p1.y);

        return true;
    }
}
