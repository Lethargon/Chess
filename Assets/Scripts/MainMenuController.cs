using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;

public class MainMenuController : MonoBehaviour
{
    private VisualElement root;

    private VisualElement rulesContainer;
    // Start is called before the first frame update
    void Start()
    {
        root = GetComponent<UIDocument>().rootVisualElement;

        rulesContainer = root.Q<VisualElement>("RulesContainer");
        rulesContainer.style.display = DisplayStyle.None;

        root.Q<Button>("PlayButton").clicked += OnPlayButtonClicked;
        root.Q<Button>("RulesButton").clicked += OnRulesButtonClicked;
        root.Q<Button>("QuitButton").clicked += OnQuitButtonClicked;
    }

    private void OnPlayButtonClicked()
    {
        SceneManager.LoadScene("SampleScene");
    }

    private void OnRulesButtonClicked()
    {
        rulesContainer.style.display = rulesContainer.style.display == DisplayStyle.None ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private void OnQuitButtonClicked()
    {
        Application.Quit();
    }
}
