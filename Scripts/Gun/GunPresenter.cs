using TMPro;
using UnityEngine;
using UniRx;

public class GunPresenter : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI ammoUI;
    private PlayerShooting playerShooting;

    private void Start()
    {
        playerShooting = GetComponent<PlayerShooting>();

        playerShooting.CurrentGun.Ammo
            .Subscribe(x =>
            {
                ammoUI.text = $"{x}/{playerShooting.CurrentGun.MaxAmmo}";
            });
    }
}