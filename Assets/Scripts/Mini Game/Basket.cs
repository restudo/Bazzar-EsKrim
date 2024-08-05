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

        private Sequence flashSequence;
        private Material originalMaterial;

        private void OnEnable()
        {
            EventHandler.BasketFlashEffect += FlashEffect;
        }

        private void OnDisable()
        {
            EventHandler.BasketFlashEffect += FlashEffect;
        }

        private void Start()
        {
            originalMaterial = visual.material;
        }

        private IEnumerator StartFlashEffect()
        {
            // Swap to the flashMaterial.
            visual.material = flashMaterial;

            // Pause the execution of this function for "duration" seconds.
            yield return new WaitForSeconds(flashDuration);

            // After the pause, swap back to the original material.
            visual.material = originalMaterial;
        }

        public void FlashEffect()
        {
            if (visual != null)
            {
                visual.material = originalMaterial;
             
                StartCoroutine(StartFlashEffect());
            }
        }
    }
}
