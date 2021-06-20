using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Universe : MonoBehaviour {
    private List<Body> bodyList;

    private void Awake() {
        DontDestroyOnLoad(transform.gameObject);
    }

    private void Start() {
        this.initializeForScene();
    }

    private void Update() {
        
    }

    
    private void initializeForScene() {
        this.bodyList = new List<Body>();

        // Find every bodys
        this.bodyList.AddRange(FindObjectsOfType<Body>());
        foreach (Body body in this.bodyList) {
            Debug.Log(body.getUUID());
        }
    }
}
