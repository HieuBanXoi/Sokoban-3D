using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Simple UI bridge: read an input field (or cached string) and call LevelGenerator.GenerateMapByLevel(id)
public class LevelLoaderUI : MonoBehaviour
{
    [Tooltip("Optional: drop the LevelGenerator instance. If empty, the script will FindObjectOfType at runtime.")]
    public LevelGenerator levelGenerator;

    [Tooltip("Optional: standard Unity UI InputField for entering the level id.")]
    public TMP_InputField levelIdInput;

    [Tooltip("Button that triggers generation. If left empty you can call OnGeneratePressed() from another UI.")]
    public Button generateButton;

    [Tooltip("Optional: Undo button to revert last move")]
    public Button undoButton;

    // cached value if you prefer connecting TMP_InputField.onValueChanged -> SetLevelId
    private string cachedLevelId = "";

    void Awake()
    {
        if (generateButton != null)
            generateButton.onClick.AddListener(OnGeneratePressed);

        if (undoButton != null)
            undoButton.onClick.AddListener(OnUndoPressed);

        if (levelIdInput != null)
            cachedLevelId = levelIdInput.text;
    }

    public void OnUndoPressed()
    {
        CommandManager.Ins.Undo();
    }

    public void SetLevelId(string value)
    {
        cachedLevelId = value;
    }

    public void OnGeneratePressed()
    {
        string s = cachedLevelId;
        if (levelIdInput != null)
            s = levelIdInput.text;

        if (string.IsNullOrWhiteSpace(s))
        {
            Debug.LogWarning("LevelLoaderUI: level id is empty");
            return;
        }

        if (!int.TryParse(s, out int id))
        {
            Debug.LogWarning($"LevelLoaderUI: invalid level id '{s}'");
            return;
        }


        if (levelGenerator == null)
        {
            Debug.LogError("LevelLoaderUI: No LevelGenerator found in scene. Assign one in inspector.");
            return;
        }

        // GenerateMapByLevel already clears the old map; ClearMap call is safe but redundant.
        levelGenerator.ClearMap();
        levelGenerator.levelIdToLoad = id; // Set the levelIdToLoad before generating, so that TutorialController can read it
        levelGenerator.GenerateMapByLevel(id);
    }

    void OnDestroy()
    {
        if (generateButton != null)
            generateButton.onClick.RemoveListener(OnGeneratePressed);
        if (undoButton != null)
            undoButton.onClick.RemoveListener(OnUndoPressed);
    }
}
