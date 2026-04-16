using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;
using System.Collections;

public class CharacterSelection : MonoBehaviourPunCallbacks
{
    [Header("UI")]
    public Text txtNama;
    public Text txtDeskripsi;
    public GameObject panelCharacterSelection;

    [Header("Loading UI")]
    public GameObject panelLoading;
    public Slider sliderLoading;
    public Text txtFunFact;
    public Text txtPersen;

    [Header("Data")]
    public List<GameObject> allCharacterPrefabs;
    public List<string> allNames;
    [TextArea] public List<string> allDescriptions;

    [Header("Spawn")]
    public Transform posKiri;
    public Transform posTengah;

    [Header("Settings")]
    public float kecepatanSwap = 10f;

    private GameObject modelKiri, modelTengah;
    private int idKiri, idTengah;
    private List<int> deckKocok = new List<int>();
    private bool sudahSetup = false;
    private int playerReadyCount = 0;

    private QuickOutline currentHoverOutline;

    // =============================
    // UPDATE
    // =============================
    void Update()
    {
        if (!sudahSetup) return;
        if (panelCharacterSelection == null) return;
        if (!panelCharacterSelection.activeSelf) return;

        HandleHoverAndClick();
        HandleAnimations();
    }

    // =============================
    // INIT
    // =============================
    public void InitCharacterSelection()
    {
        panelCharacterSelection.SetActive(true);

        if (PhotonNetwork.IsMasterClient)
            GenerateRandomDeck();
        else
            CheckExistingDeck();
    }

    // =============================
    // SHUFFLE
    // =============================
    void GenerateRandomDeck()
    {
        List<int> urutan = Enumerable.Range(0, allCharacterPrefabs.Count).ToList();

        for (int i = 0; i < urutan.Count; i++)
        {
            int rand = Random.Range(i, urutan.Count);
            int temp = urutan[i];
            urutan[i] = urutan[rand];
            urutan[rand] = temp;
        }

        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
        props["DeckData"] = urutan.ToArray();

        PhotonNetwork.CurrentRoom.SetCustomProperties(props);

        deckKocok = urutan;
        SetupKarakter();
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable changed)
    {
        if (changed.ContainsKey("DeckData") && !sudahSetup)
        {
            int[] data = (int[])changed["DeckData"];
            deckKocok = data.ToList();
            SetupKarakter();
        }
    }

    void CheckExistingDeck()
    {
        if (PhotonNetwork.CurrentRoom.CustomProperties.ContainsKey("DeckData"))
        {
            int[] data = (int[])PhotonNetwork.CurrentRoom.CustomProperties["DeckData"];
            deckKocok = data.ToList();
            SetupKarakter();
        }
    }

    // =============================
    // SETUP KARAKTER
    // =============================
    void SetupKarakter()
    {
        if (sudahSetup) return;

        var players = PhotonNetwork.PlayerList.OrderBy(p => p.ActorNumber).ToList();
        int myIndex = players.IndexOf(PhotonNetwork.LocalPlayer);

        int indexMulai = myIndex * 2;

        if (indexMulai + 1 >= deckKocok.Count)
        {
            Debug.LogError("Deck kurang!");
            return;
        }

        idTengah = deckKocok[indexMulai];
        idKiri = deckKocok[indexMulai + 1];

        SpawnModel();
        sudahSetup = true;
    }

    // =============================
    // SPAWN
    // =============================
    void SpawnModel()
    {
        if (modelTengah != null) Destroy(modelTengah);
        if (modelKiri != null) Destroy(modelKiri);

        modelTengah = Instantiate(allCharacterPrefabs[idTengah], posTengah.position, posTengah.rotation);
        modelKiri = Instantiate(allCharacterPrefabs[idKiri], posKiri.position, posKiri.rotation);

        UpdateUI();
    }

    // =============================
    // UI
    // =============================
    void UpdateUI()
    {
        txtNama.text = allNames[idTengah];
        txtDeskripsi.text = allDescriptions[idTengah];
    }

    // =============================
    // INPUT
    // =============================
    void HandleHoverAndClick()
    {
        if (Camera.main == null) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        if (currentHoverOutline != null)
        {
            currentHoverOutline.enabled = false;
            currentHoverOutline = null;
        }

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            QuickOutline outline = hit.transform.GetComponentInParent<QuickOutline>();

            if (outline != null)
            {
                outline.enabled = true;
                currentHoverOutline = outline;

                if (Input.GetMouseButtonDown(0))
                {
                    if (hit.transform.root.gameObject == modelKiri)
                        TukarPosisi();
                }
            }
        }
    }

    void TukarPosisi()
    {
        int tempID = idTengah;
        idTengah = idKiri;
        idKiri = tempID;

        GameObject tempObj = modelTengah;
        modelTengah = modelKiri;
        modelKiri = tempObj;

        UpdateUI();
    }

    // =============================
    // ANIMASI
    // =============================
    void HandleAnimations()
    {
        if (modelTengah == null || modelKiri == null) return;

        modelTengah.transform.position = Vector3.Lerp(
            modelTengah.transform.position,
            posTengah.position,
            Time.deltaTime * kecepatanSwap
        );

        modelKiri.transform.position = Vector3.Lerp(
            modelKiri.transform.position,
            posKiri.position,
            Time.deltaTime * kecepatanSwap
        );

        SetBrightness(modelTengah, 1f);
        SetBrightness(modelKiri, 0.4f);
    }

    void SetBrightness(GameObject obj, float factor)
    {
        foreach (Renderer r in obj.GetComponentsInChildren<Renderer>())
        {
            foreach (Material m in r.materials)
            {
                m.color = new Color(factor, factor, factor, 1f);
            }
        }
    }

    // =============================
    // PILIH KARAKTER
    // =============================
    public void PilihKarakter()
    {
        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
        props["KarakterPilihan"] = idTengah;

        PhotonNetwork.LocalPlayer.SetCustomProperties(props);

        panelCharacterSelection.SetActive(false);
        panelLoading.SetActive(true);

        txtFunFact.text = "Menunggu pemain lain...";
        txtPersen.text = "READY";
        sliderLoading.value = 0.5f;

        photonView.RPC("RPC_PlayerReady", RpcTarget.MasterClient);
    }

    // =============================
    // READY SYSTEM
    // =============================
    [PunRPC]
    void RPC_PlayerReady()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        playerReadyCount++;

        Debug.Log("Player ready: " + playerReadyCount);

        if (playerReadyCount >= PhotonNetwork.CurrentRoom.PlayerCount)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            StartCoroutine(LoadGameDelayed()); // ✅ panggil di sini
        }
    }

    // =============================
    // COROUTINE (HARUS DI LUAR)
    // =============================
    IEnumerator LoadGameDelayed()
    {
        yield return new WaitForSeconds(1f); // kasih waktu sync
        PhotonNetwork.LoadLevel("MainGame");
    }
}
