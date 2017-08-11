/// <summary>
/// /INSTRUCTIONS
/// 
/// 1) Press Button 'Q' and left mouse button to focus on any gameobject which has the collider and layermast "CollidableMask"
/// 
/// 2) Press Left mouse button and move you mouse left and right so that you can rotate your camera around a single point
/// 
/// 3) Press Left mouse button and press any of the ASWD key to move left right forward or backward to move constantly
///    ALSO If you press left shift button then you will acclerate to the max speed that you specified 
/// 
/// 4) Scrool you Mouse wheel so that you can Zoom in and Zoom Out ..
/// 
/// 
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UnityCameraController : MonoBehaviour {
	/// <summary>
	/// It is the constant speed for moving freely
	/// </summary>
	public float freeMoveConstantSpeed;
	/// <summary>
	/// Maximum speed which th camera can move while accelerating
	/// </summary>
	public float maxFreeMoveSpeed;
	/// <summary>
	/// Accleration for moving freely
	/// </summary>
	public float freeMoveAccleration;
	/// <summary>
	/// how fast the camera rotate when you move you mouse left or right
	/// </summary>
	public float rotSpeed;
	/// <summary>
	///  The minimum scrool Speed
	/// </summary>
	public float minScrollSpeed;
	/// <summary>
	/// The maximum ScrollSpeed It should not go above the certain value 
	/// </summary>
	public float maxScrollSpeed;
	/// <summary>
	/// How far away the camera can move in x,y,z direction 
	/// </summary>
	public Vector3 cameraClampValue;
	/// <summary>
	/// You camera will focus to only those object which has a collider and collidable mask
	/// </summary>
	public LayerMask collidableMask;

	/// <summary>
	/// This is used so that when the scene starts it will focus this transform
	/// </summary>
	public Transform currentFocusTransform;

	/// <summary>
	/// It is a class which do has call the visual stuff like panicon, rotation icon, normal mouse icon
	/// </summary>
	public CameraVisualStuff visual;

	Camera cam;
	float currentScrollSpeed;
	float currentMoveSpeed;
	float currentFreeMoveAccleration;


	const float panMultiplier = 3;
	const float terrainFocusMultiplier = 2;
	const float normalFocuaMultiplier = 3;

	void Start () {
		cam = GetComponent<Camera> ();
		FocusCameraOnGameobject (currentFocusTransform);
		visual.currentSprite = visual.panTexture;
		Cursor.visible = false;
	}
	
	void Update () {

		visual.cursor.transform.position = Input.mousePosition + visual.offset;
		visual.currentSprite = visual.panTexture;


		FocusGameObject (); 
		RotateCamera (); 
		PanCamera ();
		ZoomCamera ();
		ClampCamera ();

		visual.cursor.sprite = visual.currentSprite;
	}

	void ClampCamera ()	{
		
		Vector3 clampPos = transform.position;
		clampPos.x = Mathf.Clamp (clampPos.x, -cameraClampValue.x, cameraClampValue.x);
		clampPos.y = Mathf.Clamp (clampPos.y, -cameraClampValue.y, cameraClampValue.y);
		clampPos.z = Mathf.Clamp (clampPos.z, -cameraClampValue.z, cameraClampValue.z);
		transform.position = clampPos;

		transform.eulerAngles = new Vector3 (transform.eulerAngles.x, transform.eulerAngles.y, 0);

	}

	void ZoomCamera (){
		
		float zoom = Input.GetAxisRaw ("Mouse ScrollWheel");
		currentScrollSpeed = (currentFocusTransform.position - transform.position).sqrMagnitude;
		currentScrollSpeed = Mathf.Clamp (currentScrollSpeed, minScrollSpeed, maxScrollSpeed);

		transform.Translate (transform.forward * zoom * currentScrollSpeed * Time.deltaTime, Space.World);
	}

	void PanCamera (){
		
		if (Input.GetMouseButton (0)) {
			Vector2 panInput = new Vector2 (-Input.GetAxisRaw ("Mouse X"), -Input.GetAxisRaw ("Mouse Y"));
			transform.Translate ((transform.right * panInput.x + transform.up * panInput.y) * (currentFocusTransform.position - transform.position).magnitude *panMultiplier * Time.deltaTime, Space.World);
		}
	}

	void RotateCamera (){
		
		if (Input.GetMouseButton (1)) {
			visual.currentSprite = visual.rotateTexture;
			// Rotate left and right
			Vector2 rotInput = new Vector2 (Input.GetAxisRaw ("Mouse X"), -Input.GetAxisRaw ("Mouse Y"));
			transform.Rotate ((transform.up * rotInput.x + transform.right * rotInput.y) * rotSpeed * Time.deltaTime, Space.World);
			if (Input.GetKey (KeyCode.W)) {
				transform.Translate ((transform.forward) * currentMoveSpeed * Time.deltaTime, Space.World);
			}
			if (Input.GetKey (KeyCode.S)) {
				transform.Translate ((-transform.forward) * currentMoveSpeed * Time.deltaTime, Space.World);
			}
			if (Input.GetKey (KeyCode.A)) {
				transform.Translate ((-transform.right) * currentMoveSpeed * Time.deltaTime, Space.World);
			}
			if (Input.GetKey (KeyCode.D)) {
				transform.Translate ((transform.right) * currentMoveSpeed * Time.deltaTime, Space.World);
			}

			if ((Input.GetKey (KeyCode.W) || Input.GetKey (KeyCode.S) || Input.GetKey (KeyCode.A) || Input.GetKey (KeyCode.D)) ) {
				if (Input.GetKey (KeyCode.LeftShift)) {
					currentFreeMoveAccleration += freeMoveAccleration;
					currentMoveSpeed += Time.deltaTime * currentFreeMoveAccleration * currentFreeMoveAccleration;
					currentMoveSpeed = Mathf.Clamp (currentMoveSpeed, freeMoveConstantSpeed, maxFreeMoveSpeed);
				} else {
					currentFreeMoveAccleration = 0;
					currentMoveSpeed = freeMoveConstantSpeed;
				}
			}

		}
	}

	void FocusGameObject () {
		if (Input.GetKey (KeyCode.Q)) {
			visual.currentSprite = visual.mouseTexure;
			if (Input.GetMouseButtonDown (0)) {
				Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
				RaycastHit hit;
				if (Physics.Raycast (ray.origin, ray.direction, out hit, int.MaxValue, collidableMask)) {
					currentFocusTransform = hit.transform;
					FocusCameraOnGameobject (hit.transform);
				}
			}
		}
	}


	void FocusCameraOnGameobject (Transform hit) {
		Bounds hitBound = hit.GetComponent<Collider>().bounds;
		bool isTerrain = hit.GetComponent<Terrain> ();
		Vector3 centerPos = hit.position; 
		float maxDistance = (hitBound.center - hitBound.max).magnitude * ((isTerrain == true) ? terrainFocusMultiplier : normalFocuaMultiplier);
		Vector3 focusPoint = centerPos - transform.forward * maxDistance;
		StartCoroutine (SmoothlyMoveCameraTowardsFocusPoint (focusPoint));
	}

	IEnumerator SmoothlyMoveCameraTowardsFocusPoint (Vector3 focusPoint) {
		float focusTime = 0.1f;
		float time = 0;
		float speed = 1 / focusTime;
		Vector3 initiaPos = transform.position;

		while (time <= focusTime) {
			time += Time.deltaTime;
			float amount = Ease (time * speed);
			amount = Mathf.Clamp01 (amount);

			transform.position = Vector3.Lerp (initiaPos, focusPoint, amount );
			yield return null;
		}
	}

	float Ease (float x){
		float a = 3;
		return Mathf.Pow (x, a) / (Mathf.Pow (x, a) + Mathf.Pow ((1 - x), a));
	}
}

[System.Serializable]
public class CameraVisualStuff {
	public Sprite mouseTexure;
	public Sprite rotateTexture;
	public Sprite panTexture;
	public Vector3 offset;
	public Image cursor;

	[HideInInspector]
	public Sprite currentSprite;
}
