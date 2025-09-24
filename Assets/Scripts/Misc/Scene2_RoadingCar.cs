using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene2_RoadingCar : MonoBehaviour
{
    public Transform[] imageTransform;
    public int childCount;
    public bool start = false;
    //public float imagePositionY;
    public Vector3 imagesResetPosition;
    public float carSpeed = 10;

    public bool isStoped = true;
    public int nextViewImage = -1;

    public Sprite specialStopSprites;
    public Sprite defaultSprite;
    // Start is called before the first frame update
    void Start()
    {
        childCount = this.transform.childCount;
        imageTransform = new Transform[childCount];
        for (int i = 0; i < childCount; i++)
        {
            imageTransform[i] = this.transform.GetChild(i);

        }
        //imagePositionY = imageTransform[0].position.y;
        imagesResetPosition = new Vector3(10, 0, -0.3f);
    }

    // Update is called once per frame
    void Update()
    {
        if (start)
        {
            if (nextViewImage != -1)
            {
                imageTransform[nextViewImage].GetChild(1).GetComponent<SpriteRenderer>().sprite = defaultSprite;
            }
            for (int i = 0; i < childCount; i++)
            {
                imageTransform[i].Translate(-0.001f * carSpeed, 0, 0);

                if (imageTransform[i].localPosition.x <= -5.0f)
                {
                    imageTransform[i].localPosition = imagesResetPosition;
                }
            }

        }

        if (!start)
        {
            float stop = (imageTransform[0].localPosition.x) % 5;
            print(Mathf.Floor(stop));
            if (Mathf.Abs(stop) < 0.05)//取余本身就返回正数,所以也可以不需要abs
            {
                stop = 0;

            }
            if (stop != 0)//检测到最左边移动的位置是5的倍数,就停止
            {
                isStoped = true;
                for (int i = 0; i < childCount; i++)
                {
                    imageTransform[i].Translate(-0.001f * carSpeed, 0, 0);

                    if (imageTransform[i].localPosition.x <= -5.0f)
                    {
                        imageTransform[i].localPosition = imagesResetPosition;
                    }
                }
            }
            if (isStoped)
            {
                for (int i = 0; i < childCount; i++)
                {

                    if (Mathf.Round(imageTransform[i].localPosition.x) == 5)
                    {
                        nextViewImage = i; //获取停止后view当前图片的下一张图的index
                        imageTransform[nextViewImage].GetChild(1).GetComponent<SpriteRenderer>().sprite = specialStopSprites;
                        isStoped = false; //这样这个if只跑一次,在下一次检测到停止的时候打开判断
                        break;
                    }
                }



            }
        }

    }
    private void LateUpdate()
    {
        if (start)
        {
            nextViewImage = -1;
        }
    }

    public void CarMovement()
    {

    }

}
