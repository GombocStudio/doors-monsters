using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MonsterScript : Interactable
{    
    public float speed;
    public TerrainGenerator mapGenerator;
    public MonsterController monsterController;
        
    private Vector3 GetDestination()
    {
        var mapData = mapGenerator.terrainData;
        int r = Random.Range(0, mapData.GetLength(0));
        int c = Random.Range(0, mapData.GetLength(1));
        var room = mapData[r, c];

        return room.position;
    }
   
    void Start()
    {     
        var agent = GetComponent<NavMeshAgent>();
        agent.destination = GetDestination();
        agent.acceleration = Random.Range(2, 10);
        agent.enabled = true;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.GoodQualityObstacleAvoidance;
    }
   
    void Update()
    {
        var agent = GetComponent<NavMeshAgent>();
        if (Vector3.Distance(transform.position, agent.destination) < 5)
        {
            agent.destination = GetDestination();
        }        
    }

    public override void Interact(GameObject player) {
        monsterController.MonsterCollision(this);        
    }

    public override void Deinteract(GameObject player) {

    }
}
