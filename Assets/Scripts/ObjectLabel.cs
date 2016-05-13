using UnityEngine;
using System.Collections;
 
[RequireComponent (typeof (GUIText))]
public class ObjectLabel : MonoBehaviour {
 
	public Transform target;  // Object that this label should follow
	public GameObject loaderObject; // Object that contains loader.cs
	/*public Vector3 offset = Vector3.up;    // Units in world space to offset; 1 unit above object by default
	public bool clampToScreen = false;  // If true, label will be visible even if object is off screen
	public float clampBorderSize = 0.05f;  // How much viewport space to leave at the borders when a label is being clamped
	public bool useMainCamera = true;   // Use the camera tagged MainCamera
	public Camera cameraToUse ;   // Only use this if useMainCamera is false
	Camera cam ;
	Transform camTransform; */
	Transform thisTransform;
	float obj_curr_y;
	float target_curr_y;
	loader loaderScript;

	void Start () 
	{
		thisTransform = transform;
		obj_curr_y = thisTransform.position.y;
		target_curr_y = target.position.y;
		loaderScript = loaderObject.GetComponent<loader>();
		
		/*if (useMainCamera)
			cam = Camera.main;
		else
			cam = cameraToUse;
		camTransform = cam.transform;*/
	}
 
 
	void Update()
	{

		/*if (clampToScreen)
		{
			Vector3 relativePosition = camTransform.InverseTransformPoint(target.position + offset);
			relativePosition.z =  Mathf.Max(relativePosition.z, 1.0f);
			thisTransform.position = cam.WorldToViewportPoint(camTransform.TransformPoint(relativePosition));
			thisTransform.position = new Vector3(Mathf.Clamp(thisTransform.position.x, clampBorderSize, 1.0f - clampBorderSize),
											 Mathf.Clamp(thisTransform.position.y, clampBorderSize, 1.0f - clampBorderSize),
											 thisTransform.position.z);

		}
		else
		{
			thisTransform.position = cam.WorldToViewportPoint(target.position + offset);
		}*/
		if(target.position.y < target_curr_y){
			//Moving down
			obj_curr_y -= loaderScript.spaceBtwnLines*(target.position.y - target_curr_y)/(3*loaderScript.cols*1f/Mathf.RoundToInt(loaderScript.cols * loaderScript.aspect) + 0.01f);
			Vector3 newPosition = transform.position;
			newPosition.y = obj_curr_y;
			transform.position = newPosition;
			
			target_curr_y = target.position.y;
			Debug.Log("Moving down");
		} else if(target.position.y > target_curr_y) {
			//Moving up
			obj_curr_y += loaderScript.spaceBtwnLines*(target.position.y - target_curr_y)/(3*loaderScript.cols*1f/Mathf.RoundToInt(loaderScript.cols * loaderScript.aspect) + 0.01f);
			Vector3 newPosition = transform.position;
			newPosition.y = obj_curr_y;
			transform.position = newPosition;
			
			target_curr_y = target.position.y;
			Debug.Log("Moving up");
		}
	}
}