using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class EnemyHPBar : NetworkBehaviour {

    public Slider HpSlider;
    float height;
    Character EnemyCharacter;
    GameObject EnemyObject;
    Transform canvas;

    Vector3 EnemyWorldPos;
    Quaternion rotation;
    void Start()
    {
        canvas = transform.Find("EnemyCanvas");
        EnemyObject = gameObject;
        height = canvas.position.y - transform.position.y;
        rotation = canvas.rotation;
    }
    public void Initiate()
    {
        EnemyCharacter = gameObject.GetComponent<Character>(); //somewhy nullreference when using EnemyObject
        HpSlider.maxValue = EnemyCharacter.healthMax;
        HpSlider.value = EnemyCharacter.healthCurrent;
        HpSlider.gameObject.SetActive(false);
    }
    void LateUpdate()
    {
        canvas.transform.position = new Vector3(EnemyObject.transform.position.x, EnemyObject.transform.position.y + height, 0);
        canvas.transform.rotation = rotation;
    }

    [ClientRpc]
    public void RpcSetHealth(float value)
    {
        if (!HpSlider.gameObject.activeInHierarchy && HpSlider.maxValue > value)
        {
            HpSlider.gameObject.SetActive(true);
        }
        HpSlider.value = value;
    }
}
