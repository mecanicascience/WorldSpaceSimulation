using System.Collections.Generic;
using UnityEngine;
using System;

// Thanks to https://www.youtube.com/watch?v=mXTxQko-JH0, https://www.youtube.com/watch?v=QN39W020LqU, https://www.youtube.com/watch?v=YueAtA_YnSY
namespace QuadTree {
	public class Chunk {
		private PlanetChunk handler;
		private Chunk parent;
		private BoundingBox bounds;
		private int depth;
		private Vector3d localUp;

		private Chunk[] children;
		private bool subdivided;
		private Vector3d centerQuadPos;

		private bool shouldBeRendered;

		/** Current chunk max depth level */
		public int maxCurrentDepth;
        public uint hash;


        /*
            array = [
                [ // index 0 - Quadrant 0
                    [ // index 0 - direction 0 - direction LU
                        0, // index 0 - Quadrant Index
                        3, // index 1 - Direction index (including halt = 8)
                    ],
                    ...
                    [] // index i - direction i
                ],
                [] // index 1 - Quadrant 1
                [] // index 2 - Quadrant 2
                [] // index 3 - Quadrant 3
            ]
        */
        private uint[,,] dirArray = new uint[,,]{
            { // Quadrant 0
                {0b11, ((int)Directions.LU)}, {0b11, ((int)Directions.U)}, {0b11, ((int)Directions.L)}, {0b11, ((int)Directions.HALT)},
                {0b10, ((int)Directions.U)}, {0b01, ((int)Directions.HALT)}, {0b10, ((int)Directions.HALT)}, {0b01, ((int)Directions.L)}
            },
            { // Quadrant 1
                {0b10, ((int)Directions.U)},{0b10, ((int)Directions.RU)},{0b10, ((int)Directions.HALT)},{0b10, ((int)Directions.R)},
                {0b11, ((int)Directions.U)},{0b00, ((int)Directions.R)},{0b11, ((int)Directions.HALT)},{0b00, ((int)Directions.HALT)}
            },
            { // Quadrant 2
                {0b01, ((int)Directions.L)},{0b01, ((int)Directions.HALT)},{0b01, ((int)Directions.LD)},{0b01, ((int)Directions.D)},
                {0b00, ((int)Directions.HALT)},{0b11, ((int)Directions.HALT)},{0b00, ((int)Directions.D)},{0b11, ((int)Directions.L)}
            },
            { // Quadrant 3
                {0b00, ((int)Directions.HALT)},{0b00, ((int)Directions.R)},{0b00, ((int)Directions.D)},{0b00, ((int)Directions.RD)},
                {0b01, ((int)Directions.HALT)},{0b10, ((int)Directions.R)},{0b01, ((int)Directions.D)},{0b10, ((int)Directions.HALT)}
            }
        };


		public Chunk(PlanetChunk handler, Chunk parent, BoundingBox bounds, int depth, Vector3 localUp, uint hash) {
            this.handler = handler;
            this.localUp = (Vector3d) localUp;
			this.parent = parent;
			this.bounds = bounds;
			this.depth = depth;
            this.hash = hash;

			this.shouldBeRendered = true;
            this.subdivided = false;
			this.children = new Chunk[4];
            this.centerQuadPos = this.calculateCenterQuadPos();
			this.maxCurrentDepth = 0;
		}
		

		private Vector3d calculateCenterQuadPos() {
			Planet p = this.handler.planet;
            double scale = p.planetSize / 2f;
            Vector3d offset = new Vector3d(scale / 2f, -scale / 2f, scale / 2f);
			Vector3d pos = QuaternionD.Euler(90 * localUp)
				* (new Vector3d(this.bounds.pos.x + this.bounds.dim / 2, 0, this.bounds.pos.y + this.bounds.dim / 1) - ((Vector3d) offset)).normalized
				* p.planetSize / 2f + p.lastPos;
			return pos;
		}

