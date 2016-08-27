﻿using UnityEngine;
using System.Collections;

public class Weapon : MonoBehaviour {

	public float fireRate = 0;
	public int Damage = 10;
	public LayerMask whatToHit;  //it tells us what do we wanna hit

	//variable for particle prefab
	public Transform BulletTrailPrefab;
    public Transform HitPrefab;
	public Transform MuzzleFlashPrefab;
	float timeToSpawnEffect = 0;
	public float effectSpawnRate = 10;

    //handle camera shaking
    public float camShakeAmount = 0.05f;
    public float camShakeLength = 0.1f;
    CameraShake camShake;

    public string weaponShootSound = "DefaultShot";

	float timeToFire = 0;
	Transform firePoint;

    //caching
    AudioManager audioManager;

	// Use this for initialization
	void Awake () {
		firePoint = transform.FindChild ("FirePoint"); 
		if (firePoint == null) {
			Debug.LogError ("No firepoint");
		}
	}

    void Start()
    {
        //camShake = GameMaster.gm.GetComponent<CameraShake>();
        //if(camShake == null)
        //{
        //    Debug.LogError("No camera shake script found on GM object");
        //}
        audioManager = AudioManager.instance;
        if(audioManager == null)
        {
            Debug.LogError("No audio manager found in scene");
        }
    }
	
	// Update is called once per frame
	void Update () {
		
		if (fireRate == 0) {
			if (Input.GetButtonDown ("Fire1")) {		//or Input.GetKeyDown(KeyCode.Down);
				Shoot ();
			}
		}
		else {
			if (Input.GetButtonDown ("Fire1") && Time.time > timeToFire) {
				timeToFire = Time.time + 1 / fireRate;  //fire delay
				Shoot ();
			}
		}
	}

	void Shoot(){
		Vector2 mousePosition = new Vector2 (Camera.main.ScreenToWorldPoint(Input.mousePosition).x, Camera.main.ScreenToWorldPoint(Input.mousePosition).y);
		Vector2 firePointPosition = new Vector2 (firePoint.position.x , firePoint.position.y);
		RaycastHit2D hit = Physics2D.Raycast (firePointPosition, mousePosition - firePointPosition, 100, whatToHit);

		

		Debug.DrawLine (firePointPosition, (mousePosition - firePointPosition)*100, Color.cyan);
		if (hit.collider != null) {
			Debug.DrawLine (firePointPosition, hit.point, Color.red);

			Enemy enemy = hit.collider.GetComponent<Enemy> ();
			if (enemy != null) {
				enemy.DamageEnemy (Damage);
				Debug.Log ("We hit " + hit.collider.name + " and did " + Damage + " damage. ");
			}

		}

        if (Time.time >= timeToSpawnEffect)
        {  //limit number of bullet trails
            Vector3 hitPos;
            Vector4 hitNormal;

            if(hit.collider == null)
            {
                hitPos = (mousePosition - firePointPosition) * 30;
                hitNormal = new Vector3(9999, 9999, 9999);
            }else
            {
                hitPos = hit.point;
                hitNormal = hit.normal;
            }

            Effect(hitPos, hitNormal);
            timeToSpawnEffect = Time.time + 1 / effectSpawnRate;
        }
    }

	void Effect(Vector3 hitPosition, Vector3 hitNormal){
		Transform trail = Instantiate (BulletTrailPrefab, firePoint.position, firePoint.rotation) as Transform;   //create a bullet trail
        LineRenderer lr = trail.GetComponent<LineRenderer>();

        if(lr != null)
        {
            lr.SetPosition(0, firePoint.position);
            lr.SetPosition(1, hitPosition);
        }

        Destroy(trail.gameObject, 0.04f);

        if(hitNormal != new Vector3(9999, 9999, 9999))
        {
            Transform hitParticle = Instantiate(HitPrefab, hitPosition, Quaternion.FromToRotation(Vector3.right, hitNormal)) as Transform;
            Destroy(hitParticle.gameObject, 1f);
        }

        Transform clone = Instantiate (MuzzleFlashPrefab, firePoint.position, firePoint.rotation) as Transform;
		// OR Transform clone = (Transform) Instantiate (MuzzleFlashPrefab, firePoint.position, firePoint.rotation);
		clone.parent = firePoint;
		float size = Random.Range (0.6f, 0.9f);
		clone.localScale = new Vector3 (size , size, size );  // z doesn't matter
		Destroy (clone.gameObject, 0.02f);

        //shake the camera
        //camShake.Shake(camShakeAmount, camShakeLength);

        //play shoot sound
        audioManager.PlaySound(weaponShootSound);
	}
}