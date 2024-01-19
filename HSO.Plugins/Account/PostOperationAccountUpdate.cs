// <copyright file="PostOperationAccountUpdate.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace HSO.Plugins
{
    using System;
    using System.ServiceModel;
    using HSO.Plugins.Helper;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// A Postoperation plugin triggers on Account Update.
    /// </summary>
    public class PostOperationAccountUpdate : PluginBase
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PostOperationAccountUpdate"/> class.
        /// </summary>
        /// <param name="unsecure">Unsecured config parameter.</param>
        /// <param name="secure">Secured config parameter.</param>
        public PostOperationAccountUpdate(string unsecure, string secure)
            : base(typeof(PostOperationAccountUpdate))
        {
            // TODO: Implement your custom configuration handling.
            this.Unsecure = unsecure;
            this.Secure = secure;
        }

        /// <summary>
        /// Gets unsecure Config Param Property.
        /// </summary>
        public string Unsecure { get; }

        /// <summary>
        /// Gets secure Config Param Property.
        /// </summary>
        public string Secure { get; }

        /// <inheritdoc/>
        protected override void ExecuteCdsPlugin(ILocalPluginContext localContext)
        {
            if (localContext == null)
            {
                throw new InvalidPluginExecutionException(nameof(localContext));
            }

            // Obtain the tracing service
            ITracingService tracingService = localContext.TracingService;

            try
            {
                // Obtain the execution context from the service provider.
                IPluginExecutionContext context = (IPluginExecutionContext)localContext.PluginExecutionContext;
                if (context.Stage != 40)
                {
                    return;
                }

                if (string.Compare(context.MessageName, Constants.Update, StringComparison.OrdinalIgnoreCase) != 0)
                {
                    return;
                }

                if (context.Depth > 1)
                {
                    return;
                }

                // Obtain the organization service reference for web service calls.
                IOrganizationService callingUserService = localContext.CallingUserService;

                // TODO: Implement your custom Plug-in business logic.
                if (context.InputParameters.Contains(Constants.Target) &&
                    context.InputParameters[Constants.Target] is Entity targetEntity)
                {
                    Entity entity = targetEntity;
                    if (entity != null && entity.LogicalName != Account.EntityLogicalName)
                    {
                        return;
                    }

                    if (!context.PostEntityImages.Contains(Constants.PostImage))
                    {
                        throw new InvalidPluginExecutionException("Plugin is Missing PostImage Registration");
                    }

                    var postImageEntity = (Entity)context.PostEntityImages[Constants.PostImage];
                    var accAddressLine1 = postImageEntity.GetAttributeValue<string>(Account.Fields.Address1_Line1) ?? string.Empty;
                    var accAddressLine2 = postImageEntity.GetAttributeValue<string>(Account.Fields.Address1_Line2) ?? string.Empty;
                    var accAddressLine3 = postImageEntity.GetAttributeValue<string>(Account.Fields.Address1_Line3) ?? string.Empty;
                    var accAddressCity = postImageEntity.GetAttributeValue<string>(Account.Fields.Address1_City) ?? string.Empty;
                    var accAddressPostalCode = postImageEntity.GetAttributeValue<string>(Account.Fields.Address1_PostalCode) ?? string.Empty;
                    var accAddressCountry = postImageEntity.GetAttributeValue<string>(Account.Fields.Address1_Country) ?? string.Empty;

                    var query = new QueryExpression(Contact.EntityLogicalName)
                    {
                        ColumnSet = new ColumnSet(
                            Contact.Fields.Address3_Line1,
                            Contact.Fields.Address3_Line2,
                            Contact.Fields.Address3_Line3,
                            Contact.Fields.Address3_City,
                            Contact.Fields.Address3_PostalCode,
                            Contact.Fields.Address3_Country),
                        Criteria = new FilterExpression()
                        {
                            Conditions =
                        {
                            new ConditionExpression(Contact.Fields.ParentCustomerId, ConditionOperator.Equal, entity.Id),
                        },
                        },
                    };

                    EntityCollection result = callingUserService.RetrieveMultiple(query);

                    if (result.Entities.Count > 0)
                    {
                        foreach (var contact in result.Entities)
                        {
                            var updatedContact = new Contact
                            {
                                Id = contact.Id,
                                Address3_Line1 = accAddressLine1,
                                Address3_Line2 = accAddressLine2,
                                Address3_Line3 = accAddressLine3,
                                Address3_City = accAddressCity,
                                Address3_PostalCode = accAddressPostalCode,
                                Address3_Country = accAddressCountry,
                            };

                            callingUserService.Update(updatedContact);
                        }
                    }
                }
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                throw new InvalidPluginExecutionException("An error occurred in PostOperationAccountUpdate.", ex);
            }

            // Only throw an InvalidPluginExecutionException. Please Refer https://go.microsoft.com/fwlink/?linkid=2153829.
            catch (Exception ex)
            {
                tracingService?.Trace("An error occurred executing Plugin HSO.Plugins.PostOperationAccountUpdate : {0}", ex.ToString());
                throw new InvalidPluginExecutionException("An error occurred executing Plugin HSO.Plugins.PostOperationAccountUpdate .", ex);
            }
        }
    }
}
