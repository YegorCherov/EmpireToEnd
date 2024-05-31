using UnityEngine;

public class Catapult : MonoBehaviour
{
    public float minAttackRange = 10f;
    public float maxAttackRange = 30f;
    public int requiredBuilders = 2;
    public CatapultHand catapultHand;
    public Transform[] wheels;

    private Builder[] operatingBuilders;
    private int currentBuilders;
    private FindTarget findTargetScript;

    private void Start()
    {
        findTargetScript = GetComponent<FindTarget>();
        operatingBuilders = new Builder[requiredBuilders];
    }

    private void Update()
    {
        if (currentBuilders == 0)
        {
            Transform target = findTargetScript.target;
            if (target != null)
            {
                float distanceToTarget = Vector3.Distance(transform.position, target.position);
                if (distanceToTarget >= minAttackRange && distanceToTarget <= maxAttackRange)
                {
                    catapultHand.SetTarget(target);
                    catapultHand.Attack();
                }
                else
                {
                    catapultHand.ClearTarget();
                }
            }
            else
            {
                catapultHand.ClearTarget();
            }
        }
    }

    public void AssignBuilder(Builder builder)
    {
        if (currentBuilders < requiredBuilders)
        {
            operatingBuilders[currentBuilders] = builder;
            currentBuilders++;
        }
    }

    public void RemoveBuilder(Builder builder)
    {
        for (int i = 0; i < currentBuilders; i++)
        {
            if (operatingBuilders[i] == builder)
            {
                operatingBuilders[i] = null;
                currentBuilders--;
                break;
            }
        }
    }
    public void Move(Vector3 movement)
    {
        transform.Translate(movement);

        foreach (Transform wheel in wheels)
        {
            wheel.Rotate(Vector3.right * movement.magnitude * 100f);
        }
    }
}