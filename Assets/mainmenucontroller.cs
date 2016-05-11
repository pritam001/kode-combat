using UnityEngine;
using System.Collections;

public class mainmenucontroller : MonoBehaviour {
	// Stores all the button objects
	public InstantGuiButton[] mainmenu_btn;
	public AudioClip btnclick_audio;
	AudioSource btnaudio;
	// Use this for initialization
	void Start () {
		btnaudio = GetComponent<AudioSource>();
	}
	//do some action on button press:
	//use this script on button object
	void TestBtn()
	{
		//if (mainmenu_btn[1]==null) mainmenu_btn[1] = GetComponent(InstantGuiButton);
		int i;
		for(i=0;i<=5;i++){
			if (mainmenu_btn[i].activated)
			{
				Debug.Log(mainmenu_btn.Length);
				btnaudio.clip = btnclick_audio;
				btnaudio.Play();
			}
		}
	}
	// Update is called once per frame
	void Update () {
		TestBtn();
	}
}
