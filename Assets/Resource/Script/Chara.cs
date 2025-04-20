using System.Collections;
using System.Collections.Generic; // Needed for List<string>
using System.Linq; // Needed for LINQ methods like Where
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events; // Needed for UnityAction<string>

public class Chara : MonoBehaviour
{
    //引用和配置
    [Header("References")]
    [SerializeField] private ChatAIManager dialogueManager; // AI聊天管理器
    [SerializeField] private InputField inputField; // 共享的输入框
    [SerializeField] private Text dialogueText; // 聊天回复显示文本
    [Tooltip("对 AnalysisInterface 脚本的引用，用于转发分析请求")]
    [SerializeField] private AnalysisInterface analysisInterfaceScript; // 新增：分析接口引用
    [SerializeField] private GameObject chatboxGameObject; // 新：聊天框游戏对象引用 (更通用)
    private string characterName;

    [Header("Settings")]
    [SerializeField] private float typingSpeed = 0.05f; // 打字机效果的字符显示速度
    [Tooltip("AI 回复中用于分割段落的分隔符。使用 \n\n 表示双换行符。")]
    [SerializeField] private string segmentDelimiter = "\n\n"; // 用于分割回复的字符串

    // Store the listener action to allow removal later
    private UnityAction<string> submitListener;
    private bool forwardNextSubmitToAnalysis = false; // 新增：转发标志

    // --- 新增：用于分段显示的状态变量 ---
    private List<string> currentResponseSegments = new List<string>();
    private int currentSegmentIndex = -1;
    private bool isTypingCurrentSegment = false; // New state flag
    private Coroutine currentDisplayCoroutine; // 用于管理打字机协程
    // --- 结束新增 ---

    void Start()
    {
        // Check if dialogueManager is assigned
        if (dialogueManager == null)
        {
            Debug.LogError("Chara: Dialogue Manager (ChatAIManager) is not assigned!");
            return;
        }
        // Check if dialogueManager.npcCharacter is assigned (might be null if ChatAIManager isn't fully set up)
        if (dialogueManager.npcCharacter != null)
        {
            characterName = dialogueManager.npcCharacter.name; //角色姓名赋值
        }
        else
        {
            characterName = "AI"; // Default name if not set
            Debug.LogWarning("Chara: npcCharacter not set in Dialogue Manager. Using default name 'AI'.");
        }

        // 新增：检查 Analysis Interface 引用
        if (analysisInterfaceScript == null)
        {
            Debug.LogError("Chara: Analysis Interface Script is not assigned! Cannot forward analysis requests.");
            // Consider disabling analysis functionality or returning
        }
        // 检查 dialogueText 引用
        if (dialogueText == null)
        {
            Debug.LogError("Chara: Dialogue Text is not assigned! Cannot display responses.");
            return; // Critical component missing
        }
        // 新增：检查 Chatbox GameObject 引用
        if (chatboxGameObject == null)
        {
            Debug.LogWarning("Chara: Chatbox GameObject is not assigned. Visibility control will be disabled.");
        }
        else
        {
            // 可选：确保初始状态是隐藏的，如果 dialogueText 初始为空
            UpdateChatboxVisibility();
        }

        //输入框提交后执行的回调函数
        // Define the listener action
        submitListener = (text) =>
        {
            // 检查是否需要转发给分析接口
            if (forwardNextSubmitToAnalysis)
            {
                Debug.Log($"Chara forwarding submit to AnalysisInterface: {text}");
                if (analysisInterfaceScript != null)
                {
                    // 调用分析接口的处理方法
                    analysisInterfaceScript.HandleInputSubmit(text);
                }
                else
                {
                    Debug.LogError("Chara: Cannot forward to AnalysisInterface, reference is missing!");
                }
                forwardNextSubmitToAnalysis = false; // 重置标志，下次提交恢复聊天
                // 可选：清空输入框
                // inputField.text = "";
                return; // 停止后续的聊天处理
            }

            // --- 正常聊天逻辑 ---
            Debug.Log($"Chara handling InputField submit for Chat: {text}");
            // 在发送新请求前，清除之前的分段显示状态
            ClearSegmentState();
            if (dialogueManager != null)
            {
                dialogueManager.SendDialogueRequest(text, HandleAIResponse);//发送对话请求到 AI Manager
            }
            else
            {
                Debug.LogError("Chara: Cannot send dialogue request, Dialogue Manager is missing!");
            }
            // 可选：清空输入框
            // inputField.text = "";
        };

        // Add the listener
        inputField.onSubmit.AddListener(submitListener);
    }

