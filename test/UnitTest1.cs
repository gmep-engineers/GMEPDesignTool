using System;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GMEPDesignTool.Database;

namespace test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public async Task TestGetPlumbingFixtures()
        {
            string testProjectId = "0";
            string connectionString = "server=gmep-design-tool-test.ch8c88cauy2x.us-west-1.rds.amazonaws.com;port=3306;user id=admin;password=f51Vu3o5Qlc140r;database=gmep-design-tool;pooling=false";

            var db = new GMEPDesignTool.Database.Database(connectionString);
            var result = await db.GetPlumbingFixturesByProjectId(testProjectId);

            Assert.IsNotNull(result);

            foreach (var fixture in result)
            {
                Console.WriteLine($"{fixture.Name} - {fixture.Description} - {fixture.Make} {fixture.Model}");
            }
        }
    }
}
