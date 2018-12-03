using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Item : MonoBehaviour
{
    public string ItemName;
	public Sprite sprite;
    public int orderInLayer;
	
	public virtual void Equip (GameObject placeHolder)
	{
		SpriteRenderer spriteRend = placeHolder.GetComponent<SpriteRenderer>();
		spriteRend.sortingOrder = orderInLayer;
		spriteRend.sprite = sprite;
	}
}
