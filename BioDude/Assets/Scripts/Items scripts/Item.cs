using UnityEngine;

namespace Items_scripts
{
	public abstract class Item : MonoBehaviour
	{
		public string ItemName;
		public Sprite sprite;
		public int orderInLayer;
	
		public void Equip (GameObject placeHolder)
		{
			var spriteRend = placeHolder.GetComponent<SpriteRenderer>();
			spriteRend.sortingOrder = orderInLayer;
			spriteRend.sprite = sprite;
		}
	}
}
