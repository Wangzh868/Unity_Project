using UnityEngine;
using TMPro;

// �����࣬���ڴ洢������������Ķ�Ӧ��ϵ
public class ObjectInputFieldPair
{
    public GameObject spawnedObject; // ���ɵ�����
    public TMP_InputField inputField; // ��Ӧ��������

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




