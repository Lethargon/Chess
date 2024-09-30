using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    private static UIController instance;

    public VisualElement root;

    private Label gameInfoLabel;

    private VisualElement checkWarningBlack;
    private VisualElement checkWarningWhite;
    [SerializeField] private Sprite[] checkLightSprites;

    private VisualElement rulesContainer;

    private void Awake()
    {
        instance = this;

        root = GetComponent<UIDocument>().rootVisualElement;

        checkWarningBlack = root.Q<VisualElement>("BlackCheckLight");
        checkWarningWhite = root.Q<VisualElement>("WhiteCheckLight");
    }

    // Start is called before the first frame update
    void Start()
    {

        gameInfoLabel = root.Q<Label>("GameInfo");


        root.Q<Button>("RestartButton").clicked += OnRestartButtonClicked;
        root.Q<Button>("RulesButton").clicked += OnRulesButtonClicked;
        root.Q<Button>("MainMenuButton").clicked += OnMainMenuButtonClicked;

        rulesContainer = root.Q<VisualElement>("RulesContainer");
        rulesContainer.style.display = DisplayStyle.None;

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetGameInfo(string gameInfo)
    {
        gameInfoLabel.text = gameInfo;
    }

    public static UIController Instance()
    {
        return instance;
    }

    public void SetCheckLights(bool isWhiteKingChecked,  bool isBlackKingChecked)
    {
        checkWarningWhite.style.backgroundImage = isWhiteKingChecked ? new StyleBackground(checkLightSprites[1]) : new StyleBackground(checkLightSprites[0]);
        checkWarningBlack.style.backgroundImage = isBlackKingChecked ? new StyleBackground(checkLightSprites[1]) : new StyleBackground(checkLightSprites[0]);
    }

    public void SetCheckMateLight(bool whiteWon)
    {
        if(whiteWon)
        {
            checkWarningBlack.style.backgroundImage = new StyleBackground(checkLightSprites[2]);
        }
        else
        {
            checkWarningWhite.style.backgroundImage = new StyleBackground(checkLightSprites[2]);
        }
    }

    private void OnRestartButtonClicked()
    {
        SceneManager.LoadScene("SampleScene");
        Debug.Log("Restart");
    }

    private void OnRulesButtonClicked()
    {
        rulesContainer.style.display = rulesContainer.style.display == DisplayStyle.None? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void OnMainMenuButtonClicked()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
