using UnityEngine;
using UnityEngine.UI;

public class Example : MonoBehaviour
{
    public InputField inputField;

    private void Start()
    {
        inputField.onValueChanged.AddListener(OnInputValueChanged);
    }

    private void OnInputValueChanged(string value)
    {
        Debug.Log("输入框内容改变：" + value);
    }
}