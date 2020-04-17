using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public IntVariable score;
    public IntVariable moveCount;
    public IntVariable highestScore;
    public GameObject gameOverUI;
    public GameObject gameOverText;
    public GameEvent OnScoreUpdate;
    public GameEvent OnHighScoreUpdate;
    private string saveFile = "/gamedata.dat";
    private string previousGameManagerState;
    private HexGrid grid;

    // Start is called before the first frame update
    void Start()
    {
        grid = GameObject.Find("Grid").GetComponent<HexGrid>();
        LoadData();
    }

    public void NewGame()
    {
        score.value = 0;
        moveCount.value = 0;
    }

    public void UpdateHighScore()
    {
        if (highestScore.value < score.value)
        {
            highestScore.value = score.value;
            OnHighScoreUpdate.Raise();
        }
    }

    public void GameOverBombExplosion()
    {
        gameOverUI.SetActive(true);
        gameOverUI.GetComponentInChildren<UIGameOver>().GameOverBombExplosion();
    }

    public void GameOverNoMoreMovesLeft()
    {
        gameOverUI.SetActive(true);
        gameOverUI.GetComponentInChildren<UIGameOver>().GameOverNoValidMoves();
    }

    public void Serialize()
    {
        var gameDataJSONSerialized  =  JsonUtility.ToJson(this);
        Debug.Log(gameDataJSONSerialized);
        File.WriteAllText(Application.persistentDataPath + saveFile, gameDataJSONSerialized);
    }

    public void SerializeNew()
    {
        GameState newGameState = new GameState();
        newGameState.score = score.value;
        newGameState.highScore = highestScore.value;
        newGameState.moveCount = moveCount.value;
        newGameState.Add(grid);
        newGameState.Serialize();
    }

    public void SerializeCurrentState()
    {
        previousGameManagerState = string.Empty;
        previousGameManagerState = JsonUtility.ToJson(this);
    }
    public void LoadData()
    {
        if (File.Exists(Application.persistentDataPath + saveFile))
        {
            var gameManagerData = File.ReadAllText(Application.persistentDataPath + saveFile);
            JsonUtility.FromJsonOverwrite(gameManagerData, this);
        }
    }

    public void Undo()
    {
        JsonUtility.FromJsonOverwrite(previousGameManagerState, this);
    }

    void OnApplicationQuit()
    {
        Debug.Log("OnApplicationQuit called");
    }

}
