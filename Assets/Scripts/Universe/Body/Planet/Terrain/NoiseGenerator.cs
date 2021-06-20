using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseGenerator {
    private NoiseLayer[] layer;

    public NoiseGenerator(TerrainGenerator generator, NoiseSettings[] settings) {
        this.layer = new NoiseLayer[settings.Length];
        for (int i = 0; i < settings.Length; i++) {
            this.layer[i] = new NoiseLayer(generator, settings[i]);
        }
    }

    public void initialize(int seed) {
        for (int i = 0; i < layer.Length; i++) {
            this.layer[i].initialize(seed);
        }
    }

    public double evaluate(Vector3d pos) {
        int activatedLayerCount = 0;
        float maxStrength = -10000000;
        double val = 0;

        for (int i = 0; i < layer.Length; i++) {
            if (layer[i].settings.activated) {
                val += this.layer[i].evaluate((Vector3) pos);
                activatedLayerCount++;

                if (layer[i].settings.strength > maxStrength)
                    maxStrength = layer[i].settings.strength;
            }
        }

        return val / activatedLayerCount / maxStrength;
    }
}