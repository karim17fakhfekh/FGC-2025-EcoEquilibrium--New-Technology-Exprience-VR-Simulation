using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraLevelController : MonoBehaviour
{
    [Header("Level Transforms")]
    public Transform level4Transform;
    public Transform level3Transform;
    public Transform level2Transform;
    public Transform level1Transform;
    public Transform groveTransform;

    [Header("Movement Settings")]
    public float movementSpeed = 5f;
    public float rotationSpeed = 2f;
    public KeyCode upKey = KeyCode.UpArrow;
    public KeyCode downKey = KeyCode.DownArrow;
    public KeyCode actionKey = KeyCode.Space;

    [Header("Water Objects")]
    public GameObject waterTankObject;
    public GameObject dirtyWaterContainerObject;
    public GameObject dilutionContainerObject;

    [Header("Water Level Settings")]
    public float waterTankStartLevel = 0.8f;
    public float waterTankEndLevel = 0.2f;
    public float dirtyContainerStartLevel = 0.1f;
    public float dirtyContainerEndLevel = 0.8f;
    public float dilutionContainerStartLevel = 0.1f;
    public float dilutionContainerMidLevel = 0.5f;
    public float dilutionContainerEndLevel = 0.8f;
    public float transitionDuration = 3f;
    public float stepDelay = 1f;

    [Header("References")]
    public ColorTransitionManager fermentationColorTransition;
    public ColorTransitionManager dilutionColorTransition;
    public BranchShredder branchShredder;

    private List<Transform> levelTransforms = new List<Transform>();
    private int currentLevelIndex = 0;
    private bool isMoving = false;
    private bool fermentationDone = false;
    private bool dilutionStep1Done = false;
    private bool dilutionStep2Done = false;
    private bool transitionInProgress = false;
    private bool tankRecoveryInProgress = false;
    private float transitionTimer = 0f;
    private float recoveryTimer = 0f;

    private enum DilutionPhase { Idle, Step1, Step2 }
    private DilutionPhase currentDilutionPhase = DilutionPhase.Idle;

    private Material waterTankMaterial;
    private Material dirtyWaterMaterial;
    private Material dilutionMaterial;
    private string waterLevelProperty = "";

    void Start()
    {
        InitializeLevelTransforms();
        FindWaterMaterials();
        SetInitialWaterLevels();
    }

    void InitializeLevelTransforms()
    {
        levelTransforms.Add(level4Transform);
        levelTransforms.Add(level3Transform);
        levelTransforms.Add(level2Transform);
        levelTransforms.Add(level1Transform);
        levelTransforms.Add(groveTransform);

        if (levelTransforms.Count > 0 && levelTransforms[0] != null)
        {
            transform.SetPositionAndRotation(levelTransforms[0].position, levelTransforms[0].rotation);
            currentLevelIndex = 0;
        }
    }

    void FindWaterMaterials()
    {
        if (waterTankObject != null)
        {
            Renderer renderer = waterTankObject.GetComponent<Renderer>();
            if (renderer != null) waterTankMaterial = renderer.material;
        }

        if (dirtyWaterContainerObject != null)
        {
            Renderer renderer = dirtyWaterContainerObject.GetComponent<Renderer>();
            if (renderer != null) dirtyWaterMaterial = renderer.material;
        }

        if (dilutionContainerObject != null)
        {
            Renderer renderer = dilutionContainerObject.GetComponent<Renderer>();
            if (renderer != null) dilutionMaterial = renderer.material;
        }

        FindWaterLevelProperty();
    }

    void FindWaterLevelProperty()
    {
        string[] possibleProperties = {
            "_FillHeight", "_WaterLevel", "_FillLevel", "_Level",
            "_Fill", "_Height", "_WaterHeight", "_FillAmount" , "_fill_height"
        };

        Material[] materials = { waterTankMaterial, dirtyWaterMaterial, dilutionMaterial };
        foreach (Material mat in materials)
        {
            if (mat != null)
            {
                foreach (string prop in possibleProperties)
                {
                    if (mat.HasProperty(prop))
                    {
                        waterLevelProperty = prop;
                        return;
                    }
                }
            }
        }
    }

    void SetInitialWaterLevels()
    {
        SetWaterLevel(waterTankMaterial, waterTankStartLevel);
        SetWaterLevel(dirtyWaterMaterial, dirtyContainerStartLevel);
        SetWaterLevel(dilutionMaterial, dilutionContainerStartLevel);
    }

    void SetWaterLevel(Material material, float level)
    {
        if (material != null && !string.IsNullOrEmpty(waterLevelProperty))
        {
            material.SetFloat(waterLevelProperty, level);
        }
    }

    void Update()
    {
        HandleCameraMovement();
        HandleWaterTransition();
        HandleTankRecovery();
        HandleInput();
    }

    void HandleCameraMovement()
    {
        if (isMoving && currentLevelIndex < levelTransforms.Count && levelTransforms[currentLevelIndex] != null)
        {
            Transform targetTransform = levelTransforms[currentLevelIndex];
            transform.position = Vector3.Lerp(transform.position, targetTransform.position, movementSpeed * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetTransform.rotation, rotationSpeed * Time.deltaTime);

            if (Vector3.Distance(transform.position, targetTransform.position) < 0.1f)
            {
                isMoving = false;
            }
        }
    }

    void HandleWaterTransition()
    {
        if (transitionInProgress)
        {
            transitionTimer += Time.deltaTime;
            float t = Mathf.Clamp01(transitionTimer / transitionDuration);

            if (currentLevelIndex == 1)
            {
                float tankLevel = Mathf.Lerp(waterTankStartLevel, waterTankEndLevel, t);
                float dirtyLevel = Mathf.Lerp(dirtyContainerStartLevel, dirtyContainerEndLevel, t);

                SetWaterLevel(waterTankMaterial, tankLevel);
                SetWaterLevel(dirtyWaterMaterial, dirtyLevel);

                if (t >= 1f)
                {
                    CompleteFermentation();
                }
            }
            else if (currentLevelIndex == 2)
            {
                if (currentDilutionPhase == DilutionPhase.Step1)
                {
                    float dirtyLevel = Mathf.Lerp(dirtyContainerEndLevel, dirtyContainerStartLevel, t);
                    float dilutionLevel = Mathf.Lerp(dilutionContainerStartLevel, dilutionContainerMidLevel, t);

                    SetWaterLevel(dirtyWaterMaterial, dirtyLevel);
                    SetWaterLevel(dilutionMaterial, dilutionLevel);

                    if (t >= 1f)
                    {
                        CompleteDilutionStep1();
                    }
                }
                else if (currentDilutionPhase == DilutionPhase.Step2)
                {
                    float tankLevel = Mathf.Lerp(waterTankStartLevel, waterTankEndLevel, t);
                    float dilutionLevel = Mathf.Lerp(dilutionContainerMidLevel, dilutionContainerEndLevel, t);

                    SetWaterLevel(waterTankMaterial, tankLevel);
                    SetWaterLevel(dilutionMaterial, dilutionLevel);

                    if (t >= 1f)
                    {
                        CompleteDilutionStep2();
                    }
                }
            }
        }
    }

    void CompleteFermentation()
    {
        transitionInProgress = false;
        fermentationDone = true;

        if (fermentationColorTransition != null)
        {
            fermentationColorTransition.StartTransition();
        }
    }

    void CompleteDilutionStep1()
    {
        transitionInProgress = false;
        dilutionStep1Done = true;

        if (dilutionColorTransition != null)
        {
            dilutionColorTransition.StartTransition();
        }

        StartCoroutine(StartDilutionStep2());
    }

    void CompleteDilutionStep2()
    {
        transitionInProgress = false;
        dilutionStep2Done = true;
        StartCoroutine(StartTankRecovery());
    }

    IEnumerator StartDilutionStep2()
    {
        yield return new WaitForSeconds(stepDelay);
        currentDilutionPhase = DilutionPhase.Step2;
        transitionInProgress = true;
        transitionTimer = 0f;
    }

    IEnumerator StartTankRecovery()
    {
        yield return new WaitForSeconds(stepDelay);
        tankRecoveryInProgress = true;
        recoveryTimer = 0f;
    }

    void HandleTankRecovery()
    {
        if (tankRecoveryInProgress)
        {
            recoveryTimer += Time.deltaTime;
            float t = Mathf.Clamp01(recoveryTimer / transitionDuration);

            float tankLevel = Mathf.Lerp(waterTankEndLevel, waterTankStartLevel, t);
            SetWaterLevel(waterTankMaterial, tankLevel);

            if (t >= 1f)
            {
                tankRecoveryInProgress = false;
            }
        }
    }

    void HandleInput()
    {
        if (!isMoving && !transitionInProgress)
        {
            if (Input.GetKeyDown(upKey)) MoveUp();
            else if (Input.GetKeyDown(downKey)) MoveDown();
            else if (Input.GetKeyDown(actionKey)) HandleActionKey();
        }
    }

    private void HandleActionKey()
    {
        if (currentLevelIndex == 1)
        {
            if (fermentationDone) return;

            if (branchShredder != null && branchShredder.AllBranchesShredded)
            {
                transitionInProgress = true;
                transitionTimer = 0f;
            }
        }
        else if (currentLevelIndex == 2)
        {
            if (dilutionStep2Done) return;

            if (fermentationDone)
            {
                if (!dilutionStep1Done)
                {
                    currentDilutionPhase = DilutionPhase.Step1;
                    transitionInProgress = true;
                    transitionTimer = 0f;
                }
                else if (!dilutionStep2Done)
                {
                    currentDilutionPhase = DilutionPhase.Step2;
                    transitionInProgress = true;
                    transitionTimer = 0f;
                }
            }
        }
    }

    public void ResetAllWaterLevels()
    {
        SetWaterLevel(waterTankMaterial, waterTankStartLevel);
        SetWaterLevel(dirtyWaterMaterial, dirtyContainerStartLevel);
        SetWaterLevel(dilutionMaterial, dilutionContainerStartLevel);

        fermentationDone = false;
        dilutionStep1Done = false;
        dilutionStep2Done = false;
        transitionInProgress = false;
        tankRecoveryInProgress = false;
        currentDilutionPhase = DilutionPhase.Idle;
        transitionTimer = 0f;
        recoveryTimer = 0f;
    }

    private void MoveUp()
    {
        if (currentLevelIndex > 0) currentLevelIndex--;
        isMoving = true;
    }

    private void MoveDown()
    {
        if (currentLevelIndex < levelTransforms.Count - 1) currentLevelIndex++;
        isMoving = true;
    }

    public void GoToLevel(int levelIndex)
    {
        if (levelIndex >= 0 && levelIndex < levelTransforms.Count && levelIndex != currentLevelIndex)
        {
            currentLevelIndex = levelIndex;
            isMoving = true;
        }
    }

    public void GoToShredding() => GoToLevel(0);
    public void GoToFermentation() => GoToLevel(1);
    public void GoToDilution() => GoToLevel(2);
    public void GoToIrrigation() => GoToLevel(3);
    public void GoToGrove() => GoToLevel(4);
    public bool IsCameraMoving() => isMoving;
}