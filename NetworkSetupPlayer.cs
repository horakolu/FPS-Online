using Fusion;
using UnityEngine;
/// <summary>
/// это для того чтобы не было бага когда игрок спавнился в позициях и другой игрок смотрит как он спавнится(прыгает в позициях странно)
/// </summary>
public class NetworkSetupPlayer : NetworkBehaviour
{
    [SerializeField] private NetworkPlayer networkPlayer;
    [SerializeField] private FirstPersonController fps;
    [SerializeField] private GameObject firstPersonCharacter;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private GameObject puppet;
    [SerializeField] private NetworkWeapon networkWeapon;
    [SerializeField] private ParticlesPlayerNetwork particlesPlayerNetwork;
    [SerializeField] private Dragon dragon;
    [SerializeField] private ShaderDestroy shaderDestroy;

    public override void Spawned()
    {
        networkPlayer.SetPosCam(fps.GetCam.transform.forward);
        //Invoke(nameof(InitializationPlayer), 1f); // могут быть лаги позиционирования во время спавна // мэй би придётся вернуть эту строчку кода обратно так что пока не буду удалять
        InitializationPlayer();
    }

    private void InitializationPlayer()
    {// Зачем нужно показывать игрока другим игрокам через 1 секунду? Багуют позиции во время спавна. Баг фотона. Это происходит, когда один игрок заходит в комнату с многими игроками

        shaderDestroy.SetBasicState();

        if (HasStateAuthority)
        {
            characterController.enabled = true;
            fps.enabled = true;
            firstPersonCharacter.SetActive(true);
            UIManager.In.Setup(GetComponent<FirstPersonController>().GetCam);
            GameManager.In.StartRound();
            UIGameMenu.In.DeactivateMainPanel();
        }
        else
        {
            foreach (Transform tra in puppet.transform)
                tra.gameObject.SetActive(true);
            transform.rotation = networkPlayer.GetCorrectPlayerRot; // передача обновлённого поворота игрока по сети, который не пошел после спавна, но повернулся
        }

        fps.OnUseHeadBob();
        networkWeapon.DelayWeaponChange(TypeWeapon.BasicGun, true);
    }
}
