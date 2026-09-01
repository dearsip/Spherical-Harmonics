using UnityEngine;

namespace SphericalHarmonics.Rendering
{
    public sealed class SphereMeshData
    {
        public Vector3[] Directions;
        public int[] Triangles;
        public int[] LineIndices;
    }

    public static class SphereMeshGenerator
    {
        public static SphereMeshData Generate(int latitudeSegments, int longitudeSegments)
        {
            latitudeSegments = Mathf.Max(4, latitudeSegments);
            longitudeSegments = Mathf.Max(8, longitudeSegments);
            int columns = longitudeSegments + 1;
            var directions = new Vector3[(latitudeSegments + 1) * columns];
            for (int lat = 0; lat <= latitudeSegments; lat++)
            {
                float theta = Mathf.PI * lat / latitudeSegments;
                float sin = Mathf.Sin(theta), cos = Mathf.Cos(theta);
                for (int lon = 0; lon <= longitudeSegments; lon++)
                {
                    float phi = 2f * Mathf.PI * lon / longitudeSegments;
                    directions[lat * columns + lon] = new Vector3(sin * Mathf.Cos(phi), sin * Mathf.Sin(phi), cos);
                }
            }
            var triangles = new int[latitudeSegments * longitudeSegments * 6];
            int t = 0;
            for (int lat = 0; lat < latitudeSegments; lat++) for (int lon = 0; lon < longitudeSegments; lon++)
            {
                int a = lat * columns + lon, b = a + columns;
                triangles[t++] = a; triangles[t++] = b; triangles[t++] = a + 1;
                triangles[t++] = a + 1; triangles[t++] = b; triangles[t++] = b + 1;
            }
            var lines = new int[latitudeSegments * longitudeSegments * 4];
            int e = 0;
            for (int lat = 0; lat < latitudeSegments; lat++) for (int lon = 0; lon < longitudeSegments; lon++)
            {
                int a = lat * columns + lon;
                lines[e++] = a; lines[e++] = a + 1;
                lines[e++] = a; lines[e++] = a + columns;
            }
            return new SphereMeshData { Directions = directions, Triangles = triangles, LineIndices = lines };
        }
    }
}
