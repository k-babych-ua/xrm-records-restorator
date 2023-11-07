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
        private DateTime? FromDate = null, ToDate = null;

        private IEnumerable<EntityItem> AuditableEntities;
        private List<AuditItem> DeletedRecordsDataSource;

        private string _errorMessage;
        private Exception _exception;

        public MyPluginControl()
        {
            InitializeComponent();
        }

        private void MyPluginControl_Load(object sender, EventArgs e)
        {
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

            if (Service != null)
            {
                GetUsers();
                GetEntities();
            }
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
                    try
                    {
                        AuditableEntities = new MetadataRepository(Service)
                            .GetAuditableEntity()
                            .ToArray();
                    }
                    catch (Exception ex)
                    {
                        _errorMessage = $"Can't get auditable tables: {ex.Message}";
                        _exception = ex;
                    }
                },
                PostWorkCallBack = (args) =>
                {
                    var t = new EntityItem();

                    entitiesCb.DataSource = AuditableEntities;
                    entitiesCb.DisplayMember = nameof(t.Name);
                    entitiesCb.ValueMember = nameof(t.LogicalName);

                    entitiesCb.SelectedItem = null;
                    entitiesCb.SelectedText = "All";

                    HandleError();
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
                    try
                    {
                        args.Result = new UsersRepository(Service)
                            .GetEnabledUsers()
                            .ToArray();
                    }
                    catch (Exception ex)
                    {
                        _errorMessage = $"Can't get users information: {ex.Message}";
                        _exception = ex;
                    }
                },
                PostWorkCallBack = (args) =>
                {
                    var t = new User();

                    usersCb.DataSource = args.Result;
                    usersCb.DisplayMember = nameof(t.DisplayName);
                    usersCb.ValueMember = nameof(t.Id);

                    usersCb.SelectedItem = null;
                    usersCb.SelectedText = "All";

                    HandleError();
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
                        .ByOperation(3) //Delete
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

                    if (!string.IsNullOrWhiteSpace(objectTextBox.Text) && Guid.TryParse(objectTextBox.Text, out var objectId))
                    {
                        queryBuilder.ByObjectId(objectId);
                    }

                    if (FromDate.HasValue)
                    {
                        queryBuilder.ByCreatedOnGreaterEqual(FromDate.Value.ToUniversalTime());
                    }

                    if (ToDate.HasValue)
                    {
                        queryBuilder.ByCreatedOnLessEqual(ToDate.Value.ToUniversalTime());
                    }

                    try
                    {

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
                    }
                    catch (Exception ex)
                    {
                        _errorMessage = $"Can't fetch audit history records: {ex.Message}";
                        _exception = ex;
                    }
                },
                PostWorkCallBack = (args) =>
                {
                    deletedRecordGrid.DataSource = DeletedRecordsDataSource;

                    HandleError();
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

                    if (dataGridView?.CurrentRow == null || dataGridView.CurrentRow.Index == SelectedRow || 
                        dataGridView.CurrentRow.Index < 0)
                    {
                        args.Cancel = true;
                        return;
                    }

                    SelectedRow = dataGridView.CurrentRow.Index;

                    try
                    {
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
                    }
                    catch (Exception ex)
                    {
                        _errorMessage = $"Can't retrieve audit details: {ex.Message}";
                        _exception = ex;
                    }
                },
                PostWorkCallBack = (args) =>
                {
                    HandleError();

                    if (args.Cancelled)
                        return;

                    detailsDataGrid.DataSource = args.Result;
                }
            });
        }

        private void fromDateTimePicker_ValueChanged(object sender, EventArgs e)
        {
            if (fromDateTimePicker.Checked)
            {
                FromDate = fromDateTimePicker.Value;
            }
            else
            {
                FromDate = null;
            }
        }

        private void toDateTimePicker_ValueChanged(object sender, EventArgs e)
        {
            if (toDateTimePicker.Checked)
            {
                ToDate = toDateTimePicker.Value;
            }
            else
            {
                ToDate = null;
            }
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

                            try
                            {
                                Service.Create(recordToRestore);
                            }
                            catch (Exception ex)
                            {
                                _errorMessage = $"Can't create a record: {ex.Message}";
                                _exception = ex;
                            }
                        }
                    }
                },
                PostWorkCallBack = (args) =>
                {
                    HandleError();
                }
            });
        }

        private void HandleError()
        {
            if (!string.IsNullOrWhiteSpace(_errorMessage))
            {
                LogError(_errorMessage);
                ShowErrorDialog(_exception, "Error", _errorMessage);

                _errorMessage = null;
                _exception = null;
            }
        }
    }
}