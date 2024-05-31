using UnityEngine;
using System.Collections;

public class FlagAnimation : MonoBehaviour
{
    public float fadeDuration = 1f;

    private SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void FadeAndDestroy()
    {
        StartCoroutine(FadeAndDestroyCoroutine());
    }

    private IEnumerator FadeAndDestroyCoroutine()
    {
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            float alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            spriteRenderer.color = new Color(spriteRenderer.color.r, spriteRenderer.color.g, spriteRenderer.color.b, alpha);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}