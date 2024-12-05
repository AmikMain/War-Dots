using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static BuildingManager;

public class BuildingCell : MonoBehaviour
{
    [SerializeField] BuildingType buildingType;
    [SerializeField] Image childSprite;
    [SerializeField] TMP_Text buildingPrice;

    void Start()
    {
        childSprite.sprite = BuildingManager.Instance.buildingList[buildingType].GetComponent<SpriteRenderer>().sprite;
        buildingPrice.text = BuildingManager.Instance.buildingPriceList[buildingType].ToString();
    }

    public void OnClick()
    {
        BuildingManager.Instance.OnBuildButtonPressed(buildingType);
    }

}
