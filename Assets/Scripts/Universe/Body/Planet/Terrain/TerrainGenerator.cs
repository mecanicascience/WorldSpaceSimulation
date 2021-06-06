using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator {
    private NoiseGenerator noiseGenerator;
    private Planet planet;

    private double maxHeight;
    private double minHeight;


    public TerrainGenerator(NoiseSettings[] settings, Planet planet) {
        this.noiseGenerator = new NoiseGenerator(this, settings);
        this.planet = planet;
    }

    public void initialize() {
        this.noiseGenerator.initialize();
        this.resetExtremum();
    }

    public double getAltitudeAt(Vector3d unitarySpherePos) {
        // Computes altitude
        double altitudePercent = noiseGenerator.evaluate(unitarySpherePos) - planet.waterLevel;
        if (altitudePercent < 0)
            altitudePercent = 0;

        // Computes real altitude from percentage
        double realAltitude = altitudePercent * this.planet.terrainHeight;
        if (realAltitude > maxHeight)
            maxHeight = realAltitude;
        if (realAltitude < minHeight)
            minHeight = realAltitude;

        return realAltitude;
    }

    public Color getColorAtAltitude(double altitude) {
        double z = (altitude - this.minHeight) / (this.maxHeight - this.minHeight);

        if (z < 0)
            z = 0;
        else if (z > 1)
            z = 1;

        if (double.IsNaN(z))
            return Color.white;

        return planet.terrainGradient.Evaluate((float) z);
    }


    private double[] latitudeZones = new double[]{0, 0};
    
    public Color getColorAtLatLong(double latitude, double longitude) {
        if (latitude < 0) {
            if (longitude < 0)
                return Color.red;
            else
                return Color.green;
        }
        else {
            if (longitude < 0)
                return Color.blue;
            else
                return Color.yellow;
        }
    }

    public void resetExtremum() {
        this.maxHeight = -10000000000000;
        this.minHeight =  10000000000000;
    }
}