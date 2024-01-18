using HSO.Plugins.Helper;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace HSO.Plugins
{
    public class PostOperationAccountUpdate : PluginBase
    {
        public PostOperationAccountUpdate(string unsecure, string secure)
            : base(typeof(PostOperationAccountUpdate))
        {

            // TODO: Implement your custom configuration handling.
        }

        /// <summary>
        /// Main entry point for he business logic that the plug-in is to execute.
        /// </summary>
        /// <param name="localContext">The <see cref="LocalPluginContext"/> which contains the
        /// <see cref="IPluginExecutionContext"/>,
        /// <see cref="IOrganizationService"/>
        /// and <see cref="ITracingService"/>
        /// </param>
        /// <remarks>
        /// </remarks>
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
                context.InputParameters[Constants.Target] is Entity)
                {
                    Entity entity = (Entity)context.InputParameters[Constants.Target];
                    if (entity != null && entity.LogicalName != Account.EntityLogicalName)
                    {
                        return;
                    }

                    if (!context.PostEntityImages.Contains(Constants.PostImage))
                    {
                        throw new InvalidPluginExecutionException("Plugin is Missing PostImage Registration");
                    }
                    #region postImageDetails
                    var postImageEntity = (Entity)context.PostEntityImages["PostImage"];
                    string acc_address1_line1 = postImageEntity.Attributes.Contains(Account.Fields.Address1_Line1) ? postImageEntity[Account.Fields.Address1_Line1].ToString() : string.Empty;
                    string acc_address1_line2 = postImageEntity.Attributes.Contains(Account.Fields.Address1_Line2) ? postImageEntity[Account.Fields.Address1_Line2].ToString() : string.Empty;
                    string acc_address1_line3 = postImageEntity.Attributes.Contains(Account.Fields.Address1_Line3) ? postImageEntity[Account.Fields.Address1_Line3].ToString() : string.Empty;
                    string acc_address1_city = postImageEntity.Attributes.Contains(Account.Fields.Address1_City) ? postImageEntity[Account.Fields.Address1_City].ToString() : string.Empty;
                    string acc_address1_postalcode = postImageEntity.Attributes.Contains(Account.Fields.Address1_PostalCode) ? postImageEntity[Account.Fields.Address1_PostalCode].ToString() : string.Empty;
                    string acc_address1_country = postImageEntity.Attributes.Contains(Account.Fields.Address1_Country) ? postImageEntity[Account.Fields.Address1_Country].ToString() : string.Empty;
                    #endregion postImageDetails
                    #region retrive&UpdateAllRelatedContacts
                    QueryExpression query = new QueryExpression(Contact.EntityLogicalName);
                    query.ColumnSet = new ColumnSet(Contact.Fields.Address3_Line1, Contact.Fields.Address3_Line2, Contact.Fields.Address3_Line3, Contact.Fields.Address3_City, Contact.Fields.Address3_PostalCode, Contact.Fields.Address3_Country);
                    query.Criteria = new FilterExpression();
                    query.Criteria.AddCondition(Contact.Fields.ParentCustomerId, ConditionOperator.Equal, entity.Id);
                    EntityCollection result = callingUserService.RetrieveMultiple(query);
                    if (result.Entities.Count > 0)
                    {
                        foreach (var contact in result.Entities)
                        {
                            Contact updatedContact = new Contact();
                            updatedContact.Id = contact.Id;
                            updatedContact.Address3_Line1 = acc_address1_line1;
                            updatedContact.Address3_Line2 = acc_address1_line2;
                            updatedContact.Address3_Line3 = acc_address1_line3;
                            updatedContact.Address3_City = acc_address1_city;
                            updatedContact.Address3_PostalCode = acc_address1_postalcode;
                            updatedContact.Address3_Country = acc_address1_country;
                            callingUserService.Update(updatedContact);
                        }
                    }
                    #endregion retrive&UpdateAllRelatedContacts
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
