using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance { get { return _instance; } }

    public static GameState gameState = GameState.Idle;



    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    void Start()
    {
        PlayerController.OnPlayerInitialMove += PlayerController_OnPlayerInitialMove;
    }

    // Update is called once per frame
    void Update()
    {

    }

    void PlayerController_OnPlayerInitialMove()
    {
        // Cuando el jugador cambia de dirección, verificamos si el juego
        // está en estado Idle, en cuyo caso lo ponemos en estado Playing.
        if (gameState == GameState.Idle)
            gameState = GameState.Playing;
    }
}

public enum GameState
{
    Idle,
    Playing,
    Paused,
    Win,
    GameOver
}
