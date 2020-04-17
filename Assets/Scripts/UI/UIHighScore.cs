using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHighScore : MonoBehaviour
{
    public IntVariable highScore;
    private Text textUI;
   
    // Start is called before the first frame update
    void Awake()
    {
        textUI = GetComponent<Text>();
    }
    public void UpdateHighScore()
    {
        textUI.text = "Highscore: " + highScore.value.ToString();
    }
}
