using UnityEngine;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Collections.Generic;
 

public class MyBrowser : MonoBehaviour {
	
	private string curr = Directory.GetCurrentDirectory();
	private string filename = null;
	private int selectionGridInt = -1 ;
	private Vector2 scrollViewVector = Vector2.zero;
	public Texture2D ControlTexture1;
	public Texture2D ControlTexture2;
	public Texture2D WindowBackgroundTex;
	public Texture2D tickTex;
	public Texture2D crossTex;
	private Rect modalRect = new Rect(100, 100, Screen.width - 200, Screen.height - 200);
	private bool showModal = false;
	Texture2D tex = null;
	
	
	void Awake() {
        DontDestroyOnLoad(tex);
    }
	
	void OnGUI () {
        // Make a background box
        GUI.Box(new Rect(10,10,Screen.width - 20 ,Screen.height - 20), "<b><color=yellow>File Browser</color></b>");
		GUIStyle gsSelGrid = new GUIStyle(GUI.skin.button); //Copy the Default style for buttons
		//gsSelGrid.fontSize = 9; //Change the Font size
		GUI.skin.button.fixedHeight = 30; // make button size for all buttons fixed
		gsSelGrid.richText = true;
		
		
		// Moving up the directory
		if(GUI.Button(new Rect(0,0,30,30), new GUIContent(ControlTexture1,"Go up the directory") )){
			print(Directory.GetParent(curr).FullName);
			curr = Directory.GetParent(curr).FullName;
			selectionGridInt = -1;
			//goto skip;
		}

		
		if(GUI.Button(new Rect(Screen.width - 30,0,30,30), new GUIContent(ControlTexture2,"Open file browser") )){
			print("Open explorer");
			OpenInFileBrowser.Open(curr);
		}
		
		GUI.Label(new Rect(Screen.width - 200,40,200,20), GUI.tooltip);
		
		GUI.Label(new Rect(40,30,Screen.width - 80,30), curr);
		string[] dir = Directory.GetDirectories(curr);
		int i = 0;
		foreach(string temp in dir){
			dir[i] = (Path.GetFileName(temp));
			i = i + 1; 
		}
		int numberOfFolders = i - 1;

		//filter all image types
		var filters = new String[] { "jpg", "jpeg", "png", "gif", "tiff", "bmp" };
		var files = GetFilesFrom( curr, filters, false);
		//string[] files = Directory.GetFiles(curr, "*.png");
		i = 0;
		foreach(string temp in files){
			files[i] = (Path.GetFileName(temp));
			i = i + 1; 
		}

		string[] all = dir.Concat(files).ToArray();
		
		scrollViewVector = GUI.BeginScrollView(new Rect(40,60,Screen.width - 80,Screen.height - 100),scrollViewVector,new Rect(0,0,Screen.width,12*(numberOfFolders + i)));
		selectionGridInt = GUI.SelectionGrid(new Rect(0,0,Screen.width, 40*(numberOfFolders + i) ),selectionGridInt, all, 3,gsSelGrid);
		GUI.EndScrollView();
		
		if(GUI.changed && selectionGridInt != -1 && selectionGridInt <= numberOfFolders){
			print(selectionGridInt);
			curr = curr + "\\" + dir[selectionGridInt];
			selectionGridInt = -1;
		} else if(GUI.changed && selectionGridInt != -1 && selectionGridInt > numberOfFolders){
			print("Image is selected. " + selectionGridInt);
			filename = curr + "\\" + all[selectionGridInt];
			showModal = true;
		}
		
		if(showModal){
			modalRect = GUI.ModalWindow(0, modalRect, ImageModalWindowFunction, "Image Preview");
		}
		
    }
	
