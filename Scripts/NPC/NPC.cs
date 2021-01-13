using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using BehaviorDesigner.Runtime;
using MLAPI;

namespace SRPG {
  public class NPC : NetworkedBehaviour {
    [Header("Pawn")]
    public Pawn pawn;
    public BehaviorTree behaviorTree;
    public float FoV = 90;
    public NavMeshAgent agent;
    public float viewDistance = 15;
    public int patrolDistance = 30;
    [HideInInspector] public Vector3 spawnPos;
    [HideInInspector] public GameObject[] enemy;
  }
}
