using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class TextAnalysisManager : MonoBehaviour
{
    // --- API 配置 ---
    [Header("API Settings")]
    [Tooltip("你的 AI 服务的 API 密钥")]
    [SerializeField] private string apiKey = "sk-67073973738e41a88e92b241e13f422f"; // TODO: 替换为你的 API Key
    [Tooltip("使用的 AI 模型名称 (如果需要)")]
    [SerializeField] private string modelName = "deepseek-chat"; // TODO: 根据需要修改或移除
    [Tooltip("你的 AI 服务的 API 端点 URL")]
    [SerializeField] private string apiUrl = "https://api.deepseek.com/v1/chat/completions"; // TODO: 替换为你的 API URL

    // --- 分析参数 ---
    [Header("Analysis Settings")]
    [Tooltip("控制生成文本的随机性 (如果 API 支持)")]
    [Range(0, 2)] public float temperature = 0.5f;
    [Tooltip("生成结果的最大令牌数 (如果 API 支持)")]
    [Range(1, 1000)] public int maxTokens = 100;
    [Tooltip("发送给 AI 的分析指令")]
    [TextArea(3, 10)]
    [SerializeField] private string analysisInstruction = "无论用户输入什么，你都返回一个字符串，字符串内容为：'test'"; // 你可以在这里设置默认指令，或在 Inspector 中修改

    // --- 内部状态 ---
    // 对于简单分析，我们可能不需要维护长期对话历史
    // private List<Message> messages = new List<Message>();

    // --- 回调委托 ---
    public delegate void AnalysisCallback(string response, bool isSuccess);

    /// <summary>
    /// 发送文本分析请求
    /// </summary>
    /// <param name="textToAnalyze">需要分析的文本</param>
    /// <param name="callback">处理 API 响应的回调函数</param>
    public void SendAnalysisRequest(string textToAnalyze, AnalysisCallback callback)
    {
        // 对于单次分析，我们每次都构建新的消息列表
        List<Message> currentMessages = new List<Message>
        {
            new Message { role = "system", content = analysisInstruction },
            new Message { role = "user", content = textToAnalyze }
        };

        StartCoroutine(ProcessAnalysisRequest(currentMessages, callback));
    }

    /// <summary>
    /// 处理分析请求的协程
    /// </summary>
    /// <param name="messages">包含指令和待分析文本的消息列表</param>
    /// <param name="callback">回调函数</param>
    private IEnumerator ProcessAnalysisRequest(List<Message> messages, AnalysisCallback callback)
    {
        // 构建请求体 (假设 API 结构类似聊天 API)
        // TODO: 你可能需要根据你的 AI 服务 API 文档调整这个结构
        AnalysisRequest requestBody = new AnalysisRequest
        {
            model = modelName,
            messages = messages,
            temperature = temperature,
            max_tokens = maxTokens
        };

        string jsonBody = JsonUtility.ToJson(requestBody);
        Debug.Log("Sending Analysis JSON: " + jsonBody);

        UnityWebRequest request = CreateWebRequest(jsonBody);
        yield return request.SendWebRequest();

        if (IsRequestError(request))
        {
            Debug.LogError($"API Error: {request.responseCode}\n{request.downloadHandler.text}");
            callback?.Invoke("请求失败", false); // 将错误信息传递给回调
            yield break;
        }

        // 解析响应
        // TODO: 你可能需要根据你的 AI 服务 API 文档调整这个结构
        AnalysisResponse response = ParseResponse(request.downloadHandler.text);
        if (response != null && response.choices != null && response.choices.Length > 0 && response.choices[0].message != null)
        {
            string analysisResult = response.choices[0].message.content;
            Debug.Log("AI Analysis Result: " + analysisResult);
            callback?.Invoke(analysisResult, true);
        }
        else
        {
            Debug.LogError("未能从 API 响应中解析出有效结果。原始响应: " + request.downloadHandler.text);
            callback?.Invoke("无法解析响应", false);
        }
    }

    /// <summary>
    /// 创建 UnityWebRequest 对象
    /// </summary>
    private UnityWebRequest CreateWebRequest(string jsonBody)
    {
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
        var request = new UnityWebRequest(apiUrl, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", $"Bearer {apiKey}"); // 假设使用 Bearer Token 认证
        request.SetRequestHeader("Accept", "application/json");
        // TODO: 根据需要添加或修改请求头
        return request;
    }

    /// <summary>
    /// 检查请求是否出错
    /// </summary>
    private bool IsRequestError(UnityWebRequest request)
    {
        return request.result == UnityWebRequest.Result.ConnectionError ||
               request.result == UnityWebRequest.Result.ProtocolError ||
               request.result == UnityWebRequest.Result.DataProcessingError;
    }

    /// <summary>
    /// 解析 API 响应
    /// </summary>
    private AnalysisResponse ParseResponse(string jsonResponse)
    {
        try
        {
            // TODO: 确保 AnalysisResponse 结构匹配你的 API 响应
            return JsonUtility.FromJson<AnalysisResponse>(jsonResponse);
        }
        catch (System.Exception e)
        {
            // 将多行插值字符串合并为一行，使用 \n 表示换行
            Debug.LogError($"JSON 解析失败: {e.Message}\n响应内容：{jsonResponse}");
            return null;
        }
    }

    // --- 可序列化的数据结构 ---
    // TODO: 根据你的 AI 服务 API 文档调整这些结构

    [System.Serializable]
    private class AnalysisRequest
    {
        public string model;
        public List<Message> messages;
        public float temperature;
        public int max_tokens;
        // 可能还有其他参数
    }

    [System.Serializable]
    public class Message // 这个结构在很多 API 中通用，可能不需要改
    {
        public string role; // e.g., "system", "user", "assistant"
        public string content;
    }

    [System.Serializable]
    private class AnalysisResponse
    {
        public Choice[] choices;
        // 可能还有其他字段，如 id, usage 等
    }

    [System.Serializable]
    private class Choice
    {
        public Message message;
        // 可能还有 finish_reason 等字段
    }
} 