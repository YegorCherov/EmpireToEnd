using UnityEngine;

public class Builder : Unit
{
    private Hwacha operatedHwacha;
    public float hwachaDetectionRange = 10f; // Range to detect Hwachas
    public LayerMask hwachaLayer; // Layer for Hwachas

    private void Start()
    {
        Awake();
        Update();
    }

#pragma warning disable CS0108 // Member hides inherited member; missing new keyword
    private void Update()
#pragma warning restore CS0108 // Member hides inherited member; missing new keyword
    {
        base.Update(); // Call the base class Update method

        if (operatedHwacha == null)
        {
            Hwacha nearbyHwacha = FindNearbyHwacha();
            if (nearbyHwacha != null)
            {
                // Optional: Automatically start operating the Hwacha
                OperateHwacha(nearbyHwacha);
            }
        }
    }


    private Hwacha FindNearbyHwacha()
    {
        RaycastHit2D hit = Physics2D.CircleCast(transform.position, hwachaDetectionRange, Vector2.zero, 0, hwachaLayer);
        if (hit.collider != null)
        {
            return hit.collider.GetComponent<Hwacha>();
        }
        return null;
    }

    // Call this method to make the builder operate the Hwacha
    public void OperateHwacha(Hwacha hwacha)
    {
        if (operatedHwacha == null)
        {
            operatedHwacha = hwacha;
            operatedHwacha.OperateHwacha(this);
            // Disable movement while operating the Hwacha
            GetComponent<Movement>().enabled = false;
        }
    }

    // Call this method to stop operating the Hwacha
    public void StopOperatingHwacha()
    {
        if (operatedHwacha != null)
        {
            operatedHwacha.StopOperation();
            operatedHwacha = null;
            // Enable movement again
            GetComponent<Movement>().enabled = true;
        }
    }
}
