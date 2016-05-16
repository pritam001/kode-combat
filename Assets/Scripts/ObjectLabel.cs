using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
 
[RequireComponent (typeof (GUIText))]
public class ObjectLabel : MonoBehaviour {
 
	public Transform target;  // Object that this label should follow
	public GameObject loaderObject; // Object that contains loader.cs
	Transform thisTransform;
	float obj_curr_y;
	float target_curr_y;
	loader loaderScript;

	void Start () 
	{

		// Initialize variables to use in update
		thisTransform = transform;
		obj_curr_y = thisTransform.position.y;
		target_curr_y = target.position.y;
		loaderScript = loaderObject.GetComponent<loader>();

		position_initialize();
	}
 
	void position_initialize() {
		// Determine the slot number (0,1,2,....) target is in
		int loc = Mathf.RoundToInt((target.position.y + 1.5f)/(3*loaderScript.cols*1f/Mathf.RoundToInt(loaderScript.cols * loaderScript.aspect) + 0.01f));
		//Debug.Log("loc : " + loc.ToString() + " ;  name : " + transform.name.ToString());
		
		Vector3 newPosition = transform.position;
		newPosition.y = 110 + loc*loaderScript.spaceBtwnLines;
		//Debug.Log(loc.ToString() + "  " + newPosition.y.ToString());
		transform.position = newPosition;

		// Reinitialize this objects current position
		obj_curr_y = transform.position.y;
	}
 
	void Update()
	{
		if(target.position.y < target_curr_y){
			//Moving down
			//Debug.Log("Moving down " + ((target.position.y - target_curr_y)/(3*loaderScript.cols*1f/Mathf.RoundToInt(loaderScript.cols * loaderScript.aspect) + 0.01f)).ToString());
			obj_curr_y += loaderScript.spaceBtwnLines*(target.position.y - target_curr_y)/(3*loaderScript.cols*1f/Mathf.RoundToInt(loaderScript.cols * loaderScript.aspect) + 0.01f);
			Vector3 newPosition = transform.position;
			newPosition.y = obj_curr_y;
			transform.position = newPosition;
			
			target_curr_y = target.position.y;
			
		}

		if(target.position.y > target_curr_y) {
			//Moving up
			//Debug.Log("Moving up " + ((target.position.y - target_curr_y)/(3*loaderScript.cols*1f/Mathf.RoundToInt(loaderScript.cols * loaderScript.aspect) + 0.01f)).ToString());
			obj_curr_y += loaderScript.spaceBtwnLines*(target.position.y - target_curr_y)/(3*loaderScript.cols*1f/Mathf.RoundToInt(loaderScript.cols * loaderScript.aspect) + 0.01f);
			Vector3 newPosition = transform.position;
			newPosition.y = obj_curr_y;
			transform.position = newPosition;
			
			target_curr_y = target.position.y;
			
		}
	}
}