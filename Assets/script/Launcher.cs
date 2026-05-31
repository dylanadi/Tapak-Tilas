using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;

public class Launcher : MonoBehaviourPunCallbacks
{
    [Header("Audio (BGM & SFX)")]
    public AudioSource audioSourceBGM;
    public AudioSource audioSourceSFX;
    public AudioClip klipBGM;
    public AudioClip klipKlikTombol;

    [Header("Intro Transisi")]
    public CanvasGroup panelHitamTransisi;
    public GameObject panelStart;
    public GameObject teksKetukMulai;
    public float durasiFadeHitam = 1.5f;

    [Header("Kamera Sinematik")]
    public Camera kameraUtama;
    public Transform titikKamera1;
    public Transform titikKamera2;
    public float durasiJalanKamera = 3.5f;

    [Header("Panel UI")]
    public GameObject panelHome;
    public GameObject panelRoom;
    public GameObject panelCharacterSelection;

    [Header("Character Selection Ref")]
    public CharacterSelection characterSelection;

    [Header("Input")]
    public InputField inputNama;
    public InputField inputRoomCode;

    [Header("Display Home")]
    public Text displayRoomCode;

    [Header("Display Room (Sistem 4 Slot)")]
    public Text[] teksNamaPemain = new Text[4]; // 4 Teks untuk nama
    public Image[] ikonPemain = new Image[4]; // 4 Gambar avatar orang
    public Color warnaAktif = new Color32(253, 175, 23, 255); // Kuning Oren
    public Color warnaKosong = new Color32(217, 217, 217, 255); // Abu-abu

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
    private bool tungguKlikLayar = false;

    // 🔥 FUN FACTS BANYUWANGI
    private string[] funFacts = {
        "Banyuwangi adalah kabupaten terluas di Pulau Jawa, ngalahin luas Bali!",
        "Dijuluki Sunrise of Java, tempat pertama lihat matahari terbit di Jawa.",
        "Alas Purwo sering disebut hutan tertua di Pulau Jawa yang penuh misteri.",
        "Kawah Ijen punya pesona Blue Fire yang cuma ada dua di seluruh dunia!",
        "Nama Banyuwangi dari legenda Sritanjung, air busuk berubah jadi wangi.",
        "Taman Gandrung Terakota punya 1000 patung penari di tengah persawahan.",
        "Dulu dikenal kota santet, kini Banyuwangi jadi tujuan wisata kelas dunia.",
        "Pantai Plengkung (G-Land) punya ombak selancar terbaik kedua di dunia!",
        "Desa Kemiren adalah pusat budaya Suku Osing, penduduk asli Banyuwangi."
    };

    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        // Anti-Putus: Kasih waktu 60 detik sebelum Photon nendang player kalau ngelag loading
        PhotonNetwork.NetworkingClient.LoadBalancingPeer.DisconnectTimeout = 60000;

        if (!PhotonNetwork.IsConnected)
            PhotonNetwork.ConnectUsingSettings();

        characterSelection = FindObjectOfType<CharacterSelection>(true);
        if (characterSelection == null) Debug.LogError("CharacterSelection TIDAK DITEMUKAN!");

        if (KameraFollow.Instance != null) KameraFollow.Instance.kameraAktif = false;

        if (audioSourceBGM != null && klipBGM != null)
        {
            audioSourceBGM.clip = klipBGM;
            audioSourceBGM.loop = true;
            audioSourceBGM.Play();
        }

        Button[] semuaTombol = FindObjectsOfType<Button>(true);
        foreach (Button btn in semuaTombol)
        {
            btn.onClick.AddListener(MainkanSFXKlik);
        }

        panelStart.SetActive(true);
        panelHome.SetActive(false);
        panelRoom.SetActive(false);
        panelLoading.SetActive(false);
        panelCharacterSelection.SetActive(false);

