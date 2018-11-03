using System;
using UnityEngine;

namespace GUI_scripts
{
    [Serializable]
    public class Dialogue
    {
        [TextArea(3, 10)] public string sentence;
        public string name;
        public Sprite avatar;
    }
}