using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class selectWeaponSlot2 : MonoBehaviour, IPointerDownHandler
{
    private WeaponManager weaponManager;

    // Use this for initialization
    void Start()
    {
        weaponManager = GameObject.FindGameObjectWithTag("Player").GetComponent<WeaponManager>();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        weaponManager.SelectWeaponByIndex(1);
    }
}