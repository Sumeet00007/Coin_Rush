using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainMenuUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_InputField roomCodeInput;
    [SerializeField] private Button hostButton;
    [SerializeField] private Button joinButton;
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private Button backButton;


    private async void Start()
    {
        //Set Orientation to Potrait
        ScreenOrientationManager.SetPortrait();
        hostButton.onClick.AddListener(OnHostClicked);
        joinButton.onClick.AddListener(OnJoinClicked);


        statusText.text = "";
    }

    private async void OnHostClicked()
    {
        SetButtonsInteractable(false);
        statusText.text = "Creating room...";

        bool success = await NetworkManager.Instance.CreateRoom();

        if (!success)
        {
            statusText.text = "Failed to create room.";
            SetButtonsInteractable(true);

            return;
        }

        statusText.text = "Room created!";
    }

    private async void OnJoinClicked()
    {
        string roomCode = roomCodeInput.text.Trim().ToUpper();

        if (string.IsNullOrEmpty(roomCode))
        {
            statusText.text = "Enter a room code.";

            return;
        }

        SetButtonsInteractable(false);

        statusText.text = "Joining room...";

        bool success = await NetworkManager.Instance.JoinRoom(roomCode);

        if (!success)
        {
            statusText.text =  "Failed to join room.";

            SetButtonsInteractable(true);

            return;
        }

        statusText.text =  "Joined room!";
    }

    private void SetButtonsInteractable(bool value)
    {
        hostButton.interactable = value;
        joinButton.interactable = value;
        backButton.interactable = value;
        roomCodeInput.interactable = value;
    }
}