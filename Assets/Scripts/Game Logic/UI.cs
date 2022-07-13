using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UI : MonoBehaviour
{
    public enum ButtonFunction
    {
        MoveTool,
        RotateTool
    }

    #region Members
    public GameObject mainMenu;
    public GameObject cameraRig;
    public CameraController cameraController;
    public CustomButton selectedButton;
    public List<CustomButton> customButtons;
    #endregion

    #region Start
    public void Start()
    {
        cameraRig = GameObject.Find("Camera Rig");
        if (cameraRig != null)
        {
            cameraController = cameraRig.GetComponent<CameraController>();
        }
        if (Preserve.instance.isLocalOnline || RoomListing.IsOnline)
        {
            GameObject undoButton = GameObject.Find("Undo");
            if (undoButton != null)
            {
                undoButton.gameObject.SetActive(false);
            }
        }
    }
    #endregion

    #region Return
    public void Return()
    {
        StartCoroutine(WaitToReturn());
    }
    #endregion

    #region WaitToReturn
    private IEnumerator WaitToReturn()
    {
        if (Preserve.instance.isLocalOnline)
        {
            GameObject client = GameObject.Find("Client(Clone)");
            GameObject server = GameObject.Find("Server(Clone)");
            if (Client.isHost)
            {
                if (client != null && server != null)
                {
                    client.GetComponent<Client>().Send("HC Destroyed");
                    Destroy(client);
                    yield return new WaitUntil(() => !Server.isWaiting);
                    Destroy(server);
                }
            }
            else
            {
                if (client != null)
                {
                    client.GetComponent<Client>().Send("NHC Destroyed");
                    Destroy(client);
                }
            }
        }
        else if (RoomListing.IsOnline)
        {
            PhotonNetwork.LeaveRoom();
        }
        TabGroup.isOnFront = true;
        int[] indexesToUnload = new int[1] { (int)SceneIndex.GAME_SCREEN };
        SceneHandler.instance.LoadScene((int)SceneIndex.MAIN_MENU, indexesToUnload);
    }
    #endregion

    #region Undo
    public void Undo()
    {
        if (!RoomListing.IsOnline && !Preserve.instance.isLocalOnline)
        {
            StartCoroutine(Board.instance.WaitToUndo());
        }
    }
    #endregion

    #region Recenter
    public void Recenter()
    {
        CameraController controller = cameraRig.GetComponent<CameraController>();
        if (controller != null)
        {
            controller.ResetCameraTransform();
        }
    }
    #endregion

    #region ResetButtons
    public void ResetButtons()
    {
        foreach (CustomButton button in customButtons)
        {
            if (selectedButton != null && button == selectedButton)
            {
                continue;
            }
            if (button != null)
            {
                button.image.color = button.idleColor;
            }
        }
    }
    #endregion

    #region OnButtonEnter
    public void OnButtonEnter(CustomButton button)
    {
        ResetButtons();
        if (selectedButton == null || selectedButton != button)
        {
            button.image.color = button.hoverColor;
        }
    }
    #endregion

    #region OnButtonExit
    public void OnButtonExit()
    {
        ResetButtons();
    }
    #endregion

    #region OnButtonSelected
    public void OnButtonSelected(CustomButton button)
    {
        if (selectedButton == button)
        {
            button.image.color = button.idleColor;
            selectedButton = null;
            cameraController.translatingTouch = cameraController.rotatingTouch = false;
        }
        else
        {
            for (int i = 0; i < customButtons.Count; i++)
            {
                if (customButtons[i] == button)
                {
                    if (i == (int)ButtonFunction.MoveTool)
                    {
                        cameraController.translatingTouch = true;
                        cameraController.rotatingTouch = false;
                    }
                    else if (i == (int)ButtonFunction.RotateTool)
                    {
                        cameraController.rotatingTouch = true;
                        cameraController.translatingTouch = false;
                    }
                    break;
                }
            }
            selectedButton = button;
            button.image.color = button.selectedColor;
        }
        ResetButtons();
    }
    #endregion
}