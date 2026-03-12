using UnityEngine;
using System.Collections.Generic;
using Fusion;
using System;

public class NetworkWeaponPuppet : NetworkBehaviour
{
    /// <summary>
    /// Присваивается не здесь. В скрипте NetworkWeapon -> WeaponChange() когда беру любой бонус
    /// </summary>
    [Networked]
    public TypeWeapon NetworkWeapon { get; set; }

    [SerializeField] private GameObject[] weaponsObjs;

    private Dictionary<TypeWeapon, GameObject> weapons = new Dictionary<TypeWeapon, GameObject>();
    /// <summary>
    /// !!не присваивается владельцу скрипта!! т.е. в любой рпц функции всем нельзя изменять currentWeaponObj,у владельца будет ошибка нулл,он не может на него ссылаться
    /// </summary>
    private GameObject currentWeaponObj;
    private TypeWeapon currentWeapon;

    public TypeWeapon GetNetworkWeapon => NetworkWeapon;

    public GameObject GetObjTypeWeapon(TypeWeapon typeWeapon) => weapons[typeWeapon];

    public override void Spawned()
    {
        foreach (GameObject weaponObj in weaponsObjs)
            weapons.Add((TypeWeapon)Enum.Parse(typeof(TypeWeapon), weaponObj.name), weaponObj);

        if (!HasStateAuthority)
        {
            TypeWeapon loadTypeWeapon = NetworkWeapon;
            currentWeapon = loadTypeWeapon;
            currentWeaponObj = weapons[loadTypeWeapon];
            currentWeaponObj.SetActive(true);
        }
    }

    public override void Render()
    {
        if (!HasStateAuthority && currentWeapon != NetworkWeapon)
        {// четвёртое условие чтобы заходили по этому событию только в NetworkPuppet той сетевой марионетки,владелец которой изменил своё свойство игрока
            // второе условие - когда я меняю оружие, мне не нужно менять оружие у моей марионетки. Я не смотрю на нее
            // третье условие необходимо, поскольку Start вызывается после этого обратного вызова (OnPhotonPlayerPropertiesChanged).
            currentWeaponObj.SetActive(false);
            currentWeaponObj = weapons[NetworkWeapon];
            currentWeaponObj.SetActive(true);

            currentWeapon = NetworkWeapon;
        }
    }
}
