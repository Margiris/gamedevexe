using Enemy_scripts;
using UnityEngine;

// ReSharper disable CommentTypo

namespace Player_scripts
{
    public class Alerting : MonoBehaviour
    {
        public uint howManySeeMe;
        private Transform PLKP; //PlayerLastknownPosition


        // code for allerting enemies in area if shoot if walk near (if needed) can be a skill
        // while pursuing maybe area around player should catch non alerted enemy and allert them

        // this script should watch all gun shots and set playerLastKnownPosition i dabartine position ir alertinti visus aplinkinius enemy
        // o enmey skripte jei enemy priima damage tada ji reikia alertinti

        // Use this for initialization
        private void Start()
        {
            PLKP = GameObject.Find("PlayerLastKnownPosition").transform;
        }

        public void AlertSurroundings(float radius)
        {
            PLKP.position = transform.position;
            var objects = Physics2D.CircleCastAll(transform.position, radius, Vector2.right, radius,
                LayerMask.GetMask("Enemy"));
            foreach (var obj in objects)
            {
                if (obj.transform.GetComponent<Tank>() != null)
                    obj.transform.GetComponent<Tank>().PursuePlayer();
            }
        }
    }
}