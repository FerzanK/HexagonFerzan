using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class TilePoints : MonoBehaviour
{
    private TextMeshPro textMesh;

    private float textMoveDuration = 3.0f;
    // Start is called before the first frame update
    void Awake()
    {
        textMesh = gameObject.GetComponentInChildren<TextMeshPro>(true);
    }


    public void ShowPoints(int points)
    {
        textMesh.text = points.ToString();
        var newPosition = new Vector2(transform.position.x, transform.position.y + 1.2f);
        textMesh.color = Color.white;
        transform.DOMove(newPosition, textMoveDuration);
        StartCoroutine(FadeTextOut());
    }

    IEnumerator FadeTextOut()
    {
        float waitSeconds = textMoveDuration / 100.0f;
        for (int i = 1; i <= 100; i++)
        {
            var newAlphaValue = textMesh.color.a - 1.0f * i / 100.0f;
            textMesh.color = new Color(textMesh.color.r, textMesh.color.g, textMesh.color.b, newAlphaValue);
            yield return new WaitForSeconds(waitSeconds);
        }
        gameObject.SetActive(false);
    }
}
