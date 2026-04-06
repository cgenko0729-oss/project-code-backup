using TMPro;
using Unity.Services.Authentication.PlayerAccounts;
using UnityEngine;

public class MenuPlayerStateControl : MonoBehaviour
{
    public GameObject player;
    public PlayerData playerData;
    public TextMeshProUGUI charaName;
    public TextMeshProUGUI nowHp;
    public TextMeshProUGUI maxHp;
    public TextMeshProUGUI nowLv;
    public TextMeshProUGUI nowExp;
    public TextMeshProUGUI LvUpExp;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.Log("Not found 'Player'tag object");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (this.isActiveAndEnabled == false) { return; }

        if (playerData != null)
        {
            charaName.text = L.CharacterName(playerData.jobId);
        }

        if (player != null)
        {
            PlayerState state = player.GetComponent<PlayerState>();
            nowHp.text = ((int)state.NowHp).ToString();
            maxHp.text = state.MaxHp.ToString();
            nowLv.text = state.NowLv.ToString();
            int nowExpValue = (int)state.NowExp;
            nowExp.text = nowExpValue.ToString();
            int nextLvExpValue = (int)state.NextLvExp;
            LvUpExp.text = nextLvExpValue.ToString();
        }
    }
}
