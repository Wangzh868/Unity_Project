using UnityEngine;
using TMPro;

// 数据类，用于存储物体和输入栏的对应关系
public class ObjectInputFieldPair
{
    public GameObject spawnedObject; // 生成的物体
    public TMP_InputField inputField; // 对应的输入栏

    public ObjectInputFieldPair(GameObject obj, TMP_InputField field)
    {
        spawnedObject = obj;
        inputField = field;
    }
}
public enum AnimIdel
{
    Idel,
    Is_speaking,
    Is_thinking,
    Is_shy
}
public static class CharaAnim
{
    public static float Likeability { get; set; }
    public static AnimIdel AnimIdel { get; set; }

}




