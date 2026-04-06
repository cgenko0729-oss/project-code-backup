using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;     
using TMPro;    
using DG.Tweening;
using TigerForge;               //EventManager
using QFSW.MOP2;                //Object Pool
using MonsterLove.StateMachine; //StateMachine
using Hellmade.Sound; //SoundManager
using Cysharp.Threading.Tasks;
using System.Linq;

public class AchievementUiItem : MonoBehaviour
{

    public TextMeshProUGUI titleText;       // Reference to the title text component
    public TextMeshProUGUI descriptionText; // Reference to the description text component
    public TextMeshProUGUI rewardText;
    public Image iconImage;                 // Reference to the icon image
    public GameObject completedIndicator;   // A checkmark or glow to show completion
    public Image backgroundFrame;

    public Color lockedColor = new Color(0.5f, 0.5f, 0.5f, 0.8f);

    /// <summary>
    /// Configures the UI element with data from an Achievement scriptable object
    /// and the player's current progress.
    /// </summary>
    public void Setup(Achievement achievementData, AchievementSaveData progressData)
    {
        //titleText.text = achievementData.title;
        //descriptionText.text = achievementData.description;

        titleText.text = L.AchName(achievementData.id);
        descriptionText.text = L.AchDesc(achievementData.id);
        rewardText.text = L.AchReward(achievementData.id);

        if (achievementData.isCoinReward)
        {
            rewardText.text = L.AchCoin() + " " + achievementData.rewardCoin.ToString();
        }
        
        if (achievementData.icon != null)
        {
            iconImage.sprite = achievementData.icon;
        }

        // 2. Check if the achievement is unlocked
        bool isUnlocked = progressData.unlockedAchievementIds.Contains(achievementData.id);

        // 3. Update UI based on unlocked status
        if (isUnlocked)
        {
            // Set to "unlocked" appearance
            completedIndicator.SetActive(true);
            iconImage.color = Color.white; // Full color
            if (backgroundFrame != null) backgroundFrame.color = Color.white;
        }
        else
        {
            // Set to "locked" appearance
            completedIndicator.SetActive(false);
            iconImage.color = lockedColor; // Greyed out
            if (backgroundFrame != null) backgroundFrame.color = lockedColor;


            // 4. If it's an incremental achievement that's not yet unlocked, show progress
            if (achievementData.isIncremental)
            {
                // Try to get the current progress value from the save data
                //progressData.incrementalAchievementProgress.TryGetValue(achievementData.id, out int currentProgress);

                int currentProgress = 0; 

                // Find the specific progress entry in the list that matches the achievement's ID.
                var progressEntry = progressData.incrementalAchievementProgress.FirstOrDefault(p => p.id == achievementData.id);

                // If an entry was found, use its value.
                if (progressEntry != null)
                {
                    currentProgress = progressEntry.value;
                }

                // Append the progress to the description
                descriptionText.text += $" ({currentProgress} / {achievementData.valueToUnlock})";
            }
        }
    }

}

