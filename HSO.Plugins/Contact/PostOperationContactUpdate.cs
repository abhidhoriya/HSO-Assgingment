// <copyright file="PostOperationContactUpdate.cs" company="PlaceholderCompany">
// Copyright (c) PlaceholderCompany. All rights reserved.
// </copyright>

namespace HSO.Plugins
{
    using System;
    using System.Linq;
    using System.ServiceModel;
    using HSO.Plugins.Helper;
    using Microsoft.Xrm.Sdk;
    using Microsoft.Xrm.Sdk.Query;

    /// <summary>
    /// A PostOperation Plugin on ContactUpdate.
    /// </summary>
    public class PostOperationContactUpdate : PluginBase
    {
        /// <summary>
        /// unsecure config parameter.
        /// </summary>
        private readonly string unsecure;

        /// <summary>
        /// seccure config parameter.
        /// </summary>
        private readonly string secure;

        /// <summary>
        /// Initializes a new instance of the <see cref="PostOperationContactUpdate"/> class.
        /// </summary>
        /// <param name="unsecure">unsecure config parameter.</param>
        /// <param name="secure">secure config parameter.</param>
        public PostOperationContactUpdate(string unsecure, string secure)
            : base(typeof(PostOperationContactUpdate))
        {
            // TODO: Implement your custom configuration handling.
            this.unsecure = unsecure;
            this.secure = secure;
        }

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

                if (context.MessageName.ToLower() != Constants.Update.ToLower())
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
                    if (entity != null && entity.LogicalName != Contact.EntityLogicalName)
                    {
                        return;
                    }

                    if (entity.Attributes.Contains(Contact.Fields.ParentCustomerId))
                    {
                        EntityReference parentCustomerRef = ((EntityReference)entity[Contact.Fields.ParentCustomerId]).Id != Guid.Empty ? (EntityReference)entity[Contact.Fields.ParentCustomerId] : null;
                        if (parentCustomerRef != null)
                        {
                            ConditionExpression condition1 = new ConditionExpression
                            {
                                AttributeName = Contact.Fields.ParentCustomerId,
                                Operator = ConditionOperator.Equal,
                            };
                            condition1.Values.Add(parentCustomerRef.Id);

                            FilterExpression filter1 = new FilterExpression();
                            filter1.Conditions.Add(condition1);

                            QueryExpression query = new QueryExpression(Contact.EntityLogicalName);
                            query.ColumnSet.AddColumns(Contact.Fields.Id);
                            query.Criteria.AddFilter(filter1);
                            EntityCollection contactsCollection = callingUserService.RetrieveMultiple(query);

                            Account account = new Account
                            {
                                Id = parentCustomerRef.Id,
                                NumberOfEmployees = contactsCollection.Entities.Count,
                            };
                            callingUserService.Update(account);
                        }

                        if (!context.PreEntityImages.Contains(Constants.PreImage))
                        {
                            throw new InvalidPluginExecutionException("Plugin is Missing PreImage Registration");
                        }

                        var preImageEntity = (Entity)context.PreEntityImages["PreImage"];
                        EntityReference preParentCustomerRef = ((EntityReference)preImageEntity[Contact.Fields.ParentCustomerId]).Id != Guid.Empty ? (EntityReference)preImageEntity[Contact.Fields.ParentCustomerId] : null;

                        if (preParentCustomerRef != null)
                        {
                            ConditionExpression condition2 = new ConditionExpression
                            {
                                AttributeName = Contact.Fields.ParentCustomerId,
                                Operator = ConditionOperator.Equal,
                            };
                            condition2.Values.Add(preParentCustomerRef.Id);

                            FilterExpression filter2 = new FilterExpression();
                            filter2.Conditions.Add(condition2);

                            QueryExpression query2 = new QueryExpression(Contact.EntityLogicalName);
                            query2.ColumnSet.AddColumns(Contact.Fields.Id);
                            query2.Criteria.AddFilter(filter2);
                            EntityCollection contactsColl = callingUserService.RetrieveMultiple(query2);

                            Account previousAccount = new Account
                            {
                                Id = preParentCustomerRef.Id,
                                NumberOfEmployees = contactsColl.Entities.Count,
                            };
                            callingUserService.Update(previousAccount);
                        }
                    }
                }
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                throw new InvalidPluginExecutionException("An error occurred in PostOperationContactUpdate.", ex);
            }

            // Only throw an InvalidPluginExecutionException. Please Refer https://go.microsoft.com/fwlink/?linkid=2153829.
            catch (Exception ex)
            {
                tracingService?.Trace("An error occurred executing Plugin HSO.Plugins.PostOperationContactUpdate : {0}", ex.ToString());
                throw new InvalidPluginExecutionException("An error occurred executing Plugin HSO.Plugins.PostOperationContactUpdate .", ex);
            }
        }
    }
}
