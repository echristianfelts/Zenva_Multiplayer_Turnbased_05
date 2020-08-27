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
        Debug.Log("PlayerController: Initialize");
        photonPlayer = player;

        // if this is our local player, spawn the units
        if (player.IsLocal)
        {
            Debug.Log("PlayerController: Initialize.PlayerLocal");

            me = this;
            SpawnUnits();
        }
        else
        {
            Debug.Log("PlayerController: Initialize.PlayernotLocal");

            enemy = this;
        }

        // set the UI player text
        GameUI.instance.SetPlayerText(this);
    }

    void SpawnUnits()
    {
        Debug.Log("PlayerController: SpawnUnits");

        for (int x = 0; x < unitsToSpawn.Length; ++x)
        {
            GameObject unit = PhotonNetwork.Instantiate(unitsToSpawn[x], spawnPoints[x].position, Quaternion.identity);
            unit.GetPhotonView().RPC("Initialize", RpcTarget.Others, false);
            unit.GetPhotonView().RPC("Initialize", photonPlayer, true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!photonView.IsMine)
            return;

        if (Input.GetMouseButtonDown(0) && GameManager.instance.curPlayer == this)
        {
            Debug.Log("PlayerController: Update.Click");

            Vector3 pos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            TrySelect(new Vector3(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y), 0));
        }
    }

    void TrySelect(Vector3 selectPos)
    {
        Debug.Log("PlayerController: TrySelect");

        // are we selecting our unit?
        Unit unit = units.Find(x => x.transform.position == selectPos);

        if (unit != null)
        {
            SelectUnit(unit);
            return;
        }

        if (!selectedUnit)
            return;

        // are we selecting an enemy unit?
        Unit enemyUnit = enemy.units.Find(x => x.transform.position == selectPos);

        if (enemyUnit != null)
        {
            TryAttack(enemyUnit);
            return;
        }

        TryMove(selectPos);
    }

    void SelectUnit(Unit unitToSelect)
    {
        Debug.Log("PlayerController: Select Unit");

        // can we select the unit
        if (!unitToSelect.CanSelect())
            return;

        // un-select the current unit
        if (selectedUnit != null)
            selectedUnit.ToggleSelect(false);

        // select the new unit
        selectedUnit = unitToSelect;
        selectedUnit.ToggleSelect(true);

        // set the unit info text
        GameUI.instance.SetUnitInfoText(selectedUnit);
    }

    void DeSelectUnit()
    {
        selectedUnit.ToggleSelect(false);
        selectedUnit = null;

        // disable the unit info text
        GameUI.instance.unitInfoText.gameObject.SetActive(false);
    }

    void SelectNextAvailableUnit()
    {
        Unit availableUnit = units.Find(x => x.CanSelect());

        if (availableUnit != null)
            SelectUnit(availableUnit);
        else
            DeSelectUnit();
    }

    void TryAttack(Unit enemyUnit)
    {
        if (selectedUnit.CanAttack(enemyUnit.transform.position))
        {
            selectedUnit.Attack(enemyUnit);
            SelectNextAvailableUnit();

            // update the UI
            GameUI.instance.UpdateWaitingUnitsText(units.FindAll(x => x.CanSelect()).Count);
        }
    }

void TryMove(Vector3 movePos)
    {
        if (selectedUnit.CanMove(movePos))
        {
            selectedUnit.Move(movePos);
            SelectNextAvailableUnit();

            // update the UI
            GameUI.instance.UpdateWaitingUnitsText(units.FindAll(x => x.CanSelect()).Count);
        }
    }

    public void EndTurn()
    {
        // de-select the unit
        if (selectedUnit != null)
            DeSelectUnit();

        // start the next turn
        GameManager.instance.photonView.RPC("SetNextTurn", RpcTarget.All);
    }

    public void BeginTurn()
    {
        Debug.Log("PlayerController: BeginTurn");

        foreach (Unit unit in units)
            unit.usedThisTurn = false;

        // update the UI
        GameUI.instance.UpdateWaitingUnitsText(units.Count);
    }
}
