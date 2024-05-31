using UnityEngine;

public class Archer : Unit
{
    public AudioClip bowLoadSound;
    public AudioClip bowShootSound;

    private void Start()
    {
        this.attackDistanceMin = 5.0f;
        this.attackDistanceMax = 20.0f;
        this.attackAnimationDuration = 1.2f;
        this.canAttack = true;

        Awake();
    }

    // Animation event for playing bow load sound
    public void PlayBowLoadSound()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound(bowLoadSound, transform);
        }
    }

    // Animation event for playing bow shoot sound
    public void PlayBowShootSound()
    {
        if (SoundManager.Instance != null)
        {
            SoundManager.Instance.PlaySound(bowShootSound, transform);
        }
    }
}
