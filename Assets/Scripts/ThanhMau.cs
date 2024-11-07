using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ThanhMau : MonoBehaviour
{
    public Image _thanhMau;
    public void UpdateHealthUI(float currentHealth, float maxHealth)
    {
        _thanhMau.fillAmount = currentHealth / maxHealth;
    }
}
