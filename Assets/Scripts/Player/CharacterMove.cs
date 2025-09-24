using UnityEngine;

// namespace Inventory.Characters
// {
[RequireComponent(typeof(Rigidbody2D))]
public class CharacterMove : MonoBehaviour
{
    // public Vector2 CurPosition;
    //移动速度
    public float MoveSpeed = 4f;
    private Rigidbody2D rb;
    private Vector2 movementInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // void Start()
    // {
    //     //获取 角色初始位置
    //     CurPosition = new Vector2(transform.position.x, transform.position.y);
    // }
    void Update()
    {
        // UpdatePosition();
        // if (Input.GetMouseButtonDown(0))
        // {
        //     Vector2 mouseWorldPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //     RaycastHit2D hit = Physics2D.Raycast(mouseWorldPosition, Vector2.zero);
        //     if (hit.collider != null && hit.collider.tag == "Plane")
        //     {
        //         transform.position = Vector2.Lerp(CurPosition, mouseWorldPosition, MoveSpeed * Time.deltaTime);
        //     }
        // }
        HandlePlayerMoveInput();
    }

    void HandlePlayerMoveInput()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        movementInput = new Vector2(moveX, moveY);
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movementInput.normalized * MoveSpeed * Time.fixedDeltaTime);
    }

    public void UpdatePosition()
    {
        transform.position = new Vector2(transform.position.x, transform.position.y);
    }
}