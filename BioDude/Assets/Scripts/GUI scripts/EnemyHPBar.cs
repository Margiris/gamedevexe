using UnityEngine;
using UnityEngine.UI;

namespace GUI_scripts
{
    public class EnemyHPBar : MonoBehaviour
    {
        public Slider HpSlider;
        private float height;
        private Character EnemyCharacter;
        private GameObject EnemyObject;

        private Vector3 EnemyWorldPos;
        private Quaternion rotation;

        private void Start()
        {
            EnemyObject = gameObject.transform.parent.gameObject;
            height = gameObject.transform.position.y - EnemyObject.transform.position.y;
            rotation = transform.rotation;
        }

        public void Initiate()
        {
            EnemyCharacter =
                gameObject.transform.parent.gameObject
                    .GetComponent<Character>(); // somehow null reference when using EnemyObject
            HpSlider.maxValue = EnemyCharacter.healthMax;
            HpSlider.value = EnemyCharacter.healthCurrent;
            HpSlider.gameObject.SetActive(false);
        }

        private void LateUpdate()
        {
            transform.position = new Vector3(EnemyObject.transform.position.x,
                EnemyObject.transform.position.y + height, 0);
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
}