using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ScriptableSequence", menuName = "Scriptable Objects/Tactical Layer/ScriptableSequence")]
public class ScriptableSequence : ScriptableObject
{
    [SerializeReference, SubclassSelector]
    public List<GeneralAction> actions = new List<GeneralAction>();
}
