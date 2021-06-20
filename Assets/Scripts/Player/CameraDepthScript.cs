using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraDepthScript : MonoBehaviour {
	private void Start() {
		// Enable Depth texture on every child camera
		foreach (Camera childCam in this.gameObject.GetComponentsInChildren<Camera>()) {
			childCam.depthTextureMode = childCam.depthTextureMode | DepthTextureMode.Depth;
		}
	}
}
