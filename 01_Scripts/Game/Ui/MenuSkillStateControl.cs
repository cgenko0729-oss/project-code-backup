using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using UnityEngine.EventSystems;
using System.Linq;
using TigerForge;

public class MenuSkillStateControl : MonoBehaviour
{
    public GameObject skillGroupPrefab;
    public VerticalLayoutGroup skillList;
    public List<GameObject> uiGroup = new List<GameObject>();
    public GameObject firstListGroup;

    public List<GameObject> activeScrollViewSkillList = new List<GameObject>();

    public MenuPlayerEnchantController playerEnchantController;


    public bool _canNavigate = true;
   public  float controllerMoveCdCnt = 0.2f;
   public int currentSelectedIndex = 0;
    public bool isSelectingPlayerTrait = false;
    public int currentSelectedPlayerTraitIndex = 0;

    

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        int size = SkillManager.Instance.activeSkillCasterCollections.Count;
        for (int i = 0; i < size; i++) 
        {
            AddGroup(i);
        }

        foreach (var group in uiGroup)
        {
            SkillGroupControl temp = group.GetComponent<SkillGroupControl>();
            if (SkillManager.Instance.activeSkillCasterCollections[temp.casterId].isActivated == true)
            {
                group.SetActive(true);

                if (firstListGroup == null)
                {
                    firstListGroup = group;
                }
            }
        }

