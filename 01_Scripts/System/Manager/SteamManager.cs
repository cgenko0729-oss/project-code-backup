using UnityEngine;
using Steamworks;

// The SteamManager provides a base implementation of Steamworks.NET on Unity.
// It tracks the state of the client and handles the initialization and updates.
class SteamManager : MonoBehaviour {
    private static SteamManager s_instance;
    private static bool s_EverInitialized;

    private static SteamManager Instance {
        get {
            if (s_instance == null) {
                return new GameObject("SteamManager").AddComponent<SteamManager>();
            }
            else {
                return s_instance;
            }
        }
    }

    private bool m_bInitialized;
    // FIXED: Defined this variable locally instead of using "SteamCallbacks"
    private SteamAPIWarningMessageHook_t m_SteamAPIWarningMessageHook; 

    public static bool Initialized {
        get {
            return Instance.m_bInitialized;
        }
    }

    private void Awake() {
        if (s_instance != null) {
            Destroy(gameObject);
            return;
        }
        s_instance = this;

        if(s_EverInitialized) {
            throw new System.Exception("Tried to Initialize the SteamAPI twice in one session!");
        }

        DontDestroyOnLoad(gameObject);

        if (!Packsize.Test()) {
            Debug.Log("[Steamworks.NET] Packsize Test returned false, the wrong version of Steamworks.NET is being run in this platform.", this);
        }

        if (!DllCheck.Test()) {
            Debug.Log("[Steamworks.NET] DllCheck Test returned false, One or more of the Steamworks binaries seems to be the wrong version.", this);
        }

        try {
            //if (SteamAPI.RestartAppIfNecessary(AppId_t.Invalid)) { //txt fiie appid
            if (SteamAPI.RestartAppIfNecessary(new AppId_t(4043830))) { //type real appiD : demo = 4052210 , full = 4043830, dlc = 4390330
                Application.Quit();
                return;
            }
        }
        catch (System.DllNotFoundException e) {
            Debug.Log("[Steamworks.NET] Could not load [lib]steam_api.dll/so/dylib. It's likely not in the correct location.\n" + e, this);
            Application.Quit();
            return;
        }

        m_bInitialized = SteamAPI.Init();
        if (!m_bInitialized) {
            Debug.Log("[Steamworks.NET] SteamAPI_Init() failed. Refer to Valve's documentation.", this);
            return;
        }

        s_EverInitialized = true;
    }

    private void OnEnable() {
        if (s_instance == null) {
            s_instance = this;
        }

        if (!m_bInitialized) {
            return;
        }

        // FIXED: Using the local variable m_SteamAPIWarningMessageHook
        if (m_SteamAPIWarningMessageHook == null) {
            m_SteamAPIWarningMessageHook = new SteamAPIWarningMessageHook_t(OnSteamAPIDebugTextHook);
            SteamClient.SetWarningMessageHook(m_SteamAPIWarningMessageHook);
        }
    }

    private void OnDestroy() {
        if (s_instance != this) {
            return;
        }

        s_instance = null;

        if (!m_bInitialized) {
            return;
        }

        SteamAPI.Shutdown();
    }

    private void Update() {
        if (!m_bInitialized) {
            return;
        }

        // Run Steam client callbacks
        SteamAPI.RunCallbacks();
    }

    private static void OnSteamAPIDebugTextHook(int nSeverity, System.Text.StringBuilder pchDebugText) {
        Debug.LogWarning(pchDebugText);
    }
}