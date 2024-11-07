using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class HealthStatusUI : MonoBehaviour
    {
        [SerializeField] 
        private Image barImage;
        
        [SerializeField]
        private BaseCharacterScript characterScript;

        private void Start()
        {
            characterScript.OnHealthUpdate += BaseCharacterScript_OnHealthUpdate;
            characterScript.OnFlip += BaseCharacterScript_OnFlip;
            barImage.fillAmount = 1;
            Show();
        }

        private void BaseCharacterScript_OnFlip()
        {
            Vector3 localScale = transform.localScale;
            localScale.x *= -1;
            transform.localScale = localScale;
        }

        private void BaseCharacterScript_OnHealthUpdate(object sender, BaseCharacterScript.HeathUpdateEventArgs e)
        {
            barImage.fillAmount = e.HealthPersent;

            if (e.HealthPersent == 0)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }

        private void Show() => gameObject.SetActive(true);
        private void Hide() => gameObject.SetActive(false);
    }
}
