using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHPBar : MonoBehaviour {

    public Slider HpSlider;
    float height;
    Character EnemyCharacter;
    GameObject EnemyObject;

    Vector3 EnemyWorldPos;
    Quaternion rotation;
    void Start()
    {
        EnemyObject = gameObject.transform.parent.gameObject;
        height = gameObject.transform.position.y - EnemyObject.transform.position.y;
        rotation = transform.rotation;
    }
    public void Initiate()
    {
        EnemyCharacter = gameObject.transform.parent.gameObject.GetComponent<Character>(); //somewhy nullreference when using EnemyObject
        HpSlider.maxValue = EnemyCharacter.healthMax;
        HpSlider.value = EnemyCharacter.healthCurrent;
        HpSlider.gameObject.SetActive(false);
    }
    void LateUpdate()
    {
        transform.position = new Vector3(EnemyObject.transform.position.x, EnemyObject.transform.position.y + height, 0);
        transform.rotation = rotation;
    }
    public void SetHealth(float value)
    {
        if (!HpSlider.gameObject.activeInHierarchy && HpSlider.maxValue > value)
        {
            HpSlider.gameObject.SetActive(true);
        }
        HpSlider.value = value;
    }
}
