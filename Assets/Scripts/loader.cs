﻿using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;

public class loader : MonoBehaviour {
	
	public class piece{
		public GameObject GameObject { get; set; }
		public Vector3 Original { get; set; }
		public Vector3 Current { get; set; }
	}
	public piece[,] Matrix = new piece[20, 1]; // piece[MaxRow, MaxColumn]
	
	private Texture2D tex;  // Texture to cut up
	public Material mat;   // Material to use (texture is discarded)
	public int cols = 1;   // Number of tiles across
	public float aspect = 5f;  // Original aspect (height / width)
	public int steps = 0;	// Total steps taken
	public bool gameOn = false; // Whether game has started or not
	public bool gameEnded = false; // Whether game is finished or not
	public float timeLeft = 20f;	// Time left before game ends
	public int score = 0;	// Score made
	private float countDown = 3f;	// Count Down hits
	public bool gameWon = false;
	
	public Vector3 pos1 = Vector3.zero; // latest selected object's position 
	public Vector3 pos2 = Vector3.zero; // last selected object's position 
	public RaycastHit hit1, hit2, hit_latest;

	// Quad animation
	public GameObject BackQuadPrefab;
	public GameObject FrontQuadPrefab;
	public GameObject CodeBackQuadPrefab;
	public Material codeBackQuadMaterial;
	public Material codeFrontQuadMaterial;
	
	// GUI Elements
	private GUIStyle boxStyle1 = null;
	public int maxHealth = 100;
	public int curHealth = 100;
	public Texture2D bgImage; 
	public Texture2D fgImage; 
	public float healthBarLength;
	
	// TextCode Elements
	public GameObject TextCodePrefab;
	public int spaceBtwnLines = 46; // Controls distance of code lines
	
	// Audio related
	public AudioClip count_down;
	AudioSource loader_audio;
	float ticktimeExpired = 0.75f; // time expired till last tick; use to sync tick sound and digit change
	
	void Awake(){
		// Get components
		loader_audio = GetComponent<AudioSource>();
	}
	
	void OnGUI(){
		// Do a count down when !gameOn and !gameEnded
		// Play ticking sound as seconds value change
		if(!gameOn && !gameEnded){
			InitStyles();
			// Make a background box
			GUI.Box(new Rect(10,10,Screen.width - 20 ,Screen.height - 20), "<b><color=yellow><size=30>GAME STARTS IN</size></color></b>", boxStyle1);
			GUI.Label(new Rect(Screen.width/2 - 30,Screen.height/2 - 30,60,60), "<b><color=red><size=50>"+Mathf.RoundToInt(countDown).ToString()+"</size></color></b>");
			
			if(ticktimeExpired > 1f){
				loader_audio.clip = count_down;
				loader_audio.Play();
				ticktimeExpired -= 1;
			}
		}
		
		if(gameOn && !gameEnded){
			// Create one Group to contain both images
			// Adjust the first 2 coordinates to place it somewhere else on-screen
			GUI.BeginGroup (new Rect (20,20, healthBarLength,32));

			// Draw the background image
			GUI.Box (new Rect (0,0, healthBarLength,32), bgImage);

			// Create a second Group which will be clipped
			// We want to clip the image and not scale it, which is why we need the second Group
			GUI.BeginGroup (new Rect (0,0, healthBarLength, 32));

			// Draw the foreground image
			GUI.Box (new Rect (2,2,curHealth / maxHealth * healthBarLength - 4,28), "", boxStyle1);

			// End both Groups
			GUI.EndGroup ();

			GUI.EndGroup ();
			
			GUI.Box(new Rect(30 ,Screen.height - 40, Screen.width/2 - 60,30), "<b><color=yellow><size=20>Time Left: " + Mathf.RoundToInt(timeLeft).ToString() + "</size></color></b>", boxStyle1);
			GUI.Box(new Rect(Screen.width/2 + 30 ,Screen.height - 40, Screen.width/2 - 60,30), "<b><color=yellow><size=20>Steps Taken : " + steps.ToString() + "</size></color></b>", boxStyle1);
		}
		
		// After game ended Show GUI
		if(!gameOn && gameEnded){
			// Make a background box
			if(gameWon == true ){
				GUI.Box(new Rect(10,10,Screen.width - 20 ,Screen.height - 20), "<b><color=yellow><size=30>YOU WON THE CHALLENGE!</size></color></b>", boxStyle1);
			} else {
				GUI.Box(new Rect(10,10,Screen.width - 20 ,Screen.height - 20), "<b><color=yellow><size=30>YOU LOST THE CHALLENGE!</size></color></b>", boxStyle1);
			}
			if(GUI.Button(new Rect(Screen.width/2 - 100,Screen.height/2, 200 ,50),"<b><color=white><size=15>Restart</size></color></b>")){
				Application.LoadLevel("loadnplay");
			}
			if(GUI.Button(new Rect(Screen.width/2 - 100,Screen.height/2 + 60, 200 ,50),"<b><color=white><size=15>Go to Main Menu</size></color></b>")){
				Application.LoadLevel("startmenu");
			}
		}
	}
	
