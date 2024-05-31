using Unity.VisualScripting;
using UnityEngine;

public class Knight : Unit
{

    private void Start()
    {
        this.attackDistanceMin = 0.0f;
        this.attackDistanceMax = 1.5f;
        this.attackAnimationDuration = 1.2f;
        this.canAttack = true;

        Awake();
        Update();
    }
}
