using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System.Collections.Generic;
using HSO.Plugins;

namespace HSO.UnitTests
{
    [TestClass]
    public class PostOperationAccountUpdateTests : BaseUnitTest
    {
        [TestMethod]
        public void TestPostOperationAccountUpdate_WhenConditionsMet_ShouldUpdateRelatedContacts()
        {
            // Arrange
            var service = OrganizationService;

            // Create a sample Account record
            var accountId = Guid.NewGuid();
            var account = new Entity("account");
            account.Id = accountId;
            account["name"] = "Test Account";
            account["address1_line1"] = "123 Main Street";
            account["address1_line2"] = "Suite 456";
            account["address1_line3"] = "Building C";
            account["address1_city"] = "City";
            account["address1_postalcode"] = "12345";
            account["address1_country"] = "Country";

            // Create a sample Contact record related to the Account
            var contactId = Guid.NewGuid();
            Entity contact = new Entity("contact")
            {
                Id = contactId
            };
            contact["parentcustomerid"] = new EntityReference("account", accountId);

            // Set up the fake execution context with the created Account and Contact records
            ParameterCollection inputParameter = new ParameterCollection
            {
                { "Target", account }
            };

            PluginExecutionContext.InputParametersGet = () => inputParameter;
            PluginExecutionContext.PostEntityImagesGet = () =>
            {
                EntityImageCollection collection = new EntityImageCollection
                {
                    { "PostImage", account }
                };
                return collection;
            };

            PluginExecutionContext.InitiatingUserIdGet = () => Guid.NewGuid();
            PluginExecutionContext.OperationCreatedOnGet = () => DateTime.Now;

            PluginExecutionContext.CorrelationIdGet = () =>
            {
                return Guid.NewGuid();
            };
            // Set up the fake organization service to retrieve multiple contacts
            var contactsCollection = new EntityCollection(new List<Entity> { contact });
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
                Assert.AreEqual(6, queryExpression.ColumnSet.Columns.Count); // Make sure all address fields are retrieved
                Assert.AreEqual(1, queryExpression.Criteria.Conditions.Count);
                Assert.AreEqual(Contact.Fields.ParentCustomerId, queryExpression.Criteria.Conditions[0].AttributeName);
                Assert.AreEqual(ConditionOperator.Equal, queryExpression.Criteria.Conditions[0].Operator);
                Assert.AreEqual(accountId, queryExpression.Criteria.Conditions[0].Values[0]);
                return contactsCollection;
            };

            // Act
            var plugin = new PostOperationAccountUpdate(null, null);
            plugin.Execute(ServiceProvider);

            // Assert
            // Add assertions based on the expected behavior of your plugin
            // For example, you might check if the Address3 fields of the related contact are updated correctly.
            var updatedContact = service.RetrieveStringGuidColumnSet("contact", contactId, new ColumnSet(Contact.Fields.Address3_Line1, Contact.Fields.Address3_Line2, Contact.Fields.Address3_Line3, Contact.Fields.Address3_City, Contact.Fields.Address3_PostalCode, Contact.Fields.Address3_Country));
            Assert.AreEqual(account.GetAttributeValue<string>("address1_line1"), updatedContact.GetAttributeValue<string>(Contact.Fields.Address3_Line1));
            Assert.AreEqual(account.GetAttributeValue<string>("address1_line2"), updatedContact.GetAttributeValue<string>(Contact.Fields.Address3_Line2));
            Assert.AreEqual(account.GetAttributeValue<string>("address1_line3"), updatedContact.GetAttributeValue<string>(Contact.Fields.Address3_Line3));
            Assert.AreEqual(account.GetAttributeValue<string>("address1_city"), updatedContact.GetAttributeValue<string>(Contact.Fields.Address3_City));
            Assert.AreEqual(account.GetAttributeValue<string>("address1_postalcode"), updatedContact.GetAttributeValue<string>(Contact.Fields.Address3_PostalCode));
            Assert.AreEqual(account.GetAttributeValue<string>("address1_country"), updatedContact.GetAttributeValue<string>(Contact.Fields.Address3_Country));
        }
    }
}
