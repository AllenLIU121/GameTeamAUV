using UnityEngine;
using DialogueSystem;
namespace Inventory.Characters
{
    public class CharacterMove : MonoBehaviour
    {
        public Vector2 CurPosition;
        //移动速度
        public float MoveSpeed = 2f;
        // 对话管理器引用
        private DialogueManager dialogueManager;

        void Start()
        {
            //获取 角色初始位置
            CurPosition = new Vector2(transform.position.x, transform.position.y);
            // 获取对话管理器实例
            dialogueManager = FindObjectOfType<DialogueManager>();
        }
        void Update()
        {
            // 检查对话是否正在进行，如果是则不处理移动输入
            if (dialogueManager != null && dialogueManager.IsDialogueActive())
            {
                return;
            }

            // 使用WASD键控制移动
            float moveX = 0f;
            float moveY = 0f;

            if (Input.GetKey(KeyCode.W))
            {
                moveY = 1f;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                moveY = -1f;
            }

            if (Input.GetKey(KeyCode.D))
            {
                moveX = 1f;
            }
            else if (Input.GetKey(KeyCode.A))
            {
                moveX = -1f;
            }

            // 计算移动方向向量并归一化
            Vector2 moveDirection = new Vector2(moveX, moveY).normalized;

            // 应用移动
            if (moveDirection != Vector2.zero)
            {
                transform.Translate(moveDirection * MoveSpeed * Time.deltaTime);
            }

            // 更新当前位置
            UpdatePosition();
        }
        public void UpdatePosition()
        {
            CurPosition = new Vector2(transform.position.x, transform.position.y);
        }
    }
}