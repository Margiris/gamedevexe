﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    [SerializeField]
    float speed;
    [SerializeField]
    GameObject pistolBullet;
    [SerializeField]
    AudioSource pistolFire;

    public List<Explosive> GrenadeList;
    Explosive selectedGrenade;
    public float throwForce = 5000f;

    private Rigidbody2D rb2D;
    private float directionAngle;

    // Use this for initialization
    void Start () {
        rb2D = GetComponent<Rigidbody2D>();
        if (GrenadeList.Count > 0)
            selectedGrenade = GrenadeList[0];
    }

    // Update is called once per frame
    void FixedUpdate () {
        Movement();
        Looking();
	}
    private void Throw()
    {
        float x = transform.position.x;
        float y = transform.position.y;
        float z = transform.position.z;
        Debug.Log("throw");
        if (selectedGrenade != null)
        {
            Instantiate(selectedGrenade, new Vector3(x, y, z), transform.rotation);
            Vector2 playerPos = Camera.main.WorldToViewportPoint(transform.position);
            Vector2 mousePos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
            Vector2 finalThrowForce = new Vector2(0, throwForce);
            selectedGrenade.GetComponent<Rigidbody2D>().AddForce(finalThrowForce);
        }
    }
    
    private void Movement()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");
        gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(moveX * speed, moveY * speed);
        //rb2D.AddForce(move * speed);
    }

    private void Looking()
    {
        Vector2 playerPos = Camera.main.WorldToViewportPoint(transform.position);
        Vector2 mousePos = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        directionAngle = AngleBetweenToPoints(playerPos, mousePos);
        transform.rotation = Quaternion.Euler(new Vector3(0f, 0f, directionAngle * Mathf.Rad2Deg + 90));
        if (Input.GetMouseButtonDown(0))
            Shooting();
        if (Input.GetKeyDown(KeyCode.E))
            Throw();

    }

    public float GetDirectionAngle()
    {
        //return AngleBetweenToPoints(transform.position, Input.mousePosition) + 90;
        return directionAngle;
    }

    private void Shooting()
    {
        //sometimes bullet spawns behind the player :D
        if (GameObject.FindGameObjectWithTag("PlayerWeaponSlot").GetComponent<WeaponManager>().cooldownEnded && GameObject.FindGameObjectWithTag("PlayerWeaponSlot").GetComponent<WeaponManager>().activeWeapon != null)
        {
            pistolFire.Play();
            float x = GameObject.FindGameObjectWithTag("PlayerWeaponSlot").transform.position.x;
            float y = GameObject.FindGameObjectWithTag("PlayerWeaponSlot").transform.position.y;
            float z = GameObject.FindGameObjectWithTag("PlayerWeaponSlot").transform.position.z;
            GameObject.FindGameObjectWithTag("PlayerWeaponSlot").GetComponent<WeaponManager>().cooldownEnded = false;
            StartCoroutine("Cooldown");
            Instantiate(pistolBullet, new Vector3(x, y, z), transform.rotation);
        }
    }

    private float AngleBetweenToPoints(Vector3 a, Vector3 b)
    {
        return Mathf.Atan2(a.y - b.y, a.x - b.x);
    }

    IEnumerator Cooldown()
    {
        yield return new WaitForSeconds(GameObject.FindGameObjectWithTag("PlayerWeaponSlot").GetComponent<WeaponManager>().activeWeapon.GetComponent<Weapon>().cooldown);
        GameObject.FindGameObjectWithTag("PlayerWeaponSlot").GetComponent<WeaponManager>().cooldownEnded = true;
    }
}
