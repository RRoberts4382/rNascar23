﻿using Microsoft.Extensions.Logging;
using rNascar23.DriverStatistics.Models;
using rNascar23.Flags.Models;
using rNascar23.LapTimes.Models;
using rNascar23.LiveFeeds.Models;
using rNascar23.RaceLists.Models;
using rNascar23TestApp.CustomViews;
using rNascar23TestApp.Properties;
using rNascar23TestApp.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace rNascar23TestApp.Dialogs
{
    public partial class GridSettingsDialog : Form
    {
        #region enums

        private enum ViewState
        {
            View,
            Edit,
            Add,
            Copy,
            Delete
        }

        #endregion

        #region fields

        private bool _loading = false;
        private DataGridViewColumn _selectedColumn = null;
        private ViewState _viewState = ViewState.View;
        private readonly ILogger<GridSettingsDialog> _logger = null;
        private readonly CustomViewSettingsService _settingsService = null;
        private GridSettings _selectedGridSettings = null;
        private IList<GridSettings> _customGridSettings = null;

        private IList<string> grids = new List<string>() { "foo", "bar", "baz" };


        #endregion

        #region ctor/load

        public GridSettingsDialog(ILogger<GridSettingsDialog> logger,
            CustomViewSettingsService settingsService)
        {
            InitializeComponent();

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        }

        private void GridSettingsDialog_Load(object sender, EventArgs e)
        {
            try
            {
                SetViewState(ViewState.View);

                PopulateSelectionControl();
            }
            catch (Exception ex)
            {
                ExceptionHandler(ex);
            }
        }

        #endregion

        #region private

        // private
        private void ExceptionHandler(Exception ex)
        {
            ExceptionHandler(ex, String.Empty, true);
        }
        private void ExceptionHandler(Exception ex, string message = "")
        {
            ExceptionHandler(ex, message, true);
        }
        private void ExceptionHandler(Exception ex, string message = "", bool logMessage = false)
        {
            MessageBox.Show(ex.Message);
            if (logMessage)
            {
                string errorMessage = String.IsNullOrEmpty(message) ? ex.Message : message;

                _logger.LogError(ex, errorMessage);
            }
        }

        // ui events
        private void btnTitleForeColor_Click(object sender, EventArgs e)
        {
            try
            {
                var dialog = new ColorDialog();

                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    txtGridTitle.ForeColor = dialog.Color;
                }
            }
            catch (Exception ex)
            {

                ExceptionHandler(ex);
            }
        }

        private void btnTitleBackColor_Click(object sender, EventArgs e)
        {
            try
            {
                var dialog = new ColorDialog();

                if (dialog.ShowDialog(this) == DialogResult.OK)
                {
                    txtGridTitle.BackColor = dialog.Color;
                }
            }
            catch (Exception ex)
            {

                ExceptionHandler(ex);
            }
        }

        private void dataGridView_SelectionChanged(object sender, EventArgs e)
        {
            if (dataGridView.SelectedColumns.Count == 0)
                return;
            if (_loading)
                return;

            _selectedColumn = dataGridView.SelectedColumns[0];

            _loading = true;

            DisplayColumnDetails(_selectedColumn);

            for (int i = lstFields.SelectedItems.Count; i > 0; i--)
            {
                lstFields.SetSelected(i, false);
            }

            lstFields.SetSelected(_selectedColumn.Index, true);

            _loading = false;
        }

        private void lstFields_SelectedValueChanged(object sender, EventArgs e)
        {
            if (_loading)
                return;

            _loading = true;

            dataGridView.ClearSelection();

            _selectedColumn = dataGridView.Columns[lstFields.SelectedIndex];

            DisplayColumnDetails(_selectedColumn);

            _loading = false;
        }

        private void chkColumnVisible_CheckedChanged(object sender, EventArgs e)
        {
            if (_selectedColumn == null)
                return;
            if (_loading)
                return;

            _selectedColumn.Visible = chkColumnVisible.Checked;
        }

        private void txtColumnWidth_Leave(object sender, EventArgs e)
        {
            if (_selectedColumn == null)
                return;

            _selectedColumn.Width = int.Parse(txtColumnWidth.Text);
        }

        private void txtColumnWidth_KeyUp(object sender, KeyEventArgs e)
        {
            if (_selectedColumn == null)
                return;

            if (e.KeyCode == Keys.Enter)
            {
                _selectedColumn.Width = int.Parse(txtColumnWidth.Text);
            }
        }

        private void txtColumnHeader_KeyUp(object sender, KeyEventArgs e)
        {
            if (_selectedColumn == null)
                return;

            if (e.KeyCode == Keys.Enter)
            {
                _selectedColumn.HeaderText = txtColumnHeader.Text;
            }
        }

        private void txtColumnHeader_Leave(object sender, EventArgs e)
        {
            if (_selectedColumn == null)
                return;

            _selectedColumn.HeaderText = txtColumnHeader.Text;
        }

        private void dataGridView_ColumnWidthChanged(object sender, DataGridViewColumnEventArgs e)
        {
            if (_selectedColumn == null)
                return;

            if (e.Column.Index == _selectedColumn.Index)
            {
                txtColumnWidth.Text = e.Column.Width.ToString();
            }
        }

        private void txtDisplayIndex_KeyUp(object sender, KeyEventArgs e)
        {
            if (_selectedColumn == null)
                return;

            if (e.KeyCode == Keys.Enter)
            {
                _selectedColumn.DisplayIndex = int.Parse(txtDisplayIndex.Text);
            }
        }

        private void txtDisplayIndex_Leave(object sender, EventArgs e)
        {
            if (_selectedColumn == null)
                return;

            _selectedColumn.DisplayIndex = int.Parse(txtDisplayIndex.Text);
        }

        // ui logic
        private void SetViewState(ViewState newViewState)
        {
            switch (newViewState)
            {
                case ViewState.View:
                    btnEditSave.Text = "Edit";
                    toolTip1.SetToolTip(btnEditSave, "Allows you to edit the selected grid.");
                    btnEditSave.Enabled = (cboGridSelection.SelectedItem != null);
                    btnCancelClose.Text = "Close";
                    toolTip1.SetToolTip(btnCancelClose, "Closes this form.");
                    btnCancelClose.Enabled = true;
                    btnNew.Enabled = true;
                    btnCopy.Enabled = (cboGridSelection.SelectedItem != null);
                    btnDelete.Enabled = (cboGridSelection.SelectedItem != null);
                    pnlSelector.Enabled = true;
                    pnlGridSettings.Enabled = false;
                    splitContainer1.Enabled = false;
                    break;
                case ViewState.Edit:
                    btnEditSave.Text = "Save";
                    toolTip1.SetToolTip(btnEditSave, "Saves the changes to the grid being edited.");
                    btnEditSave.Enabled = true;
                    btnCancelClose.Text = "Cancel";
                    toolTip1.SetToolTip(btnCancelClose, "Cancels the edit that is in progress, discarding any changes.");
                    btnCancelClose.Enabled = true;
                    btnNew.Enabled = false;
                    btnCopy.Enabled = false;
                    btnDelete.Enabled = false;
                    pnlSelector.Enabled = false;
                    pnlGridSettings.Enabled = true;
                    splitContainer1.Enabled = true;
                    break;
                case ViewState.Add:
                    btnEditSave.Text = "Save";
                    toolTip1.SetToolTip(btnEditSave, "Saves the changes to the grid being added.");
                    btnEditSave.Enabled = true;
                    btnCancelClose.Text = "Cancel";
                    toolTip1.SetToolTip(btnCancelClose, "Cancels the edit that is in progress, without adding the new grid.");
                    btnCancelClose.Enabled = true;
                    btnNew.Enabled = false;
                    btnCopy.Enabled = false;
                    btnDelete.Enabled = false;
                    pnlSelector.Enabled = false;
                    pnlGridSettings.Enabled = true;
                    splitContainer1.Enabled = true;
                    break;
                case ViewState.Copy:
                    toolTip1.SetToolTip(btnEditSave, "Saves the changes to the grid being edited.");
                    btnEditSave.Text = "Save";
                    btnEditSave.Enabled = true;
                    toolTip1.SetToolTip(btnCancelClose, "Cancels the edit that is in progress, without adding the copied grid.");
                    btnCancelClose.Text = "Cancel";
                    btnCancelClose.Enabled = true;
                    btnNew.Enabled = false;
                    btnCopy.Enabled = false;
                    btnDelete.Enabled = false;
                    pnlSelector.Enabled = false;
                    pnlGridSettings.Enabled = true;
                    splitContainer1.Enabled = true;
                    break;
                case ViewState.Delete:
                    btnEditSave.Text = "Edit";
                    btnEditSave.Enabled = false;
                    btnCancelClose.Text = "Close";
                    btnCancelClose.Enabled = false;
                    btnNew.Enabled = false;
                    btnCopy.Enabled = false;
                    btnDelete.Enabled = false;
                    pnlSelector.Enabled = false;
                    pnlGridSettings.Enabled = false;
                    splitContainer1.Enabled = false;
                    break;
                default:
                    break;
            }

            _viewState = newViewState;
        }

        private void DisplayColumnDetails(DataGridViewColumn column)
        {
            DisplayColumnDetails(column, column.Index);
        }
        private void DisplayColumnDetails(DataGridViewColumn column, int index)
        {
            if (column == null)
                return;

            txtDataPropertyName.Text = column.DataPropertyName;
            txtColumnHeader.Text = column.HeaderText;
            txtColumnWidth.Text = column.Width.ToString();
            chkColumnVisible.Checked = column.Visible;
            txtDisplayIndex.Text = column.DisplayIndex.ToString();
            lblIndex.Text = column.Index.ToString();
        }

        // dialog buttons
        private void btnCancelClose_Click(object sender, EventArgs e)
        {
            switch (_viewState)
            {
                case ViewState.View: // ButtonText = "Close"
                    PerformCloseDialog();
                    break;
                case ViewState.Edit: // ButtonText = "Cancel"
                    PerformCancelEdit();
                    break;
                case ViewState.Add: // ButtonText = "Cancel"
                    PerformCancelAdd();
                    break;
                case ViewState.Copy: // ButtonText = "Cancel"
                    PerformCancelCopy();
                    break;
                case ViewState.Delete: // ButtonText = "Cancel", should never get here
                default:
                    break;
            }
        }

        private void btnEditSave_Click(object sender, EventArgs e)
        {
            switch (_viewState)
            {
                case ViewState.View: // ButtonText = "Edit"
                    BeginEdit();
                    break;
                case ViewState.Edit:// ButtonText = "Save"
                    PerformSaveEdit();
                    break;
                case ViewState.Add:// ButtonText = "Save"
                    PerformSaveAdd();
                    break;
                case ViewState.Copy:// ButtonText = "Save"
                    PerformSaveCopy();
                    break;
                case ViewState.Delete:// ButtonText = "Save", should never get here
                default:
                    break;
            }
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            try
            {
                SetViewState(ViewState.Add);

                BeginNew();
            }
            catch (Exception ex)
            {
                ExceptionHandler(ex);
            }
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            try
            {
                SetViewState(ViewState.Copy);

                BeginCopy();
            }
            catch (Exception ex)
            {
                ExceptionHandler(ex);
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            try
            {
                SetViewState(ViewState.Delete);

                BeginDelete();
            }
            catch (Exception ex)
            {
                ExceptionHandler(ex);
            }
        }

        private void BeginEdit()
        {
            SetViewState(ViewState.Edit);
        }
        private void BeginNew()
        {
            ClearGridDetails();

            _selectedGridSettings = new GridSettings()
            {
                Name = "<New Grid Settings>"
            };

            DisplaySelectedGrid(_selectedGridSettings);

            SetViewState(ViewState.Add);
        }
        private void BeginCopy()
        {
            DisplaySelectedGrid(_selectedGridSettings);

            _selectedGridSettings = new GridSettings()
            {
                Name = $"Copy of {_selectedGridSettings.Name}"
            };

            txtGridName.Text = _selectedGridSettings.Name;

            SetViewState(ViewState.Copy);
        }
        private void BeginDelete()
        {
            try
            {
                var confirmationPromptResult = MessageBox.Show(this,
                    "Are you sure you want to delete this grid?",
                    "Confirm Delete",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Warning);

                if (confirmationPromptResult == DialogResult.Yes)
                {
                    PerformSaveDelete();
                }
                else
                {
                    SetViewState(ViewState.View);
                }
            }
            catch (Exception ex)
            {
                ExceptionHandler(ex);
            }
        }

        private void PerformCloseDialog()
        {
            // Final saves here?
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        private void PerformCancelEdit()
        {
            var editedSettings = cboGridSelection.SelectedItem as GridSettings;

            ClearGridDetails();

            SetViewState(ViewState.View);

            cboGridSelection.SelectedIndex = -1;
            cboGridSelection.SelectedValue = editedSettings.Name;
        }
        private void PerformCancelAdd()
        {
            var editedSettings = cboGridSelection.SelectedItem as GridSettings;

            ClearGridDetails();

            SetViewState(ViewState.View);

            cboGridSelection.SelectedIndex = -1;
            cboGridSelection.SelectedValue = editedSettings.Name;
        }
        private void PerformCancelCopy()
        {
            var editedSettings = cboGridSelection.SelectedItem as GridSettings;

            ClearGridDetails();

            SetViewState(ViewState.View);

            cboGridSelection.SelectedIndex = -1;
            cboGridSelection.SelectedValue = editedSettings.Name;
        }

        private void PerformSaveEdit()
        {
            UpdateSelectedFromUi();

            var editedSettingsName = _selectedGridSettings.Name;

            _settingsService.SaveCustomViewSettings(_customGridSettings);

            PopulateSelectionControl();

            ClearGridDetails();

            SetViewState(ViewState.View);

            cboGridSelection.SelectedValue = editedSettingsName;
        }
        private void PerformSaveAdd()
        {
            UpdateSelectedFromUi();

            var editedSettingsName = _selectedGridSettings.Name;

            _customGridSettings.Add(_selectedGridSettings);

            _settingsService.SaveCustomViewSettings(_customGridSettings);

            PopulateSelectionControl();

            ClearGridDetails();

            SetViewState(ViewState.View);

            cboGridSelection.SelectedValue = editedSettingsName;
        }
        private void PerformSaveCopy()
        {
            UpdateSelectedFromUi();

            var editedSettingsName = _selectedGridSettings.Name;

            _customGridSettings.Add(_selectedGridSettings);

            _settingsService.SaveCustomViewSettings(_customGridSettings);

            PopulateSelectionControl();

            ClearGridDetails();

            SetViewState(ViewState.View);

            cboGridSelection.SelectedValue = editedSettingsName;
        }
        private void PerformSaveDelete()
        {
            _customGridSettings.Remove(_selectedGridSettings);

            _settingsService.SaveCustomViewSettings(_customGridSettings);

            PopulateSelectionControl();

            ClearGridDetails();

            SetViewState(ViewState.View);

            if (cboGridSelection.Items.Count > 0)
                cboGridSelection.SelectedIndex = 0;
        }

        private void cboGridSelection_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_loading)
                return;

            SetViewState(_viewState);

            _selectedGridSettings = cboGridSelection.SelectedItem as GridSettings;

            ClearGridDetails();

            if (_selectedGridSettings != null)
                DisplaySelectedGrid(_selectedGridSettings);
        }

        private void ClearGridDetails()
        {
            _loading = true;

            txtGridName.Clear();
            txtDescription.Clear();
            cboDataSource.SelectedIndex = -1;
            txtGridWidth.Clear();
            chkHideRowSelectors.Checked = false;
            txtDisplayOrder.Clear();
            cboLocation.SelectedIndex = -1;

            lstFields.Items.Clear();

            txtGridTitle.Clear();
            txtGridTitle.ForeColor = Color.Black;
            txtGridTitle.BackColor = Color.White;

            txtDataPropertyName.Clear();
            txtColumnHeader.Clear();
            txtColumnWidth.Clear();
            chkColumnVisible.Checked = false;
            txtDisplayIndex.Clear();
            dataGridView.Columns.Clear();

            _loading = false;
        }

        private void DisplaySelectedGrid(GridSettings settings)
        {
            txtGridName.Text = settings.Name;
            txtDescription.Text = settings.Description;
            cboDataSource.SelectedIndex = (int)settings.ApiSource;
            txtGridWidth.Text = settings.GridWidth.ToString();
            chkHideRowSelectors.Checked = settings.HideRowSelector;
            txtDisplayOrder.Text = settings.DisplayOrder.ToString();
            cboLocation.SelectedIndex = (int)settings.Location;

            // cboSortBy

            txtGridTitle.Text = settings.Title;
            txtGridTitle.ForeColor = _selectedGridSettings.TitleForeColor;
            txtGridTitle.BackColor = _selectedGridSettings.TitleBackColor;

            DisplayColumnsFromApiSource(settings.ApiSource);

            ApplyColumnSettingsToGrid(settings);

            if (!String.IsNullOrEmpty(settings.SortOrderField))
            {
                cboSortBy.SelectedValue = settings.SortOrderField;
                switch (settings.SortOrder)
                {
                    case 0:
                        rbSortNone.Checked = true;
                        break;
                    case 1:
                        rbSortAscending.Checked = true;
                        break;
                    case 2:
                        rbSortDescending.Checked = true;
                        break;
                    default:
                        break;
                }
            }
            else
            {
                rbSortNone.Checked = true;
            }
        }

        private void UpdateSelectedFromUi()
        {
            try
            {
                _selectedGridSettings.Name = txtGridName.Text;
                _selectedGridSettings.Description = txtDescription.Text;
                _selectedGridSettings.ApiSource = (ApiSources)cboDataSource.SelectedIndex;
                _selectedGridSettings.GridWidth = int.Parse(txtGridWidth.Text);
                _selectedGridSettings.HideRowSelector = chkHideRowSelectors.Checked;
                _selectedGridSettings.Location = (GridLocations)cboLocation.SelectedIndex;
                _selectedGridSettings.DisplayOrder = int.Parse(txtDisplayOrder.Text);
                if (cboSortBy.SelectedValue == null)
                {
                    _selectedGridSettings.SortOrderField = String.Empty;
                    _selectedGridSettings.SortOrder = 0;
                }
                else
                {
                    _selectedGridSettings.SortOrderField = cboSortBy.SelectedValue.ToString();
                    _selectedGridSettings.SortOrder = rbSortNone.Checked ? 0 :
                        rbSortAscending.Checked ? 1 :
                        rbSortDescending.Checked ? 2 : 0;
                }

                if (String.IsNullOrEmpty(txtGridTitle.Text))
                {
                    _selectedGridSettings.Title = txtGridName.Text;
                }
                else
                {
                    _selectedGridSettings.Title = txtGridTitle.Text;
                }
                _selectedGridSettings.TitleForeColor = txtGridTitle.ForeColor;
                _selectedGridSettings.TitleBackColor = txtGridTitle.BackColor;

                _selectedGridSettings.Columns.Clear();

                foreach (DataGridViewColumn dgvc in dataGridView.Columns)
                {
                    var gcs = new GridColumnSettings()
                    {
                        Index = dgvc.Index,
                        DisplayIndex = dgvc.DisplayIndex,
                        DataProperty = dgvc.DataPropertyName,
                        Visible = dgvc.Visible,
                        Width = dgvc.Visible ? dgvc.Width : 0,
                        HeaderTitle = dgvc.HeaderText
                    };

                    _selectedGridSettings.Columns.Add(gcs);
                }
            }
            catch (Exception ex)
            {
                ExceptionHandler(ex);
            }
        }

        private void PopulateSelectionControl()
        {
            try
            {
                _customGridSettings = _settingsService.GetCustomViewSettings();

                _loading = true;

                cboGridSelection.DisplayMember = "Name";
                cboGridSelection.ValueMember = "Name";
                cboGridSelection.DataSource = _customGridSettings.ToList();

                cboGridSelection.SelectedIndex = -1;
            }
            catch (Exception ex)
            {
                ExceptionHandler(ex);
            }
            finally
            {
                _loading = false;
            }
        }

        private List<PropertyInfo> LoadColumnList(ApiSources apiSource)
        {
            switch (apiSource)
            {
                case ApiSources.DriverStatistics:
                    var driverStatsObj = new rNascar23.DriverStatistics.Models.Driver();
                    return GetAllPropertiesOfType(driverStatsObj);
                case ApiSources.Flags:
                    var flagsObj = new FlagState();
                    return GetAllPropertiesOfType(flagsObj);
                case ApiSources.LapTimes:
                    var lapTimesObj = new LapDetails();
                    return GetAllPropertiesOfType(lapTimesObj);
                case ApiSources.LiveFeed:
                    var liveFeedObj = new LiveFeed();
                    return GetAllPropertiesOfType(liveFeedObj);
                case ApiSources.RaceLists:
                    var scheduleObj = new Series();
                    return GetAllPropertiesOfType(scheduleObj);
                default:
                    throw new ArgumentException($"Unrecognized Api Source: {apiSource}");
            }
        }

        private List<PropertyInfo> GetAllPropertiesOfType(object obj)
        {
            return obj.GetType().GetProperties().ToList();
        }

        #endregion

        private void cboDataSource_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboDataSource.SelectedItem == null)
                return;

            if (_loading)
                return;

            _loading = true;

            var selectedApiSource = (ApiSources)cboDataSource.SelectedIndex;

            DisplayColumnsFromApiSource(selectedApiSource);

            _loading = false;
        }

        private void DisplayColumnsFromApiSource(ApiSources apiSource)
        {
            var apiSourceColumnProperties = LoadColumnList(apiSource);

            DisplayColumnsInList(apiSourceColumnProperties);

            DisplayColumnsInGrid(apiSourceColumnProperties);

            DisplayColumnsInSortByList(apiSourceColumnProperties);
        }

        private void DisplayColumnsInList(List<PropertyInfo> columnProperties)
        {
            lstFields.Items.Clear();

            foreach (PropertyInfo columnProperty in columnProperties)
            {
                lstFields.Items.Add(columnProperty.Name);
            }
        }

        private void DisplayColumnsInGrid(List<PropertyInfo> columnProperties)
        {
            dataGridView.Columns.Clear();

            int index = 0;

            dataGridView.SelectionMode = DataGridViewSelectionMode.CellSelect;

            foreach (PropertyInfo columnProperty in columnProperties)
            {
                var newColumn = new DataGridViewTextBoxColumn()
                {
                    Name = columnProperty.Name,
                    DataPropertyName = columnProperty.Name,
                    Width = 50,
                    DisplayIndex = index
                };

                dataGridView.Columns.Add(newColumn);

                index++;
            }

            foreach (DataGridViewColumn column in dataGridView.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
            }

            dataGridView.SelectionMode = DataGridViewSelectionMode.FullColumnSelect;
        }

        private void DisplayColumnsInSortByList(List<PropertyInfo> columnProperties)
        {
            cboSortBy.DataSource = null;

            cboSortBy.DisplayMember = "Name";
            cboSortBy.ValueMember = "Name";

            cboSortBy.DataSource = columnProperties.ToList();
            cboSortBy.SelectedIndex = -1;
        }

        private void ApplyColumnSettingsToGrid(GridSettings settings)
        {
            foreach (var columnSettings in settings.Columns)
            {
                DataGridViewColumn dgvc = dataGridView.Columns[columnSettings.Index];

                if (dgvc != null)
                {
                    dgvc.DisplayIndex = columnSettings.DisplayIndex;
                    dgvc.Visible = columnSettings.Visible;
                    if (dgvc.Visible == true)
                    {
                        dgvc.Width = columnSettings.Width;
                    }
                    dgvc.HeaderText = columnSettings.HeaderTitle;
                }
            }

            foreach (DataGridViewColumn column in dataGridView.Columns)
            {
                column.SortMode = DataGridViewColumnSortMode.NotSortable;
            }

            dataGridView.SelectionMode = DataGridViewSelectionMode.FullColumnSelect;
        }
    }
}