        activeScrollViewSkillList.Clear();
        activeScrollViewSkillList = uiGroup.FindAll(g => g.activeSelf);
        currentSelectedIndex = 0;
    }

    private void OnEnable()
    {

        EventManager.StartListening("OpenStatusMenu", () => {
            isSelectingPlayerTrait = false;
        });

        foreach (var group in uiGroup)
        {
            SkillGroupControl temp = group.GetComponent<SkillGroupControl>();
            if (SkillManager.Instance.activeSkillCasterCollections[temp.casterId].isActivated == true)
            {
                group.SetActive(true);

                if (firstListGroup == null)
                {
                    firstListGroup = group;
                }
            }
        }

        activeScrollViewSkillList.Clear();
        activeScrollViewSkillList = uiGroup.FindAll(g => g.activeSelf);
        if (activeScrollViewSkillList.Count > 0)
        {
            EventSystem.current.SetSelectedGameObject(activeScrollViewSkillList[0]);
            currentSelectedIndex = 0;

            //debug log with name :
            Debug.Log("MenuSkillStateControl OnEnable: " + activeScrollViewSkillList[0].name);
        }

    }

    // Update is called once per frame
    void Update()
    {
        //foreach (var group in uiGroup)
        //{
        //    SkillGroupControl temp = group.GetComponent<SkillGroupControl>();
        //    if (SkillManager.Instance.activeSkillCasterCollections[temp.casterId].isActivated == true)
        //    {
        //        group.SetActive(true);
        //
        //        if (firstListGroup == null)
        //        {
        //            firstListGroup = group;
        //        }
        //    }
        //}

        UpdateControllerInputInStatusMenu();

    }

    void AddGroup(int casterId)
    {
        GameObject newGroup = Instantiate(skillGroupPrefab, skillList.transform);
        newGroup.SetActive(false);
        SkillGroupControl group = newGroup.GetComponent<SkillGroupControl>();
        group.CasterId = casterId;

        uiGroup.Add(newGroup);
    }

  
    void UpdateControllerInputInStatusMenu()
    {

        float moveCdMax = 0.35f;

        controllerMoveCdCnt -= Time.unscaledDeltaTime;
        float verticalInput = Input.GetAxisRaw("Vertical");

        if (verticalInput > 0.5f && _canNavigate && controllerMoveCdCnt <= 0)
        {
            controllerMoveCdCnt = moveCdMax;
            currentSelectedIndex--;
            if (currentSelectedIndex < 0)
            {
                currentSelectedIndex = activeScrollViewSkillList.Count - 1;
            }
            EventSystem.current.SetSelectedGameObject(activeScrollViewSkillList[currentSelectedIndex]);

            //Debug log name ,time and index
            Debug.Log("Selected Skill: " + activeScrollViewSkillList[currentSelectedIndex].name + " at index " + currentSelectedIndex + " at time " + Time.time);

        }
        else if (verticalInput < -0.5f && _canNavigate && controllerMoveCdCnt <= 0)
        {
            controllerMoveCdCnt = moveCdMax;
            currentSelectedIndex++;
            if (currentSelectedIndex >= activeScrollViewSkillList.Count)
            {
                currentSelectedIndex = 0;
            }
            EventSystem.current.SetSelectedGameObject(activeScrollViewSkillList[currentSelectedIndex]);

            //Debug log name ,time and index
            Debug.Log("Selected Skill: " + activeScrollViewSkillList[currentSelectedIndex].name + " at index " + currentSelectedIndex + " at time " + Time.time);
        }
        else if (Mathf.Abs(verticalInput) < 0.5f)
        {
            _canNavigate = true;
        }

         float horizontalInput = Input.GetAxisRaw("Horizontal");

        if (horizontalInput < -0.5f && _canNavigate && controllerMoveCdCnt <= 0)
        {
            controllerMoveCdCnt = moveCdMax;
            Debug.Log("goLeft");
            if (!isSelectingPlayerTrait)
            {
                isSelectingPlayerTrait = true;
                currentSelectedPlayerTraitIndex = playerEnchantController.enchantsList.Count() - 1;
                EventSystem.current.SetSelectedGameObject(playerEnchantController.enchantsList[currentSelectedPlayerTraitIndex]);

                //Debug log name ,time and index
                Debug.Log("Selected Player Trait: " + playerEnchantController.enchantsList[currentSelectedPlayerTraitIndex].name + " at index " + currentSelectedPlayerTraitIndex + " at time " + Time.time);

            }
            else if (isSelectingPlayerTrait)
            {
                if (currentSelectedPlayerTraitIndex >= 1)
                {
                    currentSelectedPlayerTraitIndex--;                
                    EventSystem.current.SetSelectedGameObject(playerEnchantController.enchantsList[currentSelectedPlayerTraitIndex]);

                    //Debug log name ,time and index
                    Debug.Log("Selected Player Trait: " + playerEnchantController.enchantsList[currentSelectedPlayerTraitIndex].name + " at index " + currentSelectedPlayerTraitIndex + " at time " + Time.time);
                }


            }

        }
        else if (horizontalInput > 0.5f && _canNavigate && controllerMoveCdCnt <= 0)
        {
            controllerMoveCdCnt = moveCdMax;
            Debug.Log("goRight");
            if (isSelectingPlayerTrait)
            {
                if(currentSelectedPlayerTraitIndex >= playerEnchantController.enchantsList.Count()-1)
                {
                    isSelectingPlayerTrait = false;
                    currentSelectedIndex = 0;
                    EventSystem.current.SetSelectedGameObject(activeScrollViewSkillList[currentSelectedIndex]);

                    //Debug log name ,time and index
                    Debug.Log("Selected Skill: " + activeScrollViewSkillList[currentSelectedIndex].name + " at index " + currentSelectedIndex + " at time " + Time.time);
                }
                else
                {
                    currentSelectedPlayerTraitIndex++;
                    EventSystem.current.SetSelectedGameObject(playerEnchantController.enchantsList[currentSelectedPlayerTraitIndex]);
                    //Debug log name ,time and index
                    Debug.Log("Selected Player Trait: " + playerEnchantController.enchantsList[currentSelectedPlayerTraitIndex].name + " at index " + currentSelectedPlayerTraitIndex + " at time " + Time.time);
                }
            }
            
        }
        else if (Mathf.Abs(horizontalInput) < 0.5f)
        {
            _canNavigate = true;
        }


        //playerEnchantController.enchantsList[0].GetComponent<>

    }

}
