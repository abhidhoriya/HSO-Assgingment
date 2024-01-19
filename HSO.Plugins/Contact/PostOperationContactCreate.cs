// <copyright file="PostOperationContactCreate.cs" company="PlaceholderCompany">
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
    /// A Post Operation plugin on Contact Create.
    /// </summary>
    public class PostOperationContactCreate : PluginBase
    {
        /// <summary>
        /// unsecure config parameter.
        /// </summary>
        private readonly string unSecureConfigParam;

        /// <summary>
        /// seccure config parameter.
        /// </summary>
        private readonly string secureConfigParam;

        /// <summary>
        /// Initializes a new instance of the <see cref="PostOperationContactCreate"/> class.
        /// </summary>
        /// <param name="unsecure">unsecure config parameter.</param>
        /// <param name="secure">secure config parameter.</param>
        public PostOperationContactCreate(string unsecure, string secure)
            : base(typeof(PostOperationContactCreate))
        {
            // TODO: Implement your custom configuration handling.
            this.unSecureConfigParam = unsecure;
            this.secureConfigParam = secure;
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

                if (context.MessageName.ToLower() != Constants.Create.ToLower())
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
                        EntityReference accountRef = ((EntityReference)entity[Contact.Fields.ParentCustomerId]).Id != Guid.Empty ? (EntityReference)entity[Contact.Fields.ParentCustomerId] : null;
                        if (accountRef != null)
                        {
                            ConditionExpression condition1 = new ConditionExpression
                            {
                                AttributeName = Contact.Fields.ParentCustomerId,
                                Operator = ConditionOperator.Equal,
                            };
                            condition1.Values.Add(accountRef.Id);

                            FilterExpression filter1 = new FilterExpression();
                            filter1.Conditions.Add(condition1);

                            QueryExpression query = new QueryExpression(Contact.EntityLogicalName);
                            query.ColumnSet.AddColumns(Contact.Fields.Id);
                            query.Criteria.AddFilter(filter1);
                            EntityCollection contactsCollection = callingUserService.RetrieveMultiple(query);

                            Account account = new Account
                            {
                                Id = accountRef.Id,
                                NumberOfEmployees = contactsCollection.Entities.Count,
                            };
                            callingUserService.Update(account);
                        }
                    }
                }
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                throw new InvalidPluginExecutionException("An error occurred in PostOperationContactCreate.", ex);
            }

            // Only throw an InvalidPluginExecutionException. Please Refer https://go.microsoft.com/fwlink/?linkid=2153829.
            catch (Exception ex)
            {
                tracingService?.Trace("An error occurred executing Plugin HSO.Plugins.PostOperationContactCreate : {0}", ex.ToString());
                throw new InvalidPluginExecutionException("An error occurred executing Plugin HSO.Plugins.PostOperationContactCreate .", ex);
            }
        }
    }
}
