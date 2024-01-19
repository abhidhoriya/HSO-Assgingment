using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.Collections.Generic;
using System.Linq;
using HSO.Plugins;

namespace HSO.UnitTests
{
    [TestClass]
    public class PostOperationContactCreateTests : BaseUnitTest
    {
        /// <summary>
        /// Service Provider Null Check
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(InvalidPluginExecutionException))]
        public void PostOperationContactCreateTest_ServiceNull()
        {
            PostOperationContactCreate plugin = new PostOperationContactCreate(null, null);
            plugin.Execute(null);
        }

        [TestMethod]
        public void TestPostOperationContactCreate_WhenConditionsMet_ShouldUpdateAccountEmployeeCount()
        {
            // Arrange
            var service = OrganizationService;

            var accountId = Guid.NewGuid();

            // Create a sample Contact record
            var contactId = Guid.NewGuid();
            var targetEntity = new Entity("contact")
            {
                Id = contactId
            };
            targetEntity["firstname"] = "John";
            targetEntity["lastname"] = "Doe";
            targetEntity["parentcustomerid"] = new EntityReference("account", accountId);

            // Set up the fake execution context with the created contact record
            ParameterCollection inputParameter = new ParameterCollection
            {
                { "Target", targetEntity }
            };
            PluginExecutionContext.InputParametersGet = () => inputParameter;
            
            PluginExecutionContext.InitiatingUserIdGet = () => Guid.NewGuid();
            PluginExecutionContext.OperationCreatedOnGet = () => DateTime.Now;
            PluginExecutionContext.CorrelationIdGet = () =>
            {
                return Guid.NewGuid();
            };
            
            // Set up the fake organization service to retrieve multiple contacts
            var contactsCollection = new EntityCollection(new List<Entity> { targetEntity });
            ServiceProvider.GetServiceType = (type) =>
            {
                if (type == typeof(IOrganizationService))
                {
                    return service;
                }
                return null;
            };
            OrganizationService.RetrieveMultipleQueryBase = (query) =>
            {
                var queryExpression = (QueryExpression)query;
                Assert.AreEqual(Contact.EntityLogicalName, queryExpression.EntityName);
                Assert.AreEqual(1, queryExpression.ColumnSet.Columns.Count);
                Assert.AreEqual(Contact.Fields.Id, queryExpression.ColumnSet.Columns.First());
                Assert.AreEqual(1, queryExpression.Criteria.Conditions.Count);
                Assert.AreEqual(Contact.Fields.ParentCustomerId, queryExpression.Criteria.Conditions[0].AttributeName);
                Assert.AreEqual(ConditionOperator.Equal, queryExpression.Criteria.Conditions[0].Operator);
                Assert.AreEqual(targetEntity.GetAttributeValue<EntityReference>(Contact.Fields.ParentCustomerId).Id, queryExpression.Criteria.Conditions[0].Values[0]);
                return contactsCollection;
            };

            // Act
            var plugin = new PostOperationContactCreate(null, null);
            plugin.Execute(ServiceProvider);

            // Assert
            // Add assertions based on the expected behavior of your plugin
            // For example, you might check if the NumberOfEmployees property of the related account is updated correctly.
            var updatedAccount = service.RetrieveStringGuidColumnSet("account", ((EntityReference)targetEntity["parentcustomerid"]).Id, new ColumnSet("numberofemployees"));
            Assert.AreEqual(1, updatedAccount.GetAttributeValue<int>("numberofemployees"));
        }
    }
}
