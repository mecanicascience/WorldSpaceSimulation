using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class Planet : Body {
    [Header("Planet Configuration")]
    public int chunkDensity = 5;
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
        int.MaxValue,
        int.MaxValue,
        int.MaxValue,
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

    public double meshDistanceColliderThreshold = 50;
    public float updateChunkRate = 0.5f;


    /** Local pointing up vectors for spheres */
    private Vector3[] localUp = {
        Vector3.up, Quaternion.FromToRotation(Vector3.up, Vector3.down).eulerAngles / 90f,
        Vector3.right, Vector3.left,
        Vector3.forward, Vector3.back
    };

    [HideInInspector]
    public PlanetChunk[] planetChunks;

    [HideInInspector]
    public MeshFilter[] planetMeshFilters;

    [HideInInspector]
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



    new protected void Start() {
        // Initialize parent
        base.Start();

        // Set tags and layers (very important)
        this.gameObject.tag = "PlanetTag";
        this.gameObject.layer = LayerMask.NameToLayer("Planet");

        // Initialize mesh presets
        QuadTree.Presets.instanciate();

        // Initialize planet data
        this.terrainGenerator = new TerrainGenerator(noiseSettings, this);
        this.shading = new PlanetShading(this);

        // Initialization
        if (planetChunks == null || planetChunks.Length == 0)
            this.initialize();

        // Mesh generation
        this.generatePlanetMesh(false);
        StartCoroutine(this.updatePlanetMesh());

        this.colliderDeltaTime = 0;
    }

    new private void Update() {
        base.Update();

        // Execute actions in Queue
        this.planetQueue.ExecuteActionInQueue();

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
        this.generateUUID();
        this.terrainGenerator.initialize(this.getUUID());

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

    public Vector3d getLaunchPadPosition() {
        Vector3d sphereUnitPosition = (new Vector3d(Random.Range(0f, 10f), Random.Range(0f, 10f), Random.Range(0f, 10f))).normalized;
        return sphereUnitPosition * (this.size / 2f + this.getAltitudeAt(sphereUnitPosition) + 2f*10f);
    }



    public double getAltitudeAt(Vector3d spherePos) {
        return this.terrainGenerator.getAltitudeAt(spherePos);
    }

    public Color getColorAtAltitude(double altitude) {
        return this.terrainGenerator.getColorAtAltitude(altitude);
    }




    private void generatePlanetMesh(bool useThreads = false) {
        if (
               this.size != this.transform.localScale.x
            || this.size != this.transform.localScale.y
            || this.size != this.transform.localScale.z
        ) this.transform.localScale = new Vector3(this.size, this.size, this.size);

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
