# HSO Assgingment
This repository contains the solution for the HSO Assignment, addressing various requirements related to Dynamics 365 CE and Power Platform. The solution is organized into three projects:

HSO.CrmPackage: A CRM Package project requiring the Power Platform Tools Extension in Visual Studio 2022 and MS Build Developer packs for deployments.
HSO.Plugins: A C# class library containing all plugins for Dataverse.
HSO.UnitTests: A project dedicated to unit tests for the plugins in HSO.Plugins.

Project Structure:
HSO.CrmPackage
CRM Package project for Dynamics 365.
Requires Power Platform Tools Extension in Visual Studio 2022 and MS Build Developer packs for deployments.

HSO.Plugins
C# class library containing plugins for Dataverse.
Custom server-side validation plugin for IBAN validation.
Plugins for total employees update and service bus message triggers.

HSO.UnitTests
Unit tests project for testing the plugins in HSO.Plugins.

Usage:
Clone the repository.
Open the solution in Visual Studio 2022.
Build and deploy the projects as needed.
Refer to the individual project README files for specific instructions.

Note:
Ensure proper permissions and licenses for Dynamics 365, Azure Logic Apps, and Power BI.
Before deployment, thoroughly test in a non-production environment.
Feel free to reach out for any questions or clarifications related to the solution.

Happy Coding!
