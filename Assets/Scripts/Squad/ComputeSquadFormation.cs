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

    static public List<Vector3> CirclePoses(Vector3 direction, Vector3 squadPos, float normFactor, float normDecreaseFactor, int nbUnit, int maxUnitPerLine, int startIndexUnit, float angleBetweenUnit, float angleIncreasePerLine)
    {
        List<Vector3> poses = new List<Vector3>();
        if (nbUnit == 0)
        {
            poses.Add(direction * normFactor + squadPos);
            return poses;
        }

        //compute nb defender to place on current defense line
        int nbUnitsToPlaceOnLine = nbUnit - startIndexUnit;
        nbUnitsToPlaceOnLine = nbUnitsToPlaceOnLine > maxUnitPerLine ? maxUnitPerLine : nbUnitsToPlaceOnLine;

        //exception if 1 to place because for rota compute /0
        if (nbUnitsToPlaceOnLine == 1)
        {
            poses.Add(direction * normFactor + squadPos);

            if (maxUnitPerLine == 1)
            {
                ++startIndexUnit;

                if (startIndexUnit < nbUnit)
                {
                    //compute next line, closer to squadPos
                    poses.AddRange(CirclePoses(direction, squadPos, normFactor * normDecreaseFactor, normDecreaseFactor, nbUnit, maxUnitPerLine, startIndexUnit, angleBetweenUnit + angleIncreasePerLine, angleIncreasePerLine));
                    return poses;
                }
            }

            return poses;
        }

        float sign = -1;
        float currentAngle = angleBetweenUnit * 0.5f;
        //if odd => place first unit align with enemy dir
        if (nbUnitsToPlaceOnLine % 2 == 1)
        {
            poses.Add(direction * normFactor + squadPos);
            --nbUnitsToPlaceOnLine;
            ++startIndexUnit;
            currentAngle = angleBetweenUnit;

        }

        int i = startIndexUnit;
        for (; i < startIndexUnit + nbUnitsToPlaceOnLine; i++)
        {
            Vector3 newPos = Quaternion.Euler(0f, sign * currentAngle, 0f) * direction;
            poses.Add(newPos * normFactor + squadPos);

            //each side of angle (45 => -45 => 45 + angleBetweenUnit => -(45 + angleBetweenUnit) => ... )
            sign = -sign;
            if (sign < 0)
                currentAngle += angleBetweenUnit;
        }

        if (i < nbUnit)
        {
            poses.AddRange(CirclePoses(direction, squadPos, normFactor * normDecreaseFactor, normDecreaseFactor, nbUnit, maxUnitPerLine, i, angleBetweenUnit + angleIncreasePerLine, angleIncreasePerLine));
            return poses;
        }

        return poses;
    }

    static public List<Vector3> LinePoses(Vector3 direction, Vector3 anchor, int nbUnit, int maxUnitPerLine, float spaceBetweenUnit, float spaceBetweenLine)
    {
        List<Vector3> unitsPoses = new List<Vector3>();

        if (nbUnit == 0)
        {
            unitsPoses.Add(anchor);
            return unitsPoses;
        }


        int nbLine = (int)(nbUnit / maxUnitPerLine) + ((nbUnit % maxUnitPerLine == 0) ? 0 : 1);
        Vector3 lineStartPos = anchor;
        Vector3 dirLine = Quaternion.Euler(0f, 90f, 0f) * direction;

        int startIndexAttackers = 0;
        for (int i = 0; i < nbLine; ++i)
        {

            int nbUnitsOnLine = nbUnit - startIndexAttackers;
            nbUnitsOnLine = nbUnitsOnLine > maxUnitPerLine ? maxUnitPerLine : nbUnitsOnLine;

            int j = startIndexAttackers;

            bool isNbOnLineOdd = nbUnitsOnLine % 2 == 1;

            Vector3 posAttacker = lineStartPos - dirLine * spaceBetweenUnit * (int)((isNbOnLineOdd ? (nbUnitsOnLine * 0.5f) : ((nbUnitsOnLine * 0.5f) - 1)));

            if (!isNbOnLineOdd)
            {
                posAttacker -= dirLine * spaceBetweenUnit * 0.5f;
            }

            for (; j < startIndexAttackers + nbUnitsOnLine; ++j)
            {
                unitsPoses.Add(posAttacker);
                posAttacker += dirLine * spaceBetweenUnit;
            }

            startIndexAttackers = j;

            lineStartPos -= direction * spaceBetweenLine;
        }

        //add last line pos
        unitsPoses.Add(lineStartPos + direction * spaceBetweenLine);
        return unitsPoses;
    }

    static public List<Vector3> PreComputeLine(List<Line> lines, Vector3 firstLinePos, Vector3 direction)
    {
        direction = direction.normalized;

        List<Vector3> unitsPoses = new List<Vector3>();
        Vector3 anchor = firstLinePos;
        foreach (Line line in lines)
        {
            anchor += line.lineSpacing * direction;
            unitsPoses.AddRange(LinePoses(direction, anchor, line.numberOfUnits, line.numberOfUnits, line.unitSpacing, 0f));
        }

        return unitsPoses;
    }
}
