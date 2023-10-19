# XRM Records Restorator
## Overview
XRM Records Restorator is a plug-in for XrmToolbox which gives a user ability to restore deleted record using audit history.
## Usage
### Pre-requisitions
You must have enabled audit in your Dataverse / Dynamics environment.
### UI / UX
- **Entity** - A dropdown that shows all the tables with enabled auditing;
- **Username** - A dropdown that shows all enabled users in the system;
- **Object** - A textfield for GUID of a deleted record;
- **From / To** - Two date fields to limit results by date of deletion.
- **Fetch Records** - A button that retrieves audit records according to select filters;
- **Restore Records** - Creates the records selected in the left grid view;
- **Grid view on the left side** shows records which were fetched from the system;
- **Grid view on the right shows** details of a record selected in the left grid.
