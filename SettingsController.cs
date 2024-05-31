using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingsController : MonoBehaviour
{
    public GameObject mainMenuPanel;
    public GameObject settingsMenuPanel;
    public Image soundNumberImage;
    public Image musicNumberImage;
    public Sprite[] numberSprites; // Assign this in the inspector, sprites for 0, 1, 2, 3

    // For music control
    public AudioSource musicSource; // Assign your music source in the inspector
    private int musicVolume = 2; // Default music volume level
    private int soundLevel = 2; // Default sound level

    public Image fullscreenButtonImage;
    public Sprite fullscreenOnSprite; // Assign in inspector
    public Sprite fullscreenOffSprite; // Assign in inspector

    private bool isFullscreen = false;


    void Start()
    {
        UpdateFullscreenSprite();
    }

    // Called when the fullscreen button is clicked
    public void ToggleFullscreen()
    {
        isFullscreen = !isFullscreen; // Toggle the state
        Screen.fullScreen = isFullscreen; // Set fullscreen state
        UpdateFullscreenSprite();
    }

    // Update the sprite based on the current fullscreen state
    private void UpdateFullscreenSprite()
    {
        fullscreenButtonImage.sprite = isFullscreen ? fullscreenOnSprite : fullscreenOffSprite;
    }

    // Increase music volume
    public void IncreaseMusicVolume()
    {
        if (musicVolume < 3)
        {
            musicVolume++;
            UpdateMusicVolume();
        }
    }

    // Decrease music volume
    public void DecreaseMusicVolume()
    {
        if (musicVolume > 0)
        {
            musicVolume--;
            UpdateMusicVolume();
        }
    }
    // Update the music volume
    private void UpdateMusicVolume()
    {
        musicNumberImage.sprite = numberSprites[musicVolume];
        SoundManager.Instance.SetMusicVolume(musicVolume / 3f);
    }

    // Increase sound level
    public void IncreaseSound()
    {
        if (soundLevel < 3)
        {
            soundLevel++;
            UpdateSoundLevel();
        }
    }

    // Decrease sound level
    public void DecreaseSound()
    {
        if (soundLevel > 0)
        {
            soundLevel--;
            UpdateSoundLevel();
        }
    }

    // Update the sound level and change the sprite
    private void UpdateSoundLevel()
    {
        soundNumberImage.sprite = numberSprites[soundLevel];
        SoundManager.Instance.SetSoundVolume(soundLevel / 3f);
    }

    // Triggered when 'Back' is clicked in the settings menu
    public void BackToMainMenu()
    {
        settingsMenuPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }
}
