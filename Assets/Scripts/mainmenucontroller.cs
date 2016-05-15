using UnityEngine;
using System.IO;
using System.Collections;

public class mainmenucontroller : MonoBehaviour {
	// Stores all the button objects to make click sound
	public InstantGuiButton[] mainmenu_btn;
	// Store all the save buttons
	public InstantGuiButton[] save_btn;
	public InstantGuiList list_level;
	public InstantGuiList list_difficulty;
	public AudioClip btnclick_audio;
	AudioSource btnaudio;
	string filename;
	// Use this for initialization
	void Start () {
		btnaudio = GetComponent<AudioSource>();
	}
	//do some action on button press:
	//use this script on button object
	void TestBtn()
	{
		//if (mainmenu_btn[1]==null) mainmenu_btn[1] = GetComponent(InstantGuiButton);
		// check if any button is clicked
		int i;
		for(i=0;i<mainmenu_btn.Length;i++){
			if (mainmenu_btn[i].activated)
			{
				btnaudio.clip = btnclick_audio;
				btnaudio.Play();
			}
		}
		
		// When start is clicked, load appropriate filename of the image and load level
		if (mainmenu_btn[1].activated){
			filename = Directory.GetCurrentDirectory() + "\\Assets\\Images\\1.jpg";
			Debug.Log(filename);
			PlayerPrefs.SetString("filename", filename);
			Application.LoadLevel("loadnplay");
		}
		
		// Exit on click
		if (mainmenu_btn[5].activated){
			Application.Quit();
		}
		
		// If Game Settings Save button is clicked
		if (save_btn[0].activated) {
			PlayerPrefs.SetString("difficulty", list_difficulty.labels[list_difficulty.selected]);
			Debug.Log("Set difficulty : " + list_difficulty.labels[list_difficulty.selected]);
			PlayerPrefs.SetString("level", list_level.labels[list_level.selected]);
			Debug.Log("Set level : " + list_level.labels[list_level.selected]);
		}
	}
	// Update is called once per frame
	void Update () {
		TestBtn();
	}
}
