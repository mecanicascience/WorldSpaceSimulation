using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Universe : MonoBehaviour {
    [Header("Configuration")]
    public GameObject player;
    public Planet mainPlanet;

    private List<Body> bodyList;
    private bool instanciated;
    private PlayerControler playerControler;


    private void Awake() {
        this.instanciated = false;

        this.playerControler = this.player.GetComponent<PlayerControler>();
        this.playerControler.nearestBody = FindObjectsOfType<Body>()[0]; // temporary body assigned

        DontDestroyOnLoad(transform.gameObject);
        DontDestroyOnLoad(this.player.gameObject);
    }

    private void Start() {
        // Initialize Universe for the actual Scene
        this.initializeForScene();

        // Finished initialization
        this.instanciated = true;
    }

    private void Update() {
        // Switch scene if distance to current planet > threshold
        if (!SceneManager.GetSceneByName("UniverseScene").isLoaded) {
            if (Vector3d.Distance(this.playerControler.pos, this.playerControler.nearestBody.pos) > this.playerControler.nearestBody.size) {
                // TODO
            }
        }
    }



    public Body nearestBodyFrom(Vector3d point) {
        Body curMinBody = null;
        double curMinBodyDist = int.MaxValue;
        
        foreach (Body body in this.bodyList) {
            double dist = Vector3d.Distance(point, body.pos);
            if (dist < curMinBodyDist) {
                curMinBody = body;
                curMinBodyDist = dist;
            }
        }

        return curMinBody;
    }

    private void initializeForScene() {
        this.bodyList = new List<Body>();

        // Find every bodys
        this.bodyList.AddRange(FindObjectsOfType<Body>());
        foreach (Body body in this.bodyList) {
            body.setPlayer(this.player);
        }
    }


    public bool isInstanciated() {
        return this.instanciated;
    }

    public GameObject getPlayer() {
        return this.player;
    }

    enum CoordinatesType {
        PLAYER_TO_CENTER,
        PLANET_TO_CENTER
    }
}
