using UnityEngine;

public class UIManager : Ply_Singleton<UIManager>
{
    public GameObject winUI;
    public GameObject loseUI;
    public GameObject tutorial;
    public float screenWidth;
    public float screenHeight;
    public float scaleHeightOnWidth;
    public bool isVertical;
    public Camera cam;
   
    protected void Start()
    {
        winUI.SetActive(false);
        loseUI.SetActive(false);
        tutorial.SetActive(true);
        GetScreenSize();
        GetSreenType();
        ScreenScale();
    }

    private void GetScreenSize()
    {
        screenHeight = Screen.height;
        screenWidth = Screen.width;
    }
    private void GetSreenType()
    {
        isVertical =(screenHeight > screenWidth);
        scaleHeightOnWidth = screenHeight / screenWidth;
    }


    private void ScreenScale()
    {
        if (!isVertical)
        {
            cam.orthographicSize = 1.1f;
            return;
        }
        cam.orthographicSize = 1.14f;

        if (scaleHeightOnWidth > 2)
        {

            cam.orthographicSize = 1.2f;
        }
        if (scaleHeightOnWidth > 2.1f)
        {

            cam.orthographicSize = 1.3f;
        }
        if (scaleHeightOnWidth>2.3)
        {

            cam.orthographicSize = 1.4f;
        }
        
        
    }

    public void ActiveGameWinUI(bool isActive)
    {
        winUI.SetActive(isActive);
    }
    public void ActiveGameLoseUI(bool isActive)
    {
        loseUI.SetActive(isActive);
    }
    public void ActiveTutorialUI(bool isActive)
    {
        tutorial.SetActive(isActive);
    }
}
