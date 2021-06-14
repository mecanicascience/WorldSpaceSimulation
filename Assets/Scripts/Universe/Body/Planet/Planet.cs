using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;



public class Planet : MonoBehaviour {
    [Header("Planet Configuration")]
    public float planetSize = 60000;
    public int chunkDensity = 5;
    public double mass = 88300;
    public float terrainHeight = 1000;

    [Range(0, 0.5f)]
    public float waterLevel = 0.1f;
    public NoiseSettings[] noiseSettings;
    public Gradient terrainGradient;


    [Header("Level Of Detail")]
    public bool lodEnabled = true;
    public bool useThreads = true;
    public float maxRenderingAngle = 0.47f;

    public int[] lodDistances = new int[] {
        Int32.MaxValue,
        Int32.MaxValue,
        Int32.MaxValue,
        45000,
        20000,
        10000,
        5000,
        3000,
        1600,
        800,
        400,
        230,
        140,
        75,
        50,
        30
    };
    public int nonLODLevelOfDetail = 5;

    [Header("Advanced")]
    public GameObject player = null;
    public bool debug = false;
    public double meshDistanceColliderThreshold = 50;
    public float updateChunkRate = 0.5f;
    public Material surfaceMaterial;


    /** Local pointing up vectors for spheres */
    private Vector3[] localUp = {
        Vector3.up, Quaternion.FromToRotation(Vector3.up, Vector3.down).eulerAngles / 90f,
        Vector3.right, Vector3.left,
        Vector3.forward, Vector3.back
    };

    public Vector3d pos;
    public Vector3d vel;



    [HideInInspector]
    public Vector3d lastPos;
    [HideInInspector]
    public Vector3d lastPlayerPos;


    [SerializeField, HideInInspector]
    public PlanetChunk[] planetChunks;

    [SerializeField, HideInInspector]
    public MeshFilter[] planetMeshFilters;

    [SerializeField, HideInInspector]
    public MeshCollider[] planetMeshCollider;

    [HideInInspector]
    public GameObject[] planetChunksGO;

    [HideInInspector]
    public Material[] planetMaterials;



    /** Planet mesh update thread */
    public AwaitableQueue planetQueue = new AwaitableQueue();

    private float colliderDeltaTime;
    private TerrainGenerator terrainGenerator;
    private PlanetShading shading;



    private void Start() {
        // Initialize mesh presets
        QuadTree.Presets.instanciate();

        // Initialize planet data
        this.terrainGenerator = new TerrainGenerator(noiseSettings, this);
        this.pos = new Vector3d(this.transform.position);
        this.vel = Vector3d.zero;
        this.shading = new PlanetShading(this);

        // Initialization
        if (planetChunks == null || planetChunks.Length == 0)
            this.initialize();

        // Mesh generation
        this.generatePlanetMesh(false);
        StartCoroutine(this.updatePlanetMesh());

        this.colliderDeltaTime = 0;
    }

    private void Update() {
        // Execute actions in Queue
        this.planetQueue.ExecuteActionInQueue();

        // Stores player datas
        this.lastPos = this.pos;
        this.lastPlayerPos = this.player.GetComponent<PlayerControler>().pos;

        // Update collider every second 
        if (this.colliderDeltaTime > 1) {
            for (int i = 0; i < this.planetMeshCollider.Length; i++) {
                Mesh tmpMesh = planetMeshCollider[i].sharedMesh;
                planetMeshCollider[i].sharedMesh = null;
                planetMeshCollider[i].sharedMesh = tmpMesh;
            }
        }
        this.colliderDeltaTime += Time.deltaTime;

        // Update material vals
        this.updateMaterialDatas();
    }


