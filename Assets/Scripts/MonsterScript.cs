using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MonsterScript : Interactable
{    
    public float speed;
    public int points;

    // Time the monster is stunned after melee attack (in secons)
    public float stunTime = 2.0f;

    // Is monster stunned
    private bool _stunned = false;

    // When the monster will stop being stunned
    private float _timeStunned = 0.0f;
    private ScoreManager scoreManager;
    private MonsterController monsterController;

    private Vector3 GetDestination()
    {
        if (!monsterController) { return transform.position; }

        TerrainStructure[,] terrainData = monsterController.GetTerrainData();

        int r = Random.Range(0, terrainData.GetLength(0));
        int c = Random.Range(0, terrainData.GetLength(1));
        var room = terrainData[r, c];

        return room.position;
    }
   
    void Start()
    {
        // Initialize score manager component
        scoreManager = FindObjectOfType<ScoreManager>();

        // Monsters only move on the master client
        if (PhotonNetwork.IsMasterClient)
        {
            var agent = GetComponent<NavMeshAgent>();
            if (!agent) { return; } // Remember, eggs dont move around the map

            agent.destination = GetDestination();
            agent.acceleration = Random.Range(2, 10);
            agent.enabled = true;
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.GoodQualityObstacleAvoidance;
        }
    }
   
    void Update()
    {
        // Monsters only move on the master client
        if (PhotonNetwork.IsMasterClient)
        {
            var agent = GetComponent<NavMeshAgent>();
            if (!agent) { return; } // Remember, eggs dont move around the map

            if (Vector3.Distance(transform.position, agent.destination) < 5)
            {
                agent.destination = GetDestination();
            }

            if (_stunned)
            {
                agent.destination = this.transform.position;
            }
        }

        // Check stunned
        if (_stunned)
        {
            _stunned = Time.time <= _timeStunned;
        }
    }

    public void SetController(MonsterController mc)
    {
        monsterController = mc;
    }

    #region Interactable Interface Methods
    public override void Interact(GameObject player) 
    {
        MyCharacterController cc = player.GetComponent<MyCharacterController>();
        
        // Increase score of the player that interacted with the egg
        if (scoreManager && cc) { scoreManager.UpdatePlayerScore(player, cc.scoreMul * points); }

        // Destroy monster and update monster contoller status
        if (monsterController) { monsterController.MonsterCollision(this); }
    }

    public override void Deinteract(GameObject player) {}

    public void StunMonster()
    {
        if (!_stunned)
        {
            _stunned = true;
            _timeStunned = Time.time + stunTime;
        }
    }

    #endregion
}
