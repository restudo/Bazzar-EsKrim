using System.Collections.Generic;
using UnityEngine;

namespace BazarEsKrim
{
    public class GameplayAdjustByAspect : MonoBehaviour
    {
        // The aspect ratio data for this GameObject
        public List<AspectRatioData> aspectRatios = new List<AspectRatioData>();

        private const float tolerance = 0.1f; // 10% tolerance;

        private void Start()
        {
            AdjustObjectOnStart();
        }

        // Adjust the GameObject when the game starts
        public void AdjustObjectOnStart()
        {
            float currentAspect = (float)Screen.width / Screen.height;

            // Get the matching aspect ratio for this specific object
            AspectRatioData matchingAspect = GetMatchingAspectRatio(aspectRatios, currentAspect);

            // If a matching aspect ratio is found, apply the scale and/or position changes
            if (matchingAspect != null)
            {
                // Get the Transform component (either Transform or RectTransform)
                Transform transform = GetComponent<Transform>();
                RectTransform rectTransform = transform as RectTransform;

                // Adjust Scale if needed
                if (matchingAspect.adjustScale)
                {
                    transform.localScale = matchingAspect.scale;
                }

                // Adjust Position if needed
                if (matchingAspect.adjustPosition)
                {
                    if (rectTransform != null)
                    {
                        // It's a RectTransform, so adjust anchoredPosition
                        rectTransform.anchoredPosition = matchingAspect.position;
                    }
                    else
                    {
                        // It's a regular Transform, so adjust localPosition
                        transform.localPosition = matchingAspect.position;
                    }
                }
            }
            else
            {
                Debug.LogWarning("Aspect doesn't match for object: " + gameObject.name);
            }
        }

        // Find the matching aspect ratio based on the current screen aspect ratio
        private AspectRatioData GetMatchingAspectRatio(List<AspectRatioData> aspectRatios, float currentAspect)
        {
            foreach (AspectRatioData data in aspectRatios)
            {
                // Calculate the target aspect ratio
                float targetAspect = data.WidthRatio / data.HeightRatio;

                // Compare using Mathf for floating-point precision
                if (Mathf.Abs(currentAspect - targetAspect) < tolerance)
                {
                    return data;
                }
            }

            // Return null if no match is found
            return null;
        }
    }
}
