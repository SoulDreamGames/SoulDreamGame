using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ResultsData", menuName = "ScriptableObjects/ResultsData", order = 1)]
public class ResultsData : ScriptableObject
{
    public bool victory;
    public int domeEnergy;
    public int nDeaths;
    public int enemiesKilled;
    public int evacuees;
}
