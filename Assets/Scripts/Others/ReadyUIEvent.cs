using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReadyUIEvent : MonoBehaviour
{
    public void OnEvent()
    {
        //Ready 애니메이션이 종료되면 아래 함수들을 호출해서 게임이 시작되도록 애니메이션 종료 시점에 OnEvent()함수를 호출한다. 
        UIManager.GetInstance.SetActiveIngameUIPanel(true);
        GameManager.GetInstance.SetProcess("Ingame");
        GameManager.GetInstance.PlayGame(true);
        UIManager.GetInstance.SetActiveReadyUI(false);
        Board.GetInstance.ShuffleBlocks(false);
    }
}
