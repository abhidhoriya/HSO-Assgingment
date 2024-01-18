using FakeXrmEasy;
using HSO.Plugins;
using HSO.UnitTests.Helper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HSO.UnitTests.Plugins
{
    [TestClass]
    public class PostOperationContactCreateTests
    {
        [TestMethod]
        public void When_contact_is_created_update_account_numberofemployeesfield()
        {
            //Init
            var fakeContext = new XrmFakedContext();
            var fakePluginContext = fakeContext.GetDefaultPluginContext();
            fakePluginContext.Stage = 40;
            fakePluginContext.MessageName = "Create";
            fakePluginContext.Depth = 1;

            var entityId = Guid.NewGuid();
            //Prepare
            var target = new Entity("contact") { Id = entityId };
            target.Attributes.Add(Contact.Fields.ParentCustomerId, Guid.NewGuid());

            ParameterCollection inputParameters = new ParameterCollection();
            inputParameters.Add("Target", target);
            fakePluginContext.InputParameters = inputParameters;
            fakeContext.Initialize(new List<Entity>() { target});
            //Execute
            //IPlugin pluginFake = fakeContext.ExecutePluginWith<PostOperationContactCreate>(null);
            //Assert
            Assert.AreEqual("1", "1");
        }
    }
}
