using Fusion;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance { get; private set; }

    private NetworkRunner runner;
    private NetworkSceneManagerDefault sceneManager;

    public string RoomCode { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        runner =GetComponent<NetworkRunner>();
        Instance = this;

        DontDestroyOnLoad(gameObject);
    }

    public async Task<bool> CreateRoom()
    {
        RoomCode = GenerateRoomCode();

        return await StartFusion(GameMode.Host,RoomCode);
    }

    public async Task<bool> JoinRoom(string roomCode)
    {
        RoomCode = roomCode.Trim().ToUpper();

        if (string.IsNullOrEmpty(RoomCode))
        {
            Debug.LogError("Room code is empty.");
            return false;
        }

        return await StartFusion(GameMode.Client,RoomCode);
    }

    private async Task<bool> StartFusion(GameMode gameMode,string sessionName)
    {

        if (runner == null)
        {
            runner = gameObject.AddComponent<NetworkRunner>();
        }

        runner.ProvideInput = true;

        if (sceneManager == null)
        {
            sceneManager =
                gameObject.AddComponent<NetworkSceneManagerDefault>();
        }

        // Start Fusion
        StartGameResult result = await runner.StartGame(
            new StartGameArgs
            {
                GameMode = gameMode,

                SessionName = sessionName,

                PlayerCount = 2,

                SceneManager = sceneManager
            }
        );

        if (!result.Ok)
        {
            Debug.LogError(
                $"Failed to start Fusion: {result.ShutdownReason}"
            );

            Destroy(runner);
            runner = null;

            return false;
        }

        Debug.Log(
            $"Fusion started successfully. Room: {sessionName}"
        );

        // Host/Client has connected.
        // Now load Lobby scene.
        LoadLobby();

        return true;
    }

    private void LoadLobby()
    {
        if (runner == null)
            return;

        // Only Host / Scene Authority can load a networked scene.
        if (!runner.IsSceneAuthority)
        {
            Debug.Log("Waiting for Scene Authority to load the Lobby scene.");
            return;
        }

        SceneRef lobbyScene = SceneRef.FromIndex(1);

        runner.LoadScene(lobbyScene,LoadSceneMode.Single);

        Debug.Log("Loading Lobby scene...");
    }

    private string GenerateRoomCode()
    {
        const string characters =
            "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        string code = "";

        for (int i = 0; i < 6; i++)
        {
            code += characters[
                Random.Range(0, characters.Length)
            ];
        }

        return code;
    }

    public NetworkRunner GetRunner()
    {
        return runner;
    }
}
