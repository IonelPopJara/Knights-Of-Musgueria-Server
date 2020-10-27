using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public float roundTimeOut;
    public float currentTime;

    public int winnerId = 1;
    public string winnerUserName;



    public void InitializeTimer()
    {
        currentTime = roundTimeOut;
    }

    private void Start()
    {
        winnerId = 1;

        InitializeTimer();
    }

    private void Update()
    {
        ServerSend.RoundCurrentTime(this);

        if(currentTime > 0)
        {
            currentTime -= Time.deltaTime;
        }
        else if(currentTime <= 0)
        {
            print("Round Ended");
            RoundEnded();
        }
    }

    public void RoundEnded()
    {
        foreach (Client _client in Server.clients.Values)
        {
            if (_client.player != null)
            {
                if(_client.player.points >= Server.clients[winnerId].player.points)
                {
                    winnerId = _client.id;
                }
            }
        }

        print($"{Server.clients[winnerId].player.username} es el weon mas wea");

        winnerUserName = Server.clients[winnerId].player.username;

        ServerSend.RoundFinished(this);

        RespawnPlayers();

        currentTime = roundTimeOut;
    }

    public void RespawnPlayers()
    {
        foreach (Client _client in Server.clients.Values)
        {
            if (_client.player != null)
            {
                _client.player.RespawnAtSpawnPoint(NetworkManager.instance.spawnPoints.transform.GetChild(_client.id).transform.position);
            }
        }
    }
}