        StartCoroutine(FadeDariHitam());
    }

    public void MainkanSFXKlik()
    {
        if (audioSourceSFX != null && klipKlikTombol != null)
            audioSourceSFX.PlayOneShot(klipKlikTombol);
    }

    IEnumerator FadeDariHitam()
    {
        if (panelHitamTransisi != null)
        {
            panelHitamTransisi.gameObject.SetActive(true);
            panelHitamTransisi.alpha = 1f;

            yield return new WaitForSeconds(0.5f);

            float waktu = 0;
            while (waktu < durasiFadeHitam)
            {
                waktu += Time.deltaTime;
                panelHitamTransisi.alpha = Mathf.Lerp(1f, 0f, waktu / durasiFadeHitam);
                yield return null;
            }
            panelHitamTransisi.gameObject.SetActive(false);
        }
        tungguKlikLayar = true;
    }

    IEnumerator IntroFakeLoading()
    {
        MainkanSFXKlik();
        panelStart.SetActive(false);
        panelLoading.SetActive(true);

        sliderLoading.value = 0f;
        txtFunFact.text = funFacts[Random.Range(0, funFacts.Length)];

        float waktu = 0;
        float durasi = 5f;

        while (waktu < durasi)
        {
            waktu += Time.deltaTime;
            float persen = waktu / durasi;
            sliderLoading.value = persen;
            txtPersen.text = (persen * 100).ToString("F0") + "%";

            if (ujungSegitiga != null)
                ujungSegitiga.Rotate(0, 0, -250 * Time.deltaTime);

            yield return null;
        }

        panelLoading.SetActive(false);
        panelHome.SetActive(true);
    }

    // =============================
    // SERVER & ROOM SYSTEM
    // =============================
    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

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

    // 🔥 UPDATE UI ROOM TERBARU (SISTEM SLOT)
    void UpdateUI()
    {
        // Ambil daftar pemain saat ini
        Player[] players = PhotonNetwork.PlayerList;

        for (int i = 0; i < 4; i++)
        {
            // Jika index masih dalam batas jumlah player yang ada di room (Slot Terisi)
            if (i < players.Length)
            {
                Player p = players[i];
                string namaPlayer = p.NickName;

                // 🔥 Logika Pemotongan Nama Maksimal 13 Karakter
                if (namaPlayer.Length > 13)
                {
                    // Ambil 10 karakter pertama, sisanya diganti spasi dan dua titik
                    namaPlayer = namaPlayer.Substring(0, 10) + " ..";
                }

                if (teksNamaPemain[i] != null) teksNamaPemain[i].text = namaPlayer;

                // Warnai gambar jadi Kuning Oren
                if (ikonPemain[i] != null) ikonPemain[i].color = warnaAktif;
            }
            // Jika tidak ada playernya (Slot Kosong)
            else
            {
                if (teksNamaPemain[i] != null) teksNamaPemain[i].text = "-";

                // Warnai gambar jadi Abu-abu
                if (ikonPemain[i] != null) ikonPemain[i].color = warnaKosong;
            }
        }

        // Tampilkan tombol Mulai cuma buat Host
        if (tombolStartGame != null)
            tombolStartGame.gameObject.SetActive(PhotonNetwork.IsMasterClient);
    }

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
        if (tungguKlikLayar && teksKetukMulai != null)
        {
            CanvasGroup cgTeks = teksKetukMulai.GetComponent<CanvasGroup>();
            if (cgTeks == null) cgTeks = teksKetukMulai.AddComponent<CanvasGroup>();
            cgTeks.alpha = Mathf.PingPong(Time.time * 1f, 1f);
        }

        if (tungguKlikLayar && Input.GetMouseButtonDown(0))
        {
            tungguKlikLayar = false;
            StartCoroutine(IntroFakeLoading());
        }

        if (!isLoading) return;

        if (sliderLoading.value >= targetProgress - 0.01f)
        {
            float loncat = Random.Range(0.1f, 0.3f);
            targetProgress = Mathf.Clamp(targetProgress + loncat, 0, 1f);
        }

        sliderLoading.value = Mathf.SmoothDamp(sliderLoading.value, targetProgress, ref velocity, 0.4f);
        txtPersen.text = (sliderLoading.value * 100).ToString("F0") + "%";

        if (ujungSegitiga != null)
            ujungSegitiga.Rotate(0, 0, -250 * Time.deltaTime);

        if (sliderLoading.value >= 0.99f)
        {
            isLoading = false;
            panelLoading.SetActive(false);
            StartCoroutine(SinematikKameraKeKarakter());
        }
    }

    IEnumerator SinematikKameraKeKarakter()
    {
        if (kameraUtama != null && titikKamera1 != null && titikKamera2 != null)
        {
            kameraUtama.transform.position = titikKamera1.position;
            kameraUtama.transform.rotation = Quaternion.Euler(0, 180f, 0);

            if (characterSelection != null)
            {
                characterSelection.InitCharacterSelection();
                characterSelection.panelCharacterSelection.SetActive(false);
            }

            float t = 0;
            Vector3 posisiAwal = kameraUtama.transform.position;

            while (t < 1f)
            {
                t += Time.deltaTime / durasiJalanKamera;
                float easeOut = 1f - Mathf.Pow(1f - t, 3f);

                kameraUtama.transform.position = Vector3.Lerp(posisiAwal, titikKamera2.position, easeOut);
                kameraUtama.transform.rotation = Quaternion.Euler(0, 180f, 0);

                yield return null;
            }

            kameraUtama.transform.position = titikKamera2.position;
            kameraUtama.transform.rotation = Quaternion.Euler(0, 180f, 0);
        }

        if (characterSelection != null && characterSelection.panelCharacterSelection != null)
        {
            StartCoroutine(FadeInUIKarakter());
        }
    }

    IEnumerator FadeInUIKarakter()
    {
        GameObject panelUI = characterSelection.panelCharacterSelection;
        panelUI.SetActive(true);

        CanvasGroup cg = panelUI.GetComponent<CanvasGroup>();
        if (cg == null) cg = panelUI.AddComponent<CanvasGroup>();

        cg.alpha = 0f;
        float waktu = 0;
        float durasiFadeUI = 1.0f;

        while (waktu < durasiFadeUI)
        {
            waktu += Time.deltaTime;
            cg.alpha = Mathf.Lerp(0f, 1f, waktu / durasiFadeUI);
            yield return null;
        }

        cg.alpha = 1f;
    }
}