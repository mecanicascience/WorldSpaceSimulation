using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerControler : MonoBehaviour {
	[Header("Movement controls")]
	public KeyCode sprintKeyCode = KeyCode.LeftShift;
	public bool useCustomPhysics = false; // False if player move without forces
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
	public Planet planet;
	public GameObject collisionPointer;
	
	public Vector3d pos;
    public Vector3d vel;
    public Vector3d acc;

    private Rigidbody rb;
	private Vector3d smoothVRef;
    private Vector3d localGravity;
	



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
	}

	private void Start() {
        this.pos = new Vector3d(this.rb.position);
	}
	



	private void Update() {
		if (this.useCustomPhysics)
			this.handleCameraPhysicsMovement();
		else
			this.handleCameraFreeMovement();
	}

	private void FixedUpdate() {
        // Move
        this.pos += this.vel * Time.fixedDeltaTime;
		this.vel += this.acc * Time.fixedDeltaTime;
        this.acc = Vector3d.zero;

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
		Planet mainPlanet = this.planet;

		// Gravity
		double sqrtDistance = (mainPlanet.pos - this.pos).sqrMagnitude;
		Vector3d forceGravDir = (mainPlanet.pos - this.pos).normalized;
		this.localGravity = forceGravDir * Constants.gravitationalConstant * mainPlanet.mass * this.mass / sqrtDistance;

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


	private bool isGrounded() {
        bool grounded = false;
		Ray rayCollision = new Ray((Vector3) this.pos, this.collisionPointer.transform.forward);
        RaycastHit rayHitInfo;

        if(Physics.Raycast(rayCollision, out rayHitInfo)) {
			// Debug.Log("Hauteur : " + rayHitInfo.distance);
			if (rayHitInfo.distance < this.maxPlayerGroundDistance)
                grounded = true;

            Debug.DrawLine((Vector3) this.pos, rayHitInfo.point, Color.green);
		}
		
		return grounded;
    }
}
