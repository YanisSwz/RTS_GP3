using System.Collections.Generic;
using UnityEngine;

public class ComputeSquadFormation
{
    public static List<Vector3> FreestyleFormation(Vector3 anchor, List<Vector3> freestylePosesSaved)
    {
        for (int i = 0; i < freestylePosesSaved.Count; ++i)
            freestylePosesSaved[i] += anchor;

        return freestylePosesSaved;
    }
}
