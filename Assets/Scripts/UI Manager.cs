using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

public class UIManager : NetworkBehaviour
{
    [SerializeField] private GameObject MainMenuUI;
    [SerializeField] private GameObject LobbyUI;
    [SerializeField] private GameObject GameUI;
    //[SerializeField] private GameObject GameOverUI;

    [SerializeField] TMP_Text joinCodeText;
    [SerializeField] private TMP_InputField joinCodeInput;
    [SerializeField] private TMP_Text playerCountText;
    [SerializeField] private TMP_Text warningText;
    [SerializeField] private TMP_Text tooltipText;
    private GameObject[] allUIs;
    
    public static UIManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        allUIs = new GameObject[] { MainMenuUI, LobbyUI, GameUI, /*GameOverUI*/ };
    }

    private void Start() {
        ShowMainMenu();
    }

    public void ShowMainMenu()
    {
        foreach (var ui in allUIs)
        {
            ui.SetActive(false);
        }
        MainMenuUI.SetActive(true);
    }

    public void ShowLobbyUI()
    {
        foreach (var ui in allUIs)
        {
            ui.SetActive(false);
        }
        LobbyUI.SetActive(true);
    }

    public void ShowGameUI()
    {
        foreach (var ui in allUIs)
        {
            ui.SetActive(false);
        }
        GameUI.SetActive(true);
    }

    public void UpdateJoinCode(string joinCode)
    {
        joinCodeText.text = joinCode;
    }

    public string GetJoinInputCode()
    {
        return joinCodeInput.text;
    }

    public string GetJoinCode()
    {
        return joinCodeText.text;
    }

    public void SetJoinCodeText(string code)
    {
        joinCodeText.text = code;
    }

    public void SetPlayerCountText(int count)
    {
        playerCountText.text = $"Players: {count.ToString()}";
    }

    public void ShowWarning(string message)
    {
        StartCoroutine(ShowWarningCoroutine(message));
    }

    private IEnumerator ShowWarningCoroutine(string message)
    {
        warningText.text = message;
        warningText.gameObject.SetActive(true);
        yield return new WaitForSeconds(1f);
        warningText.gameObject.SetActive(false);
    }

    public void ShowTooltip()
    {
        StartCoroutine(ShowTooltipCoroutine());
    }

    private IEnumerator ShowTooltipCoroutine()
    {
        tooltipText.gameObject.SetActive(true);
        yield return new WaitForSeconds(20f);
        tooltipText.gameObject.SetActive(false);
    }
}
