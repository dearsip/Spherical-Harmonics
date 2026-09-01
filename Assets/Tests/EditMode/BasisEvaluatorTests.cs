using NUnit.Framework;
using UnityEngine;
using SphericalHarmonics.Math;
using SphericalHarmonics.Flow;
using SphericalHarmonics.Rotation;

namespace SphericalHarmonics.Tests
{
    public sealed class BasisEvaluatorTests
    {
        [Test]
        public void TableContainsAllSixteenDefinitions()
        {
            int count = 0;
            foreach (BasisDefinition ignored in BasisDefinitionTable.All) count++;
            Assert.AreEqual(16, count);
        }

        [TestCase(1, 1, 1, 0, 0, 0.4886025119029199)]
        [TestCase(1, -1, 0, 1, 0, 0.4886025119029199)]
        [TestCase(1, 0, 0, 0, 1, 0.4886025119029199)]
        [TestCase(2, 2, 1, 0, 0, 0.5462742152960396)]
        [TestCase(2, 0, 0, 0, 1, 0.6307831305050401)]
        [TestCase(3, 3, 1, 0, 0, 0.5900435899266435)]
        public void RealBasisMatchesConstants(int l, int m, float x, float y, float z, double expected)
        {
            Assert.That(SphericalHarmonicEvaluator.RealBasis(l, m, new Vector3(x,y,z)), Is.EqualTo(expected).Within(1e-10));
        }

        [Test]
        public void PositiveMIsCosineAndNegativeMIsSine()
        {
            float phi = 0.37f;
            Vector3 n = new Vector3(Mathf.Cos(phi), Mathf.Sin(phi), 0);
            for (int l = 1; l <= 3; l++)
            {
                for (int m = 1; m <= l; m++)
                {
                    double positive = SphericalHarmonicEvaluator.RealBasis(l, m, n);
                    double negative = SphericalHarmonicEvaluator.RealBasis(l, -m, n);
                    Assert.That(positive / Mathf.Cos(m * phi), Is.EqualTo(negative / Mathf.Sin(m * phi)).Within(1e-7), $"l={l},|m|={m}");
                    Assert.AreEqual(RealBasisType.Cosine,BasisDefinitionTable.Get(l,m).Type);
                    Assert.AreEqual(RealBasisType.Sine,BasisDefinitionTable.Get(l,-m).Type);
                }
            }
        }

        [Test]
        public void ComplexNegativeMObeysCondonShortleyRelation()
        {
            Vector3 n = new Vector3(0.31f, -0.47f, 0.826f).normalized;
            for (int l = 1; l <= 3; l++) for (int m = 1; m <= l; m++)
            {
                ComplexValue positive = SphericalHarmonicEvaluator.ComplexBasis(l, m, n);
                ComplexValue negative = SphericalHarmonicEvaluator.ComplexBasis(l, -m, n);
                double sign = (m & 1) == 0 ? 1 : -1;
                Assert.That(negative.Real, Is.EqualTo(sign * positive.Real).Within(1e-12));
                Assert.That(negative.Imaginary, Is.EqualTo(-sign * positive.Imaginary).Within(1e-12));
            }
        }

        [Test]
        public void AllSixteenRealEntriesMatchEmbeddedJsonPolynomials()
        {
            for(int l=0;l<=3;l++)for(int m=-l;m<=l;m++)for(int i=0;i<12;i++)
            {
                Vector3 n=RotationService.FibonacciDirection(i,12);
                Assert.That(SphericalHarmonicEvaluator.RealBasis(l,m,n),Is.EqualTo(FlowFieldBuilder.SolidHarmonic(l,m,n)).Within(2e-7),$"(l,m)=({l},{m}), sample={i}");
            }
        }
    }
}
