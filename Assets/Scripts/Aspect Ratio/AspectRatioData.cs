using System.Collections.Generic;
using UnityEngine;

namespace BazarEsKrim
{
    // Class to define aspect ratio and corresponding scale/position
    [System.Serializable]
    public class AspectRatioData
    {
        public AspectRatio aspectRatio; // The aspect ratio
        public Vector3 scale;
        public Vector3 position;
        public bool adjustScale; // Control if scale should be adjusted for this aspect ratio
        public bool adjustPosition; // Control if position should be adjusted for this aspect ratio

        // Dictionary to map aspect ratios to their corresponding width and height values
        private static readonly Dictionary<AspectRatio, (float width, float height)> aspectRatioValues = new Dictionary<AspectRatio, (float width, float height)>
                {
                    { AspectRatio.Aspect22_9, (22f, 9f) },
                    { AspectRatio.Aspect21_9, (21f, 9f) },
                    { AspectRatio.Aspect20_9, (20f, 9f) },
                    { AspectRatio.Aspect19_9, (19f, 9f) },
                    { AspectRatio.Aspect18_9, (18f, 9f) },
                    { AspectRatio.Aspect16_9, (16f, 9f) },
                    { AspectRatio.Aspect16_10, (16f, 10f) },
                    { AspectRatio.Aspect5_3, (5f, 3f) },
                    // Add more aspect ratios as needed
                };

        // Property to calculate the width and height of the aspect ratio
        public float WidthRatio => aspectRatioValues[aspectRatio].width;
        public float HeightRatio => aspectRatioValues[aspectRatio].height;
    }
}
