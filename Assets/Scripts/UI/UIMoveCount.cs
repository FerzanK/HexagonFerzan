using UnityEngine;
using UnityEngine.UI;

public class UIMoveCount : MonoBehaviour
{

    public IntVariable moveCount;

    private Text textUI;

    void Awake()
    {
        textUI = GetComponent<Text>();
    }
    
    public void UpdateMoveCount()
    {
        textUI.text = moveCount.value.ToString();
    }

    public void ResetMoveCount()
    {
        textUI.text = "0";
    }

}
