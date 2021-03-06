﻿using UnityEngine;
using System.Collections;

public class PlayerControl : MonoBehaviour {
	
	public float speed;

	private static Animator playerAnim;

	public static float speedBoost;

	public static bool immobile = false;

	public static bool rolling = false;

	private bool comboTime = false;

	private bool swordAttack1, swordAttack2, swordAttack3;

	private static Transform camTransform;

	private static Player playerref;
	
	private static LineRenderer rangedIndicator;

	private float mouseAngle = 0f;

	private float groundY = 0f;
	
	// Use this for initialization
	void Start () {
		playerAnim = this.GetComponent<Animator>();
		playerref = this.GetComponent<Player>();
		rangedIndicator = GameObject.Find("RangedAimIndicator").GetComponent<LineRenderer>();
		camTransform = transform.parent.GetChild(1).transform;
		rangedIndicator.enabled = false;
	}
	
	// Update is called once per frame
	void FixedUpdate () {
		/****** Resetting animation booleans to false ****/
		playerAnim.SetBool("Slash1", false);
		playerAnim.SetBool("Slash2", false);
		playerAnim.SetBool("Slash3", false);
		playerAnim.SetBool("ShootBow", false);
		playerAnim.SetBool("ShootHandgun", false);
		playerAnim.SetBool("ShootMediumGun", false);

		/***** Updates booleans to check what attack player is in *****/
		swordAttack1 = playerAnim.GetCurrentAnimatorStateInfo(0).IsName("Base.Slash1") || playerAnim.GetCurrentAnimatorStateInfo(1).IsName("RightHandLayer.SlashWalking");
		swordAttack2 = playerAnim.GetCurrentAnimatorStateInfo(0).IsName("Base.Slash2");
		swordAttack3 = playerAnim.GetCurrentAnimatorStateInfo(0).IsName("Base.Slash3");

		if(!rolling && playerAnim.GetCurrentAnimatorStateInfo(0).IsName("Base.Roll")) {
			FMOD_StudioSystem.instance.PlayOneShot("event:/player/playerRoll", transform.position,PlayerPrefs.GetFloat("MasterVolume")/2f);
		}

		rolling = playerAnim.GetCurrentAnimatorStateInfo(0).IsName("Base.Roll");
		playerref.rolling = rolling;

		/****** Set movement variables *****/
		if(!immobile) {
			if(Input.GetKey(PersistentInfo.forwardKey)) {
				playerAnim.SetFloat("Speed",Mathf.MoveTowards(playerAnim.GetFloat("Speed"),1,Time.deltaTime*10f));
			} else if(Input.GetKey(PersistentInfo.backKey)) {
				playerAnim.SetFloat("Speed",Mathf.MoveTowards(playerAnim.GetFloat("Speed"),-1,Time.deltaTime*10f));
			} else {
				playerAnim.SetFloat("Speed",Mathf.MoveTowards(playerAnim.GetFloat("Speed"),0,Time.deltaTime*10f));
			}
//			playerAnim.SetFloat("Speed",(Input.GetKey(PersistentInfo.forwardKey) ? 1 : 0));
//			playerAnim.SetFloat("Speed",(Input.GetKey(PersistentInfo.backKey) ? -1 : playerAnim.GetFloat("Speed")));
		} else {
			playerAnim.SetFloat("Speed",0);
		}

//		playerAnim.SetFloat("Direction",Input.GetAxis("Horizontal"));
		if(playerAnim.GetFloat("Speed") == 0) {
			playerAnim.transform.Rotate(new Vector3(0f, playerAnim.GetFloat("Direction"), 0f));
		}

		/***** Rolling functionality ****/
		if(Input.GetKey(PersistentInfo.rollKey)) {
			playerAnim.SetBool("Roll", true);
		} else {
			playerAnim.SetBool("Roll", false);
		}

		/***** Running functionality ****/
		if (playerAnim.GetFloat("Speed") > 0.5f && Input.GetKey(PersistentInfo.sprintKey)) {
			if(playerAnim.speed < 1.5f + speedBoost) {
				playerAnim.speed += Time.deltaTime/2f;
			}
		} else {
			playerAnim.speed = 1 + speedBoost;
		}
		//We don't want people rolling under buildings, so no collider shrinking on roll
		//GetComponent<CapsuleCollider>().height = 0.1787377f - 0.1f*playerAnim.GetFloat("ColliderHeight");
		//GetComponent<CapsuleCollider>().center = new Vector3(0,0.09f - 0.09f*playerAnim.GetFloat("ColliderY"), 0f);


		/**** Simple front collision handler *****/
		if(playerAnim.GetFloat("Speed") > 0.5f) {
			RaycastHit info = new RaycastHit();
			if(Physics.Raycast(new Ray(this.transform.position + new Vector3(0f, 1f, 0f), this.transform.forward), out info, playerAnim.GetFloat("Speed")*1.5f)) {
				if(!info.collider.gameObject.name.Equals("Byte")) {
					playerAnim.SetFloat("Speed",Mathf.Min((Input.GetKey(PersistentInfo.forwardKey) ? 1 : 0),0));
				}
			}
		}

		/**** Messy way of handling LightStick's green trail ****/
		if(swordAttack2) {
			GetComponent<Player>().StartAttack();
		} else {
			GetComponent<Player>().StopAttack();
		}

		/***** Kinda messy way of binding numbers to quick access items ****/
		if(Input.GetKeyDown(KeyCode.Alpha1)) {
			playerref.SetActiveItem(0);
		} else if (Input.GetKeyDown(KeyCode.Alpha2)) {
			playerref.SetActiveItem(1);
		} else if(Input.GetKeyDown(KeyCode.Alpha3)) {
			playerref.SetActiveItem(2);
		} else if (Input.GetKeyDown(KeyCode.Alpha4)) {
			playerref.SetActiveItem(3);
		} else if (Input.GetKeyDown(KeyCode.Alpha5)) {
			playerref.SetActiveItem(4);
		} else if(Input.GetKeyDown(KeyCode.Alpha6)) {
			playerref.SetActiveItem(5);
		} else if (Input.GetKeyDown(KeyCode.Alpha7)) {
			playerref.SetActiveItem(6);
		} else if (Input.GetKeyDown(KeyCode.Alpha8)) {
			playerref.SetActiveItem(7);
		} else if(Input.GetKeyDown(KeyCode.Alpha9)) {
			playerref.SetActiveItem(8);
		} else if (Input.GetKeyDown(KeyCode.Alpha0)) {
			playerref.SetActiveItem(9);
		}

		/**** Handling attacking ****/
		if(!PlayerCanvas.inConsole && !immobile) {
			if (playerref.GetWeapon() != null && playerref.GetWeapon().Type().Equals(WeaponType.Melee)) {
				playerAnim.SetBool("HoldingMediumGun", false);
//				Debug.Log(swordAttack1 + " " + swordAttack2 + " " + swordAttack3);

				if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(PersistentInfo.attackKey)) && comboTime && !swordAttack1 && swordAttack2) {
					playerAnim.SetBool("Slash3", true);
					comboTime = false;
				}

				if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(PersistentInfo.attackKey)) && comboTime && swordAttack1 && !swordAttack2) {
					playerAnim.SetBool("Slash2", true);
					comboTime = false;
				}

				if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(PersistentInfo.attackKey)) && !swordAttack2 && !swordAttack2) {
					playerAnim.SetBool("Slash1", true);
//					Debug.Log(playerAnim.GetFloat("Speed"));
					comboTime = false;
				}
			} else if (playerref.GetWeapon() != null && playerref.GetWeapon().Type().Equals(WeaponType.Bow)) {
				playerAnim.SetBool("HoldingMediumGun", false);
				if ((Input.GetMouseButtonDown(0) || Input.GetKeyDown(PersistentInfo.attackKey))) {
					if(playerAnim.GetFloat("Speed") < 0.2f) {
						rangedIndicator.enabled = true;
						FMOD_StudioSystem.instance.PlayOneShot("event:/weapons/bowDraw",transform.position,PlayerPrefs.GetFloat("MasterVolume"));
					}
					playerAnim.SetBool("DrawArrow", true);
				} else if ((Input.GetMouseButtonUp(0) || Input.GetKeyUp(PersistentInfo.attackKey))) {
					rangedIndicator.enabled = false;
					if(playerAnim.GetFloat("Speed") < 0.2f) {
						FMOD_StudioSystem.instance.PlayOneShot("event:/weapons/arrowFire",transform.position,PlayerPrefs.GetFloat("MasterVolume"));
					}
					playerAnim.SetBool("DrawArrow", false);
					playerAnim.SetBool("ShootBow", true);
				}

				if(!(Input.GetMouseButton(0) || Input.GetKey(PersistentInfo.attackKey))) {
					rangedIndicator.enabled = false;
					playerAnim.SetBool("DrawArrow", false);
				}
			} else if (playerref.GetWeapon() != null && playerref.GetWeapon().Type().Equals(WeaponType.Handgun)) {
				playerAnim.SetBool("HoldingMediumGun", false);
				if((Input.GetMouseButton(0) || Input.GetKey(PersistentInfo.attackKey))) {
					playerAnim.SetBool("ShootHandgun", true);
					rangedIndicator.enabled = true;
					if(playerref.CanAttack()) {
						playerAnim.SetBool("ContinuedShooting", true);
					} else {
						playerAnim.SetBool("ContinuedShooting", false);
					}
				} else {
					rangedIndicator.enabled = false;
					playerAnim.SetBool("ShootHandgun", false);
				}
			} else if (playerref.GetWeapon() != null && playerref.GetWeapon().Type().Equals(WeaponType.MediumGun)) {
				playerAnim.SetBool("HoldingMediumGun", true);
				if((Input.GetMouseButton(0) || Input.GetKey(PersistentInfo.attackKey)) && playerref.CanAttack()) {
					rangedIndicator.enabled = true;
					playerAnim.SetBool("ShootMediumGun", true);
				} else if (!(Input.GetMouseButton(0) || Input.GetKey(PersistentInfo.attackKey))) {
					rangedIndicator.enabled = false;
					playerAnim.SetBool("ShootMediumGun", false);
				}
			} else if(playerref.GetWeapon() != null && playerref.GetWeapon().Type().Equals(WeaponType.Dagger)) {
				if((Input.GetMouseButton(0) || Input.GetKey(PersistentInfo.attackKey))) {
					playerAnim.SetBool("DaggerSlash",true);
				} else {
					playerAnim.SetBool("DaggerSlash",false);
				}
			}

			if (playerref.GetHack() != null && (Input.GetMouseButton(1) || Input.GetKey(PersistentInfo.hackKey))) {
				playerref.Hack();
			}
		}

		/****** Making the player look at the mouse *******/
		float mousePosX = Input.mousePosition.x;
		float mousePosY = Input.mousePosition.y + Screen.height/10f;
		float screenX = Screen.width;
		float screenY = Screen.height;
