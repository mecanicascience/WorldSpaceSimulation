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


		public Chunk(PlanetChunk handler, Chunk parent, BoundingBox bounds, int depth, Vector3 localUp) {
            this.handler = handler;
            this.localUp = (Vector3d) localUp;
			this.parent = parent;
			this.bounds = bounds;
			this.depth = depth;

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
            double d = Vector3d.Distance((Vector3d)this.centerQuadPos, p.lastPlayerPos); // distance player - chunk center

            if (this.subdivided)
                return;

			if (
				   ( p.lodEnabled && this.depth <= p.lodDistances.Length - 1 && this.depth >= 0)
				|| (!p.lodEnabled && this.depth <= p.nonLODLevelOfDetail - 1 && this.depth >= 0)
			) {
				if (p.lodEnabled && !(d <= p.lodDistances[this.depth]))
					return;
				
				double maxDist = 1.4142 * p.planetSize/2 * 1.05; // sqrt(R^2 + R^2) * 1.01
				if (p.cutNonVisibleChunks && d >= maxDist) { // > Max Radius
					this.shouldBeRendered = false;
					return;
                }

				double x = this.bounds.pos.x;
                double y = this.bounds.pos.y;
                double dim = this.bounds.dim / 2;

				this.children = new Chunk[4];
				this.children[0] = new Chunk(handler, this, new QuadTree.BoundingBox(x, y, dim), depth + 1, (Vector3) localUp);
                this.children[1] = new Chunk(handler, this, new QuadTree.BoundingBox(x + dim, y, dim), depth + 1, (Vector3)localUp);
                this.children[2] = new Chunk(handler, this, new QuadTree.BoundingBox(x, y + dim, dim), depth + 1, (Vector3)localUp);
                this.children[3] = new Chunk(handler, this, new QuadTree.BoundingBox(x + dim, y + dim, dim), depth + 1, (Vector3) localUp);

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
			else {
                if (
                    !this.handler.planet.cutNonVisibleChunks
					|| this.handler.planet.cutNonVisibleChunks && this.shouldBeRendered
					|| (this.handler.planet.cutNonVisibleChunks && !this.shouldBeRendered && this.depth <= 2)
				) toBeRendered.Add(this);
			}

            if (this.depth == this.handler.maxCurrentDepth || this.depth == 0)
                colliderChunks.Add(this);

			return (toBeRendered.ToArray(), colliderChunks.ToArray());
		}

		public (Vector3[], Vector3[], int[], Color[]) calculateVerticesAndTriangles(int triangleOffset) {
			int density = this.handler.planet.chunkDensity + 1;

            Vector3[] vertices = new Vector3[density * density];
            Vector3[] normals = new Vector3[density * density];
            int[] triangles = new int[6 * (density - 1) * (density - 1)];
            Color[] colors = new Color[density * density];

			float scale = this.handler.planet.planetSize / 2f;
			Vector3d offset = new Vector3d(scale / 2f, -scale / 2f, scale / 2f);

            int indexTr = 0;
            for (int j = 0; j < density; j++) {
                for (int i = 0; i < density; i++) {
                    Vector3d off = new Vector3d(scale / 2, -scale / 2, scale / 2);
					int index = i + j * density;

                    double x = this.bounds.pos.x + this.bounds.dim / (density - 1) * i;
                    double y = this.bounds.pos.y + this.bounds.dim / (density - 1) * j;
                    double z = 0;

					Vector3d sphereUnitPosition = QuaternionD.Euler(90 * localUp) * (new Vector3d(x, z, y) - (Vector3d) offset).normalized;
                    z = this.handler.planet.getAltitudeAt(sphereUnitPosition);

                    vertices[index] = (Vector3) (sphereUnitPosition * (scale + z));
                    normals[index] = vertices[index].normalized;
                    colors[index] = this.handler.planet.getColorAtAltitude(z); // tmp
					// colors[index] = this.handler.planet.getColorAt(sphereUnitPosition);

                    if (i != density - 1 && j != density - 1) {
                        triangles[indexTr + 0] = index + density + triangleOffset;
                        triangles[indexTr + 1] = index + 1 + triangleOffset;
                        triangles[indexTr + 2] = index + triangleOffset;
                        indexTr += 3;
                    }
                    if (i != 0 && j != density - 1) {
                        triangles[indexTr + 0] = index + triangleOffset;
                        triangles[indexTr + 1] = index - 1 + density + triangleOffset;
                        triangles[indexTr + 2] = index + density + triangleOffset;
                        indexTr += 3;
                    }
				}
			}

			return (vertices, normals, triangles, colors);
		}


		private void propagateSubdivideInfo(int maxCurrentDepth) {
			this.maxCurrentDepth = maxCurrentDepth;
			if (this.parent != null && this.parent.maxCurrentDepth < maxCurrentDepth)
				this.parent.propagateSubdivideInfo(maxCurrentDepth);
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
}
