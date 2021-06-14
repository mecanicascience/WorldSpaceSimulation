using System.Collections.Generic;
using UnityEngine;
using System;

namespace QuadTree {
    public class Presets {
        /** Must be even */
        public const int DENSITY = 8;

        public static Vector3d[][] vertices = new Vector3d[16][];
        public static int[][] triangles = new int[16][];

        private static bool generated = false;


        public static void instanciate() {
            if (!Presets.generated)
                Presets.generateGridTemplate(Presets.DENSITY + 1);
        }

        private static void generateGridTemplate(int res) {
            if ((res - 1) % 2 != 0)
                Debug.LogError("Planet DENSITY parameter must be an even number.");

            Presets.generated = true;

            vertices  = new Vector3d[16][];
            triangles = new int[16][];

            for (int quadI = 0; quadI < 16; quadI++) { // 0b0000 - 0b1111
                (Vector3d[], int[]) grid = Presets.generateGridWithIndex((uint) quadI, res);
                vertices[quadI] = grid.Item1;
                triangles[quadI] = grid.Item2;
            }
        }

        private static (Vector3d[], int[]) generateGridWithIndex(uint quadI, int res) {
            List<Vector3d> vertices = new List<Vector3d>();
            List<int> triangles = new List<int>();

            // Compute vertices and main triangles
            for (int j = 0; j < res; j++) {
                for (int i = 0; i < res; i++) {
                    float x = (res - 1f) * i * 100f / (res - 1f) / (res - 1f);
                    float y = (res - 1f) * j * 100f / (res - 1f) / (res - 1f);
                    int index = i + j * res;

                    // Vertices
                    vertices.Add(new Vector3d(x, y));

                    // Triangles main center (for all triangle, replace by i,j < res-1)
                    if (i < res - 2 && j < res - 2 && i > 0 && j > 0) {
                        // First triangle
                        triangles.Add(index + res); // up left
                        triangles.Add(index + 1); // down right
                        triangles.Add(index); // left down

                        // Second triangle
                        triangles.Add(index + res); // up left
                        triangles.Add(index + 1 + res); // up right
                        triangles.Add(index + 1); // down right
                    }
                }
            }


            // Compute border triangles based on quad index
            // Up - 1xxx
            for (int i = 0; i < res - 1; i++) {
                // First triangle
                if ((quadI & 0b1000) == 0b1000) {
                    if (i % 2 == 0) {
                        triangles.Add( res * (res - 1) + i);
                        triangles.Add(res * (res - 1) + i + 2);
                        triangles.Add(res * (res - 1) + i - res + 1);
                    }
                }
                else {
                    triangles.Add(res * (res - 1) + i);
                    triangles.Add(res * (res - 1) + i + 1);
                    if (i % 2 == 0)
                        triangles.Add(res * (res - 1) + i - res + 1);
                    else
                        triangles.Add(res * (res - 1) + i - res);
                }

                // Second triangle
                if (i != 0 && i != res - 2) {
                    if (i % 2 == 0)
                        triangles.Add(res * (res - 1) + i);
                    else
                        triangles.Add(res * (res - 1) + i + 1);
                    triangles.Add(res * (res - 1) + i - res + 1);
                    triangles.Add(res * (res - 1) + i - res);
                }
            }


            // Right - x1xx
            for (int i = 0; i < res - 1; i++) {
                // First triangle
                if ((quadI & 0b0100) == 0b0100) { // x1xx
                    if (i % 2 == 0) {
                        triangles.Add((i + 1) * res + res - 2);
                        triangles.Add((i + 1) * res + 2 * res - 1);
                        triangles.Add(i * res + res - 1);
                    }
                }
                else {
                    if (i % 2 == 0)
                        triangles.Add((i + 1) * res + res - 2);
                    else
                        triangles.Add(i * res + res - 2);
                    triangles.Add((i + 1) * res + res - 1);
                    triangles.Add(i * res + res - 1);
                }

                // Second triangle
                if (i != 0 && i != res - 2) {
                    triangles.Add((i + 1) * res + res - 2);
                    if (i % 2 == 0)
                        triangles.Add(i * res + res - 1);
                    else
                        triangles.Add((i + 1) * res + res - 1);
                    triangles.Add(i * res + res - 2);
                }
            }


            // Down - xx1x
            for (int i = 0; i < res - 1; i++) {
                // First triangle
                if ((quadI & 0b0010) == 0b0010) {
                    if (i % 2 == 0) {
                        triangles.Add(i + res + 1);
                        triangles.Add(i + 2);
                        triangles.Add(i);
                    }
                }
                else {
                    if (i % 2 == 0)
                        triangles.Add(i + res + 1);
                    else
                        triangles.Add(i + res);
                    triangles.Add(i + 1);
                    triangles.Add(i);
                }

                // Second triangle
                if (i != 0 && i != res - 2) {
                    triangles.Add(i + res);
                    triangles.Add(i + res + 1);
                    if (i % 2 == 0)
                        triangles.Add(i);
                    else
                        triangles.Add(i + 1);
                }
            }


            // Left - xxx1
            for (int i = 0; i < res - 1; i++) {
                // First triangle
                if ((quadI & 0b0001) == 0b0001) {
                    if (i % 2 == 0) {
                        triangles.Add((i + 2) * res);
                        triangles.Add((i + 1) * res + 1);
                        triangles.Add(i * res);
                    }
                }
                else {
                    triangles.Add((i + 1) * res);
                    if (i % 2 == 0)
                        triangles.Add((i + 1) * res + 1);
                    else
                        triangles.Add(i * res + 1);
                    triangles.Add(i * res);
                }

                // Second triangle
                if (i != 0 && i != res - 2) {
                    triangles.Add((i + 1) * res + 1);
                    triangles.Add(i * res + 1);
                    if (i % 2 == 0)
                        triangles.Add(i * res);
                    else
                        triangles.Add((i + 1) * res);
                }
            }

            return (vertices.ToArray(), triangles.ToArray());
        }
    }
}