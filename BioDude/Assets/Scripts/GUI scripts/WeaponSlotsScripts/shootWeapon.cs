using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.EventSystems;

public class shootWeapon : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    private WeaponManager weaponManager;
    private bool isTryingAuto;
    private player pl;

    // Use this for initialization
    void Start()
    {
        weaponManager = GameObject.FindGameObjectWithTag("Player").GetComponent<WeaponManager>();
        pl = GameObject.FindGameObjectWithTag("Player").GetComponent<player>();
        isTryingAuto = false;
    }

    void Update()
    {
        if (isTryingAuto)
        {
            weaponManager.AutomaticShoot();
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        pl.Turning();
        weaponManager.Shoot();
        isTryingAuto = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isTryingAuto = false;
    }
}