		public void subdivide() {
            this.propagateSubdivideInfo(this.depth);

            Planet p = this.handler.planet;
            double dstPlayerQuadCenter = Vector3d.Distance((Vector3d) this.centerQuadPos, p.lastPlayerPos); // distance player - chunk center

            if (this.subdivided)
                return;

			if (
				   ( p.lodEnabled && this.depth <= p.lodDistances.Length - 1 && this.depth >= 0)
				|| (!p.lodEnabled && this.depth <= p.nonLODLevelOfDetail - 1 && this.depth >= 0)
			) {
				if (p.lodEnabled && !(dstPlayerQuadCenter <= p.lodDistances[this.depth]))
					return;

                // Do not render non visible chunks
                // c^2 = a^2 + b^2 - 2ab.cosT
				double dstPlayerToPlanetCenter = (this.handler.planet.pos - p.lastPlayerPos).magnitude;
				double cullingAngle = Math.Acos((p.planetSize * p.planetSize / 4 + dstPlayerQuadCenter * dstPlayerQuadCenter
                    - dstPlayerToPlanetCenter * dstPlayerToPlanetCenter) / (p.planetSize * dstPlayerQuadCenter));

				if (cullingAngle <= this.handler.planet.maxRenderingAngle) { // > Max Radius
					this.shouldBeRendered = false;
					return;
                }

				double x = this.bounds.pos.x;
                double y = this.bounds.pos.y;
                double dim = this.bounds.dim / 2;

				this.children = new Chunk[4];
                // LD
                this.children[2] = new Chunk(handler, this, new QuadTree.BoundingBox(x, y, dim), depth + 1, (Vector3) localUp, (this.hash << 2) + 2);
                // RD
                this.children[3] = new Chunk(handler, this, new QuadTree.BoundingBox(x + dim, y, dim), depth + 1, (Vector3)localUp, (this.hash << 2) + 3);
                // LU
                this.children[0] = new Chunk(handler, this, new QuadTree.BoundingBox(x, y + dim, dim), depth + 1, (Vector3)localUp, (this.hash << 2) + 0);
                // RU
                this.children[1] = new Chunk(handler, this, new QuadTree.BoundingBox(x + dim, y + dim, dim), depth + 1, (Vector3) localUp, (this.hash << 2) + 1);

				// Create subchildren
				foreach (Chunk child in this.children) {
					child.subdivide();
				}

                this.subdivided = true;
			}
		}

		public (Chunk[], Chunk[]) getVisibleChildren() { // (Rendered Chunks, Collider Chunks)
			List<Chunk> toBeRendered = new List<Chunk>();
            List<Chunk> colliderChunks = new List<Chunk>();

			if (this.subdivided) {
				foreach (Chunk child in this.children) {
                    (Chunk[], Chunk[]) chunks = child.getVisibleChildren();
                    toBeRendered.AddRange(chunks.Item1);
                    colliderChunks.AddRange(chunks.Item2);
				}
			}
			else if (this.shouldBeRendered)
                toBeRendered.Add(this);

            if (this.depth == this.handler.maxCurrentDepth || this.depth == 0)
                colliderChunks.Add(this);

			return (toBeRendered.ToArray(), colliderChunks.ToArray());
		}



		public (Vector3[], Vector3[], int[], Color[]) calculateVerticesAndTriangles(int triangleOffset) {
            uint neighborsHash = this.calculateNeighborsSequence();

            // Get vertices and triangles
            Vector3[] vertices = new Vector3[Presets.vertices[neighborsHash].Length];
            int[] triangles = new int[Presets.triangles[neighborsHash].Length];

            // Update vertices and calculate normals and colors
            Vector3[] normals = new Vector3[vertices.Length];
            Color[] colors = new Color[vertices.Length];

            float scale = this.handler.planet.planetSize / 2f;
            Vector3d offset = new Vector3d(scale / 2f, -scale / 2f, scale / 2f);

            for (int i = 0; i < vertices.Length; i++) {
                double xPos = Presets.vertices[neighborsHash][i].x * this.bounds.dim / 100d + this.bounds.pos.x;
                double yPos = Presets.vertices[neighborsHash][i].y * this.bounds.dim / 100d + this.bounds.pos.y;
                double zPos = 0;

                Vector3d sphereUnitPosition = QuaternionD.Euler(90 * localUp) * (new Vector3d(xPos, zPos, yPos) - offset).normalized;
                double z = this.handler.planet.getAltitudeAt(sphereUnitPosition);

                vertices[i] = (Vector3) (sphereUnitPosition * (scale + z));
                normals[i] = vertices[i].normalized;
                colors[i] = this.handler.planet.getColorAtAltitude(z);
            }

            // Set up triangles
            triangles = Presets.triangles[neighborsHash];
            int[] offsetedTriangles = new int[triangles.Length];
            for (int i = 0; i < triangles.Length; i++) {
                offsetedTriangles[i] = triangles[i] + triangleOffset;
            }

            // Compute normals
            int vertexIndexA;
            int vertexIndexB;
            int vertexIndexC;

            for (int i = 0; i < triangles.Length / 3; i++) {
                int normalTriangleIndex = i * 3;
                vertexIndexA = triangles[normalTriangleIndex];
                vertexIndexB = triangles[normalTriangleIndex + 1];
                vertexIndexC = triangles[normalTriangleIndex + 2];

                Vector3 triangleNormal = this.getSurfaceNormalFromIndices(vertices, vertexIndexA, vertexIndexB, vertexIndexC);
                normals[vertexIndexA] += triangleNormal;
                normals[vertexIndexB] += triangleNormal;
                normals[vertexIndexC] += triangleNormal;
            }

            for (int i = 0; i < normals.Length; i++) {
                normals[i].Normalize();
            }


            return (vertices, normals, offsetedTriangles, colors);
		}


