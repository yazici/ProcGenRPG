﻿using UnityEngine;
using System.Collections;

public class MouseTracker : MonoBehaviour {

	public bool aim = false;

	private static GameObject mouseHover;

	public static Transform mouseHoverTransform;

	// Use this for initialization
	void Start () {
		mouseHover = GameObject.Find("MouseHoverPos");
		mouseHoverTransform = mouseHover.transform;
	}
	
	// Update is called once per frame
	void Update () {
		if(!aim) {
			RaycastHit[] inf = Physics.RaycastAll(new Ray(Camera.main.ScreenPointToRay(Input.mousePosition).origin, Camera.main.ScreenPointToRay(Input.mousePosition).direction * 100f));
			bool groundFound = false;
			foreach(RaycastHit rH in inf) {
				if(rH.collider.gameObject.tag.Equals("Ground") && ((!rH.collider.gameObject.name.Equals("DefinitelyLavaStuff")) || (rH.collider.gameObject.name.Equals("DefinitelyLavaStuff") && !groundFound))) {
					groundFound = true;
	//				float tempDist = Vector3.Distance(Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, inf.distance)), Player.playerPos.position);
					transform.position = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, rH.distance));
					transform.position = new Vector3(transform.position.x, 0.1f, transform.position.z);
	//				transform.position = Player.playerPos.position + Player.playerPos.forward*tempDist + Vector3.up/20f;
		//			Debug.DrawRay(Player.playerPos.position + Vector3.up, Player.playerPos.forward*10f);
	//				Debug.Log(tempDist);
				}
			}
		} else {
			transform.LookAt(mouseHover.transform);
		}
	}
}