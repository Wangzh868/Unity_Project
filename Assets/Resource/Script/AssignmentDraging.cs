using UnityEngine;

public class DragItem : MonoBehaviour
{
    private bool isDragging = false;
    private Vector3 offset; // 鼠标与物品中心的偏移量

    void Update()
    {
        
        if (Input.GetMouseButtonDown(0))
        {
           
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = transform.position.z; // 设置z轴位置与物品相同，确保在同一平面上

            // 检查鼠标是否点击到了物品
            Collider2D hit = Physics2D.OverlapPoint(mousePosition);
            if (hit != null && hit.gameObject == gameObject)
            {
                isDragging = true;

                offset = gameObject.transform.position - mousePosition;
            }
        }

        // 如果正在拖动，更新物品位置
        if (isDragging)
        {

            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = transform.position.z; // 设置z轴位置与物品相同


            transform.position = mousePosition + offset;
        }


        if (Input.GetMouseButtonUp(0))
        {
            isDragging = false;
        }
    }
}