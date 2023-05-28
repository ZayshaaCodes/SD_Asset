namespace DefaultNamespace
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class PiApproximation : MonoBehaviour
    {
        // The number of points to generate
        public int numPoints = 1000;

        // The radius of the inscribed circle
        public float radius = 1.0f;

        // The number of points that fall within the circle
        private int numInside = 0;

        void Start()
        {
            // Generate the random points
            for (int i = 0; i < numPoints; i++)
            {
                // Generate a random point within the square
                float x = Random.Range(-radius, radius);
                float y = Random.Range(-radius, radius);

                // Check if the point falls within the circle
                if (x * x + y * y <= radius * radius)
                {
                    numInside++;
                }
            }

            // Calculate the approximate value of pi
            float pi = 4.0f * numInside / numPoints;

            // Display the result on the screen
            Debug.Log("Approximate value of pi: " + pi);
        }
    }
}