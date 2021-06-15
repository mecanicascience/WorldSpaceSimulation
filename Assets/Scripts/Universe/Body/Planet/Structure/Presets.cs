using System.Collections.Generic;
using UnityEngine;
using System;

namespace QuadTree {
    public class Presets {
        /** Must be even */
        public const int DENSITY = 8;

        public static Vector3d[][] vertices = new Vector3d[16][];
        public static int[][] triangles = new int[16][];
        public static Vector3d[][] edgeVertices = new Vector3d[16][];
        public static int[][] edgeTriangles = new int[16][];

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
            edgeVertices  = new Vector3d[16][];
            edgeTriangles = new int[16][];

            for (int quadI = 0; quadI < 16; quadI++) { // 0b0000 - 0b1111
                (Vector3d[], int[], Vector3d[], int[]) grid = Presets.generateGridWithIndex((uint) quadI, res);
                vertices[quadI] = grid.Item1;
                triangles[quadI] = grid.Item2;
                edgeVertices[quadI] = grid.Item3;
                edgeTriangles[quadI] = grid.Item4;
            }
        }

        private static (Vector3d[], int[], Vector3d[], int[]) generateGridWithIndex(uint quadI, int res) {
            Vector3d[] vertices = new Vector3d[res * res];
            Vector3d[] edgeVertices = new Vector3d[4 * res + 8];
            List<int> triangles = new List<int>();
            List<int> edgeTriangles = new List<int>();

            // Compute vertices and main triangles
            for (int j = 0; j < res; j++) {
                for (int i = 0; i < res; i++) {
                    // default size : 10x10
                    float x = i * 100f / (res - 1f);
                    float y = j * 100f / (res - 1f);
                    int index = i + j * res;

                    // Vertices
                    vertices[index] = new Vector3d(x, y);

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

            // Edge vertices
            int it = -1;
            int[] itForBlocks = new int[4]{ 0, res + 1, 2 * (res + 1), 3 * (res + 1) };
            for (int i = 0; i < 4 * (res + 1); i++) {
                if (i == 0) { // edge top left
                    if ((quadI & 0b1000) == 0b1000 || (quadI & 0b0001) == 0b0001)
                        edgeVertices[itForBlocks[0]] = new Vector3d(-100f / (res - 1f) * 2f, 100f / (res - 1f) * (res + 1f));
                    else
                        edgeVertices[itForBlocks[0]] = new Vector3d(-100f / (res - 1f), 100f / (res - 1f) * res);
                    it++;
                }
                else if (i <= res) { // up
                    if ((quadI & 0b1000) == 0b0000) { // 0xxx
                        float x = 100f / (res - 1f) * (i - 1f);
                        float y = 100f / (res - 1f) * res;
                        edgeVertices[++it] = new Vector3d(x, y);
                    }
                    else if (i % 2 != 0) { // 1xxx
                        float x = 100f / (res - 1f) * (i - 1f);
                        float y = 100f / (res - 1f) * (res + 1f);
                        edgeVertices[++it] = new Vector3d(x, y);
                    }
                }
                else if (i == res + 1) { // edge top right
                    if ((quadI & 0b1000) == 0b1000 || (quadI & 0b0101) == 0b0100)
                        edgeVertices[itForBlocks[1]] = new Vector3d(100f / (res - 1f) * (res + 1f), 100f / (res - 1f) * (res + 1f));
                    else
                        edgeVertices[itForBlocks[1]] = new Vector3d(100f / (res - 1f) * res, 100f / (res - 1f) * res);
                    it = 0;
                }
                else if (i <= 2 * res + 1) { // right
                    if ((quadI & 0b0100) == 0b0000) { // x0xx
                        float x = 100f / (res - 1f) * res;
                        float y = -100f / (res - 1f) * (i - (2f * res + 1f));
                        edgeVertices[++it + itForBlocks[1]] = new Vector3d(x, y);
                    }
                    else if (i % 2 != 0) { // x1xx
                        float x = 100f / (res - 1f) * (res + 1f);
                        float y = -100f / (res - 1f) * (i - (2f * res + 1f));
                        edgeVertices[++it + itForBlocks[1]] = new Vector3d(x, y);
                    }
                }
                else if (i == 2 * (res + 1)) { // edge bottom right
                    if ((quadI & 0b0100) == 0b0100 || (quadI & 0b0010) == 0b0010)
                        edgeVertices[itForBlocks[2]] = new Vector3d(100f / (res - 1f) * (res + 1f), -100f / (res - 1f) * 2f);
                    else
                        edgeVertices[itForBlocks[2]] = new Vector3d(100f / (res - 1f) * res, -100f / (res - 1f));
                    it = 0;
                }
                else if (i <= 3 * res + 2) { // down
                    if ((quadI & 0b0010) != 0b0010) { // xx0x
                        float x = -100f / (res - 1f) * (i - 3f * res - 2f);
                        float y = -100f / (res - 1f);
                        edgeVertices[++it + itForBlocks[2]] = new Vector3d(x, y);
                    }
                    else if (i % 2 != 0) { // xx1x
                        float x = -100f / (res - 1f) * (i - 3f * res - 2f);
                        float y = -100f / (res - 1f) * 2f;
                        edgeVertices[++it + itForBlocks[2]] = new Vector3d(x, y);
                    }
                }
                else if (i == 3 * (res + 1)) { // edge top left
                    if ((quadI & 0b0010) == 0b0010 || (quadI & 0b0001) == 0b0001)
                        edgeVertices[itForBlocks[3]] = new Vector3d(-100f / (res - 1f) * 2f, -100f / (res - 1f) * 2f);
                    else
                        edgeVertices[itForBlocks[3]] = new Vector3d(-100f / (res - 1f), -100f / (res - 1f));
                    it = 0;
                }
                else { // left
                    if ((quadI & 0b0001) != 0b0001) { // xxx0
                        float x = -100f / (res - 1f);
                        float y = 100f / (res - 1f) * (i - 3f * res - 4f);
                        edgeVertices[++it + itForBlocks[3]] = new Vector3d(x, y);
                    }
                    else if ((i - 1) % 2 == 0) {
                        float x = -100f / (res - 1f) * 2f;
                        float y = 100f / (res - 1f) * (i - 3f * res - 4f);
                        edgeVertices[++it + itForBlocks[3]] = new Vector3d(x, y);
                    }
                }
            }


            // Compute border triangles based on quad index
            // Up - 1xxx
            for (int i = 0; i < res - 1; i++) {
                // First triangle
                if ((quadI & 0b1000) == 0b1000) {
                    if (i % 2 == 0) {
                        triangles.Add(res * (res - 1) + i);
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

                // Edge triangles (+ 1 if it is in edgeTriangles[] array)
                if ((quadI & 0b1000) == 0b1000) {
                    if (i % 2 == 0) {
                        if (i % 4 == 0)
                            edgeTriangles.Add(-(res * res - res + i));
                        else
                            edgeTriangles.Add(i / 2 + 2);
                        edgeTriangles.Add(i / 2 + 2 + 1);
                        edgeTriangles.Add(-(res * res - res + i + 2));

                        edgeTriangles.Add(i / 2 + 2);
                        if (i % 4 == 0)
                            edgeTriangles.Add(i / 2 + 3);
                        else
                            edgeTriangles.Add(-(res * res - res + i + 2));
                        edgeTriangles.Add(-(res * res - res + i));
                    }
                }
                else {
                    edgeTriangles.Add(i + 2 + 1);
                    if (i % 2 == 0)
                        edgeTriangles.Add(-(res * res - res + i));
                    else
                        edgeTriangles.Add(-(res * res - res + i + 1));
                    edgeTriangles.Add(i + 2);

                    edgeTriangles.Add(-(res * res - res + i + 1));
                    edgeTriangles.Add(-(res * res - res + i));
                    if (i % 2 == 0)
                        edgeTriangles.Add(i + 3);
                    else
                        edgeTriangles.Add(i + 2);
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

                // Edge triangles
                if ((quadI & 0b0100) == 0b0100) {
                    if (i % 2 == 0) {
                        if (i % 4 == 0)
                            edgeTriangles.Add(-(res * res - 1 - i * res));
                        else
                            edgeTriangles.Add(-(res * res - 1 - (i + 2) * res));
                        edgeTriangles.Add(itForBlocks[1] + i / 2 + 2);
                        edgeTriangles.Add(itForBlocks[1] + i / 2 + 3);

                        edgeTriangles.Add(-(res * res - 1 - i * res));
                        if (i % 4 == 0)
                            edgeTriangles.Add(itForBlocks[1] + (i + 2) / 2 + 2);
                        else
                            edgeTriangles.Add(itForBlocks[1] + i / 2 + 2);
                        edgeTriangles.Add(-(res * res - 1 - (i + 2) * res));
                    }
                }
                else {
                    if (i % 2 == 0)
                        edgeTriangles.Add(-(res * res - 1 - i * res));
                    else
                        edgeTriangles.Add(-(res * res - 1 - (i + 1) * res));
                    edgeTriangles.Add(itForBlocks[1] + i + 2);
                    edgeTriangles.Add(itForBlocks[1] + i + 3);

                    if (i % 2 == 0)
                        edgeTriangles.Add(itForBlocks[1] + i + 3);
                    else
                        edgeTriangles.Add(itForBlocks[1] + i + 2);
                    edgeTriangles.Add(-(res * res - 1 - (i + 1) * res));
                    edgeTriangles.Add(-(res * res - 1 - i * res));
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

                // Edge triangles
                if ((quadI & 0b0010) == 0b0010) {
                    if (i % 2 == 0) {
                        if (i % 4 == 0)
                            edgeTriangles.Add(-(res - i - 1));
                        else
                            edgeTriangles.Add(-(res - i - 3));
                        edgeTriangles.Add(itForBlocks[2] + i / 2 + 2);
                        edgeTriangles.Add(itForBlocks[2] + i / 2 + 3);

                        if (i % 4 == 0)
                            edgeTriangles.Add(itForBlocks[2] + i / 2 + 3);
                        else
                            edgeTriangles.Add(itForBlocks[2] + i / 2 + 2);
                        edgeTriangles.Add(-(res - i - 3));
                        edgeTriangles.Add(-(res - i - 1));
                    }
                }
                else {
                    if (i % 2 == 0)
                        edgeTriangles.Add(-(res - i - 1));
                    else
                        edgeTriangles.Add(-(res - i - 2));
                    edgeTriangles.Add(itForBlocks[2] + i + 2);
                    edgeTriangles.Add(itForBlocks[2] + i + 3);

                    if (i % 2 == 0)
                        edgeTriangles.Add(itForBlocks[2] + i + 3);
                    else
                        edgeTriangles.Add(itForBlocks[2] + i + 2);
                    edgeTriangles.Add(-(res - i - 2));
                    edgeTriangles.Add(-(res - i - 1));
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

                // Edge triangles
                if ((quadI & 0b0001) == 0b0001) {
                    if (i % 2 == 0) {
                        if (i % 4 == 0)
                            edgeTriangles.Add(-((i + 2) * res));
                        else
                            edgeTriangles.Add(-(i * res));
                        edgeTriangles.Add(itForBlocks[3] + i / 2 + 2);
                        edgeTriangles.Add(itForBlocks[3] + i / 2 + 3);

                        if (i % 4 == 0)
                            edgeTriangles.Add(itForBlocks[3] + i / 2 + 2);
                        else
                            edgeTriangles.Add(itForBlocks[3] + i / 2 + 3);
                        edgeTriangles.Add(-((i + 2) * res));
                        edgeTriangles.Add(-(i * res));
                    }
                }
                else {
                    if (i % 2 == 0)
                        edgeTriangles.Add(-(i * res));
                    else
                        edgeTriangles.Add(-((i + 1) * res));
                    edgeTriangles.Add(itForBlocks[3] + i + 2);
                    edgeTriangles.Add(itForBlocks[3] + i + 3);

                    if (i % 2 == 0)
                        edgeTriangles.Add(itForBlocks[3] + i + 3);
                    else
                        edgeTriangles.Add(itForBlocks[3] + i + 2);
                    edgeTriangles.Add(-((i + 1) * res));
                    edgeTriangles.Add(-(i * res));
                }
            }

            return (vertices, triangles.ToArray(), edgeVertices, edgeTriangles.ToArray());
        }
    }
}