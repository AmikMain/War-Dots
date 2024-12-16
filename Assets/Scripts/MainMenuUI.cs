using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [SerializeField] private Button StartHostButton;
    [SerializeField] private Button StartClientButton;
    [SerializeField] private Button StartGameButton;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Button OpenButton;
    [SerializeField] private TMP_Text VersionText;
    [SerializeField] private Image colorDisplayImage;

    private bool showingToolkit = false;

    private void Start() {
        VersionText.text = "Build " + Application.version;
        GameManager.Instance.onColorChanged += DisplayColor;
    }

    public void ToggleToolkit()
    {
        if(showingToolkit)
        {
            OpenButton.transform.Rotate(new Vector3(0,0,90));
            StartHostButton.gameObject.SetActive(false);
            StartClientButton.gameObject.SetActive(false);
            StartGameButton.gameObject.SetActive(false);
            inputField.gameObject.SetActive(false);
            showingToolkit = false;
        }
        else
        {
            OpenButton.transform.Rotate(new Vector3(0,0,-90));
            StartHostButton.gameObject.SetActive(true);
            StartClientButton.gameObject.SetActive(true);
            StartGameButton.gameObject.SetActive(true);
            inputField.gameObject.SetActive(true);
            showingToolkit = true;
        }
    }

    public void StartGame()
    {
        Debug.Log("UI Tries to start the game.");
        GameManager.Instance.TryStartGame();
    }

    public void ChangeColor()
    {
        GameManager.Instance.AssignLocalColor();
    }

    private void DisplayColor(Color color)
    {
        colorDisplayImage.color = color;
    }
}