    // --- 新增：Update 方法处理鼠标点击 ---
    void Update()
    {
        // 检测鼠标左键点击，并且我们正在处理分段显示 (index is valid)
        if (currentSegmentIndex != -1 && Input.GetMouseButtonDown(0))
        {
            if (isTypingCurrentSegment && currentDisplayCoroutine != null)
            {
                // --- 状态 1: 正在打字 -> 显示完整当前段落 ---
                StopCoroutine(currentDisplayCoroutine);
                currentDisplayCoroutine = null;
                string fullPrefix = characterName + "";
                dialogueText.text = fullPrefix + currentResponseSegments[currentSegmentIndex]; // Show full text
                isTypingCurrentSegment = false; // Typing finished (interrupted)
                UpdateChatboxVisibility(); // 更新可见性
                Debug.Log($"Typewriter interrupted. Displaying full segment {currentSegmentIndex + 1}.");
            }
            else if (!isTypingCurrentSegment)
            {
                // --- 状态 2: 当前段落已完整显示 -> 显示下一段或清除 ---
                ShowNextSegmentOrClear();
            }
        }
    }
    // --- 结束新增 ---

    /// <summary>
    /// 公开方法，由 LabelController 调用，准备下一次提交进行分析。
    /// </summary>
    public void PrepareForAnalysisSubmit()
    {
        forwardNextSubmitToAnalysis = true;
        Debug.Log("Chara prepared: Next submit will be forwarded for analysis.");
        // 可选：在这里添加视觉提示，比如改变输入框颜色或显示提示信息
        // 如果正在等待分段显示，也需要清除状态
        ClearSegmentState();
        dialogueText.text = "等待分析输入..."; // 提示用户
    }

    /// <summary>
    /// 在对话框使用打字机效果显示一条即时消息，并停止当前的打字机/分段效果。
    /// </summary>
    /// <param name="message">要显示的消息内容</param>
    public void ShowImmediateMessage(string message)
    {
        // 清除分段状态并停止任何正在运行的协程
        ClearSegmentState();

        if (dialogueText != null)
        {
            // 启动新的打字机协程来显示完整消息
            currentDisplayCoroutine = StartCoroutine(TypewriterEffect(message));
            Debug.Log($"Chara starting typewriter for immediate message: {message}");
        }
        else
        {
            Debug.LogError("Chara: Cannot show immediate message, dialogueText is missing!");
        }
    }


    /// <summary>
    /// 处理AI的响应，实现分段显示
    /// </summary>
    /// <param name="response">AI的回复内容</param>
    /// <param name="success">请求是否成功</param>
    private void HandleAIResponse(string response, bool success)
    {
        // 清除之前的状态
        ClearSegmentState();

        Debug.Log($"AI Response Received: {response}");
        string responseToShow = success ? response : "（通讯中断）";
        string fullPrefix = characterName + "";

        // 使用配置的分隔符分割回复，并移除空条目
        // 注意：需要将编辑器中的 "\n\n" 转换为实际的 "\n\n"
        string processedDelimiter = segmentDelimiter.Replace("\\n", "\n"); // Correct way to handle literal \n from Inspector
        currentResponseSegments = responseToShow.Split(new[] { processedDelimiter }, System.StringSplitOptions.RemoveEmptyEntries)
                                             .Select(s => s.Trim()) // 去除每段前后的空白
                                             .Where(s => !string.IsNullOrEmpty(s)) // 过滤掉纯空白段落
                                             .ToList();

        if (currentResponseSegments.Count > 0)
        {
            currentSegmentIndex = 0; // Start with the first segment
            // 显示第一段 (使用打字机效果)
            string firstSegmentText = fullPrefix + currentResponseSegments[currentSegmentIndex];
            currentDisplayCoroutine = StartCoroutine(TypewriterEffect(firstSegmentText));
            isTypingCurrentSegment = true; // Start typing

            Debug.Log(string.Format("Started typewriter for segment {0}/{1}.",
                currentSegmentIndex + 1,
                currentResponseSegments.Count));
        }
        else
        {
            // 如果分割后没有有效段落，直接显示完整回复或错误信息 (也可以用打字机)
            currentDisplayCoroutine = StartCoroutine(TypewriterEffect(fullPrefix + responseToShow));
            isTypingCurrentSegment = false; // No segments, so not typing a segment
            currentSegmentIndex = -1; // Indicate no active segments
            Debug.LogWarning("AI response was empty or contained no segments after splitting. Displaying full response.");
        }
    }

