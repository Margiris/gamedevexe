using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Gamer : NetworkBehaviour
{
    [SyncVar]
    int MyID = -1;
    //[SyncVar]
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
            Debug.Log("Local player setting up stuff " );
            pausemenu = transform.Find("Pausemenu Canvas").GetComponent<PauseMenu>();
            player = transform.Find("player").gameObject;
            PLKP = transform.Find("PlayerLastKnownPosition");
            playerAllerting = GetComponent<Allerting>();
            levelManager = GameObject.Find("LevelManager").GetComponent<LevelManager>();
        }
        if (isServer)
        {
            pausemenu = transform.Find("Pausemenu Canvas").GetComponent<PauseMenu>();
            player = transform.Find("player").gameObject;
            PLKP = transform.Find("PlayerLastKnownPosition");
            playerAllerting = GetComponent<Allerting>();
            levelManager = GameObject.Find("LevelManager").GetComponent<LevelManager>();
            levelManager.RegisterNewPlayer(gameObject);
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
            levelManager.CmdDisconnectPlayer(MyID);
            NetworkServer.Destroy(gameObject);
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
