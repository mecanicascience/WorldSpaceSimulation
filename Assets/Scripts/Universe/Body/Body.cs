using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using sys = System;

public abstract class Body : MonoBehaviour {
	private int bodyUUID = -1;
    private Universe universe;

    protected GameObject player = null;

    public Vector3d pos;
    public Vector3d vel;
	

    [Header("Body Configuration")]
    public double mass = 88300;
    public Material surfaceMaterial;
	public bool useCustomSeed = false;
	public int customSeed = 27092001;


    [HideInInspector]
    public float size = 60000; // Diameter of the body

    [HideInInspector]
    public Vector3d lastPos;

    [HideInInspector]
    public Vector3d lastPlayerPos;
    


    protected void Start() {
        // Initialize UUID and find universe
		this.generateUUID();
        this.universe = FindObjectOfType<Universe>();
        this.player = this.universe.getPlayer();

		// Setup planet material
		this.transform.GetComponent<MeshRenderer>().sharedMaterial = surfaceMaterial;

        // Initialize body size
        this.size = this.transform.localScale.x;
        this.transform.localScale.Set(this.size, this.size, this.size);

		// Setup planet position
        this.pos = new Vector3d(this.transform.position);
        this.vel = Vector3d.zero;
	}

    protected void Update() {
        // Stores player datas
        this.lastPos = this.pos;
        this.lastPlayerPos = this.player.GetComponent<PlayerControler>().pos;

        // Updates body size
        this.size = this.transform.localScale.x;
        this.transform.localScale.Set(this.size, this.size, this.size);
	}



	public void generateUUID() {
        if (!useCustomSeed)
            this.setUUID(this.generateNewUUID());
        else
            this.setUUID(this.customSeed);
	}

	public int getUUID() {
		return this.bodyUUID;
	}

	public void setUUID(int UUID) {
		this.bodyUUID = UUID;
	}

    private int generateNewUUID() {
        // Initialize Random seed generator with unique seed
        // Not perfect but will work a few dozen year (then need to change to long :) )
        Random.InitState((int)((sys.DateTime.Now.ToFileTime() - 132686698435331775) / 100000));

        // Generate new UUID
        this.bodyUUID = Random.Range(0, int.MaxValue);
        return this.bodyUUID;
    }

    public void setPlayer(GameObject player) {
        this.player = player;
    }
}
