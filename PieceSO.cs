using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NEW SO", menuName = "PieceSO")]

public class PieceSO : ScriptableObject
{
    public List<Vector3> MoveLocations = new List<Vector3>();
    public int type;
}
