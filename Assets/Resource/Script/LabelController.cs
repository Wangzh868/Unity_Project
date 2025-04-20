using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LabelController : MonoBehaviour
{
    [Header("Core References")]
    [Tooltip("The GameObject representing the clickable label.")]
    [SerializeField] private GameObject labelObject;
    [Tooltip("The shared InputField used for both chat and analysis.")]
    [SerializeField] private InputField sharedInputField;
    [Tooltip("Reference to the Chara script component.")]
    [SerializeField] private Chara charaScript;

    // Keep other existing fields if needed for DeletePair, etc.
    [Header("Object Interaction (Optional)")]
    [SerializeField] private GameObject fireObject;
    [SerializeField] private GameObject bookObject;
    // Removed prefab and pair list references as they are not used for mode switching
    // [SerializeField] private GameObject objectPrefab;
    // [SerializeField] private TMPro.TMP_InputField inputFieldPrefab;
    // [SerializeField] private Canvas canvas;
    // [SerializeField] private RectTransform rectTransform;
    // [SerializeField] private List<ObjectInputFieldPair> pairs = new List<ObjectInputFieldPair>();
    // [SerializeField] private List<ObjectInputFieldPair> pairsToRemove = new List<ObjectInputFieldPair>();

    void Start()
    {
        // Basic validation
        if (labelObject == null || sharedInputField == null || charaScript == null)
        {
            Debug.LogError("LabelController: LabelObject, SharedInputField, or CharaScript reference is not set!");
            enabled = false;
            return;
        }

        // Ensure the initial listener is set by Chara (Chara's Start should handle this)
        // No explicit action needed here if Chara adds its listener in Start
    }

    void Update()
    {
        TryActivateAnalysisMode();

        // Keep existing logic if needed
        // DeletePair();
        // ConnectionInputFieldObject(); // This likely needs removal or modification
    }

    /// <summary>
    /// Checks for mouse click on the label object and switches InputField listener if clicked.
    /// </summary>
    private void TryActivateAnalysisMode()
    {
        if (Input.GetMouseButtonDown(0)) // Check for left mouse button click
        {
            // Convert mouse position to world point for 2D raycast
            // Ensure you have a Main Camera tagged in your scene
            if (Camera.main == null)
            {
                Debug.LogError("LabelController: No Main Camera found!");
                return;
            }
            Vector2 rayOrigin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.zero);

            // Check if the ray hit something and if it's the designated label object
            if (hit.collider != null && hit.collider.gameObject == labelObject)
            {
                Debug.Log("Label clicked! Preparing Chara for next submit to be analysis.");
                if (charaScript != null)
                {
                    // 调用 Chara 的方法来准备分析转发
                    charaScript.PrepareForAnalysisSubmit();
                    
                    // 新增：调用 Chara 显示即时消息
                    charaScript.ShowImmediateMessage("有新的事情要做吗？快告诉我吧。"); // 在这里替换为你想要的提示文字

                    // 可选：在这里添加视觉提示，比如 Label 短暂高亮
                }
                else
                {
                     Debug.LogError("LabelController: Chara script reference is missing! Cannot prepare for analysis.");
                }
            }
        }
    }

    // --- Keep or remove old methods based on whether they are still needed --- 

    // DeletePair might still be relevant if you spawn objects in another way
    // private void DeletePair()
    // {
    //     // ... existing DeletePair logic ...
    // }

    // ConnectionInputFieldObject is likely obsolete as we don't create new InputFields here
    // private void ConnectionInputFieldObject()
    // {
    //     // ... existing ConnectionInputFieldObject logic ...
    // }

    // IsPointInRectangle might be needed by DeletePair
    // private bool IsPointInRectangle(Vector2 point, Bounds bounds)
    // {
    //     // ... existing IsPointInRectangle logic ...
    // }

    // The ObjectInputFieldPair class definition might be needed if DeletePair is kept
    // [System.Serializable] // Ensure it's serializable if used in lists shown in Inspector
    // public class ObjectInputFieldPair 
    // { 
    //    public GameObject spawnedObject; 
    //    public TMPro.TMP_InputField inputField; // Or standard InputField? Needs consistency
    //    public ObjectInputFieldPair(GameObject obj, TMPro.TMP_InputField field) 
    //    { 
    //        spawnedObject = obj; 
    //        inputField = field; 
    //    } 
    // } 
}
