using UnityEngine;
using GameEventType = GameManager.GameEventType;

public class NPCManager : MonoBehaviour
{
    //GameManager
    private GameManager _gameManager;
    
    public void Initialize(GameManager gameManager)
    {
        //Init gameManager
        _gameManager = gameManager;
    }

    public void OnUpdate()
    {
        ////
    }

    public void OnFixedUpdate()
    {
        ////
    }
    
    
    
    //ToDo: Call this method every time a civilian is about to die (inside OnDestroy)
    public void NPCDied()
    {
        //Invoke corresponding event
        _gameManager.InvokeEvent(GameEventType.onNPCDied);
    }
}