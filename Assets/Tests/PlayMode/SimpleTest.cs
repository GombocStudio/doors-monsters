using NUnit.Framework;
public class SimpleTest
{
    [Test]
    public void SimpleTestSimplePasses()
    {
        Assert.AreEqual(2 + 2, 4);
    }
}