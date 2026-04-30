using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class Launcher : MonoBehaviourPunCallbacks
{
    [Header("Panel UI")]
    public GameObject panelHome;
    public GameObject panelRoom;
    public GameObject panelCharacterSelection;

    [Header("Character Selection Ref")]
    public CharacterSelection characterSelection;

    [Header("Input")]
    public InputField inputNama;
    public InputField inputRoomCode;

    [Header("Display")]
    public Text displayRoomCode;
    public Text displayPlayerList;

    [Header("Button")]
    public Button tombolStartGame;

    [Header("Loading")]
    public GameObject panelLoading;
    public Slider sliderLoading;
    public Text txtPersen;
    public Text txtFunFact;
    public RectTransform ujungSegitiga;

    private bool isLoading = false;
    private float targetProgress = 0f;
    private float velocity = 0f;

    private string[] funFacts = {
        "Tahu gak sih? Banyuwangi ternyata kabupaten terluas di Pulau Jawa, lho! Luasnya bahkan mengalahkan luas Pulau Bali",
        "Tahu gak sih? Banyuwangi ternyata dijuluki The Sunrise of Java. Ini karena Banyuwangi adalah wilayah pertama di Pulau Jawa yang melihat matahari terbit.",
        "Tahu gak? Alas Purwo itu termasuk hutan tertua!",
        "Banyuwangi ternyata punya fenomena api biru (blue fire) yang cuma ada dua di dunia, yaitu di kawah Ijen",
        "Nama \"Banyuwangi\" ternyata berasal dari legenda Sritanjung Sidopekso, di mana air sungai yang tercemar bau busuk berubah menjadi wangi setelah difitnah",
        "Ada suatu tempat di banyuwangi yang isinya 1000 patung gandrung loh, tepatnya di Taman gandrung terakota",
        "Banyuwangi ternyata pernah dikenal sebagai pusat klenik/santet, namun kini bertransformasi total menjadi salah satu kabupaten paling maju di Indonesia"
    };

    void Start()
    {
        //characterSelection = FindObjectOfType<CharacterSelection>();
        //PhotonNetwork.ConnectUsingSettings();


        PhotonNetwork.AutomaticallySyncScene = true;

        if (!PhotonNetwork.IsConnected)
            PhotonNetwork.ConnectUsingSettings();

        // 🔥 INI YANG PENTING
        characterSelection = FindObjectOfType<CharacterSelection>(true);

        if (characterSelection == null)
        {
            Debug.LogError("CharacterSelection TIDAK DITEMUKAN!");
        }
        else
        {
            Debug.Log("CharacterSelection KETEMU!");
        }




        panelHome.SetActive(true);
        panelRoom.SetActive(false);
        panelLoading.SetActive(false);
        panelCharacterSelection.SetActive(false);
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Terkoneksi ke Master Server!");
        PhotonNetwork.JoinLobby();
    }

    // =============================
    // ROOM SYSTEM
    // =============================

    public void CreateRoom()
    {
        if (string.IsNullOrEmpty(inputNama.text)) return;

        PhotonNetwork.NickName = inputNama.text;

        string kode = Random.Range(1000, 9999).ToString();
        PhotonNetwork.CreateRoom(kode, new RoomOptions { MaxPlayers = 4 });
    }

    public void JoinRoom()
    {
        if (string.IsNullOrEmpty(inputNama.text) || string.IsNullOrEmpty(inputRoomCode.text)) return;

        PhotonNetwork.NickName = inputNama.text;
        PhotonNetwork.JoinRoom(inputRoomCode.text);
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnJoinedRoom()
    {
        panelHome.SetActive(false);
        panelRoom.SetActive(true);

        displayRoomCode.text = "ROOM CODE: " + PhotonNetwork.CurrentRoom.Name;

        UpdateUI();
    }

    public override void OnLeftRoom()
    {
        panelRoom.SetActive(false);
        panelHome.SetActive(true);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer) => UpdateUI();
    public override void OnPlayerLeftRoom(Player otherPlayer) => UpdateUI();
    public override void OnMasterClientSwitched(Player newMasterClient) => UpdateUI();

    void UpdateUI()
    {
        displayPlayerList.text = "PLAYERS:\n";

        foreach (Player p in PhotonNetwork.PlayerList)
        {
            string tag = p.IsMasterClient ? " (Host)" : "";
            displayPlayerList.text += "- " + p.NickName + tag + "\n";
        }

        if (tombolStartGame != null)
        {
            tombolStartGame.gameObject.SetActive(PhotonNetwork.IsMasterClient);
        }
    }

    // =============================
    // START GAME FLOW
    // =============================

    public void StartGame()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        photonView.RPC("RPC_StartLoading", RpcTarget.All);
    }

    [PunRPC]
    void RPC_StartLoading()
    {
        panelRoom.SetActive(false);
        panelLoading.SetActive(true);

        sliderLoading.value = 0f;
        targetProgress = 0f;
        isLoading = true;

        txtFunFact.text = funFacts[Random.Range(0, funFacts.Length)];
    }

    void Update()
    {
        if (!isLoading) return;

        // efek loading loncat-loncat
        if (sliderLoading.value >= targetProgress - 0.01f)
        {
            float loncat = Random.Range(0.1f, 0.3f);
            targetProgress = Mathf.Clamp(targetProgress + loncat, 0, 1f);
        }

        sliderLoading.value = Mathf.SmoothDamp(
            sliderLoading.value,
            targetProgress,
            ref velocity,
            0.4f
        );

        txtPersen.text = (sliderLoading.value * 100).ToString("F0") + "%";

        if (ujungSegitiga != null)
            ujungSegitiga.Rotate(0, 0, -250 * Time.deltaTime);

        // 🔥 SAAT LOADING SELESAI
        if (sliderLoading.value >= 0.99f)
        {
            isLoading = false;

            panelLoading.SetActive(false);

            // 🔥 INIT CHARACTER SELECTION
            if (characterSelection != null)
            {
                characterSelection.InitCharacterSelection();
            }
            else
            {
                Debug.LogError("CharacterSelection belum di assign di Inspector!");
            }
        }
    }
}