    // --- 修改：显示下一段或清除 ---
    private void ShowNextSegmentOrClear() // Renamed for clarity
    {
        currentSegmentIndex++; // Advance to next index potentiall

        if (currentSegmentIndex < currentResponseSegments.Count)
        {
            // 索引在范围内：显示下一段
            string fullPrefix = characterName + "";
            string nextSegmentText = fullPrefix + currentResponseSegments[currentSegmentIndex];
            currentDisplayCoroutine = StartCoroutine(TypewriterEffect(nextSegmentText));
            isTypingCurrentSegment = true; // Start typing the new segment

            Debug.Log(string.Format("Started typewriter for segment {0}/{1}.",
                currentSegmentIndex + 1,
                currentResponseSegments.Count));
        }
        else
        {
            // 索引超出范围：清除文本并重置状态
            dialogueText.text = ""; // 清除文本
            isTypingCurrentSegment = false; // Ensure state is reset
            currentSegmentIndex = -1; // Mark segment display as inactive
            currentDisplayCoroutine = null; // Ensure coroutine handle is cleared
            UpdateChatboxVisibility(); // 更新可见性
            Debug.Log("All segments displayed and text cleared on final click.");
        }
    }
    // --- 结束修改 ---

    // --- 新增：清除分段状态和协程 ---
    private void ClearSegmentState()
    {
        if (currentDisplayCoroutine != null)
        {
            StopCoroutine(currentDisplayCoroutine);
            currentDisplayCoroutine = null;
        }
        currentResponseSegments.Clear();
        currentSegmentIndex = -1;
        isTypingCurrentSegment = false; // Reset the new flag
        // 可选：不清空 dialogueText.text，取决于你希望旧对话如何处理
        // dialogueText.text = "";
        UpdateChatboxVisibility(); // 在清除状态时也检查一下可见性
    }
    // --- 结束新增 ---

    // --- 新增：控制聊天框可见性的方法 ---
    private void UpdateChatboxVisibility()
    {
        if (chatboxGameObject != null)
        {
            bool shouldBeVisible = !string.IsNullOrEmpty(dialogueText.text);
            if (chatboxGameObject.activeSelf != shouldBeVisible)
            {
                 chatboxGameObject.SetActive(shouldBeVisible);
                 // Debug.Log($"Chatbox Active: {shouldBeVisible} (Text: '{dialogueText.text}')"); // Optional debug
            }
        }
    }
    // --- 结束新增 ---

    /// <summary>
    /// 打字机效果协程（现在用于显示每一段和即时消息）
    /// </summary>
    /// <param name="text">要显示的完整文本</param>
    /// <returns></returns>
    private IEnumerator TypewriterEffect(string text)
    {
        dialogueText.text = ""; // 清空文本开始打字
        UpdateChatboxVisibility(); // 开始打字前（文本为空），隐藏聊天框

        string currentText = "";//当前显示的文本
        // 如果传入的文本本身就是空的，直接结束
        if (string.IsNullOrEmpty(text))
        {
            isTypingCurrentSegment = false;
            currentDisplayCoroutine = null;
            UpdateChatboxVisibility(); // 确保空文本时隐藏
            yield break; // 退出协程
        }

        // 确保在开始循环前聊天框是可见的（因为文本即将有内容）
        if (chatboxGameObject != null && !chatboxGameObject.activeSelf) // 新方法
        {
            chatboxGameObject.SetActive(true);
        }

        foreach (char c in text)//遍历每个字符
        {
            currentText += c;//添加字符到当前文本
            dialogueText.text = currentText;//更新显示文本
            // UpdateChatboxVisibility(); // 不需要在每帧都调用，文本非空时应一直显示
            yield return new WaitForSeconds(typingSpeed);//等待一定时间
        }
        // Typewriter finished naturally
        isTypingCurrentSegment = false; // Mark as finished typing
        currentDisplayCoroutine = null; // Coroutine handle is no longer valid
        UpdateChatboxVisibility(); // 确保文本显示完整后聊天框是可见的
        Debug.Log("Typewriter effect finished naturally.");
    }

    // --- 新增：OnDestroy 清理监听器 ---
    void OnDestroy()
    {
        if (inputField != null && submitListener != null)
        {
            inputField.onSubmit.RemoveListener(submitListener);
        }
        // 停止所有协程，以防万一
        StopAllCoroutines();
    }
    // --- 结束新增 ---
}