//		Transform temp = 
//		if (mousePosY < screenY/2 && !rolling) {
//			mouseAngle = Mathf.Rad2Deg * Mathf.Atan(((mousePosX/screenX*2) - 1)/((mousePosY/screenY*2) - 1)) + 180;
//		} else if (!rolling) {
//			mouseAngle = Mathf.Rad2Deg * Mathf.Atan(((mousePosX/screenX*2) - 1)/((mousePosY/screenY*2) - 1));
//		}
//		mouseAngle = temp.eulerAngles.y;
		float tempRot = 0f;
		if(Mathf.Abs(playerAnim.GetFloat("Speed")) < 0.1f && !immobile) {
//			Cursor.lockState = CursorLockMode.None;
			transform.LookAt(MouseTracker.mouseHoverTransform);
			transform.eulerAngles = new Vector3(0f, transform.eulerAngles.y, 0f);
//			if(rolling) {
//				tempRot = Mathf.MoveTowardsAngle(transform.eulerAngles.y, mouseAngle + camTransform.eulerAngles.y + 15f, Time.deltaTime*500f);
//			} else {
//				tempRot = Mathf.MoveTowardsAngle(transform.eulerAngles.y, mouseAngle + camTransform.eulerAngles.y, Time.deltaTime*500f);
//			}
		} else {
//			Cursor.lockState = CursorLockMode.Locked;
			if(rolling) {
				tempRot = Mathf.MoveTowardsAngle(transform.eulerAngles.y, camTransform.eulerAngles.y + 15f, Time.deltaTime*500f);
			} else {
				tempRot = Mathf.MoveTowardsAngle(transform.eulerAngles.y, camTransform.eulerAngles.y, Time.deltaTime*500f);
			}
			transform.eulerAngles = new Vector3(0f, tempRot, 0f);
		}

