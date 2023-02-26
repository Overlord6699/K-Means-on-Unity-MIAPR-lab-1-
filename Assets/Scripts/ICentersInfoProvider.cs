using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ICentersInfoProvider
{
    public Vector3[] GetCentersPositions();
    public Color[] GetCentersColors();
    public Color GetCenterColorByPos(Vector3 pos);
    public Circle GetCenterByPos(Vector3 pos);
}
