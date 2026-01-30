using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


public enum CarColor
{
    Yellow,
    Blue,
    Orange,
    Purple
}
public class GameManager : MonoBehaviour
{
    [SerializeField] private UIManager uiManager;
    [SerializeField] private float gameDuration = 30f;
    [SerializeField] private ArenaController arena;

    [Header("Cars")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private GameObject opponentPrefab;
    [SerializeField] private List<CarPrefabEntry> carPrefabs;
    [SerializeField] private float randomOffsetSpeed = 2f;

    [Header("Pickups")]
    [SerializeField] private GameObject[] moneyPickupPrefabs;
    [SerializeField] private float pickupSpawnInterval = 3f;
    [SerializeField] private int maxPickups = 5;

    [Header("Traffic")]
    [SerializeField] private GameObject trafficCarPrefab;
    [SerializeField] private float trafficSpawnInterval = 4f;

    private readonly List<GameObject> activePickups = new();
    private readonly List<GameObject> activeTrafficCars = new();
    private readonly List<IPlayerCar> players = new();
    private readonly List<CarPrefabEntry> tempCarList = new();
    private readonly Dictionary<IPlayerCar, int> playerScores = new();

    void Start()
    {
        uiManager.OnPressStartButton += StartGame;
        uiManager.OnTimerFinished += FinishGame;
        uiManager.OnCountdownFinished += CountdownFinished;
    }

    private void StartGame(int numberOfPlayers)
    {
        Time.timeScale = 1;
        ResetGame();
        uiManager.SetScoreUI(numberOfPlayers);
        SpawnCars(numberOfPlayers);
        uiManager.ShowCountdown();
    }

    private void CountdownFinished()
    {
        uiManager.StartTimer(gameDuration);
        ActivateCars(true);
        StartCoroutine(PickupSpawner());
        StartCoroutine(TrafficSpawner());
    }
    
    private void FinishGame()
    {
        Time.timeScale = 0;
        uiManager.ShowHighscoreUI();
    }
    
    void SpawnCars(int numberOfPlayers)
    {
        var spawnPoints = arena.CarSpawnPoints;

        AddPlayer(playerPrefab, spawnPoints[0]);
        
        for (int i = 1; i < numberOfPlayers; i++)
        {
            AddPlayer(opponentPrefab, spawnPoints[i]);
            var opponent = players[i];
            opponent.GetSteeringAgent().maxSpeed += Random.Range(-randomOffsetSpeed, randomOffsetSpeed);
        }

        RegisterCollisionEvents();
        ActivateCars(false); // wait for countdown
    }

    private void AddPlayer(GameObject playerPrefab, Transform spawnPoint)
    {
        var playerObj = Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);
        var player = playerObj.GetComponent<IPlayerCar>();
        players.Add(player);
        playerScores[player] = 0;
        var playerCar = tempCarList[Random.Range(0, tempCarList.Count)];
        player.CarColor = playerCar.color;
        uiManager.SetPlayerColor(playerCar.color, players.Count-1);
        tempCarList.Remove(playerCar);
        Instantiate(playerCar.prefab, playerObj.transform);
    }

    private void ResetGame()
    {
        uiManager.ResetScores();

        foreach (var car in players)
        {
            car.Destroy();
        }
        players.Clear();
        playerScores.Clear();
        
        foreach (var car in activeTrafficCars)
        {
            Destroy(car);
        }
        activeTrafficCars.Clear();
        
        foreach (var pickup in activePickups)
        {
            Destroy(pickup);
        }
        activePickups.Clear();
        
        tempCarList.Clear();
        tempCarList.AddRange(carPrefabs);
        
        StopAllCoroutines();
    }

    private void RegisterCollisionEvents()
    {
        foreach (var car in players)
        {
            car.RegisterForPickupCollisionEvent(OnPickupCollision);
            car.RegisterForTrafficCollisionEvent(OnTrafficCollision);
        }
    }
    
    private void OnTrafficCollision(IPlayerCar car, TrafficCarController traffic)
    {
        playerScores[car] += traffic.CollisionPenalty;
        if (playerScores[car] < 0)
        {
            playerScores[car] = 0;
        }
        uiManager.UpdateScore(car.CarColor, playerScores[car]);
        Destroy(traffic.gameObject);
    }
    
    private void OnPickupCollision(IPlayerCar car, Pickup pickup)
    {
        playerScores[car] += pickup.Value;
        uiManager.UpdateScore(car.CarColor, playerScores[car]);
        activePickups.Remove(pickup.gameObject);
        Destroy(pickup.gameObject);
    }

    private void ActivateCars(bool activate)
    {
        foreach (var car in players)
        {
            car.ChangeActivation(activate);
        }
    }
    
    private IEnumerator PickupSpawner()
    {
        while (true)
        {

            if (activePickups.Count >= maxPickups)
            {
                yield return 0;
                continue;
            }

            SpawnPickup();
            
            yield return new WaitForSeconds(pickupSpawnInterval);
        }
    }

    private void SpawnPickup()
    {
        var prefab = moneyPickupPrefabs[Random.Range(0, moneyPickupPrefabs.Length)];
        var pos = arena.GetRandomPointInside(1f, arena.Radius - 1f);
        var pickup = Instantiate(prefab, pos, Quaternion.identity);
        activePickups.Add(pickup);
    }
    
    private IEnumerator TrafficSpawner()
    {
        while (true)
        {
            yield return new WaitForSeconds(trafficSpawnInterval);

            var spawnPos = arena.GetRandomPointOnEdge();
            var trafficObj = Instantiate(trafficCarPrefab, spawnPos, Quaternion.identity);
            activeTrafficCars.Add(trafficObj);
            trafficObj.GetComponent<TrafficCarController>().StartPath(spawnPos, arena.GetPointOnEdge(-spawnPos));
        }
    }
}

[System.Serializable]
public class CarPrefabEntry
{
    public CarColor color;
    public GameObject prefab;
}