    public void initialize() {
        // Generate terrain
        this.terrainGenerator.initialize();

        // Clear Planet Current Mesh
        this.gameObject.GetComponent<MeshRenderer>().enabled = false;

        if (planetChunksGO == null || planetChunksGO.Length == 0)
            planetChunksGO = new GameObject[6];
        if (planetMeshFilters == null || planetMeshFilters.Length == 0)
            planetMeshFilters = new MeshFilter[6];
        if (planetMeshCollider == null || planetMeshCollider.Length == 0)
            planetMeshCollider = new MeshCollider[6];
        if (planetMaterials == null || planetMaterials.Length == 0)
            planetMaterials = new Material[6]; 
        planetChunks = new PlanetChunk[6];

        // Get planet mesh filters from existing planet
        for (int i = 0; i < transform.childCount; i++) {
            planetMeshFilters[i] = transform.GetChild(i).GetComponent<MeshFilter>();
            planetMeshCollider[i] = transform.GetChild(i).GetComponent<MeshCollider>();
        }

        for (int i = 0; i < 6; i++) {
            // Create new filters
            if (planetMeshFilters[i] == null || planetMeshCollider[i] == null) {
                GameObject meshGO = new GameObject("Planet Chunk " + i);
                meshGO.transform.parent = transform;
                meshGO.transform.position = transform.position;
                meshGO.tag = "PlanetTag";
                meshGO.layer = LayerMask.NameToLayer("Planet");
                planetChunksGO[i] = meshGO;

                Material mat = new Material(this.surfaceMaterial);
                planetMaterials[i] = mat;
                meshGO.AddComponent<MeshRenderer>().sharedMaterial = mat;

                // MeshFilter
                Mesh mesh = new Mesh();
                planetMeshFilters[i] = meshGO.AddComponent<MeshFilter>();
                planetMeshFilters[i].sharedMesh = mesh;
                planetMeshFilters[i].sharedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; // Allow more vertices

                // MeshCollider
                Mesh meshCollider = new Mesh();
                planetMeshCollider[i] = meshGO.AddComponent<MeshCollider>();
                planetMeshCollider[i].sharedMesh = meshCollider;
                planetMeshCollider[i].sharedMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; // Allow more vertices
            }

            // Update old filters
            else {
                if (planetMeshFilters[i].GetComponent<MeshRenderer>() == null)
                    planetMeshFilters[i].gameObject.AddComponent<MeshRenderer>();

                Material mat = new Material(this.surfaceMaterial);
                planetMaterials[i] = mat;
                planetMeshFilters[i].GetComponent<MeshRenderer>().sharedMaterial = mat;
                planetMeshFilters[i].gameObject.layer = LayerMask.NameToLayer("Planet");
            }

            // Construct Class Chunks
            planetChunks[i] = new PlanetChunk(
                this, planetMeshFilters[i].sharedMesh,
                planetMeshCollider[i].sharedMesh, localUp[i],
                planetChunksGO[i].GetComponent<MeshRenderer>().sharedMaterial
            );
        }
    }

    public void updateMaterialDatas() {
        for (int i = 0; i < this.planetChunks.Length; i++) {
            this.shading.updateTerrainDatas(planetMaterials[i], terrainGenerator);
        }
    }



    public double getAltitudeAt(Vector3d spherePos) {
        return this.terrainGenerator.getAltitudeAt(spherePos);
    }

    public Color getColorAtAltitude(double altitude) {
        return this.terrainGenerator.getColorAtAltitude(altitude);
    }




    private void generatePlanetMesh(bool useThreads = false) {
        if (
               this.planetSize != this.transform.localScale.x
            || this.planetSize != this.transform.localScale.y
            || this.planetSize != this.transform.localScale.z
        ) this.transform.localScale = new Vector3(this.planetSize, this.planetSize, this.planetSize);

        foreach (PlanetChunk ch in planetChunks) {
            if (useThreads) {
                Thread t = new Thread(() => {
                    ch.generateChunk(useThreads);
                });
                t.Start();
            }
            else {
                ch.generateChunk(useThreads);
            }
        }
    }


    private IEnumerator updatePlanetMesh() {
        while (true) {
            yield return new WaitForSeconds(updateChunkRate);
            this.generatePlanetMesh(this.useThreads);
        }
    }
}
