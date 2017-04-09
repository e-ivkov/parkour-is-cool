using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.UI;

public class Dialog : MonoBehaviour {

	public Image Image;
	public Image Background;
	public Text Text;
	
	DateTime timer = DateTime.Now;
	bool shown = true;
	int seconds = 5;

	private void Start() {
		Hide();
	}

	private void FixedUpdate() {
		if ((DateTime.Now-timer).Seconds>=seconds) {
			Hide();
		}
	}

	public void Hide() {
		Image.enabled=false;
		Background.enabled=false;
		Text.enabled=false;
		shown=false;
	}
	public void Show(int sec_time=5) { //Очередь на показ
		Image.enabled=true;
		Background.enabled=true;
		Text.enabled=true;
		shown=true;
		timer=DateTime.Now;
		seconds=sec_time;
	}
}
