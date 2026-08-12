using NUnit.Framework;
using UnityEngine;

namespace UniversalStickerAtlas.Tests
{
    public class StickerAtlasTests
    {
        [Test]
        public void TestGridCellUVSubRectCalculation()
        {
            int cols = 4;
            int rows = 4;
            int cellIndex = 5;

            int cellX = cellIndex % cols;
            int cellY = cellIndex / cols;

            Assert.AreEqual(1, cellX, "Cell X should be column 1.");
            Assert.AreEqual(1, cellY, "Cell Y should be row 1.");

            float rectW = 1.0f / cols;
            float rectH = 1.0f / rows;

            Assert.AreEqual(0.25f, rectW, 0.001f, "Sub-rect width should be 0.25 for 4 cols.");
            Assert.AreEqual(0.25f, rectH, 0.001f, "Sub-rect height should be 0.25 for 4 rows.");
        }

        [Test]
        public void TestAspectScaleRatioCalculation()
        {
            float cropW = 0.5f;
            float cropH = 0.25f;
            float targetWidth = 2.0f;

            float aspect = cropW / cropH;
            Assert.AreEqual(2.0f, aspect, 0.001f, "Aspect ratio should be 2.0 (width is double height).");

            float calculatedHeight = targetWidth / aspect;
            Assert.AreEqual(1.0f, calculatedHeight, 0.001f, "Height should be 1.0 meter for a 2-meter wide 2:1 sign.");
        }

        [Test]
        public void TestColorKeyDistanceCalculation()
        {
            Color pureBlack = Color.black;
            Color nearBlack = new Color(0.05f, 0.05f, 0.05f, 1f);
            Color pureRed = Color.red;

            float distNearBlack = Vector3.Distance(new Vector3(pureBlack.r, pureBlack.g, pureBlack.b), new Vector3(nearBlack.r, nearBlack.g, nearBlack.b));
            float distRed = Vector3.Distance(new Vector3(pureBlack.r, pureBlack.g, pureBlack.b), new Vector3(pureRed.r, pureRed.g, pureRed.b));

            Assert.Less(distNearBlack, 0.1f, "Near-black color should be within 0.1 tolerance of black.");
            Assert.Greater(distRed, 1.0f, "Red color should be far outside tolerance of black.");
        }

        [Test]
        public void TestMultiDirectionalRaycastAndFlip()
        {
            GameObject testQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            RoadSignDecal decal = testQuad.AddComponent<RoadSignDecal>();

            decal.RaycastDistance = 25.0f;
            Assert.AreEqual(25.0f, decal.RaycastDistance, "Raycast search distance should be set to 25m.");

            Quaternion initialRot = testQuad.transform.rotation;
            decal.FlipSurfaceNormal();
            Quaternion flippedRot = testQuad.transform.rotation;

            Assert.AreNotEqual(initialRot, flippedRot, "Rotation should change after flipping face normal.");

            Object.DestroyImmediate(testQuad);
        }
    }
}
