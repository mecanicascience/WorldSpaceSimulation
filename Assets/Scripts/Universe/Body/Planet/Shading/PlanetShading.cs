using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlanetShading {
    private Planet planet;

    public PlanetShading(Planet planet) {
        this.planet = planet;
    }

    public void updateTerrainDatas(Material mat, TerrainGenerator generator) {
        mat.SetFloat("_MaxHeight", (float)generator.getMaxHeight());
        mat.SetFloat("_MinHeight", (float)generator.getMinHeight());
        mat.SetFloat("_BodyRadius", this.planet.size);
    }
}