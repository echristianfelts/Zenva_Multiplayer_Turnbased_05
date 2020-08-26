using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerController : MonoBehaviourPun
{

    public Player photonPlayer;                     //  Photon.Realtime.Player class
    public string[] unitsToSpawn;
    public Transform[] spawnPoints;                 //  Array of spawn positions for this player

    public List<Unit> units = new List<Unit>();     //  List of all of our units.
    private Unit selectedUnit;                      //  Currently selected unit.

    public static PlayerController me;
    public static PlayerController enemy;           //  Q: What is the best a way to have more than two players..?

    [PunRPC]
    void Initialize(Player player)
    {
        photonPlayer = player;

        // if this is our local player, spawn the units
        if (player.IsLocal)
        {
            me = this;
            SpawnUnits();
        }
        else
        {
            enemy = this;
        }

        // set the UI player text
    }

    void SpawnUnits()
    {
        for (int x = 0; x < unitsToSpawn.Length; ++x)
        {
            GameObject unit = PhotonNetwork.Instantiate(unitsToSpawn[x], spawnPoints[x].position, Quaternion.identity);
            unit.GetPhotonView().RPC("Initialize", RpcTarget.Others, false);
            unit.GetPhotonView().RPC("Initialize", photonPlayer, true);
        }
    }




    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