	public void AdjustCurrentHealth(int adj){
 
		curHealth += adj;
 
        if(curHealth <0)
          curHealth = 0;
 
        if(curHealth > maxHealth)
          curHealth = maxHealth;
 
        if(maxHealth <1)
          maxHealth = 1;
 
        healthBarLength =(Screen.width - 40) * (curHealth / (float)maxHealth);
     }
	
	private void InitStyles()
	{
		if( boxStyle1 == null )
		{
			boxStyle1 = new GUIStyle( GUI.skin.box );
			boxStyle1.normal.background = MakeTex( 2, 2, new Color( 1f, 0f, 0f, 0.25f ) );
		}
	}
	 
	private Texture2D MakeTex( int width, int height, Color col )
	{
		Color[] pix = new Color[width * height];
		for( int i = 0; i < pix.Length; ++i )
		{
			pix[ i ] = col;
		}
		Texture2D result = new Texture2D( width, height );
		result.SetPixels( pix );
		result.Apply();
		return result;
	}
	
	
	
	// Start Function
	void Start() {
		// Set time according to difficulty
		if (PlayerPrefs.GetString("difficulty") == "Easy"){
			Debug.Log("Difficulty Easy. Initializing . . .");
			timeLeft = 60f;
			countDown = 5f;
		} else if (PlayerPrefs.GetString("difficulty") == "Medium"){
			Debug.Log("Difficulty Medium. Initializing . . .");
			timeLeft = 40f;
			countDown = 3f;
		} else if (PlayerPrefs.GetString("difficulty") == "Hard"){
			Debug.Log("Difficulty Hard. Initializing . . .");
			timeLeft = 20f;
			countDown = 1f;
		} 
		// Start healthbar creation
		bgImage = MakeTex(2,2, new Color( 1f, 0f, 0f, 1f ));
		fgImage = MakeTex(2,2, new Color( 0f, 0f, 0f, 1f ));
		healthBarLength = Screen.width - 40;
		Resources.UnloadUnusedAssets();  //release the memory of previous texture loaded
		// Get filepath to read file according to level
		string filepath = Directory.GetCurrentDirectory() + "\\Assets\\Images\\"+PlayerPrefs.GetString("level")+".jpg";
		byte[] fileData = File.ReadAllBytes(filepath);
        tex = new Texture2D(2, 2);
        tex.LoadImage(fileData);
		mat.mainTexture = tex;
		BuildPieces();
		AddCodeLine();
		randomize();
	}
 
