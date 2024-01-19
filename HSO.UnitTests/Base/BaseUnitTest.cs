using Microsoft.Xrm.Sdk.Fakes;
using Microsoft.Xrm.Sdk;
using System.Fakes;
using System;

namespace HSO.UnitTests
{
    /// <summary>
    /// UnitTestBase Class.
    /// </summary>
    public class BaseUnitTest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BaseUnitTest"/> class
        /// </summary>
        public BaseUnitTest()
        {
            ServiceProvider = new StubIServiceProvider();
            PluginExecutionContext = new StubIPluginExecutionContext();
            TracingService = new StubITracingService();
            OrganizationFactory = new StubIOrganizationServiceFactory();
            OrganizationService = new StubIOrganizationService();
            ExecutionContext = new StubIExecutionContext();

            PluginExecutionContext.UserIdGet = () => Guid.Empty;
            OrganizationFactory.CreateOrganizationServiceNullableOfGuid = (userId) => OrganizationService;

            ServiceProvider.GetServiceType =
                (type) =>
                {
                    if (type == typeof(IPluginExecutionContext))
                    {
                        return PluginExecutionContext;
                    }

                    if (type == typeof(IOrganizationServiceFactory))
                    {
                        return OrganizationFactory;
                    }

                    if (type == typeof(ITracingService))
                    {
                        return TracingService;
                    }

                    if (type == typeof(IExecutionContext))
                    {
                        return ExecutionContext;
                    }

                    return null;
                };
        }

        /// <summary>
        /// Gets or Sets PluginExecutionContext Attribute.
        /// </summary>
        protected static StubIPluginExecutionContext PluginExecutionContext { get; set; }

        /// <summary>
        /// Gets or Sets TracingService Attribute.
        /// </summary>
        protected static StubITracingService TracingService { get; set; }

        /// <summary>
        /// Gets or Sets OrganizationFactory Attribute.
        /// </summary>
        protected static StubIOrganizationServiceFactory OrganizationFactory { get; set; }

        /// <summary>
        /// Gets or Sets OrganizationService Attribute.
        /// </summary>
        protected static StubIOrganizationService OrganizationService { get; set; }

        /// <summary>
        /// Gets or Sets ServiceProvider Attribute.
        /// </summary>
        protected static StubIServiceProvider ServiceProvider { get; set; }

        /// <summary>
        /// Gets or Sets ExecutionContext Attribute.
        /// </summary>
        protected static StubIExecutionContext ExecutionContext { get; set; }
    }
}
