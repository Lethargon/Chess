using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIController : MonoBehaviour
{
    private static UIController instance;

    public VisualElement root;

    private Label gameInfoLabel;

    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        root = GetComponent<UIDocument>().rootVisualElement;

        gameInfoLabel = root.Q<Label>("GameInfo");
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
}
