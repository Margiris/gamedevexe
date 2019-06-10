using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Gamer : NetworkBehaviour
{
    int MyID = -1;
    LevelManager levelManager;
    public GameObject player;
    public Transform PLKP;
    public Allerting playerAllerting;
    public PauseMenu pausemenu;

    // Start is called before the first frame update
    void Start()
    {
        if (this.isLocalPlayer)
        {
            pausemenu = transform.Find("Pausemenu Canvas").GetComponent<PauseMenu>();
            player = transform.Find("player").gameObject;
            PLKP = transform.Find("PlayerLastKnownPosition");
            playerAllerting = player.GetComponent<Allerting>();
            levelManager = GameObject.Find("LevelManager").GetComponent<LevelManager>();
            levelManager.RegisterNewPlayer(this);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnDisable()
    {
        if (this.isLocalPlayer)
        {
            levelManager.DisconnectPlayer(MyID);
        }
    }

    public int getPlayerID()
    {
        return MyID;
    }

    public void setPLayerID(int ID) // set player id not change id if already set (set only once)
    {
        if(MyID == -1)
        {
            MyID = ID;
            GetComponent<WeaponManager>().playerID = ID;
        }
    }
}