	void BuildPieces() {
		int rows = Mathf.RoundToInt(cols * aspect);
		Vector3 offset = Vector3.zero;
		offset.x = 0f;
		offset.y = -1.5f;
		float startX = offset.x;
		float uvWidth = 1.0f / cols;
		float uvHeight = 1.0f / rows;


		for (int i = 0; i < rows; i++) {
			for (int j = 0; j < cols; j++) {
				GameObject go = GameObject.CreatePrimitive (PrimitiveType.Quad);
				// Gameobject named according to row number 
				go.name = i.ToString();
				Transform t = go.transform;
				t.position = offset;
				t.localScale = new Vector3(3f, 3*cols*0.99f/rows, 1f);
				go.GetComponent<Renderer>().material = mat;
				
				Matrix[i,j] = new piece();
				Matrix[i,j].GameObject = go;
				Matrix[i,j].Original = offset;
				Matrix[i,j].Current = offset;

				Mesh mesh = go.GetComponent<MeshFilter>().mesh;
				Vector2[] uvs = mesh.uv;
				uvs[0] = new Vector2(j * uvWidth, i * uvHeight);
				uvs[3] = new Vector2(j * uvWidth, (i + 1) * uvHeight);
				uvs[1] = new Vector2((j + 1) * uvWidth, (i + 1) * uvHeight);
				uvs[2] = new Vector2((j + 1) * uvWidth, i * uvHeight);
				mesh.uv = uvs;
				offset.x += 1f;

				// Create a BackQuad as child of GameObject go
				GameObject backChild = Instantiate(BackQuadPrefab, new Vector3 (0,0,0), Quaternion.identity) as GameObject;
				// Static backChild ???
				backChild.transform.parent = go.transform;
				backChild.transform.position = go.transform.position + new Vector3(0,0,0.05f);
				backChild.transform.localScale = new Vector3 (1.01f,1.05f,1f);
				// Create a FrontQuad as child of GameObject go
				GameObject frontChild = Instantiate(FrontQuadPrefab, new Vector3 (0,0,0), Quaternion.identity) as GameObject;
				frontChild.transform.parent = go.transform;
				frontChild.transform.position = go.transform.position - new Vector3(0,0,0.05f);
				frontChild.transform.localScale = new Vector3 (1f,1f,1f);
				frontChild.SetActive(false);
				// Add a CodeBackQuad as a child of imageQuad to select code lines by clicking
				GameObject codeBackQuadChild = Instantiate(CodeBackQuadPrefab, new Vector3 (0,0,0), Quaternion.identity) as GameObject;
				codeBackQuadChild.transform.parent = go.transform;
				codeBackQuadChild.transform.position = go.transform.position + new Vector3(3.25f,0,0.05f); // controls position of CodeBackQuad
				codeBackQuadChild.transform.localScale = new Vector3 (1f,1f,1f);
			}
			offset.y += 3*cols*1f/rows + 0.01f;
			offset.x = startX;
		}
	}
	
	// Add code lines that will follow image pieces as they move up and down
	void AddCodeLine() {
		int rows = Mathf.RoundToInt(cols * aspect);
		// Read each line of the file into a string array. Each element
        // of the array is one line of the file.
        string[] lines = System.IO.File.ReadAllLines(Directory.GetCurrentDirectory() + "\\Assets\\Codes\\"+PlayerPrefs.GetString("level")+".txt");
		for (int i = 0; i < rows; i++) {
			GameObject go = Instantiate(TextCodePrefab, new Vector3 (520,440 - i*spaceBtwnLines,0), Quaternion.identity) as GameObject; 
			go.name = "Code" + i.ToString();
			go.transform.SetParent(GameObject.Find("CodeCanvas").transform);
			Text myText;
			myText = go.GetComponent <Text>();
			// 0th object is the lowest object
			myText.text  = lines[rows - 1 - i];
			ObjectLabel targetScript = go.GetComponent<ObjectLabel>();
			targetScript.target = GameObject.Find(i.ToString()).transform;
		}
	}
	
	void randomize(){
		start_random:
		int perfect_random = 0;
		for (int i = 0; i < cols*aspect; i++){
			int random_i = UnityEngine.Random.Range(0, (int)aspect*cols);
			if(Matrix[i,0].Current == Matrix[i,0].Original){
				MatrixSwap(i,random_i);
			} else{
				perfect_random += 1;
			}
		}
		if(perfect_random < cols*aspect){
			goto start_random;
		}
		
	}
	
	void MatrixSwap(int i, int j){
		Vector3 temp1 = Matrix[i,0].GameObject.transform.position;
		Vector3 temp2 = Matrix[j,0].GameObject.transform.position;
		Matrix[i,0].GameObject.transform.position = temp2;
		Matrix[i,0].Current = temp2;
		Matrix[j,0].GameObject.transform.position = temp1;
		Matrix[j,0].Current = temp1;
	}
	
