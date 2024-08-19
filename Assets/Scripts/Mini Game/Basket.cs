using UnityEngine;
using DG.Tweening;
using System.Collections;

namespace BazarEsKrim
{
    public class Basket : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer visual;
        [SerializeField] private Material flashMaterial;
        [SerializeField] private float flashDuration = 0.1f; // Duration of each flash
        [SerializeField] private int flashCount = 1; // Number of flashes
        [SerializeField] private GameObject[] basketContentVisuals;

        private Material originalMaterial;

        private void OnEnable()
        {
            EventHandler.BasketFlashEffect += FlashEffect;
            EventHandler.MiniGameScoreTier += ActivateBasketVisual;
        }

        private void OnDisable()
        {
            EventHandler.BasketFlashEffect += FlashEffect;
            EventHandler.MiniGameScoreTier -= ActivateBasketVisual;
        }

        private void Start()
        {
            originalMaterial = visual.material;

            foreach (GameObject content in basketContentVisuals)
            {
                content.SetActive(false);
            }
        }

        private IEnumerator StartFlashEffect()
        {
            for (int i = 0; i < flashCount; i++)
            {
                // Swap to the flashMaterial.
                visual.material = flashMaterial;

                // Pause the execution of this function for "duration" seconds.
                yield return new WaitForSeconds(flashDuration);

                // After the pause, swap back to the original material.
                visual.material = originalMaterial;
            }
        }

        private void FlashEffect()
        {
            if (visual != null)
            {
                visual.material = originalMaterial;

                StartCoroutine(StartFlashEffect());
            }
        }

        private void ActivateBasketVisual(int scoreTier)
        {
            if (scoreTier >= basketContentVisuals.Length) return; // Check if scoreTier is within bounds

            basketContentVisuals[scoreTier].SetActive(true);
        }
    }
}
