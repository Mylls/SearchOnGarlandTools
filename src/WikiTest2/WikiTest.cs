using Microsoft.VisualStudio.TestTools.UnitTesting;
using PriceCheck;

namespace WikiTest2
{
    [TestClass]
    public class WikiTest
    {
        [TestMethod]
        public void TestWiki()
        {
            var url = "https://www.garlandtools.org/db/#item/17462";
            System.Diagnostics.Process.Start("explorer",url);

        }
    }
}
