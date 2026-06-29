using UnityEngine;

[CreateAssetMenu(fileName = "SquadDataPreset", menuName = "Scriptable Objects/Tactical Layer/SquadDataPreset")]
public class SquadDataPreset : ScriptableObject
{
    public SquadDataPreset(SquadDataPreset preset) 
    {
        squadData = preset.squadData;
    }

    public SquadData squadData = null;
}
