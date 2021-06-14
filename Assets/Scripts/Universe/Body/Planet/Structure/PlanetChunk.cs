using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetChunk {
    public Planet planet;
	public Mesh mesh;
	public Mesh meshCollider;
    public int maxCurrentDepth;

	
	private Vector3 localUp;
    private Material material;


	// Chunk datas
    public List<Vector3> vertices = new List<Vector3>();
    public List<int> triangles = new List<int>();
    public List<Vector3> normals = new List<Vector3>();
    public List<Color> colors = new List<Color>();

    // Collider datas
    public List<Vector3> verticesCollider = new List<Vector3>();
    public List<int> trianglesCollider = new List<int>();
    public List<Vector3> normalsCollider = new List<Vector3>();

    public QuadTree.Chunk mainChunk;


    public PlanetChunk(Planet planet, Mesh mesh, Mesh meshCollider, Vector3 localUp, Material material) {
		this.localUp = localUp;
        this.planet = planet;
        this.mesh = mesh;
		this.meshCollider = meshCollider;
        this.material = material;
        this.maxCurrentDepth = 0;
        this.mainChunk = new QuadTree.Chunk(this, null, new QuadTree.BoundingBox(0, 0, 1), 0, localUp, 0b0);
	}

	public void generateChunk(bool useThreads = false) {
		// Reset Mesh data
		this.vertices .Clear();
        this.normals  .Clear();
		this.triangles.Clear();
        this.colors   .Clear();
        this.verticesCollider.Clear();
        this.normalsCollider.Clear();
        this.trianglesCollider.Clear();

        // Generate Chunks
        this.mainChunk = new QuadTree.Chunk(this, null, new QuadTree.BoundingBox(0, 0, this.planet.planetSize / 2f), 0, localUp, 0b1);
        this.mainChunk.subdivide();
        this.maxCurrentDepth = this.mainChunk.maxCurrentDepth;

		// Update Mesh based on chunks subdivision
		(QuadTree.Chunk[], QuadTree.Chunk[]) children = this.mainChunk.getVisibleChildren();

        // Mesh
        int triangleOffset = 0;
        for (int i = 0; i < children.Item1.Length; i++) {
			(Vector3[], Vector3[], int[], Color[]) verticesAndTriangles = children.Item1[i].calculateVerticesAndTriangles(triangleOffset);
			vertices .AddRange(verticesAndTriangles.Item1);
            normals  .AddRange(verticesAndTriangles.Item2);
			triangles.AddRange(verticesAndTriangles.Item3);
            colors   .AddRange(verticesAndTriangles.Item4);
            triangleOffset += verticesAndTriangles.Item1.Length;
		}


        // Collider
        triangleOffset = 0;
        for (int i = 0; i < children.Item2.Length; i++) {
            (Vector3[], Vector3[], int[], Color[]) verticesAndTriangles = children.Item2[i].calculateVerticesAndTriangles(triangleOffset);
            verticesCollider .AddRange(verticesAndTriangles.Item1);
            normalsCollider  .AddRange(verticesAndTriangles.Item2);
            trianglesCollider.AddRange(verticesAndTriangles.Item3);
            triangleOffset += verticesAndTriangles.Item1.Length;
		}


		if (useThreads) {
			this.planet.planetQueue.Enqueue(() => {
                updateMesh(mesh, vertices.ToArray(), normals.ToArray(), triangles.ToArray(), colors.ToArray());
                updateMesh(meshCollider, verticesCollider.ToArray(), normalsCollider.ToArray(), trianglesCollider.ToArray(), null);
            });
		}
		else {
            lock (this.planet.planetQueue.getQueueLock()) {
                updateMesh(mesh, vertices.ToArray(), normals.ToArray(), triangles.ToArray(), colors.ToArray());
                updateMesh(meshCollider, verticesCollider.ToArray(), normalsCollider.ToArray(), trianglesCollider.ToArray(), null);
            }
		}
	}

	public void updateMesh(Mesh mesh, Vector3[] vertices, Vector3[] normals, int[] triangles, Color[] colors) {
        mesh.Clear();
        mesh.vertices  = vertices;
        mesh.normals   = normals;
        mesh.triangles = triangles;

        if (colors != null)
            mesh.colors = colors;
            
		mesh.RecalculateBounds();
	}
}
