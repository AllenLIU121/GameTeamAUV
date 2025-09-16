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
                //做个2d的，其实我感觉这里做3d的射线会好一些，先这样做吧
                Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                RaycastHit2D hit = Physics2D.Raycast(mouseWorldPosition, Vector2.zero);
                //这里可以选择不同的判断条件，tag、层级、碰撞体名字，后期可改，如果是检测到了多个目标，则说明此处不能移动
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