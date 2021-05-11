using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public int m_NumRoundsToWin = 5;             //单个玩家要赢得游戏的回合数。
    public float m_StartDelay = 3f;              //RoundStarting和RoundPlaying阶段开始之间的延迟。
    public float m_EndDelay = 3f;               //RoundPlaying 和 RoundEnding 阶段结束之间的延迟。
    public CameraControl m_CameraControl;        // 参考CameraControl脚本，在不同阶段进行控制。
    public Text m_MessageText;                  // 参考覆盖文字显示中奖文字等。
    public GameObject m_TankPrefab;             //参考玩家将控制的预制件。
    public TankManager[] m_Tanks;               //一个管理者的集合，用于启用和禁用坦克的不同方面。

    private int m_RoundNumber;                  //目前正在进行哪一轮比赛。
    private WaitForSeconds m_StartWait;          //曾经有一个延迟，而该轮开始。
    private WaitForSeconds m_EndWait;            //用于回合或游戏结束时的延迟。
    private TankManager m_RoundWinner;          // 指本轮比赛的获胜者。 用于宣布谁赢了。
    private TankManager m_GameWinner;           //指游戏的赢家。 用于宣布谁赢了。


    private void Start()
    {
        // 创建延迟，因此只需要做一次。
        m_StartWait = new WaitForSeconds(m_StartDelay);
        m_EndWait = new WaitForSeconds(m_EndDelay);

        SpawnAllTanks();
        SetCameraTargets();

        // 一旦创建了坦克，并且摄像机将它们作为目标，就开始游戏。
        StartCoroutine(GameLoop());
    }

    //生成所有坦克
    private void SpawnAllTanks()
    {
        // 对于所有的坦克...
        for(int i = 0;i < m_Tanks.Length; i++)
        {
            //......创建它们，设置它们的播放器编号和控制所需的参考资料。
            m_Tanks[i].m_Instance =
                Instantiate(m_TankPrefab, m_Tanks[i].m_SpawnPoint.position, m_Tanks[i].m_SpawnPoint.rotation) as GameObject;
            m_Tanks[i].m_PlayerNumber = i + 1;
            m_Tanks[i].Setup();

        }


    }

    //设置摄像头目标
    private void SetCameraTargets()
    {
        // 创建一个与坦克数量相同大小的变形集合。
        Transform[] targets = new Transform[m_Tanks.Length];

        // 对于每一个变换... ...
        for (int i = 0; i < targets.Length; i++)
        {
            //......将其设置为适当的油箱变换。
            targets[i] = m_Tanks[i].m_Instance.transform;
        }

        // 这些是摄像机应该跟踪的目标。
        m_CameraControl.m_Targets = targets;

    }

    // 这是从一开始就被调用的，将逐一运行游戏的每个阶段。
    private IEnumerator GameLoop()        //游戏循环
    {
        //首先运行 "RoundStarting "coroutine，但在完成之前不要返回。
        yield return StartCoroutine(RoundStarting());

        // 一旦 "RoundStarting "程序完成，运行 "RoundPlaying "程序，但在完成之前不要返回。
        yield return StartCoroutine(RoundPlaying());

        // 一旦执行到这里返回，运行'RoundEnding'coroutine，同样不要返回，直到它完成。
        yield return StartCoroutine(RoundEnding());

        // 在 "RoundEnding "结束之前，这段代码不会运行。 这时，检查是否找到了游戏赢家。
        if (m_GameWinner!=null)
        {
            // 如果有游戏赢家，重新开始关卡。
            SceneManager.LoadScene(0);
        }
        else
        {
            // 如果还没有赢家，就重新启动这个程序，使循环继续。
            // 请注意，这个outine不会产生任何结果，这意味着当前版本的GameLoop将结束。 这意味着当前版本的GameLoop将结束。
            StartCoroutine(GameLoop());
        }
    }

    //回合开始
    private IEnumerator RoundStarting()
    {
        // 一回合开始就重置坦克，确保他们不能移动。
        ResetAllTanks();
        DisableTankControl();

        // 将相机的变焦和位置调整到适合重置坦克的位置。
        m_CameraControl.SetStartPositionAndSize();

        // 递增回合数，并显示文字显示玩家的回合数。
        m_RoundNumber++;
        m_MessageText.text = "ROUND" + m_RoundNumber;

        // 等待指定的时间长度，直到将控制权交还给游戏循环。
        yield return m_StartWait;
     }

    //循环播放
    private IEnumerator RoundPlaying()
    {
        //本轮比赛一开始就让玩家控制坦克。
        EnableTankControl();

        // 清除屏幕上的文字。
        m_MessageText.text = string.Empty;

        // 虽然已经没有一辆坦克了... ...
        while (!OneTankLeft())
        {
            //......在下一帧返回。
            yield return null;
        }
    }

    //结束
    private IEnumerator RoundEnding()
    {
        // 停止坦克移动。
        DisableTankControl();

        // 清除上一轮的赢家。
        m_RoundWinner = null;

        // 看看是否有赢家，现在这一轮结束了。
        m_RoundWinner = GetRoundWinner();

        // 如果有赢家，则将其分数递增。
        if (m_RoundWinner != null)
            m_RoundWinner.m_Wins++;

        // 现在赢家的分数已经递增，看看是否有人在游戏中获得了一个分数。
        m_GameWinner = GetGameWinner();

        // 根据分数和是否有游戏赢家获取信息并显示出来。
        string message = EndMessage();
        m_MessageText.text = message;

        // 等待指定的时间长度，直到将控制权交还给游戏循环。
        yield return m_EndWait;


    }

    // 这用于检查是否还有一个或更少的坦克，因此该回合应该结束。
    private bool OneTankLeft()
    {
        // 从零开始计算剩下的坦克数量。
        int numTanksLeft = 0;

        // 检查所有的坦克...
        for (int i=0;i<m_Tanks.Length;i++)
        {
            // ... ...如果它们处于活动状态，则递增计数器。
            if (m_Tanks[i].m_Instance.activeSelf)
                numTanksLeft++;
        }

        // 如果还有一个或更少的油箱，返回true，否则返回false。
        return numTanksLeft <= 1;

    }

    // 该函数用于查找本轮比赛是否有赢家。
    // 这个函数的调用是假设当前有1个或更少的坦克在活动。
    private TankManager GetRoundWinner()
    {
        // 检查所有的坦克...
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            //......如果其中有一个是活动的，它就是赢家，所以要返回它。
            if (m_Tanks[i].m_Instance.activeSelf)
                return m_Tanks[i];
        }

        // 如果没有一个油箱是活动的，那就是抽签，所以返回null。
        return null;
    }


    // 这个函数是用来查找是否有游戏的赢家。
    private TankManager GetGameWinner()
    {
        // 检查所有的坦克...
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            //......如果其中一个人有足够的回合数来赢得比赛，就把它退回。
            if (m_Tanks[i].m_Wins == m_NumRoundsToWin)
                return m_Tanks[i];
        }

        // 如果没有坦克有足够的回合数获胜，返回null。
        return null;
    }


    // 返回每轮结束时显示的字符串信息。
    private string EndMessage()
    {
        // 默认情况下，当一轮结束时，没有赢家，所以默认的结束信息是平局。
        string message = "DRAW!";

        // 如果有一个赢家，那么改变信息以反映这一点。
        if (m_RoundWinner != null)
            message = m_RoundWinner.m_ColoredPlayerText + " WINS THE ROUND!";

        // 在初始信息后添加一些换行符。
        message += "\n\n\n\n";

        // 翻阅所有的坦克，并将它们的每一个分数添加到信息中。
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            message += m_Tanks[i].m_ColoredPlayerText + ": " + m_Tanks[i].m_Wins + " WINS\n";
        }

        // 如果有一个游戏的赢家，改变整个信息以反映这一点。
        if (m_GameWinner != null)
            message = m_GameWinner.m_ColoredPlayerText + " WINS THE GAME!";

        return message;
    }


    // 该功能用于重新开启所有坦克，并重置其位置和属性。
    private void ResetAllTanks()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            m_Tanks[i].Reset();
        }
    }

    //启动坦克控制
    private void EnableTankControl()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            m_Tanks[i].EnableControl();
        }
    }

    //禁用油箱控制
    private void DisableTankControl()
    {
        for (int i = 0; i < m_Tanks.Length; i++)
        {
            m_Tanks[i].DisableControl();
        }
    }
}
