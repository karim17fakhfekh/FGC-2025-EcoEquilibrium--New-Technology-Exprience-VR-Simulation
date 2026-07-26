using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class SceneTransitionManager : MonoBehaviour
{
    [Header("UI References - TMP")]
    public GameObject introPanel;
    public TMP_Text machineText;
    public TMP_Text fieldText;
    public TMP_Text quitText;

    [Header("Scene Names")]
    public string machineSceneName = "MachineSimulation";
    public string fieldSceneName = "FieldSimulation";

    [Header("Key Settings")]
    public KeyCode machineKey = KeyCode.A;
    public KeyCode fieldKey = KeyCode.B;
    public KeyCode quitKey = KeyCode.C;

    private void Start()
    {
        if (introPanel != null)
        {
            introPanel.SetActive(true);
        }

        UpdateUIText();
    }

    private void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (Input.GetKeyDown(machineKey) && currentScene != machineSceneName)
        {
            LoadMachineScene();
        }

        if (Input.GetKeyDown(fieldKey) && currentScene != fieldSceneName)
        {
            LoadFieldScene();
        }

        if (Input.GetKeyDown(quitKey))
        {
            QuitSimulation();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (SceneManager.GetActiveScene().name != "IntroScene")
            {
                SceneManager.LoadScene("IntroScene");
            }
            else
            {
                QuitSimulation();
            }
        }
    }

    void UpdateUIText()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (machineText != null)
        {
            if (currentScene == machineSceneName)
            {
                machineText.text = $"Press {fieldKey} to go to Field Simulation";
            }
            else if (currentScene == fieldSceneName)
            {
                machineText.text = $"Press {machineKey} to go to Machine Simulation";
            }
            else
            {
                machineText.text = $"Press {machineKey} for Machine Simulation";
            }
        }

        if (fieldText != null)
        {
            if (currentScene == machineSceneName)
            {
                fieldText.text = $"Press {fieldKey} to go to Field Simulation";
            }
            else if (currentScene == fieldSceneName)
            {
                fieldText.text = $"Press {machineKey} to go to Machine Simulation";
            }
            else
            {
                fieldText.text = $"Press {fieldKey} for Field Simulation";
            }
        }

        if (quitText != null)
        {
            quitText.text = $"Press {quitKey} to quit Simulation";
        }
    }

    public void LoadMachineScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene != machineSceneName)
        {
            SceneManager.LoadScene(machineSceneName);
        }
    }

    public void LoadFieldScene()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene != fieldSceneName)
        {
            SceneManager.LoadScene(fieldSceneName);
        }
    }

    public void QuitSimulation()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateUIText();

        if (introPanel != null && scene.name != "IntroScene")
        {
            introPanel.SetActive(false);
        }
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}