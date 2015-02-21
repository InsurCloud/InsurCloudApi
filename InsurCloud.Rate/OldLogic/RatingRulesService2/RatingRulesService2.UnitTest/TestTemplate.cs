using NUnit.Framework;

namespace RatingRulesservice2.UnitTest
{
    //Example Test
    [TestFixture]
    public class TestTemplate
    {
        [Test]
        public void MethodName_MethodConditionOrState_MethodExpectedResult()
        {
            //Arrange
            int num1 = 1;
            int num2 = 1;
            int expected = 2;

            //Act
            var result = num1 + num2;

            //Assert
            Assert.IsTrue(expected == result);
        }
    }
}
