using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BuildingsList
{
    enum BuildingType
    {
        Castle, 
        Fortification,
        Barracks,
        OilRig,
        Turret,
        Artillery,
        MissleLauncher,
        AirDefence
    }

    [SerializeField] private static Dictionary<BuildingType, GameObject> buildingList;
}
