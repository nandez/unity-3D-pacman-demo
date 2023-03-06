using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    public static GameManager Instance { get { return _instance; } }

    [Header("UI References")]
    [SerializeField] protected TextMeshProUGUI scoreText;
    [SerializeField] protected List<GameObject> livesImages;

    [Header("Game Settings")]
    [SerializeField] protected float powerPelletEffectDuration = 19f;
    [SerializeField] protected float powerPelletEffectWarning = 12f;


    public GameState GameState { get; private set; } = GameState.Idle;
    public int PlayerLives { get; private set; } = 3;
    public int Score { get; private set; } = 0;


    // Power Pellet
    public delegate void PowerPelletStatusChangeEvent(bool active);
    public delegate void PowerPelletFadeWarningEvent();
    public event PowerPelletStatusChangeEvent OnPowerPelletStatusChange;
    public event PowerPelletFadeWarningEvent OnPowerPelletFadeWarning;
    private Coroutine powerPelletEffectDeactivateCoroutine;
    public bool IsPowerPelletActive { get; private set; } = false;
    private int enemyEatenCounter = 0;

    public delegate void OnLevelResetEvent();
    public event OnLevelResetEvent OnLevelReset;

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
        // Seteamos los handlers para los eventos
        PlayerController.OnPlayerInitialMove += PlayerController_OnPlayerInitialMove;
        PlayerController.OnPlayerDeath += PlayerController_OnPlayerDeath;
        EnemyController.OnEnemyEaten += EnemyController_OnEnemyEaten;
        Pellet.OnPelletCollected += Pellet_OnPelletCollected;


        scoreText.SetText(Score.ToString());
    }



    protected void ChangeState(GameState newState)
    {
        Debug.Log($"Game State Changed: {GameState} -> {newState}");
        GameState = newState;
    }

    protected void PlayerController_OnPlayerInitialMove()
    {
        // Cuando el jugador cambia de dirección, verificamos si el juego
        // está en estado Idle, en cuyo caso lo ponemos en estado Playing.
        if (GameState == GameState.Idle)
            GameState = GameState.Playing;
    }

    protected void PlayerController_OnPlayerDeath()
    {
        if (PlayerLives - 1 <= 0)
        {
            // Desactivamos todas las vidas y cambiamos el estado del juego a GameOver.
            foreach (var lifeImage in livesImages)
                lifeImage.SetActive(false);

            PlayerLives = 0;
            ChangeState(GameState.GameOver);
        }
        else
        {
            // Restamos una vida y desactivamos la imagen de la UI.
            PlayerLives--;
            livesImages[PlayerLives - 1].SetActive(false);

            // Cambiamos el estado del juego a Idle y reiniciamos el nivel.
            ChangeState(GameState.Idle);
            OnLevelReset?.Invoke();
        }
    }

    protected void Pellet_OnPelletCollected(Pellet pellet)
    {
        Score += pellet.Points;
        scoreText.SetText(Score.ToString());

        // Eliminamos el pellet de la lista de pellets y lo destruimos.
        MapManager.Instance.pellets.Remove(pellet);
        Destroy(pellet.gameObject);

        // Si no quedan mas pellets, el nivel está completo.
        if (MapManager.Instance.pellets.Count == 0)
            ChangeState(GameState.LevelCompleted);

        if (pellet.IsPowerPellet)
        {
            // Si el pellet es un power pellet, activamos el efecto y notificamos la activación.
            IsPowerPelletActive = true;
            OnPowerPelletStatusChange?.Invoke(true);

            // En caso que el efecto este activo, cancelamos la corrutina que lo desactiva.
            if (powerPelletEffectDeactivateCoroutine != null)
                StopCoroutine(powerPelletEffectDeactivateCoroutine);

            // Iniciamos la corrutina para desactivar el efecto (en cuyo caso se reinicia).
            powerPelletEffectDeactivateCoroutine = StartCoroutine(DeactivatePowerPellet());
        }
    }

    protected void EnemyController_OnEnemyEaten(int points)
    {
        // Cada vez que un enemigo es comido, aumentamos el contador.
        enemyEatenCounter++;

        // Sumamos los puntos al score multiplicados por el contador de enemigos comidos
        // dado que mientras dure el efecto del power pellet, el jugador va duplicando los puntos
        // por matar enemigos segun la siguiente estructura:
        //
        // 1 enemigo comido = 200 puntos
        // 2 enemigos comidos = 400 puntos
        // 3 enemigos comidos = 800 puntos
        // 4 enemigos comidos = 1600 puntos
        //
        // Cuando el efecto desaparece, el contador vuelve a 0.

        Score += points * enemyEatenCounter;
    }

    protected IEnumerator DeactivatePowerPellet()
    {
        // Esperamos el tiempo de advertencia para avisar que el efecto se va a desactivar,
        // emitimos el evento para que los enemigos comiencen a cambiar de color.
        yield return new WaitForSeconds(powerPelletEffectDuration - powerPelletEffectWarning);
        OnPowerPelletFadeWarning?.Invoke();

        // Esperamos el tiempo restante para desactivar el efecto y emitir el evento.
        yield return new WaitForSeconds(powerPelletEffectWarning);
        IsPowerPelletActive = false;
        OnPowerPelletStatusChange?.Invoke(false);
        powerPelletEffectDeactivateCoroutine = null;

        // Cuando se desactiva el efecto, reiniciamos el contador de enemigos comidos.
        enemyEatenCounter = 0;
    }
}

public enum GameState
{
    Idle,
    Playing,
    GameOver,
    LevelCompleted
}
