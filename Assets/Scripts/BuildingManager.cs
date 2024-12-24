using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class BuildingManager : NetworkBehaviour
{
    [SerializeField] GameObject castlePrefab;
    [SerializeField] int castlePrice;
    [SerializeField] GameObject barracksPrefab;
    [SerializeField] int barracksPrice;
    [SerializeField] GameObject fortificationPrefab;
    [SerializeField] int fotificationPrice;
    [SerializeField] GameObject oilRigPrefab;
    [SerializeField] int oilRigPrice;
    [SerializeField] GameObject artilleryPrefab;
    [SerializeField] int artilleryPrice;
    [SerializeField] GameObject turretPrefab;
    [SerializeField] int turretPrice;
    //[SerializeField] GameObject missleLauncherPrefab;
    //[SerializeField] GameObject airDefencePrefab;

    public enum BuildingType
    {
        Castle, 
        Barracks,
        Fortification,  
        OilRig,
        Artillery,
        Turret,
        MissleLauncher,
        AirDefence
    }
    public Dictionary<BuildingType, GameObject> buildingList = new Dictionary<BuildingType, GameObject>();
    public Dictionary<BuildingType, int> buildingPriceList = new Dictionary<BuildingType, int>();
    private void AssignBuildings()
    {
        buildingList.Add(BuildingType.Castle, castlePrefab);
        buildingList.Add(BuildingType.Barracks, barracksPrefab);
        buildingList.Add(BuildingType.Fortification, fortificationPrefab);
        buildingList.Add(BuildingType.OilRig, oilRigPrefab);
        buildingList.Add(BuildingType.Artillery, artilleryPrefab);
        buildingList.Add(BuildingType.Turret, turretPrefab);

        buildingPriceList.Add(BuildingType.Castle, castlePrice);
        buildingPriceList.Add(BuildingType.Barracks, barracksPrice);
        buildingPriceList.Add(BuildingType.Fortification, fotificationPrice);
        buildingPriceList.Add(BuildingType.OilRig, oilRigPrice);
        buildingPriceList.Add(BuildingType.Artillery, artilleryPrice);
        buildingPriceList.Add(BuildingType.Turret, turretPrice);
    }

    public static BuildingManager Instance;
    public bool isBulding = false;
    public bool canFinishBuilding = true;
    private BuildingType currentBuildingType;
    Vector3 buildingPosition;
    [SerializeField] GameObject buildingProjectionPrefab;
    GameObject buildingProjection;

    private void Awake() {

        DontDestroyOnLoad(gameObject);

        if (Instance != null && Instance != this) 
        { 
            Destroy(this); 
        } 
        else 
        { 
            Instance = this; 
        }

        AssignBuildings();
    }

    private void Start() {  
        buildingProjection = Instantiate(buildingProjectionPrefab);
        buildingProjection.GetComponent<SpriteRenderer>().color = new Color(Color.green.r, Color.green.g, Color.green.b, 0.3f);
        buildingProjection.SetActive(false);
        InputManager.Instance.onRMBPerformed += StopBuilding;
        InputManager.Instance.onLMBCanceled += FinishBuilding;
    }

    private void Update() {
        TryToBuild();
    }

    public void FinishBuilding()
    {
        if(isBulding && canFinishBuilding && GameManager.Instance.GetOilCount() >= buildingPriceList[currentBuildingType])
        {
            FinishBuildingServerRpc(NetworkManager.Singleton.LocalClientId, currentBuildingType, new Vector3(buildingPosition.x, buildingPosition.y, 0));
            GameManager.Instance.ChangeOilAmount(-buildingPriceList[currentBuildingType]);
            CastleControlManager.Instance.ClearSelection();
            StopBuilding();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void FinishBuildingServerRpc(ulong ownerId, BuildingType type, Vector3 position)
    {
        if(!IsHost) return;

        GameObject newCastle = Instantiate(buildingList[type], position, Quaternion.identity);

        newCastle.GetComponent<NetworkObject>().SpawnWithOwnership(ownerId);

        newCastle.GetComponent<Castle>().SetCastleColor(GameManager.Instance.playerColors[ownerId]);

        newCastle.GetComponent<Castle>().SetCastlePosition(position);
    }

    private void TryToBuild()
    {
        if(!isBulding) return;
        
        buildingPosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        
        buildingProjection.transform.position = new Vector3(buildingPosition.x, buildingPosition.y, 0);
    }

    public void StartBuilding()
    {
        isBulding = true;
        buildingProjection.SetActive(true);
    }

    public void StopBuilding()
    {
        isBulding = false;
        buildingProjection.SetActive(false);
    }

    public void AllowToBuild(bool state)
    {
        canFinishBuilding = state;
        if(state == true)
        {
            buildingProjection.GetComponent<SpriteRenderer>().color = new Color(Color.green.r, Color.green.g, Color.green.b, 0.3f);
        }
        else
        {
            buildingProjection.GetComponent<SpriteRenderer>().color = new Color(Color.red.r, Color.red.g, Color.red.b, 0.3f);
        }
    }

    public void OnBuildButtonPressed(BuildingType type)
    {
        if(!isBulding)
        {
            currentBuildingType = type;
            buildingProjection.GetComponent<SpriteRenderer>().sprite = buildingList[type].GetComponent<SpriteRenderer>().sprite;
            StartBuilding();
        }
        else
        {
            StopBuilding();
        }
    }
}
