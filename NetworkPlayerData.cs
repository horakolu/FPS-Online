using Fusion;
using UnityEngine;

public class NetworkPlayerData : NetworkBehaviour
{
    public static NetworkPlayerData In;

    [Networked]
    private string Nickname { get; set; }

    [Networked]
    private int Score { get; set; }

    [Networked]
    private int Deaths { get; set; }

    [Networked]
    private int SumAmountDamageDragon { get; set; }

    [Networked]
    private bool ActivatedDragon { get; set; }

    public int GetScore => Score;

    public string GetNickname => Nickname;

    public int GetDeaths => Deaths;

    public bool GetActivatedDragon => ActivatedDragon;

    public int GetSumAmountDamageDragon => SumAmountDamageDragon;

    public void DragonActivate()
    {
        ActivatedDragon = true;
    }

    public void RestartDragonDeActivate()
    {
        ActivatedDragon = false;
    }

    public void AddScore()
    {
        Score += 1;
    }

    public void AddDeath()
    {
        Deaths += 1;
    }

    public void RestartScore()
    {
        Score = 0;
    }

    public void RestartDeaths()
    {
        Deaths = 0;
    }

    public void AddSumAmountDamageDragon()
    {
        SumAmountDamageDragon += 1;
    }

    public void SumAmountDamageDragonToZero()
    {
        SumAmountDamageDragon = 0;
    }

    public override void Spawned()
    {
        if (HasStateAuthority) // чтобы я ссылался на свои собственные данные игрока,чтобы не лез в чужие NetworkPlayerData сетевые инстансы
        {
            In = this;

            SetNickname(PlayerPrefs.GetString(SavePlayerPrefs.Nickname.ToString()));
            UIGameMenu.In.FinishInicUIInfoSession();
        }
    }

    private void SetNickname(string getNickname)
    {
        Nickname = getNickname;
    }
}
/*
можно находить свой сетевой объект так(через линк функцию) - 
NetworkPlayerData[] networksPlayersData = FindObjectsByType<NetworkPlayerData>(FindObjectsSortMode.None);
var myNetworksPlayersData = networksPlayersData.Single(x => x.GetComponent<NetworkObject>().HasStateAuthority == true);
но зачем,когда можно просто через синголтон найти свой объект среди всех других NetworkPlayerData в сцене иниц-я его в эвейке
*/
