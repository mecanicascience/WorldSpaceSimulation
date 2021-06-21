using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerControler : MonoBehaviour {
	[Header("Movement controls")]
	public KeyCode sprintKeyCode = KeyCode.LeftShift;
	public bool useCustomPhysics = true; // False if player move without forces
	public bool spawnOnLaunchPad = true;
	public float flySpeed = 800f;
	public float flyRunSpeed = 5000f;


	[Header("Movement settings")]
	public float walkSpeed = 8f;
	public float runSpeed = 14f;
	public float jumpForce = 20f;
	public float vSmoothTime = 0.1f;
	public float airSmoothTime = 0.5f;
    public float maxPlayerGroundDistance = 1f;


    [Header("Character data")]
	public double mass = 80f;
	public Transform feetPosition;
	public Camera mainCamera;
	public GameObject collisionPointer;

	[HideInInspector]
	public Body nearestBody;
	
	public Vector3d pos;
    public Vector3d vel;
    public Vector3d acc;
	private float lastPlayerPlanetDistance;

    private Rigidbody rb;
	private Vector3d smoothVRef;
    private Vector3d localGravity;
	private Universe universe;

	private GameObject altitudeGOLabelSee;
    private GameObject altitudeGOLabelGround;
    private GameObject speedGOLabel;
    private GameObject accelerationGOLabel;
	



	private void Awake() {
		// Init rigidbody (custom physics)
		this.rb = this.GetComponent<Rigidbody>();
		this.rb.interpolation = RigidbodyInterpolation.Interpolate;
		this.rb.useGravity  = false;
		this.rb.isKinematic = false;
		this.rb.constraints = RigidbodyConstraints.FreezeRotation;
		this.rb.mass = (float) this.mass;

        this.pos = Vector3d.zero;
        this.vel = Vector3d.zero;
        this.acc = Vector3d.zero;

		this.localGravity = Vector3d.zero;
		this.universe = FindObjectOfType<Universe>();
	}

	private void Start() {
		if (this.spawnOnLaunchPad) {
			this.useCustomPhysics = true;
            this.rb.position = (Vector3) ((Planet) this.nearestBody).getLaunchPadPosition();
		}
        this.pos = new Vector3d(this.rb.position);

        // Setup altitude label
        this.lastPlayerPlanetDistance = 0f;
        this.altitudeGOLabelSee = GameObject.Find("AltitudeValueSee");
        this.altitudeGOLabelGround = GameObject.Find("AltitudeValueGround");
        this.speedGOLabel = GameObject.Find("SpeedValue");
        this.accelerationGOLabel = GameObject.Find("AccelerationValue");
	}
	



	private void Update() {
		// Handle physics
		if (this.useCustomPhysics)
			this.handleCameraPhysicsMovement();
		else
			this.handleCameraFreeMovement();

        // Update altitude
		if (Time.fixedTime % 0.5f == 0) {
            this.altitudeGOLabelSee.GetComponent<Text>().text = "Altitude niveau mer : " + (Mathf.Round(this.getAltitude(AltitudeMode.OCEAN_LEVEL) / 10f) / 100f).ToString() + " km";
            this.altitudeGOLabelGround.GetComponent<Text>().text = "Altitude niveau sol : " + (Mathf.Round(this.getAltitude(AltitudeMode.TERRAIN_LEVEL) / 10f) / 100f).ToString() + " km";
            this.speedGOLabel.GetComponent<Text>().text =
                "Vitesse : " + Mathf.Round((float)this.vel.magnitude * 100f / 1000f) / 100f + " km/s (<i>"
                + Mathf.Round((float)this.vel.magnitude * 100f / 1000f * 3600f) / 100f + " km/h</i>)";
            this.accelerationGOLabel.GetComponent<Text>().text =
                "Accélération : " + Mathf.Round((float)this.acc.magnitude * 100f) / 100f + " m/s²";
		}
	}

	private void FixedUpdate() {
        // Move
        this.pos += this.vel * Time.fixedDeltaTime;
		this.vel += this.acc * Time.fixedDeltaTime;
        this.acc = Vector3d.zero;

        // Update nearest body pointer
        this.nearestBody = this.universe.nearestBodyFrom(this.pos);

        // Update physics model
        if (this.useCustomPhysics)
            this.updatePhysicsIntegrator();

		// Update Rigidbody position
        this.rb.MovePosition((Vector3) this.pos);
	}

	private void OnCollisionEnter(Collision other) {
		this.vel = Vector3d.zero;
		this.acc = Vector3d.zero;
	}

	private void OnCollisionStay(Collision other) {
		this.addForce(-this.localGravity * 1.0001);
	}


	
	private void updatePhysicsIntegrator() {
		if (!this.universe.isInstanciated())
			return;

		// Gravity
		double sqrtDistance = (this.nearestBody.pos - this.pos).sqrMagnitude;
		Vector3d forceGravDir = (this.nearestBody.pos - this.pos).normalized;
		this.localGravity = forceGravDir * Constants.gravitationalConstant * this.nearestBody.mass * this.mass / sqrtDistance;

		this.addForce(localGravity);

		// Global acceleration
		this.rb.rotation = Quaternion.FromToRotation(-transform.up, (Vector3) forceGravDir) * this.rb.rotation;
	}


    private void handleCameraFreeMovement() {
		bool isRunning = Input.GetKey(this.sprintKeyCode);

        Vector3 playerInput = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        Vector3 targetVelocity = 
			  mainCamera.transform.right   * playerInput.x
			+ mainCamera.transform.forward * playerInput.z;

        targetVelocity *= (isRunning) ? flyRunSpeed : flySpeed;

        vel = Vector3d.SmoothDamp(vel, (Vector3d) targetVelocity, ref smoothVRef, airSmoothTime);
    }

	private void handleCameraPhysicsMovement() {
		if (Time.timeScale == 0)
			return;

		// Movement
		bool isGrounded = this.isGrounded();

		Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
		bool running = Input.GetKey(KeyCode.LeftShift);
		Vector3 targetVelocity = transform.TransformDirection(input.normalized) * ((running) ? runSpeed : walkSpeed);
		vel = Vector3d.SmoothDamp(vel, (Vector3d) targetVelocity, ref smoothVRef, (isGrounded) ? vSmoothTime : airSmoothTime);


		// Is Grounded
		if (isGrounded) {
			// Add Jump Force
			if (Input.GetKeyDown(KeyCode.Space)) {
				this.addForce((Vector3d) transform.up * jumpForce);
			}
		}
	}

	private void addForce(Vector3d force) {
		this.acc += force / this.mass;
    }

	private float getAltitude(AltitudeMode mode) {
		if (mode.Equals(AltitudeMode.OCEAN_LEVEL))
			return (float) (Vector3d.Distance(this.pos, this.nearestBody.pos) - this.nearestBody.size / 2f) * 100f;

		if (this.useCustomPhysics)
            return this.lastPlayerPlanetDistance * 100f;
		
		Debug.LogWarning("Player doesn't use custom physics, so player to planet surface distance will not be calculated.");
        return 0;
	}

	private bool isGrounded() {
        bool grounded = false;
        Ray rayCollision = new Ray((Vector3)this.pos, this.collisionPointer.transform.forward);
        RaycastHit rayHitInfo;

        if (Physics.Raycast(rayCollision, out rayHitInfo)) {
            this.lastPlayerPlanetDistance = rayHitInfo.distance;
            if (rayHitInfo.distance < this.maxPlayerGroundDistance)
                grounded = true;

            Debug.DrawLine((Vector3)this.pos, rayHitInfo.point, Color.green);
        }

        return grounded;
    }
}


enum AltitudeMode {
    OCEAN_LEVEL,
    TERRAIN_LEVEL
}
