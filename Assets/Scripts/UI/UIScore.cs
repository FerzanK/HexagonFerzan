using UnityEngine;
using UnityEngine.UI;

public class UIScore : MonoBehaviour
{
    public IntVariable score;
    private Text textUI;

    // Start is called before the first frame update
    void Awake()
    {
        textUI = GetComponent<Text>();
    }

    public void ResetScore()
    {
        textUI.text = "0";
    }

    public void UpdateScore()
    {
        textUI.text = score.value.ToString();
    }
}
