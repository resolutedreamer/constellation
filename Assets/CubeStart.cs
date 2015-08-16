﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using SimpleJSON; 	
using System;

public class CubeStart : MonoBehaviour {
	public static int speed = 142;
	public static int maxCharsPerLine = 15;
	public static float sphereRadius = 1.0f;

	public GameObject player;
	public SpringJoint radiusSpring;
	public TextMesh text;
	public TextMesh text2;
	public string assocText;
	public Text suggestText;

	private bool launchDone = false;
	private Rigidbody rb;
	private BoxCollider bc;
	

	// Use this for initialization
	void Start () {
		rb = GetComponent<Rigidbody> ();
		rb.AddForce (this.transform.forward * speed);
		radiusSpring.spring = 0f;

		// format the text so that it fits in a nice box
		string finalText = FormatText (assocText);
		text.text = finalText;
		text2.text = finalText;

		// we want to add a collider that fits exactly around the text.
		Bounds textBounds = text.GetComponent<Renderer> ().bounds;
		this.transform.position = textBounds.center;

		bc = this.transform.FindChild("Cube").GetComponent<BoxCollider> ();
		bc.size = new Vector3 (textBounds.extents.x * 2, textBounds.extents.y * 2, 0f);
		launchDone = true;

		StartCoroutine("ProcessText");
	}
	
	// Update is called once per frame
	void Update () {
		if ((player.transform.position - this.transform.position).magnitude > sphereRadius) {
			radiusSpring.minDistance = sphereRadius - .4f;
			radiusSpring.maxDistance = sphereRadius + .4f;
			radiusSpring.spring = 200f;
		}

		// FOR TESTING ONLY
		if(Input.GetKeyDown("right")) {
			rb.AddForce(new Vector3(30, 0, 0));
		}
		// FOR TESTING ONLY
		if(Input.GetKeyDown("left")) {
			rb.AddForce(new Vector3(-30, 0, 0));
		}
		// FOR TESTING ONLY
		if(Input.GetKeyDown("up")) {
			rb.AddForce(new Vector3(0, 30, 0));
		}

		// FOR TESTING ONLY
		if(Input.GetKeyDown("down")) {
			rb.AddForce(new Vector3(0, -30, 0));
		}
	}
	void OnTriggerEnter(Collider col) {
		Debug.Log ("hi");
		// then add a spring that acts as a rigid rod to keep them tied together
		SpringJoint newSpring = this.transform.gameObject.AddComponent<SpringJoint> ();
		newSpring.connectedBody = col.gameObject.GetComponent<Rigidbody>();
		newSpring.minDistance = 0.02f;
		newSpring.maxDistance = 0.08f;
		newSpring.spring = 900f;
	}
	void OnCollisionEnter (Collision col) {
		// TODO (zliu): we probably have to add logic here to make sure the spring is only
		// added if the user lets go of the object (Kinect integration).
		
		// TODO: while it's being held on top of the other object, change text color to green
		// TODO: wait until release
		// TODO: on release, change text color back to white
		
		// on release, add a repulsive force between the objects -- or just use spring??

		// then add a spring that acts as a rigid rod to keep them tied together
		SpringJoint newSpring = this.transform.gameObject.AddComponent<SpringJoint> ();
		Debug.Log (col.gameObject.name);
		newSpring.connectedBody = col.gameObject.GetComponent<Rigidbody>();
		newSpring.minDistance = 0.02f;
		newSpring.maxDistance = 0.08f;
		newSpring.spring = 900f;
		
		// also add a visible line that always connects the centers. the connection code is in 
		// the line object's script
	}


	public static string FormatText(string assocText) {
		char[] delim = {' '};
		string[] words = assocText.Split (delim);
		int currLineLen = 0;
		string finalStr = "";
		foreach ( string word in words) {
			if (word.Length + currLineLen > maxCharsPerLine) {
				finalStr += "\n" + word + " ";
				currLineLen = 0;
			} else {
				finalStr += word + " ";
				currLineLen += word.Length;
			}
		}
		return finalStr;
	}

	IEnumerator ProcessText() {
		string url = "http://localhost:5000/interpret/" + System.Uri.EscapeUriString(assocText);
		WWW www = new WWW (url);
		string suggestStr;
		Debug.Log ("1" + url);
		yield return www;
		if (www.error == null) {
			Debug.Log(url);
			string jsonStr = www.data;
			JSONNode jsn = JSON.Parse(jsonStr);
			suggestStr = jsn["0"]["abstract"];
			if (suggestStr.Length < 1) {
				suggestStr = jsn["definitions"]["0"]["definition"];
			}
			suggestText.text = suggestStr;
		} else {
			Debug.Log("ERROR: " + www.error);
		}
	}
}