		private void propagateSubdivideInfo(int maxCurrentDepth) {
			this.maxCurrentDepth = maxCurrentDepth;
			if (this.parent != null && this.parent.maxCurrentDepth < maxCurrentDepth)
				this.parent.propagateSubdivideInfo(maxCurrentDepth);
		}


        private uint calculateNeighborsSequence() {
            // Calculate Hash
            (uint, bool)[] neighborsHash = new (uint, bool)[] {
                this.getHashForNeighbor(this.hash, Directions.U),
                this.getHashForNeighbor(this.hash, Directions.R),
                this.getHashForNeighbor(this.hash, Directions.D),
                this.getHashForNeighbor(this.hash, Directions.L)
            };

            // Calculate neighbors depth (real use)
            uint[] neighborLODStatus = new uint[4]; // [Level (false, true, ... or true if unknown (out of the parent chunk))
            for (int i = 0; i < neighborsHash.Length; i++) {
                if (!neighborsHash[i].Item2) // if REASON == 0 (not found)
                    neighborLODStatus[i] = 0;
                else
                    neighborLODStatus[i] = this.handler.mainChunk.checkNeighborDepth(neighborsHash[i].Item1, this.depth);
            }

            // BinaryNumber : U = 0,1, R = 0,1, D = 0,1, L = 0,1
            return BinaryExtension.asBinarySequence(neighborLODStatus);
        }

        private (uint, bool) getHashForNeighbor(uint hash, Directions direction) {
            uint neighborBit = hash | 0b0; // copy
            uint dir = ((uint)direction);
            int i = 0;

            while (hash != 0b1 && dir != ((uint)Directions.HALT)) {
                // Get parent direction and quadrant
                uint ansArr0 = this.dirArray[hash & 0b11, dir, 0];
                uint ansArr1 = this.dirArray[hash & 0b11, dir, 1];

                // Clear using an AND and NOT mask, then OR to add new bits
                neighborBit = (uint) ((neighborBit & ~(0b11 << i * 2)) | ansArr0 << i * 2);

                // Compute values for nextLoop
                dir = ansArr1;
                hash = hash >> 2;
                i++;
            }

            // [neighborBit, REASON (true == HALT : false == NOT FOUND)]
            return (neighborBit, dir == ((uint)Directions.HALT));
        }

        private uint checkNeighborDepth(uint hash, int detailLevel) {
            if (hash == this.hash)
                return 0; // false = searched hash has same LOD (cause exist)

            if (this.subdivided)
                return this.children[(hash >> (detailLevel - 1) * 2) & 0b11].checkNeighborDepth(hash, detailLevel - 1);

            // No children with searched LOD with searched hash (only parent exist)
            return 1; // true = is a neighbor with higher LOD
        }

        private Vector3 getSurfaceNormalFromIndices(Vector3[] vertices, int indexA, int indexB, int indexC) {
            Vector3 pointA = vertices[indexA];
            Vector3 pointB = vertices[indexB];
            Vector3 pointC = vertices[indexC];

            // Get an aproximation of the vertex normal using two other vertices that share the same triangle
            Vector3 sideAB = pointB - pointA;
            Vector3 sideAC = pointC - pointA;
            return Vector3.Cross(sideAB, sideAC).normalized;
        }
	}

    



    public struct BoundingBox {
        public Vector3d pos;
        public double dim;

        public BoundingBox(double x, double y, double dimension) {
            this.pos = new Vector3d(x, y);
            this.dim = dimension;
        }
    }

    enum Directions {
        LU, // 0
        RU, // 1
        LD, // 2
        RD, // 3
        U, // 4
        R, // 5
        D, // 6
        L, // 7
        HALT // 8
    }
}
