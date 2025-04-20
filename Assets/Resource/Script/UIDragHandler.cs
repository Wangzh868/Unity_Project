using UnityEngine;
using UnityEngine.EventSystems; // Required for event system interfaces
using UnityEngine.UI; // Required if you might interact with UI elements like Image

public class UIDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private Canvas canvas;
    private CanvasGroup canvasGroup; // Optional: for visual feedback while dragging
    private Vector2 offset;

    // 新增：引用 Fire 碰撞器
    private Collider2D fireCollider;
    private Collider2D bookCollider; // 新增：Book 碰撞器引用
    private const string fireTag = "DestructionZone"; // 使用常量存储 Tag 名称
    private const string bookTag = "FinishZone"; // 新增：Book 的 Tag

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        // Find the root Canvas to handle coordinate conversions
        canvas = GetComponentInParent<Canvas>(); 
        if (canvas == null)
        {
            Debug.LogError("UIDragHandler requires a Canvas parent!");
            enabled = false;
            return;
        }
        // Optional: Add CanvasGroup for visual feedback (e.g., make slightly transparent while dragging)
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            // Add one if it doesn't exist, useful for BlocksRaycasts
             canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        // 新增：查找 Fire Collider
        GameObject fireObject = GameObject.FindWithTag(fireTag);
        if (fireObject != null)
        {
            fireCollider = fireObject.GetComponent<Collider2D>();
            if (fireCollider == null)
            {
                Debug.LogError($"UIDragHandler: Found object with tag '{fireTag}' but it has no Collider2D component!");
            }
        }
        else
        {
             Debug.LogWarning($"UIDragHandler: Could not find GameObject with tag '{fireTag}'. Destruction check will be skipped.");
             // 你也可以选择禁用此脚本或采取其他错误处理
             // enabled = false;
        }

        // 新增：查找 Book Collider
        GameObject bookObject = GameObject.FindWithTag(bookTag);
        if (bookObject != null)
        {
            bookCollider = bookObject.GetComponent<Collider2D>();
            if (bookCollider == null) { Debug.LogError($"Found '{bookTag}' but no Collider2D!"); }
        }
        else { Debug.LogWarning($"Could not find GameObject with tag '{bookTag}'."); }

        // 添加一个总的检查，如果两个都没找到，或许这个脚本就没意义了
        if (fireCollider == null && bookCollider == null)
        {
             Debug.LogWarning($"UIDragHandler: Neither '{fireTag}' nor '{bookTag}' colliders found. Interaction checks will be skipped.");
             // enabled = false; // 可以选择禁用
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("OnBeginDrag");

        // --- 修正 Offset 计算 ---
        // 1. 获取父对象的 RectTransform
        RectTransform parentRectTransform = transform.parent as RectTransform;
        if (parentRectTransform == null)
        {
            Debug.LogError("UIDragHandler needs a RectTransform parent for correct offset calculation.");
            // 如果没有父级RectTransform，无法正确计算，可能导致后续拖动问题
             offset = Vector2.zero; // 或者根据情况处理
            // return; // 考虑是否要阻止拖动
        }

        // 2. 将当前鼠标点击的屏幕坐标转换到父对象的本地坐标系中
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRectTransform, eventData.position, canvas.worldCamera, out Vector2 parentLocalMousePosition))
        {
            // 3. 计算偏移量：
            //    offset = (物体在其父容器中的位置) - (鼠标点击位置在父容器中的位置)
            offset = rectTransform.anchoredPosition - parentLocalMousePosition;
            Debug.Log($"Offset calculated: {offset} (AnchoredPos: {rectTransform.anchoredPosition}, ParentLocalMouse: {parentLocalMousePosition})");
        }
        else
        {
            Debug.LogError("Failed to convert screen point to parent's local point in OnBeginDrag. Using zero offset.");
            // 如果转换失败，偏移量可能不正确，但拖动仍可继续（物体会跳到鼠标位置）
            offset = Vector2.zero;
        }
        // --- 结束修正 Offset 计算 ---

        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = false; // Allow raycasts to pass through while dragging
            // canvasGroup.alpha = 0.8f; // Optional: make it slightly transparent
        }
        // Optional: Bring to front visually if needed (might require adjusting sibling index)
        // transform.SetAsLastSibling(); 
    }

    public void OnDrag(PointerEventData eventData)
    {
        RectTransform parentRectTransform = transform.parent as RectTransform;
        if (parentRectTransform == null) { return; } // Already logged error in OnBeginDrag if missing

        // 将当前鼠标屏幕坐标转换为父对象的本地坐标
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRectTransform, eventData.position, canvas.worldCamera, out Vector2 localPointerPosition))
        {
            // 使用正确的 offset 应用位置
            rectTransform.anchoredPosition = localPointerPosition + offset;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("OnEndDrag");
        if (canvasGroup != null)
        {
            canvasGroup.blocksRaycasts = true; // Restore raycast blocking
            // canvasGroup.alpha = 1.0f; // Restore alpha
        }
        // 不需要重置 offset，因为它只在 OnBeginDrag 中计算一次
        // offset = Vector2.zero;

        // --- 修改：碰撞检测逻辑 ---
        // 首先检查是否找到了任何目标碰撞器
        if (fireCollider == null && bookCollider == null)
        {
            // 如果一个都没找到，直接返回，不做检查
             return;
        }

        Camera eventCamera = (canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : canvas.worldCamera;
        if (eventCamera == null && canvas.renderMode != RenderMode.ScreenSpaceOverlay) {
             Debug.LogError("UIDragHandler: Canvas camera not found for world point conversion.");
             return;
        }

        // 优先使用 Physics2D.OverlapPoint
        if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            Vector3 screenPoint = eventData.position;
            screenPoint.z = canvas.planeDistance;
            Vector3 worldPoint = eventCamera.ScreenToWorldPoint(screenPoint);
            Collider2D hitCollider = Physics2D.OverlapPoint(new Vector2(worldPoint.x, worldPoint.y));

            if (hitCollider != null)
            {
                // 检查是否碰撞到 Fire
                if (fireCollider != null && hitCollider == fireCollider)
                {
                    Debug.Log($"Dragged item dropped onto '{fireTag}'. Destroying item.");
                    Destroy(gameObject);
                    // TODO:--- 在这里添加与 Fire 碰撞销毁后的额外逻辑 ---
                    // 例如：播放火焰音效、减少好感度等
                    // FireEffect();
                    // ---------------------------------------------
                }
                // 检查是否碰撞到 Book
                else if (bookCollider != null && hitCollider == bookCollider)
                {
                    Debug.Log($"Dragged item dropped onto '{bookTag}'. Destroying item.");
                    Destroy(gameObject);
                    // TODO:--- 在这里添加与 Book 碰撞销毁后的额外逻辑 ---
                    // 例如：播放翻书音效、增加知识点、增加好感度等
                    // BookEffect();
                    // ---------------------------------------------
                }
                else
                {
                     Debug.Log($"Dropped onto: {hitCollider.name}, not a target zone.");
                }
            }
            else
            {
                 Debug.Log("Dropped onto empty space.");
            }
        }
        else // Screen Space Overlay 的处理（仍需谨慎）
        {
             Debug.LogWarning("Collision check in Screen Space Overlay is less reliable. Consider using Screen Space Camera or World Space Canvas if possible.");
             // 这里的逻辑可以保持不变，或者根据具体情况调整，但优先推荐非 Overlay 模式
        }
        // --- 结束碰撞检测逻辑 ---
    }

    // 可以添加具体的 Effect 方法，如果逻辑比较复杂
    // void FireEffect() { ... }
    // void BookEffect() { ... }
} 