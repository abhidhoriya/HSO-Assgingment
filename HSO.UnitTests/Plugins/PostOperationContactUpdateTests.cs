// <copyright file="PostOperationContactUpdateTests.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace HSO.UnitTests
{
    using System;
    using System.Collections.Generic;
    using HSO.Plugins;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// PostOperationContactUpdateTests Unit Test Class.
    /// </summary>
    [TestClass]
    public class PostOperationContactUpdateTests : BaseUnitTest
    {
        /// <summary>
        /// TestPostOperationContactUpdate WhenConditionsMet_ShouldUpdateAccountEmployeeCount.
        /// </summary>
        [TestMethod]
        public void TestPostOperationContactUpdate_WhenConditionsMet_ShouldUpdateAccountEmployeeCount()
        {
            // Arrange
            var service = OrganizationService;

            // Create a sample Account record
            var accountId = Guid.NewGuid();
            var account = new Entity("account");
            account.Id = accountId;
            account["name"] = "Test Account";

            // Create a sample Contact record related to the Account
            var contactId = Guid.NewGuid();
            var contact = new Entity("contact")
            {
                Id = contactId,
            };
            contact["parentcustomerid"] = new EntityReference("account", accountId);

            // Set up the fake execution context with the created Contact and PreImage
            ParameterCollection inputParameter = new ParameterCollection
            {
                { "Target", contact },
            };

            PluginExecutionContext.InputParametersGet = () => inputParameter;
            PluginExecutionContext.PreEntityImagesGet = () =>
            {
                EntityImageCollection collection = new EntityImageCollection
                {
                    { "PreImage", contact },
                };
                return collection;
            };

            PluginExecutionContext.InitiatingUserIdGet = () => Guid.NewGuid();
            PluginExecutionContext.OperationCreatedOnGet = () => DateTime.Now;

            PluginExecutionContext.CorrelationIdGet = () =>
            {
                return Guid.NewGuid();
            };

            // Set up the fake organization service to retrieve multiple contacts for both current and previous parent customer
            var contactsCollection = new EntityCollection(new List<Entity> { contact });

            OrganizationService.RetrieveMultipleQueryBase = (query) =>
            {
                var queryExpression = (QueryExpression)query;
                Assert.AreEqual(Contact.EntityLogicalName, queryExpression.EntityName);
                Assert.AreEqual(1, queryExpression.ColumnSet.Columns.Count);
                Assert.AreEqual(1, queryExpression.Criteria.Conditions.Count);
                Assert.AreEqual(Contact.Fields.ParentCustomerId, queryExpression.Criteria.Conditions[0].AttributeName);
                Assert.AreEqual(ConditionOperator.Equal, queryExpression.Criteria.Conditions[0].Operator);
                Assert.AreEqual(accountId, queryExpression.Criteria.Conditions[0].Values[0]);
                return contactsCollection;
            };

            // Act
            var plugin = new PostOperationContactUpdate(null, null);
            plugin.Execute(ServiceProvider);

            // Assert
            // Add assertions based on the expected behavior of your plugin
            // For example, you might check if the NumberOfEmployees field of the related account is updated correctly.
            var updatedAccount = OrganizationService.RetrieveStringGuidColumnSet("account", accountId, new ColumnSet("numberofemployees"));
            Assert.AreEqual(contactsCollection.Entities.Count, updatedAccount.GetAttributeValue<int>("numberofemployees"));
        }
    }
}
