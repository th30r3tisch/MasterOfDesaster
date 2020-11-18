using SharedLibrary;
using SharedLibrary.Models;
using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

    public static GameManager instance;

    public GameObject townPrefab;
    public GameObject landPrefab;
    public GameObject obstaclePrefab;
    public Canvas ui;

    private static QuadTree world;
    private static Player game;
    private Quaternion horizontalOrientation = new Quaternion(0, 0, 0, 0);
    private static System.Random r;

    private void Awake() {
        if (instance == null) {
            instance = this;
        }
        else if (instance != this) {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }

    public void AddEnemies(Player _enemy, List<Vector3> _towns) {
        foreach (Vector3 _townPos in _towns) {
            UTown _town = (UTown)world.GetTown(ConversionManager.ToNumericVector( _townPos));
            if (_town == null) {
                CreateTown(Constants.TOWN_NUMBER + Client.instance.enemies.Count + 1, _townPos, _enemy);
            }
            else {
                TownManager _tm = _town.go.GetComponent<TownManager>();
                _enemy.addTown(_town);
                _town.player = _enemy;
                _tm.ownerid = _enemy.id;
                _tm.ownerName = _enemy.username;
                _town.go.GetComponent<Renderer>().material.color = ConversionManager.DrawingToColor32(_enemy.color);
            }
        }
        Client.instance.enemies.Add(_enemy);
    }

    public void InitMap(int _seed, Vector3 _townPos, Player _player, DateTime _creationTime) {
        GameObject _ground;
        r = new System.Random(_seed);

        world = new QuadTree(1, new TreeBoundry(0, 0, Constants.MAP_WIDTH, Constants.MAP_HEIGHT));
        game = new Player(-1, "game", System.Drawing.Color.FromArgb(100, 100, 100), _creationTime);
        _ground = Instantiate(landPrefab, new Vector3(Constants.MAP_WIDTH / 2, 0, Constants.MAP_HEIGHT / 2), horizontalOrientation);
        _ground.transform.localScale = new Vector3(Constants.MAP_WIDTH, 1, Constants.MAP_HEIGHT);

        CreateObstacles();
        CreateTowns();

        CreateTown(Constants.TOWN_NUMBER, _townPos, _player);
        ui.GetComponent<GameUIManager>().Init();
    }

    private void CreateTowns() {
        for (int _i = 0; _i < Constants.TOWN_NUMBER; _i++) {
            SearchTownPos(_i);
        }
    }

    private void SearchTownPos(int _i) {
        bool flag = false;
        while (flag == false) {
            int _x = RandomNumber(Constants.DISTANCE_TO_EDGES, Constants.MAP_WIDTH - Constants.DISTANCE_TO_EDGES);
            int _z = RandomNumber(Constants.DISTANCE_TO_EDGES, Constants.MAP_HEIGHT - Constants.DISTANCE_TO_EDGES);
            if (world.GetAllContentBetween(
                (_x - Constants.TOWN_MIN_DISTANCE),
                    (_z - Constants.OBSTACLE_MAX_LENGTH / 2), // divided by 2 because point is center of object
                    (_x + Constants.TOWN_MIN_DISTANCE),
                    (_z + Constants.OBSTACLE_MAX_LENGTH / 2)).Count == 0) { // check vertical objects
                if (world.GetAllContentBetween(
                    (_x - Constants.OBSTACLE_MAX_LENGTH / 2),
                    (_z - Constants.TOWN_MIN_DISTANCE),
                    (_x + Constants.OBSTACLE_MAX_LENGTH / 2),
                    (_z + Constants.TOWN_MIN_DISTANCE)).Count == 0) { // check horizontal objects
                    CreateTown(_i, new Vector3(_x, 5, _z), game);
                    flag = true;
                }
            }
        }
    }

    private void CreateTown(int _i, Vector3 _position, Player _owner) {
        GameObject _town;
        UTown _t;

        _t = new UTown(ConversionManager.ToNumericVector(_position)) {
            player = _owner
        };

        _town = Instantiate(townPrefab, _position, horizontalOrientation);
        _town.GetComponent<TownManager>().id = _i;
        _town.GetComponent<TownManager>().ownerName = _owner.username;
        _town.GetComponent<TownManager>().ownerid = _owner.id;
        _town.GetComponent<TownManager>().life = _t.life;
        _town.GetComponent<TownManager>().town = _t;
        _town.GetComponent<Renderer>().material.color = ConversionManager.DrawingToColor32(_owner.color);

        _t.go = _town;
        _t.creationTime = _owner.creationTime;
        _owner.addTown(_t);
        world.Insert(_t);
    }

    private void CreateObstacles() {
        GameObject _obstacle;
        Obstacle _o;
        for (int i = 0; i < Constants.OBSTACLE_NUMBER; i++) {
            Vector3 _position = new Vector3(
                        RandomNumber(Constants.DISTANCE_TO_EDGES, Constants.MAP_WIDTH - Constants.DISTANCE_TO_EDGES),
                        1,
                        RandomNumber(Constants.DISTANCE_TO_EDGES, Constants.MAP_HEIGHT - Constants.DISTANCE_TO_EDGES));
            int _orientation = RandomNumber(0, 1);
            int _length = RandomNumber(Constants.OBSTACLE_MIN_LENGTH, Constants.OBSTACLE_MAX_LENGTH);
            _o = new Obstacle(ConversionManager.ToNumericVector(_position), _orientation, _length);
            world.Insert(_o);
            _obstacle = Instantiate(obstaclePrefab, _position, horizontalOrientation);
            _obstacle.transform.localScale = new Vector3(_o.width, 8, _o.length);
        }
    }

    public void AttackTown(Vector3 _lineStart, Vector3 _lineEnd) {
        UTown _deffT = (UTown)world.GetTown(ConversionManager.ToNumericVector(_lineEnd));
        UTown _atkT = (UTown)world.GetTown(ConversionManager.ToNumericVector(_lineStart));
        GameObject _line;

        if (_deffT.player.id == _atkT.player.id) {
            _line = CreateLineMesh(_atkT.player.id, _atkT, _deffT, "sup");
            _deffT.incoming.Add(_line);
        }
        else {
            _line = CreateLineMesh(_atkT.player.id, _atkT, _deffT, "atk");
            _deffT.incoming.Add(_line);
        }
        _atkT.outgoingGO.Add(_line);
        world.AddTownAtk(_deffT, _atkT);
    }

    public void RetreatTroops(Vector3 _lineStart, Vector3 _lineEnd) {
        UTown _atkT = (UTown)world.GetTown(ConversionManager.ToNumericVector(_lineStart));
        UTown _deffT = (UTown)world.GetTown(ConversionManager.ToNumericVector(_lineEnd));
        TownManager _atkTm = _atkT.go.GetComponent<TownManager>();
        _atkTm.RetreatTroopsFromTown(_deffT);

        world.RmTownAtk(_atkT, _deffT);
    }

    public void ConquerTown(int _conquererId, Vector3 _conqueredTownCoord) {
        UTown _conqueredT = (UTown)world.GetTown(ConversionManager.ToNumericVector(_conqueredTownCoord));
        Player _conquerer = GetPlayer(_conquererId);

        UpdateTownReferences(_conqueredT, _conquerer);
    }

    private Player GetPlayer(int _id) {
        foreach (Player _player in Client.instance.enemies) {
            if (_player.id == _id) {
                return _player;
            }
        }
        if (_id == Client.instance.myId) {
            return Client.instance.me;
        }
        return null;
    }

    private void UpdateTownReferences(UTown _conqueredT, Player _player) {
        TownManager _conqueredTm = _conqueredT.go.GetComponent<TownManager>();
        List<GameObject> _incoming = _conqueredT.incoming;
        List<GameObject> _outgoing = _conqueredT.outgoingGO;

        // removes all incoming troops and deletes references in both towns
        for (int i = _incoming.Count; i > 0; i--) {
            AttackManager atkM = _incoming[i-1].GetComponent<AttackManager>();
            UTown _atkT = (UTown)world.GetTown(atkM.start.position);
            _atkT.go.GetComponent<TownManager>().RetreatTroopsFromTown(_conqueredT);
            world.RmTownAtk(_atkT, _conqueredT);
        }

        // removes all outgoing troops and deletes references in both towns
        for (int i = _outgoing.Count; i > 0; i--) {
            AttackManager atkM = _outgoing[i-1].GetComponent<AttackManager>();
            UTown _deffT = (UTown)world.GetTown(atkM.end.position);
            _conqueredTm.RetreatTroopsFromTown(_deffT);
            world.RmTownAtk(_conqueredT, _deffT);
        }

        _conqueredTm.ownerid = _player.id;
        _conqueredTm.ownerName = _player.username;
        world.UpdateOwner(_player, _conqueredT);
        _conqueredT.go.GetComponent<Renderer>().material.color = ConversionManager.DrawingToColor32(_player.color);
    }

    /// <summary>
    /// Creates a mesh line that indicates an attack between towns.
    /// </summary>
    /// <param name="_ownerId">Player id who sent the attack</param>
    /// <param name="_startTown">Town from where the attack starts</param>
    /// <param name="_endTown">Town where the attack ends</param>
    /// <param name="_type">Type of the line (atk, deff)</param>
    /// <returns>The Gameobject of the mesh line</returns>
    private GameObject CreateLineMesh(int _ownerId, UTown _startTown, UTown _endTown, string _type) {
        GameObject _atkLine = new GameObject();
        MeshRenderer _meshRenderer = _atkLine.AddComponent<MeshRenderer>();
        MeshFilter _meshFilter = _atkLine.AddComponent<MeshFilter>();
        Mesh _mesh = new Mesh();
        Vector3 _lineStart = ConversionManager.ToUnityVector( _startTown.position);
        Vector3 _lineEnd = ConversionManager.ToUnityVector(_endTown.position);
        Vector3 _direction = _lineEnd - _lineStart;

        _meshRenderer.sharedMaterial = Resources.Load("Line", typeof(Material)) as Material;

        Vector3[] _vertices = new Vector3[4]{
            Vector3.Cross(_direction, Vector3.up).normalized * Constants.ATTACK_LINE_WIDTH + _lineStart,
            Vector3.Cross(_direction, Vector3.up).normalized * (-Constants.ATTACK_LINE_WIDTH) + _lineStart,
            Vector3.Cross(_direction, Vector3.up).normalized * Constants.ATTACK_LINE_WIDTH + _lineEnd,
            Vector3.Cross(_direction, Vector3.up).normalized * (-Constants.ATTACK_LINE_WIDTH) + _lineEnd
        };
        int[] _tris = new int[6]{
            // lower left triangle
            0, 2, 1,
            // upper right triangle
            2, 3, 1
        };
        Vector3[] normals = new Vector3[4]{
            -Vector3.forward,
            -Vector3.forward,
            -Vector3.forward,
            -Vector3.forward
        };
        Vector2[] uv = new Vector2[4]{
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
        _atkLine.AddComponent<AttackManager>();
        _atkLine.GetComponent<AttackManager>().ownerid = _ownerId;
        _atkLine.GetComponent<AttackManager>().start = _startTown;
        _atkLine.GetComponent<AttackManager>().end = _endTown;
        _atkLine.GetComponent<AttackManager>().type = _type;
        _atkLine.AddComponent<MeshCollider>();

        return _atkLine;
    }

    /// <summary>
    /// Generates a random number within the bounds min/max.
    /// </summary>
    /// <param name="_min">min random number created</param>
    /// <param name="_max">max random number created</param>
    /// <returns>The created random number</returns>
    private static int RandomNumber(int _min, int _max) {
        return r.Next(_max - _min + 1) + _min;
    }
}
