using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIController : MonoBehaviour
{
    private static UIController instance;

    public VisualElement root;

    private Label gameInfoLabel;

    private VisualElement checkWarningBlack;
    private VisualElement checkWarningWhite;
    [SerializeField] private Sprite[] checkLightSprites;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        root = GetComponent<UIDocument>().rootVisualElement;

        gameInfoLabel = root.Q<Label>("GameInfo");

        checkWarningBlack = root.Q<VisualElement>("BlackCheckLight");
        checkWarningWhite = root.Q<VisualElement>("WhiteCheckLight");
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
}
