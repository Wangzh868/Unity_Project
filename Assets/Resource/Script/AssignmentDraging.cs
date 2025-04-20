using UnityEngine;

public class DragItem : MonoBehaviour
{
    private bool isDragging = false;
    private Vector3 offset; // �������Ʒ���ĵ�ƫ����

    void Update()
    {
        
        if (Input.GetMouseButtonDown(0))
        {
           
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = transform.position.z; // ����z��λ������Ʒ��ͬ��ȷ����ͬһƽ����

            // �������Ƿ���������Ʒ
            Collider2D hit = Physics2D.OverlapPoint(mousePosition);
            if (hit != null && hit.gameObject == gameObject)
            {
                isDragging = true;

                offset = gameObject.transform.position - mousePosition;
            }
        }

        // ��������϶���������Ʒλ��
        if (isDragging)
        {

            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = transform.position.z; // ����z��λ������Ʒ��ͬ


            transform.position = mousePosition + offset;
        }


        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
    }
}