	// Update is called once per frame
	void Update () {
		//Before game starts
		if(gameOn == false && gameEnded == false){
			countDown -= Time.deltaTime;
			ticktimeExpired += Time.deltaTime;
			if(countDown < 0){
				gameOn = true;
			}
		} else if (gameOn == true && gameEnded == false){ 
			// Decrease time counter
			timeLeft -= Time.deltaTime;
			// If time is over, player loses
			if(timeLeft < 0){
				gameOn = false;
				gameEnded = true;
			}
			
			// Modify score and bar
			AdjustCurrentHealth(1);
			
			if (Input.GetButtonDown("Fire1")) {
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				raycasting_run:
				if(Physics.Raycast(ray,out hit_latest,Mathf.Infinity)){
					//Debug.Log(hit_latest.transform.gameObject.name);
					// If CodeBackQuad is hit, rerun raycast using new ray with a new mouse position
					if (hit_latest.transform.gameObject.name == "CodeBackQuad(Clone)") {
						ray = Camera.main.ScreenPointToRay(new Vector3(200f,Input.mousePosition.y,0f));
						goto raycasting_run;
					}

					// Destroy(hit1.transform.gameObject);
					// if 1st click in game, initialize RaycastHit objects and Vector3 positions
					if(pos2 == Vector3.zero){
						hit1 = hit2 = hit_latest;
						pos1 = pos2 = hit_latest.transform.position;
						// highlight the selected imageQuad
						GameObject selected_go1 = hit1.transform.Find("FrontQuad(Clone)").gameObject;
						selected_go1.SetActive(true);
						// highlight selected code line
						hit1.transform.Find("CodeBackQuad(Clone)").gameObject.GetComponent<Renderer>().material = codeFrontQuadMaterial;
					} else {
						hit2 = hit1;
						hit1 = hit_latest;
						pos2 = pos1;
						pos1 = hit_latest.transform.position;
						// If same imageQuad is clicked twice
						if(pos1 == pos2){
							goto doubleClick_jump;
						}
						GameObject selected_go1 = hit1.transform.Find("FrontQuad(Clone)").gameObject;
						selected_go1.SetActive(false);
						GameObject selected_go2 = hit2.transform.Find("FrontQuad(Clone)").gameObject;
						selected_go2.SetActive(false);
						// UN-highlight selected code line
						hit2.transform.Find("CodeBackQuad(Clone)").gameObject.GetComponent<Renderer>().material = codeBackQuadMaterial;
					}

					doubleClick_jump:
					if(pos1 != pos2){
						swap();
						steps += 1;
					} else { // Clicking an object twice
						//hit1 = hit2 = null;
					}
					
				}
			}
			// check if game has been won
			CheckForVictory();
		} else if (gameOn == false && gameEnded == true){ 
		
		}
	}
	
	int CheckForVictory(){
		int matched = 0;
		for (int i = 0; i < cols*aspect; i++){
			try{
				if(Matrix[i,0].Current == Matrix[i,0].Original){
					matched += 1;
				}
			} catch(Exception exception)
			{
			   Debug.Log(exception.ToString() + i.ToString()); // Null Reference Exception appears for unknown reasons.
			}
		}

		if(matched == cols*aspect){
			gameOn = false;
			gameEnded = true;
			gameWon = true;
			return 1;
		}
		return 0;
	}

	public bool moving = false;// Determines if a Quad is moving right now or not
	IEnumerator startLerping(RaycastHit hit_first, RaycastHit hit_second, Vector3 pointA1, Vector3 pointB1, Vector3 pointA2, Vector3 pointB2, float time){
		if (!moving) {                     // Do nothing if already moving
	        moving = true;                 // Set flag to true
	        float t = 0f;
			while (t < 1.0f) {
				t += Time.deltaTime / time; // Sweeps from 0 to 1 in time seconds
				hit_first.transform.position = Vector3.Lerp(pointA1, pointB1, t); // Set position proportional to t
				hit_second.transform.position = Vector3.Lerp(pointA2, pointB2, t);
				yield return null;         // Leave the routine and return here in the next frame
			}
			moving = false;             // Finished moving
		}
	}
	
	void swap(){
		//hit2.transform.position = pos1;
		//hit1.transform.position = pos2;
		StartCoroutine(startLerping(hit1, hit2, hit1.transform.position, pos2, hit2.transform.position, pos1, 0.2f));

		// Modify current position of Matrix class after swap
		for(int temp = 0; temp < cols*aspect; temp++){
			try{
				if(Matrix[temp,0].Current == pos1){
					Matrix[temp,0].Current = pos2;
				} else if(Matrix[temp,0].Current == pos2){
					Matrix[temp,0].Current = pos1;
				} 
			} catch(Exception exception)
			{
			   Debug.Log(exception); // Null Reference Exception appears for unknown reasons.
			}
		}
		
		// re initialize
		pos1 = pos2 = Vector3.zero;
		hit_latest = hit1 = hit2;
	}
}
