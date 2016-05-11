using UnityEngine;
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
	private float countDown = 6f;	// Count Down hits
	public bool gameWon = false;
	
	private Vector3 pos1 = Vector3.zero; // latest selected object's position 
	private Vector3 pos2 = Vector3.zero; // last selected object's position 
	private RaycastHit hit1, hit2, hit_latest;
	
	// GUI Elements
	private GUIStyle boxStyle1 = null;
	public int maxHealth = 100;
	public int curHealth = 100;
	public Texture2D bgImage; 
	public Texture2D fgImage; 
	public float healthBarLength;
	
	// Audio related
	public AudioClip count_down;
	AudioSource loader_audio;
	float ticktimeExpired = 0.99f; // time expired till last tick
	
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
		bgImage = MakeTex(2,2, new Color( 1f, 0f, 0f, 1f ));
		fgImage = MakeTex(2,2, new Color( 0f, 0f, 0f, 1f ));
		healthBarLength = Screen.width - 40;
		Resources.UnloadUnusedAssets();  //release the memory of previous texture loaded
		string filepath = PlayerPrefs.GetString("filename");
		byte[] fileData = File.ReadAllBytes(filepath);
        tex = new Texture2D(2, 2);
        tex.LoadImage(fileData);
		mat.mainTexture = tex;
		BuildPieces();
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
			}
			offset.y += 3*cols*1f/rows + 0.01f;
			offset.x = startX;
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
			
			//Modify score and bar
			AdjustCurrentHealth(1);
			
			if (Input.GetButtonDown("Fire1")) {
				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				if(Physics.Raycast(ray,out hit_latest,Mathf.Infinity)){
					//Destroy(hit1.transform.gameObject);
					// if 1st click in game, initialize RaycastHit objects and Vector3 positions
					if(pos2 == Vector3.zero){
						hit1 = hit2 = hit_latest;
						pos1 = pos2 = hit_latest.transform.position;
					} else {
						hit2 = hit1;
						hit1 = hit_latest;
						pos2 = pos1;
						pos1 = hit_latest.transform.position;
					}
					
					if(pos1 != pos2){
						swap();
						steps += 1;
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
	
	void swap(){
		hit2.transform.position = pos1;
		hit1.transform.position = pos2;
		
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
