using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SettingsData
{
    public float sfxVolume;
    public float musVolume;
    public float retSens;

    public SettingsData(GameManager manager)
    {
        sfxVolume = manager.sfxVolume;
        musVolume = manager.musVolume;
        retSens = manager.retSens;
    }
}
