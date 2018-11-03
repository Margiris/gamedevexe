using Items_scripts;
using UnityEngine;

namespace Enemy_scripts
{
    /// <inheritdoc />
    /// <summary>
    /// firearm should have only one child: object where to instantiate bullets on fire and this script
    /// </summary>
    public class Firearm : MonoBehaviour
    {
        public float shootingRate = 2f;
        public GameObject bulletPrefab;
        public float damage;
        public float bulletSpeed;
        public float bulletDestroyAfter;
        public float accuracy;
        public Animator animator;

        private Transform projectileParent;
        private Transform shootingFrom;
        private float timeTillNextShoot;

        // Use this for initialization
        private void Start()
        {
            shootingFrom = transform.GetChild(0);
            projectileParent = GameObject.Find("Projectiles").transform;
        }

        // Update is called once per frame
        private void Update()
        {
            if (timeTillNextShoot > 0)
                timeTillNextShoot -= Time.deltaTime;
        }

        public void Shoot()
        {
            if (!(timeTillNextShoot <= 0)) return;
            if (animator != null)
                animator.SetTrigger("Fire");
            timeTillNextShoot = shootingRate;
            var bulletAngle = Random.Range(-accuracy, accuracy);
            var newBullet = Instantiate(bulletPrefab, shootingFrom.transform.position,
                Quaternion.Euler(0f, 0f, transform.rotation.eulerAngles.z + bulletAngle), projectileParent);
            newBullet.gameObject.layer = 18;
            //GameObject newBullet = Instantiate(bulletPrefab, shootingFrom.position, transform.rotation, projectileParent);
            newBullet.GetComponent<Bullet>().Instantiate(bulletDestroyAfter, bulletSpeed, damage);
        }
    }
}