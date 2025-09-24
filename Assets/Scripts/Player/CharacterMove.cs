using System.Collections;
using UnityEngine;

// namespace Inventory.Characters
// {
[RequireComponent(typeof(Rigidbody2D))]
public class CharacterMove : MonoBehaviour
{
    // public Vector2 CurPosition;
    //移动速度
    public float MoveSpeed = 4f;
    private SpriteRenderer portrait;
    private Rigidbody2D rb;
    private Vector2 movementInput;
    public bool canMove = true;

    void Awake()
    {
        portrait = GetComponentInChildren<SpriteRenderer>();
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
        if (canMove)
            HandlePlayerMoveInput();
    }

    void HandlePlayerMoveInput()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        movementInput = new Vector2(moveX, moveY);

        if (moveX > 0f)
        {
            portrait.flipX = true;
        }
        else if (moveX < 0f)
        {
            portrait.flipX = false;
        }
    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + movementInput.normalized * MoveSpeed * Time.fixedDeltaTime);
    }

    public void BeforeSceneChanged()
    {
        canMove = false;

        GetComponent<CapsuleCollider2D>().enabled = false;
        GetComponentInChildren<SpriteRenderer>().enabled = false;

        StoreManager.Instance.CloseStore();

        StartCoroutine(PlayCarAnimation());
    }

    private IEnumerator PlayCarAnimation()
    {
        GameObject carGO = GameObject.Find("Car");
        if (carGO != null)
        {
            carGO.GetComponent<Animation>().Play();
            yield return new WaitForSeconds(2f);
        }

        SceneController.Instance.LoadSceneAsync(GameConstants.SceneName.ChapterTwoScene);
    }
}