using UnityEngine;

public static class GamePrefs {
    private static GameObject[] weaponArray;

    public static void DeletePlayerProgress()
    {
        PlayerPrefs.DeleteKey("LastLevelCheckpoint");
        PlayerPrefs.DeleteKey("PlayerHP");
        PlayerPrefs.DeleteKey("PistolAmmo");
        PlayerPrefs.DeleteKey("ShotgunAmmo");
        PlayerPrefs.DeleteKey("AssaultRifleAmmo");
        PlayerPrefs.DeleteKey("RocketAmmo");
        PlayerPrefs.DeleteKey("fragGrenadeAmmo");
        PlayerPrefs.DeleteKey("gravnadeAmmo");
        PlayerPrefs.DeleteKey("PistolDiscovered");
        PlayerPrefs.DeleteKey("RocketLauncherDiscovered");
        PlayerPrefs.DeleteKey("AssaultRifleDiscovered");
        PlayerPrefs.DeleteKey("ShotgunDiscovered");
        PlayerPrefs.DeleteKey("DualPistolDiscovered");
    }


}
