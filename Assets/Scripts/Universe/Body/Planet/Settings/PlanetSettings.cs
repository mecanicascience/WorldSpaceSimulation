using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Planet))]
public class PlanetSettings : Editor {
    public override void OnInspectorGUI() {
        DrawDefaultInspector();
        
        Planet planet = (Planet) target;
        if (GUILayout.Button("Regenerate Planet")) {
            planet.initialize();
        }
    }
}