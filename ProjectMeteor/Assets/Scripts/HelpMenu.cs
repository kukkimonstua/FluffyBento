using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HelpMenu : MonoBehaviour
{
    
    private bool axisInUse = false;
    public RawImage[] helpPages;
    private int helpPageIndex;

    public Image rightDPad;
    public Image leftDPad;

    void Update()
    {
        if (gameObject.activeSelf)
        {
            if (Input.GetAxisRaw("Horizontal") == 1)
            {
                if (!axisInUse)
                {
                    axisInUse = true;
                    ChangeHelpPage(1);
                }
            }
            else if (Input.GetAxisRaw("Horizontal") == -1)
            {
                if (!axisInUse)
                {
                    axisInUse = true;
                    ChangeHelpPage(-1);
                }
            }
            else
            {
                axisInUse = false;
            }
        }
    }

    public void GoToFirstHelpPage()
    {
        rightDPad.gameObject.SetActive(true);
        leftDPad.gameObject.SetActive(false);
        helpPageIndex = 0;
        foreach (RawImage page in helpPages)
        {
            page.gameObject.SetActive(false);
        }
        helpPages[helpPageIndex].gameObject.SetActive(true);
    }

    private void ChangeHelpPage(int direction)
    {
        rightDPad.gameObject.SetActive(true);
        leftDPad.gameObject.SetActive(true);
        helpPageIndex += direction;
        if (helpPageIndex >= helpPages.Length - 1)
        {
            rightDPad.gameObject.SetActive(false);
            helpPageIndex = helpPages.Length - 1;
        }
        if (helpPageIndex <= 0)
        {
            leftDPad.gameObject.SetActive(false);
            helpPageIndex = 0;
        }

        foreach (RawImage page in helpPages)
        {
            page.gameObject.SetActive(false);
        }
        helpPages[helpPageIndex].gameObject.SetActive(true);
    }

    public void CloseHelp()
    {
        foreach (RawImage page in helpPages)
        {
            page.gameObject.SetActive(true);
        }
        gameObject.SetActive(false);
    }
}
