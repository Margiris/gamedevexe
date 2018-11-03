using UnityEngine;

namespace Items_scripts
{
	public class SelfDestruct : MonoBehaviour {

		public float TimeUntilSelfDestruct;
	
		// Update is called once per frame
		private void Update () {
			TimeUntilSelfDestruct -= Time.deltaTime;
			if (TimeUntilSelfDestruct <= 0)
				Destroy(gameObject);
		}
	}
}
