using UnityEngine;

/**
 *	Demo Scene Script for WAR FX
 *	
 *	(c) 2015, Jean Moreno
**/

namespace VFX.JMO_Assets.WarFX.Demo.Assets
{
	public class WFX_Demo_Wall : MonoBehaviour
	{
		public WFX_Demo_New demo;

		private void OnMouseDown()
		{
			var hit = new RaycastHit();
			if(GetComponent<Collider>().Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, 9999f))
			{
				var particle = demo.spawnParticle();
				particle.transform.position = hit.point;
				particle.transform.rotation = Quaternion.FromToRotation(Vector3.forward, hit.normal);
			}
		}
	}
}
