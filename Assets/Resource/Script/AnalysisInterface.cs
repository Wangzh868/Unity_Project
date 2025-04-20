using UnityEngine;
using UnityEngine.UI; // 需要 UI 命名空间
using TMPro; // 需要 TextMeshPro 命名空间
using System.Collections; // 如果需要使用协程或其他集合

public class AnalysisInterface : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("用于输入待分析文本的输入框 (InputField)")]
    [SerializeField] private InputField textToAnalyzeInput; // InputField reference (potentially unused directly now but kept for context)
    // 移除单条结果的 Text 引用
    // [Tooltip("用于显示分析结果的文本框 (Text)")]
    // [SerializeField] private Text analysisResultText; 

    [Header("Dynamic Results Display")][Space(5)] // Add Space for better Inspector grouping
    [Tooltip("包含 Image, Text 和 UIDragHandler 的可拖动分析结果预制件")]
    [SerializeField] private GameObject analysisResultPrefab; // 改名并更新 Tooltip
    [Tooltip("用于放置实例化的分析结果对象的容器 (通常是 Canvas 或特定 Panel)")]
    [SerializeField] private RectTransform resultsContainer; // 容器引用保持
    [Tooltip("要在实例化的预制件 Image 组件上显示的 Sprite")][Space(5)] // Add Space
    [SerializeField] private Sprite analysisSprite; // 新增：用于图像的 Sprite

    [Header("Logic References")]
    [Tooltip("对 TextAnalysisManager 组件的引用")]
    [SerializeField] private TextAnalysisManager analysisManager; // AI 分析服务的引用

    void Start()
    {
        // 更新验证逻辑
        if (textToAnalyzeInput == null || analysisManager == null || analysisResultPrefab == null || resultsContainer == null)
        {
            Debug.LogError("AnalysisInterface: 请在 Inspector 中设置 InputField, TextAnalysisManager, AnalysisResultPrefab 和 ResultsContainer 引用!");
            enabled = false; // Disable if setup is incomplete
            return;
        }
        // 检查预制件是否包含所需组件 (更全面的检查)
        if (analysisResultPrefab.GetComponentInChildren<Text>() == null || 
            analysisResultPrefab.GetComponentInChildren<Image>() == null ||
            analysisResultPrefab.GetComponent<UIDragHandler>() == null) // Drag handler on the root
        {
             Debug.LogError("AnalysisInterface: AnalysisResultPrefab 必须包含 UIDragHandler 脚本，并且其子对象中必须包含 Text 和 Image 组件!");
             enabled = false;
             return;
        }
        // 可选：检查 analysisSprite 是否已分配
        if (analysisSprite == null)
        {
             Debug.LogWarning("AnalysisInterface: Analysis Sprite 未在 Inspector 中分配，Image 将为空。");
        }
    }

    /// <summary>
    /// 处理 InputField 提交事件 (由 Chara 调用)
    /// </summary>
    public void HandleInputSubmit(string inputText)
    {
        if (string.IsNullOrWhiteSpace(inputText))
        {
            Debug.LogWarning("AnalysisInterface: Input text is empty or whitespace.");
            // Maybe display a temporary message in the container?
            // InstantiateTemporaryMessage("请输入需要分析的文本。"); 
            return;
        }

        // 可选：在开始分析时显示某种"加载中"提示
        // InstantiateTemporaryMessage("正在分析中...");

        analysisManager.SendAnalysisRequest(inputText, HandleAnalysisResponse);
    }

    /// <summary>
    /// 处理从 TextAnalysisManager 返回的分析结果的回调函数
    /// </summary>
    private void HandleAnalysisResponse(string response, bool success)
    {
        // 可选：移除之前的"加载中"提示
        // RemoveTemporaryMessages();

        if (success)
        {
            // 实例化可拖动的分析结果预制件
            GameObject newResultInstance = Instantiate(analysisResultPrefab, resultsContainer);
            
            // --- 配置实例化的预制件 ---
            // 获取 Text 组件 (假设在子对象中)
            Text resultTextComponent = newResultInstance.GetComponentInChildren<Text>();
            if (resultTextComponent != null)
            {
                resultTextComponent.text = response; // 直接显示 AI 回复，不再加前缀
            }
            else { Debug.LogError("AnalysisInterface: 在实例化的预制件中找不到 Text 组件!"); }

            // 获取 Image 组件 (假设在子对象中或根对象上)
            Image resultImageComponent = newResultInstance.GetComponentInChildren<Image>();
            if (resultImageComponent != null)
            {
                if (analysisSprite != null) {
                    resultImageComponent.sprite = analysisSprite;
                }
                else { Debug.LogWarning("AnalysisInterface: 无法设置图像，因为 analysisSprite 未分配。"); }
            }
            else { Debug.LogError("AnalysisInterface: 在实例化的预制件中找不到 Image 组件!"); }

            // UIDragHandler 脚本已在预制件上，无需额外操作

            // --- 布局和滚动 (可选) ---
            // 注意: 如果 resultsContainer 使用了 LayoutGroup, 拖动可能会与布局冲突。
            // 可能需要将实例化的对象放在一个没有 LayoutGroup 的父对象下，
            // 或者在开始拖动时临时改变父对象或禁用 LayoutGroup。
            // 如果使用了 ScrollView，可能需要滚动到底部
            // StartCoroutine(ScrollToBottom()); 
        }
        else
        {
            // 分析失败，实例化一个错误提示 Text
            InstantiateTemporaryMessage("分析失败: " + response, true); // Pass true to indicate error
            Debug.LogError("AnalysisInterface: 分析请求失败或无法解析响应。");
        }
    }

    // 可选辅助方法：实例化一个临时的（或错误的）消息
    private void InstantiateTemporaryMessage(string message, bool isError = false)
    {
         GameObject messageObject = Instantiate(analysisResultPrefab, resultsContainer);
         Text messageText = messageObject.GetComponentInChildren<Text>();
         if(messageText != null)
         {
            messageText.text = message;
            if(isError) {
                messageText.color = Color.red; // 标红错误消息
            }
            // 可以添加一个脚本让这个消息过几秒后自动销毁
            // Destroy(messageObject, 5f);
         }
         else
         {
             Destroy(messageObject);
         }
    }

    // 可选: 如果使用 ScrollView, 确保滚动到底部显示最新消息
    /*
    IEnumerator ScrollToBottom()
    {
        // Wait for the end of the frame to ensure layout is updated
        yield return new WaitForEndOfFrame();
        ScrollRect scrollRect = resultsContainer.GetComponentInParent<ScrollRect>();
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 0f; // 0 is bottom, 1 is top
        }
    }
    */

    // 旧的打字机效果代码不再直接适用，已移除
} 