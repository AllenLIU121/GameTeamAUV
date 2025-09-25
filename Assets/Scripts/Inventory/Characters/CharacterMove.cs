using UnityEngine;

namespace Inventory.Characters
{
    public class CharacterMove:MonoBehaviour
    {
        public Vector2 CurPosition;
        //移动速度
        public float MoveSpeed = 2f;
        void Start()
        {
            //获取 角色初始位置
            CurPosition = new Vector2(transform.position.x, transform.position.y);
        }
        void Update()
        {
            UpdatePosition();
            if (Input.GetMouseButtonDown(0))
            {
                AudioManager.Instance.PlaySFX("Assets/Audio/ME/8.鼠标点击音效0925_01.wav");
                Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(mouseWorldPosition, Vector2.zero);
                if (hit.collider != null && hit.collider.tag == "Plane")
                {
                    transform.position = Vector2.Lerp(CurPosition, mouseWorldPosition, MoveSpeed * Time.deltaTime);
                }
            }
        }
        public  void UpdatePosition()
        {
            CurPosition = new Vector2(transform.position.x, transform.position.y);
        }
    }
}