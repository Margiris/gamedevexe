using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class Dialogue {

    [TextArea(3,10)]
    public string sentence;
    public string name;
    public Sprite avatar;
	
}