//		if(Mathf.Abs(playerAnim.GetFloat("Speed")) < 0.1f && !immobile) {
//			if (mousePosY < screenY/2 && !rolling) {
//				mouseAngle = Mathf.Rad2Deg * Mathf.Atan(((mousePosX/screenX*2) - 1)/((mousePosY/screenY*2) - 1)) + 180;
//			} else if (!rolling) {
//				mouseAngle = Mathf.Rad2Deg * Mathf.Atan(((mousePosX/screenX*2) - 1)/((mousePosY/screenY*2) - 1));
//			}
//			if(rolling) {
//				transform.eulerAngles = Vector3.RotateTowards(transform.eulerAngles, new Vector3(0f, mouseAngle + camTransform.eulerAngles.y + 15f, 0f), Time.deltaTime*150f,0f);
//			} else {
//				transform.eulerAngles = Vector3.RotateTowards(transform.eulerAngles, new Vector3(0f, mouseAngle + camTransform.eulerAngles.y, 0f), Time.deltaTime*150f,0f);
//				Debug.Log(transform.eulerAngles);
//			}
//		} else {
//			if(rolling) {
//				transform.eulerAngles = Vector3.RotateTowards(transform.eulerAngles, new Vector3(0f, camTransform.eulerAngles.y + 15f, 0f), Time.deltaTime*150f,0f);
//			} else {
//				transform.eulerAngles = Vector3.RotateTowards(transform.eulerAngles, new Vector3(0f, camTransform.eulerAngles.y, 0f), Time.deltaTime*15000f,0f);
//			}
//		}



		if(transform.position.y > groundY) {
			transform.position = new Vector3(transform.position.x,groundY,transform.position.z);
		}
}

	/**
	 * Called by Mecanim to set when combo attacks can and can't occur
	 */
	void SetComboTime() {
		comboTime = !comboTime;
	}

	public void PlayFootStepSound() {
		FMOD_StudioSystem.instance.PlayOneShot("event:/player/playerFootsteps",transform.position,PlayerPrefs.GetFloat("MasterVolume"));
	}

	public void PlaySwordSlashSound() {
		FMOD_StudioSystem.instance.PlayOneShot("event:/weapons/lightsaber",transform.position,PlayerPrefs.GetFloat("MasterVolume")/4f);
	}

	void OnCollisionEnter(Collision other) {
		if(other.gameObject.tag.Equals("Ground") && groundY == 0) {
			groundY = transform.position.y;
		}
	}

}
