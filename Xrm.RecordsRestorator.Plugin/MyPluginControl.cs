using McTools.Xrm.Connection;
using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using Xrm.RecordsRestorator.Plugin.Builders;
using Xrm.RecordsRestorator.Plugin.Model;
using Xrm.RecordsRestorator.Plugin.Repositories;
using Xrm.RecordsRestorator.Plugin.Wrappers;
using XrmToolBox.Extensibility;

namespace Xrm.RecordsRestorator.Plugin
{
    public partial class MyPluginControl : PluginControlBase
    {
        private Settings mySettings;

        private Guid? SelectedUser = Guid.Empty;
        private string SelectedEntity = string.Empty;
        private int SelectedRow = -1;

        private IEnumerable<EntityItem> AuditableEntities;
        private List<AuditItem> DeletedRecordsDataSource;

        public MyPluginControl()
        {
            InitializeComponent();
        }

        private void MyPluginControl_Load(object sender, EventArgs e)
        {
            ShowInfoNotification("This is a notification that can lead to XrmToolBox repository", new Uri("https://github.com/MscrmTools/XrmToolBox"));

            // Loads or creates the settings for the plugin
            if (!SettingsManager.Instance.TryLoad(GetType(), out mySettings))
            {
                mySettings = new Settings();

                LogWarning("Settings not found => a new settings file has been created!");
            }
            else
            {
                LogInfo("Settings found and loaded");
            }

            GetUsers();
            GetEntities();
        }

        private void tsbClose_Click(object sender, EventArgs e)
        {
            CloseTool();
        }

        private void loadEntitiesTsb_Click(object sender, EventArgs e)
        {
            ExecuteMethod(GetEntities);
        }


        private void loadUsersTsb_Click(object sender, EventArgs e)
        {
            ExecuteMethod(GetUsers);
        }

        private void fetchRecordsBtn_Click(object sender, EventArgs e)
        {
            ExecuteMethod(GetRecords);
        }

        private void GetEntities()
        {
            WorkAsync(new WorkAsyncInfo()
            {
                Message = "Retrieving auditable entities",
                Work = (worker, args) =>
                {
                    AuditableEntities = new MetadataRepository(Service)
                        .GetAuditableEntity()
                        .ToArray();
                },
                PostWorkCallBack = (args) =>
                {
                    var t = new EntityItem();

                    entitiesCb.DataSource = AuditableEntities;
                    entitiesCb.DisplayMember = nameof(t.Name);
                    entitiesCb.ValueMember = nameof(t.LogicalName);

                    entitiesCb.SelectedItem = null;
                    entitiesCb.SelectedText = "All";
                }
            });
        }

        private void GetUsers()
        {
            WorkAsync(new WorkAsyncInfo()
            {
                Message = "Retrieving the users",
                Work = (worker, args) =>
                {
                    args.Result = new UsersRepository(Service)
                        .GetEnabledUsers()
                        .ToArray();
                },
                PostWorkCallBack = (args) =>
                {
                    var t = new User();

                    usersCb.DataSource = args.Result;
                    usersCb.DisplayMember = nameof(t.DisplayName);
                    usersCb.ValueMember = nameof(t.Id);

                    usersCb.SelectedItem = null;
                    usersCb.SelectedText = "All";
                }
            });
        }

        private void GetRecords()
        {
            WorkAsync(new WorkAsyncInfo()
            {
                Message = "Fetching the records",
                Work = (worker, args) =>
                {
                    AuditQueryBuilder queryBuilder = new AuditQueryBuilder()
                        .ByOperation(3)
                        .WithColumns("objectid", "auditid", "createdon", "objecttypecode", "userid")
                        .OrderBy("createdon", Microsoft.Xrm.Sdk.Query.OrderType.Descending)
                        .SetNoLock(true)
                        .SetPageSize(5000) as AuditQueryBuilder;

                    if (SelectedUser.HasValue && SelectedUser != Guid.Empty)
                    {
                        queryBuilder.ByUser(SelectedUser.Value);
                    }

                    if (!string.IsNullOrWhiteSpace(SelectedEntity))
                    {
                        queryBuilder.ByEntity(SelectedEntity);
                    }

                    DeletedRecordsDataSource = new AuditRepository(Service)
                        .RetrieveMultiple(queryBuilder.GetQuery())
                        .Select(x => new AuditItem()
                        {
                            Id = x.Id,
                            Entity = x.GetAttributeValue<string>("objecttypecode"),
                            ObjectId = new EntityReferenceWrapper(x.GetAttributeValue<EntityReference>("objectid")),
                            UserId = new EntityReferenceWrapper(x.GetAttributeValue<EntityReference>("userid")),
                            CreatedDate = x.GetAttributeValue<DateTime>("createdon")
                        })
                        .ToList();
                },
                PostWorkCallBack = (args) =>
                {
                    deletedRecordGrid.DataSource = DeletedRecordsDataSource;
                }
            });
        }

