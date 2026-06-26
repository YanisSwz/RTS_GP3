using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Line 
{
    public Line(UnitDataScriptable _unitType, int _numberOfSlots, float _slotSpacing, float _lineSpacing) 
    {
        unitType = _unitType;
        numberOfUnits = _numberOfSlots;
        unitSpacing = _slotSpacing;
        lineSpacing = _lineSpacing;
    }

    public UnitDataScriptable unitType;
    public int numberOfUnits;
    public float unitSpacing;
    public float lineSpacing;
}

[System.Serializable]
public class SquadData
{
    [SerializeField]
    private List<Line> lines = new List<Line>();

    public List<Line> Lines { get { return lines; } }
}
