using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIGameOver : MonoBehaviour
{
    public string noMoreValidMovesText;
    public string bombExplosionText;
    public GameEvent OnNewGame;
    public GameObject gameOverReasonText;

    public void TryAgainButton()
    {
        OnNewGame.Raise();
        gameObject.SetActive(false);
    }

    public void GameOverNoValidMoves()
    {
        gameOverReasonText.GetComponent<Text>().text = noMoreValidMovesText;
    }

    public void GameOverBombExplosion()
    {
        gameOverReasonText.GetComponent<Text>().text = bombExplosionText;
    }

}
