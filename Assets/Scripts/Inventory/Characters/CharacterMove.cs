using UnityEngine;
using DialogueSystem;
namespace Inventory.Characters
{
    public class CharacterMove : MonoBehaviour
    {
        public float moveSpeed = 2f;
        private Rigidbody2D rb;
        private Vector2 movementInput;

        // 对话管理器引用
        private DialogueManager dialogueManager;

        void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        void Start()
        {
            // 获取对话管理器实例
            dialogueManager = FindObjectOfType<DialogueManager>();
        }

        void Update()
        {
            HandlePlayerMoveInput();

            // 检查对话是否正在进行，如果是则不处理鼠标点击
            if (dialogueManager != null && dialogueManager.IsDialogueActive())
            {
                return;
            }

            // if (Input.GetMouseButtonDown(0))
            // {
            //     Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            //     // 检测点击点是否在“任何碰撞器”内部
            //     Collider2D collider = Physics2D.OverlapPoint(mouseWorldPosition);

            //     // 如果“不在任何碰撞器内部”（即点击绿色线外），执行移动
            //     if (collider == null)
            //     {
            //         transform.position = Vector2.Lerp(CurPosition, mouseWorldPosition, MoveSpeed * Time.deltaTime);
            //     }
            // }
        }

        void FixedUpdate()
        {
            rb.MovePosition(rb.position + movementInput.normalized * moveSpeed * Time.fixedDeltaTime);
        }

        void HandlePlayerMoveInput()
        {
            float moveX = Input.GetAxisRaw("Horizontal");
            float moveY = Input.GetAxisRaw("Vertical");
            
            movementInput = new Vector2(moveX, moveY);
        }
    }
}