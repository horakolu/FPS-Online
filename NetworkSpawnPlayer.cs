using Fusion;
using UnityEngine;
using System.Collections;

public class NetworkSpawnPlayer : NetworkBehaviour
{
    [SerializeField] private UIGameMenu uiGameMenu;
    [SerializeField] private UIRoundTime uiRoundTime;
    [SerializeField] private GameObject prefabPlayer;
    [SerializeField] private GameObject networkPlayerData;
    [SerializeField] private Transform[] centralSpawnPoints;

    private Vector3 fixNetworkPos;
    private NetworkObject netObjPlayer;

    public GameObject GetGOPlayer => netObjPlayer.gameObject;

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void SpawnAllPlayersRPC()
    {
        uiGameMenu.DeactivateMainPanel(); //!
        GameStateManager.In.SpawnPlayerAndBonus(); //!
    }

    public void Launch()
    {
        if (NetworkPlayerData.In == null) // первый раз когда спавним только заходим сюда. После перерождений не надо спавнить снова этот объект. Он никогда не удаляется.
            uiGameMenu.GetRunner.Spawn(networkPlayerData);

        if (UIGameMenu.In.GetInRoom == false)
            StartCoroutine(Where());
    }

    public IEnumerator Where()
    {
        Vector3 spawnPos = Vector3.zero;
        if (GameStateManager.In.GetStateGame == StateGame.Simple)
            spawnPos = centralSpawnPoints[Random.Range(0, centralSpawnPoints.Length)].position + new Vector3(Random.Range(-15, 16), 0, Random.Range(-15, 16));
        else if (GameStateManager.In.GetStateGame != StateGame.TestBonusWeapon)
            spawnPos = BonusWeaponSpawner.In.GetPosForTestBonusWeapon;

            // Бонусное оружение не должно спавнится два или более раз в одну и ту же точку спавна. Иначе игрок возьмёт одновременно два бонусных оружия(дублирование скрипта на игрока)
        Collider[] enemies = Physics.OverlapSphere(spawnPos, 2);
        foreach (Collider enemy in enemies)
        {
            if (enemy.name == "Floor") continue;

            if (enemy.tag == Tags.WeaponBonus.ToString() || enemy.tag == Tags.Player.ToString() || enemy.tag == Tags.Collision.ToString())
            {
                yield return new WaitForSeconds(0.001f);
                StartCoroutine(Where());
                yield break;
            }
        }

        Realize(spawnPos);
    }


    public void Realize(Vector3 getSpawnPos)
    {
        fixNetworkPos = getSpawnPos;
        netObjPlayer = uiGameMenu.GetRunner.Spawn(prefabPlayer, getSpawnPos, Quaternion.identity, null, InitializeObjBeforeSpawn);
    }

    private void InitializeObjBeforeSpawn(NetworkRunner runner, NetworkObject obj)
    {
        obj.transform.position = fixNetworkPos;
    }
}
