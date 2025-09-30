using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene2BackgroundScale : MonoBehaviour
{
    public float[] screenWH;
    // Start is called before the first frame update
    void Start()
    {
        ScreenRatioScale();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ScreenRatioScale()
    {
      screenWH = new float[] { Screen.width, Screen.height };

        if (screenWH[0] / screenWH[1] > 1)
        {
            this.transform.localScale *= (screenWH[0] / screenWH[1]);
        }
        else
        {
            this.transform.localScale *= screenWH[0] / screenWH[1];
        }
    }
}