	void ImageModalWindowFunction(int windowID){
		GUIStyle centeredStyle = new GUIStyle(GUI.skin.label);
		centeredStyle.alignment = TextAnchor.UpperCenter;
		
		GUI.DrawTexture(new Rect(-100,-100,Screen.width, Screen.height ), WindowBackgroundTex, ScaleMode.ScaleToFit, true, 1.0F);
		GUI.Label(new Rect(Screen.width/2 - 150, 10, 200, 30),"<color=blue><b> Image Preview : </b></color>");
		GUI.Box(new Rect(0, 0, Screen.width - 200, 40), "");
		
		
		byte[] fileData = File.ReadAllBytes(filename);
        tex = new Texture2D(2, 2);
        tex.LoadImage(fileData);
		
		GUI.Label(new Rect(20,40,Screen.width - 200,Screen.height - 300), new GUIContent(tex), centeredStyle);
		
		if (GUI.Button(new Rect(Screen.width/2 - 250, Screen.height - 250, 100, 20), new GUIContent("Select",tickTex))){
			print("Import to next scene");
			PlayerPrefs.SetString("filename", filename);
			Application.LoadLevel ("loadnplay"); 
		}
		if (GUI.Button(new Rect(Screen.width/2 - 50, Screen.height - 250, 100, 20), new GUIContent("Close",crossTex))){
			fileData = null;
			Resources.UnloadUnusedAssets();  //release the memory of previous texture loaded
			System.GC.Collect();
			selectionGridInt = -1;
			showModal =false;
		}
	}
	
	
	// Get certain types of file. Use GetFilesFrom(searchFolder, filters, false);
	public static String[] GetFilesFrom(String searchFolder, String[] filters, bool isRecursive){
		List<String> filesFound = new List<String>();
		var searchOption = isRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
		foreach (var filter in filters)
		{
		   filesFound.AddRange(Directory.GetFiles(searchFolder, String.Format("*.{0}", filter), searchOption));
		}
		return filesFound.ToArray();
	}
	
	//Use `OpenInFileBrowser.Open()` for a cross-platform way of opening any file/folder.
	public static class OpenInFileBrowser
	{
		public static bool IsInMacOS
		{
			get
			{
				return UnityEngine.SystemInfo.operatingSystem.IndexOf("Mac OS") != -1;
			}
		}
	 
		public static bool IsInWinOS
		{
			get
			{
				return UnityEngine.SystemInfo.operatingSystem.IndexOf("Windows") != -1;
			}
		}
	 
		[UnityEditor.MenuItem("Window/Test OpenInFileBrowser")]
		public static void Test()
		{
			Open(UnityEngine.Application.dataPath);
		}
	 
		public static void OpenInMac(string path)
		{
			bool openInsidesOfFolder = false;
	 
			// try mac
			string macPath = path.Replace("\\", "/"); // mac finder doesn't like backward slashes
	 
			if ( System.IO.Directory.Exists(macPath) ) // if path requested is a folder, automatically open insides of that folder
			{
				openInsidesOfFolder = true;
			}
	 
			if ( !macPath.StartsWith("\"") )
			{
				macPath = "\"" + macPath;
			}
	 
			if ( !macPath.EndsWith("\"") )
			{
				macPath = macPath + "\"";
			}
	 
			string arguments = (openInsidesOfFolder ? "" : "-R ") + macPath;
	 
			try
			{
				System.Diagnostics.Process.Start("open", arguments);
			}
			catch ( System.ComponentModel.Win32Exception e )
			{
				// tried to open mac finder in windows
				// just silently skip error
				// we currently have no platform define for the current OS we are in, so we resort to this
				e.HelpLink = ""; // do anything with this variable to silence warning about not using it
			}
		}
	 
		public static void OpenInWin(string path)
		{
			bool openInsidesOfFolder = false;
	 
			// try windows
			string winPath = path.Replace("/", "\\"); // windows explorer doesn't like forward slashes
	 
			if ( System.IO.Directory.Exists(winPath) ) // if path requested is a folder, automatically open insides of that folder
			{
				openInsidesOfFolder = true;
			}
	 
			try
			{
				System.Diagnostics.Process.Start("explorer.exe", (openInsidesOfFolder ? "/root," : "/select,") + winPath);
			}
			catch ( System.ComponentModel.Win32Exception e )
			{
				// tried to open win explorer in mac
				// just silently skip error
				// we currently have no platform define for the current OS we are in, so we resort to this
				e.HelpLink = ""; // do anything with this variable to silence warning about not using it
			}
		}
	 
		public static void Open(string path)
		{
			if ( IsInWinOS )
			{
				OpenInWin(path);
			}
			else if ( IsInMacOS )
			{
				OpenInMac(path);
			}
			else // couldn't determine OS
			{
				OpenInWin(path);
				OpenInMac(path);
			}
		}
	}
	
	/*private string url = null;
	private Texture2D newTex = null;
	IEnumerator Loadimage() {
		print("ok");
		url = "file:///" + getfilename(filename);
        WWW www = new WWW(url);
        yield return www;
        Renderer renderer = GetComponent<Renderer>();
        renderer.material.mainTexture = www.texture;
		newTex = www.texture;
    }
	
	public string getfilename(string filename){
		string temp = filename.Replace(@"\",@"/");
		print(url);
		return filename;
	}*/
	
	
	void Start(){
		
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
