using UnityEngine;
using UnityEngine.UI;

public class ClearInputField : MonoBehaviour
{
    public InputField inputField; // 绑定你的 InputField

    // 在 Inspector 中调用的方法
    public void ClearText()
    {
        inputField.text = ""; // 清空内容
        inputField.ActivateInputField(); // 可选：保持输入框焦点
    }
}