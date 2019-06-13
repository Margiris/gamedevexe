using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public abstract class Item : NetworkBehaviour
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
