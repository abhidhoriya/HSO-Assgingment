using HSO.Plugins.Helper;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk;
using System;
using System.Linq;
using System.ServiceModel;

namespace HSO.Plugins
{
    public class PostOperationContactCreate : PluginBase
    {
        public PostOperationContactCreate(string unsecure, string secure)
            : base(typeof(PostOperationContactCreate))
        {
            // TODO: Implement your custom configuration handling.
        }
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
                context.InputParameters[Constants.Target] is Entity)
                {
                    Entity entity = (Entity)context.InputParameters[Constants.Target];
                    if (entity != null && entity.LogicalName != Contact.EntityLogicalName)
                    {
                        return;
                    }

                    if (entity.Attributes.Contains(Contact.Fields.ParentCustomerId))
                    {
                        #region updateNewAccountEmployeeCount
                        EntityReference accountRef = ((EntityReference)entity[Contact.Fields.ParentCustomerId]).Id != Guid.Empty ? (EntityReference)entity[Contact.Fields.ParentCustomerId] : null;
                        if (accountRef != null)
                        {
                            ConditionExpression condition1 = new ConditionExpression();
                            condition1.AttributeName = Contact.Fields.ParentCustomerId;
                            condition1.Operator = ConditionOperator.Equal;
                            condition1.Values.Add(accountRef.Id);

                            FilterExpression filter1 = new FilterExpression();
                            filter1.Conditions.Add(condition1);

                            QueryExpression query = new QueryExpression(Contact.EntityLogicalName);
                            query.ColumnSet.AddColumns(Contact.Fields.Id);
                            query.Criteria.AddFilter(filter1);
                            EntityCollection contactsCollection = callingUserService.RetrieveMultiple(query);

                            Account account = new Account();
                            account.Id = accountRef.Id;
                            account.NumberOfEmployees = contactsCollection.Entities.Count;
                            callingUserService.Update(account);
                        }
                        #endregion updateNewAccountEmployeeCount
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