        /// <summary>
        /// This event occurs when the plugin is closed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MyPluginControl_OnCloseTool(object sender, EventArgs e)
        {
            // Before leaving, save the settings
            SettingsManager.Instance.Save(GetType(), mySettings);
        }

        /// <summary>
        /// This event occurs when the connection has been updated in XrmToolBox
        /// </summary>
        public override void UpdateConnection(IOrganizationService newService, ConnectionDetail detail, string actionName, object parameter)
        {
            base.UpdateConnection(newService, detail, actionName, parameter);

            if (mySettings != null && detail != null)
            {
                mySettings.LastUsedOrganizationWebappUrl = detail.WebApplicationUrl;
                LogInfo("Connection has changed to: {0}", detail.WebApplicationUrl);
            }
        }

        private void entitiesCb_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedEntity = (entitiesCb.SelectedItem as EntityItem)?.LogicalName;
        }

        private void usersCb_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedUser = (usersCb.SelectedItem as User)?.Id;
        }

        private void deletedRecordGrid_Click(object sender, EventArgs e)
        {
            WorkAsync(new WorkAsyncInfo()
            {
                Message = "Retrieving audit details",
                Work = (worker, args) =>
                {
                    DataGridView dataGridView = sender as DataGridView;

                    if (dataGridView.CurrentRow.Index == SelectedRow || dataGridView.CurrentRow.Index < 0)
                    {
                        args.Cancel = true;
                        return;
                    }

                    SelectedRow = dataGridView.CurrentRow.Index;

                    var response = (RetrieveAuditDetailsResponse)Service.Execute(new RetrieveAuditDetailsRequest()
                    {
                        AuditId = DeletedRecordsDataSource[SelectedRow].Id
                    });

                    if (response.Results.Count > 0 && response.Results.Values.FirstOrDefault() is AttributeAuditDetail auditDetails)
                    {
                        args.Result = auditDetails.OldValue.Attributes
                            .Select(attr => new AttributeWrapper(attr))
                            .ToArray();
                    }
                },
                PostWorkCallBack = (args) =>
                {
                    if (args.Cancelled)
                        return;

                    detailsDataGrid.DataSource = args.Result;
                }
            });
        }

        private void restoreRecordsButton_Click(object sender, EventArgs e)
        {
            WorkAsync(new WorkAsyncInfo()
            {
                Message = "Restoring selected records",
                Work = (worker, args) =>
                {
                    foreach (DataGridViewRow row in deletedRecordGrid.SelectedRows)
                    {
                        var response = (RetrieveAuditDetailsResponse)Service.Execute(new RetrieveAuditDetailsRequest()
                        {
                            AuditId = (row.DataBoundItem as AuditItem).Id
                        });

                        if (response.Results.Count > 0 && response.Results.Values.FirstOrDefault() is AttributeAuditDetail auditDetails)
                        {
                            string primaryKey = AuditableEntities
                                .FirstOrDefault(x => x.LogicalName == auditDetails.OldValue.LogicalName)
                                .PrimaryKey;

                            Entity recordToRestore = auditDetails.OldValue;
                            recordToRestore[primaryKey] = auditDetails.AuditRecord.GetAttributeValue<EntityReference>("objectid").Id;

                            Service.Create(recordToRestore);
                        }
                    }
                }
            });
        }
    }
}