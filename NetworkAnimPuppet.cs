using UnityEngine;
using System.Collections;
using UnityStandardAssets.CrossPlatformInput;
using Fusion;

public class NetworkAnimPuppet : NetworkBehaviour
{
    [SerializeField] private NetworkStateSuperman behvariorFly;
    [SerializeField] private StateSupermanController stateSupermanController;

    [Networked] public float speedX { get; set; }
    [Networked] public float speedY { get; set; }
    [Networked] public int animSpeed { get; set; }
    [Networked] public bool activateAnimRun { get; set; }
    [Networked] public float turnX { get; set; }
    [Networked] public float directional { get; set; }


    private NetworkPlayer networkPlayer;
    private Animator anim;
    private int turnLeft;
    private int turnRight;
    private float sum;

    public bool GetRuning => activateAnimRun;

    public bool GetWalking => CrossPlatformInputManager.GetAxis("Horizontal") != 0 || CrossPlatformInputManager.GetAxis("Vertical") != 0;
    

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void FixAnimInJumpingRPC()
    {
        if(anim.GetBool("Run"))
            anim.SetBool("Run", false);
    }

    public bool GetTurn()
    {
        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(2);
        return stateInfo.fullPathHash == turnRight || stateInfo.fullPathHash == turnLeft || animSpeed == 1;
    }

    public void JumpTrue()
    {
        JumpTrueRPC();
    }

    public void JumpFalse()
    {
        JumpFalseRPC();
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void JumpTrueRPC()
    {
        anim.SetBool("Jump", true);
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void JumpFalseRPC()
    {
        StartCoroutine(DelayCallJumpFalseRPC());
    }

    private IEnumerator DelayCallJumpFalseRPC()
    {
        yield return new WaitUntil(() => behvariorFly.GetIsGrounded);
        anim.SetBool("Jump", false);
    }

    public override void Spawned()
    {
        turnLeft = Animator.StringToHash("Move.idle_turn_idle_90_L");
        turnRight = Animator.StringToHash("Move.idle_turn_idle_90_R");
        anim = GetComponent<Animator>();
        networkPlayer = transform.parent.GetComponent<NetworkPlayer>();
    }

    public override void Render()
    {

        if(HasStateAuthority == false)
        {
            if (networkPlayer.GetActivateBonusSwordAttack == false)
            {
                anim.SetBool("Run", activateAnimRun);
                anim.SetInteger("Move", animSpeed);
                anim.SetFloat("Speed_X", speedX);
                anim.SetFloat("Speed_Y", speedY);
            }
            else
            {// бег вызывается тут не обычными кнопками движения на которые накладывается кнопка шифт,а он просто вызывается,а он вызывается на показатели Move,а Move на Speed_Y или Speed_X
                anim.SetBool("Run", true);
                anim.SetInteger("Move", 1);
                anim.SetFloat("Speed_Y", 1);
            }
            anim.SetFloat("Turn_X", turnX);
            anim.SetFloat("Directional", directional);
            Vector3 directionMove = networkPlayer.GetCorrectPlayerPos - networkPlayer.transform.position;
            if (directionMove.magnitude > 0.1f)
            {
                float angle = Vector3.Angle(networkPlayer.transform.forward, directionMove.normalized);
                Quaternion rotForward = Quaternion.LookRotation(Vector3.forward) * Quaternion.Euler(0, angle, 0);
                Vector3 direction = rotForward * Vector3.forward;
                float sign = Vector3.Cross(networkPlayer.transform.forward, directionMove.normalized).y;
                direction = new Vector3(sign * direction.normalized.x, 0, direction.normalized.z);
                anim.SetFloat("Speed_X", direction.normalized.x);
                anim.SetFloat("Speed_Y", direction.normalized.z);
            }
            else
            {
                anim.SetFloat("Speed_X", 0);
                anim.SetFloat("Speed_Y", 0);
            }

            if (GetTurn()) // исправляет баг поворота анимации тот игрок у которого он есть. А он есть у того на кого мы смотрим и поэтому HasStateAuthority == false 
            { 
                FixAnimTurnRPC(); // нужно всем игрокам исправить этот баг
            }
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (stateSupermanController.GetStateInAir == false)
        {
            float horizontal = CrossPlatformInputManager.GetAxis("Horizontal");
            float vertical = CrossPlatformInputManager.GetAxis("Vertical");
            int anim_speed = horizontal != 0 || vertical != 0 ? 1 : 0;
            animSpeed = anim_speed;
            speedX = horizontal;
            speedY = vertical;
            activateAnimRun = Input.GetKey(KeyCode.LeftShift);
        }
        else
        {
            activateAnimRun = false;
            animSpeed = 0;
            speedX = 0;
            speedY = 0;
        }


        turnX = networkPlayer.GetFPS.GetCam.transform.forward.x;
        anim.SetFloat("Turn_X", turnX); // для себя это анимацию воспроизводить не надо но это нужно для включения булевской turn в состояние true,для того чтобы обнулить передачу сетевого свойства turnX
        directional = CrossPlatformInputManager.GetAxis("Mouse X");
        if (CrossPlatformInputManager.GetAxis("Mouse X") != 0)
            sum += CrossPlatformInputManager.GetAxis("Mouse X");
        turnX = sum;
        anim.SetFloat("Turn_X", turnX); // для себя это анимацию воспроизводить не надо но это нужно для включения булевской turn в состояние true,для того чтобы обнулить передачу сетевого свойства turnX
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void FixAnimTurnRPC()
    {
        sum = 0;
        turnX = 0;
        anim.SetFloat("Turn_X", 0);
    }
}
