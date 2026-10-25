using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    [Header("Buttons")]
    [SerializeField] private Button _continueButton;
    [SerializeField] private Button _newGameButton;
    [SerializeField] private Button _optionsButton;
    [SerializeField] private Button _creditsButton;
    [SerializeField] private Button _quitButton;

    [Header("Panels & Modals")]
    [SerializeField] private GameObject _mainMenuPanel;
    [SerializeField] private GameObject _newGameModal;
    [SerializeField] private GameObject _quitModal;
    [SerializeField] private GameObject _optionsPanel;
    [SerializeField] private GameObject _creditsPanel;

    private void Start()
    {
        _continueButton.onClick.AddListener(ContinueGame);
        _newGameButton.onClick.AddListener(() => OpenPanel(_newGameModal));
        _optionsButton.onClick.AddListener(() => OpenPanel(_optionsPanel));
        _creditsButton.onClick.AddListener(() => OpenPanel(_creditsPanel));
        _quitButton.onClick.AddListener(() => OpenPanel(_quitModal));

        bool hasSave = CheckForGlobalSave();
        _continueButton.interactable = hasSave;

        ShowMainMenu();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ShowMainMenu();
        }
    }

    private void ContinueGame()
    {
        //TODO: fade to black.
        SceneManager.LoadScene("GameplayScene");
    }

    public void ConfirmNewGame()
    {
        //TODO: Wipe save data logic here.
        SceneManager.LoadScene("Zone0_Intro");
    }

    public void ConfirmQuit()
    {
        Application.Quit();
    }

    public void ShowMainMenu()
    {
        _newGameModal.SetActive(false);
        _quitModal.SetActive(false);
        _optionsPanel.SetActive(false);
        _creditsPanel.SetActive(false);
        _mainMenuPanel.SetActive(true);
    }

    private void OpenPanel(GameObject panel)
    {
        _mainMenuPanel.SetActive(false);
        panel.SetActive(true);
    }

    private bool CheckForGlobalSave()
    {
        //TODO: interface with SaveManager here
        return false; 
    }
}