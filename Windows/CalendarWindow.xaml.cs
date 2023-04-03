using MoneyCalendar.Data;
using MoneyCalendar.DataModels;
using MoneyCalendar.ViewModels;

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Ribbon;
using System.Windows.Input;
using Microsoft.Reporting.WinForms;

namespace MoneyCalendar.Windows
{
    public partial class CalendarWindow : RibbonWindow
    {
        #region Properties
        public CalendarViewModel CalendarViewModel { get => (this.DataContext as CalendarViewModel); }

        public TransactionBatchWindow _transactionBatchWindow { get; private set; }
        public LoadDueDatesWindow _loadDueDatesWindow { get; private set; }
        public MonthlyExpensesWindow _monthlyExpensesWindow { get; private set; }
        #endregion
        
        #region Class Methods
        public CalendarWindow()
        {
            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.CalendarViewModel.Close();

            MoneyApplication.Current.Shutdown();
        }
        #endregion

        #region Menus
        private void DatedListView_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            if (sender is ListView ListView)
            {
                ListView.ContextMenu.Items.Clear();

                MenuItem menuitem = new MenuItem() { Header = "New Transaction" };
                menuitem.Command = this.CalendarViewModel.StartNewTransactionCommand;
                menuitem.Click += this.Menu_NewTransaction_Click;
                ListView.ContextMenu.Items.Add(menuitem);

                menuitem = new MenuItem() { Header = "New Transaction Batch" };
                menuitem.Click += this.Menu_NewTransactionBatch_Click;
                ListView.ContextMenu.Items.Add(menuitem);

                if (this.CalendarViewModel.HasSelectedTransaction)
                {
                    Transaction selectedtransaction = this.CalendarViewModel.SelectedTransaction;
                    TransactionType transactiontype = selectedtransaction.TransactionType;

                    ListView.ContextMenu.Items.Add(new Separator());

                    menuitem = new MenuItem();
                    if (transactiontype?.IsDueType ?? false)
                    {
                        menuitem.Click += this.Menu_DueAmount_Click;
                        menuitem.Header = "Edit Amount: " + selectedtransaction.DueAmount?.ToString("c");
                    }
                    else
                    {
                        menuitem.Click += this.Menu_Amount_Click;
                        menuitem.Header = selectedtransaction.Amount.ToString("c");
                    }
                    ListView.ContextMenu.Items.Add(menuitem);

                    if (!string.IsNullOrEmpty(selectedtransaction.Description))
                    {
                        menuitem = new MenuItem();
                        menuitem.Click += this.Menu_Description_Click;
                        menuitem.Header = selectedtransaction.Description;
                        ListView.ContextMenu.Items.Add(menuitem);
                    }

                    if (!transactiontype?.IsDueType ?? false)
                    {
                        menuitem = new MenuItem();
                        menuitem.Header = "Posted";
                        menuitem.IsCheckable = true;
                        menuitem.IsChecked = selectedtransaction.IsCompleted;
                        //menuitem.Click += Menu_IsCompleted_Click;
                        menuitem.Command = this.CalendarViewModel.SelectedTransactionPostCommand;
                        //menuitem.SetBinding(MenuItem.IsCheckedProperty, new Binding($"{nameof(CalendarViewModel.SelectedTransaction)}.{nameof(Transaction.IsCompleted)}"));
                        //menuitem.SetBinding(MenuItem.CommandProperty, new Binding(nameof(CalendarViewModel.SaveSelectedTransactionCommand)));                        
                        ListView.ContextMenu.Items.Add(menuitem);
                    }

                    if (transactiontype?.IsDueType ?? false)
                    {
                        menuitem = new MenuItem();
                        if (transactiontype.IsNegative)
                            menuitem.Header = "Pay Bill";
                        else
                            menuitem.Header = "Receive Income";
                        menuitem.Click += SelectedTransaction_SettleDueType_Click;
                        ListView.ContextMenu.Items.Add(menuitem);

                        menuitem = new MenuItem();
                        if (transactiontype.IsNegative)
                            menuitem.Header = "Pay New Bill Amount";
                        else
                            menuitem.Header = "Receive New Income Amount";
                        menuitem.Click += SelectedTransaction_SettleDueTypeNewAmount_Click;
                        ListView.ContextMenu.Items.Add(menuitem);
                    }

                    //ListView.ContextMenu.Items.Add(new Separator());

                    menuitem = new MenuItem();
                    menuitem.Header = "Move To Account ->";
                    foreach (AccountBalance account in this.CalendarViewModel.AccountSelectionList)
                    {
                        if (account.AccountID != selectedtransaction.AccountID)
                        {
                            MenuItem submenuitem = new MenuItem();
                            submenuitem.Header = account.AccountName;
                            submenuitem.Command = CalendarViewModel.SelectedTransactionMoveToAccountCommand;
                            submenuitem.CommandParameter = account.AccountID;
                            menuitem.Items.Add(submenuitem);
                        }
                    }
                    ListView.ContextMenu.Items.Add(menuitem);

                    menuitem = new MenuItem();
                    menuitem.Header = "Delete";
                    menuitem.Command = CalendarViewModel.DeleteSelectedTransactionCommand;
                    ListView.ContextMenu.Items.Add(menuitem);

                    ListView.ContextMenu.Items.Add(new Separator());

                    menuitem = new MenuItem();
                    menuitem.Header = "Cut";
                    menuitem.Command = CalendarViewModel.CutSelectedTransactionCommand;
                    ListView.ContextMenu.Items.Add(menuitem);

                    menuitem = new MenuItem();
                    menuitem.Header = "Copy";
                    menuitem.Command = CalendarViewModel.CopySelectedTransactionCommand;
                    ListView.ContextMenu.Items.Add(menuitem);

                    menuitem = new MenuItem();
                    menuitem.Header = "Paste";
                    menuitem.Command = CalendarViewModel.PasteSelectedTransactionCommand;
                    ListView.ContextMenu.Items.Add(menuitem);
                }
                else
                {
                    ListView.ContextMenu.Items.Add(new Separator());

                    menuitem = new MenuItem();
                    menuitem.Header = "Paste";
                    menuitem.Command = CalendarViewModel.PasteSelectedTransactionCommand;
                    ListView.ContextMenu.Items.Add(menuitem);
                }
            }
        }

        private void Menu_Amount_Click(object sender, RoutedEventArgs e)
        {
            this.txtSelectedTransactionAmount.SelectAll();
            this.txtSelectedTransactionAmount.Focus();
        }

        private void Menu_DueAmount_Click(object sender, RoutedEventArgs e)
        {
            this.txtSelectedTransactionDueAmount.SelectAll();
            this.txtSelectedTransactionDueAmount.Focus();
        }

        private void Menu_Description_Click(object sender, RoutedEventArgs e)
        {
            this.txtSelectedTransactionDescription.SelectAll();
            this.txtSelectedTransactionDescription.Focus();
        }

        private void Menu_NewTransaction_Click(object sender, RoutedEventArgs e)
        {
            this.grpSelectedTransaction.Visibility = Visibility.Visible;
            FocusManager.SetFocusedElement(this, this.cboSelectedTransactionType);
        }

        private void Menu_NewTransactionBatch_Click(object sender, RoutedEventArgs e)
        {
            if (this._transactionBatchWindow == null)
                this._transactionBatchWindow = new TransactionBatchWindow(this);

            this._transactionBatchWindow.Show();
            this._transactionBatchWindow.WindowState = WindowState.Normal;
        }

        private void Menu_OpenLoadDueDates_Click(object sender, RoutedEventArgs e)
        {

            this._loadDueDatesWindow = new LoadDueDatesWindow(this);
            this._loadDueDatesWindow.Show();
            this._loadDueDatesWindow.WindowState = WindowState.Normal;
        }

        private void Menu_OpenMonthlyExpenses_Click(object sender, RoutedEventArgs e)
        {
            if (this._monthlyExpensesWindow == null)
            {
                this._monthlyExpensesWindow = new MonthlyExpensesWindow(this);
                this._monthlyExpensesWindow.Closed += ChildWindow_Closed;
                this._monthlyExpensesWindow.Show();
            }
            
            this._monthlyExpensesWindow.WindowState = WindowState.Normal;
        }

        private void ChildWindow_Closed(object sender, EventArgs e)
        {
            if (sender == this._monthlyExpensesWindow)
                this._monthlyExpensesWindow = null;
        }

        private void Menu_OpenAccounts_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new AccountsWindow(this).ShowDialog();
                this.CalendarViewModel.RefreshAllCommand.Execute();
            }
            catch (Exception ex)
            {
                MoneyApplication.ErrorHandler(ex);
            }
        }

        private void Menu_OpenBills_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                new BillsWindow(this).ShowDialog();
                this.CalendarViewModel.RefreshAllCommand.Execute();
            }
            catch (Exception ex)
            {
                MoneyApplication.ErrorHandler(ex);
            }
        }
        #endregion

        #region Mouse
        private void DatedListView_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            if (e.Delta > 0)    //Scroll up (back in time)
                this.CalendarViewModel.SelectStartDateCommand?.Execute(this.CalendarViewModel.StartDate + new TimeSpan(-7, 0, 0, 0));
                //await this.CalendarViewModel.SelectStartDate(this.CalendarViewModel.StartDate + new TimeSpan(-7, 0, 0, 0));

            else //Scroll down (forward in time)
                //await this.CalendarViewModel.SelectStartDate(this.CalendarViewModel.StartDate + new TimeSpan(7, 0, 0, 0));
                this.CalendarViewModel.SelectStartDateCommand?.Execute(this.CalendarViewModel.StartDate + new TimeSpan(7, 0, 0, 0));
        }

        private void DatedListView_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                ((sender as System.Windows.Controls.ListView).DataContext as DatedTransactionSet).SelectDatedTransactionSetCommand.Execute();
            }
            catch (Exception ex)
            {
                MoneyApplication.ErrorHandler(ex);
            }
        }
        #endregion

        #region Control Events        
        private void AutoSelectAllTextBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (sender is TextBox textbox)
                textbox.SelectAll();
        }

        private void AutoSelectAllTextBox_GotMouseCapture(object sender, MouseEventArgs e)
        {
            if (sender is TextBox textbox)
                textbox.SelectAll();
        }

        private void cboSelectedTransactionType_Selected(object sender, RoutedEventArgs e)
        {
            this.txtSelectedTransactionAmount.SelectAll();
            this.txtSelectedTransactionAmount.Focus();
        }

        private void SelectedTransaction_SettleDueType_Click(object sender, RoutedEventArgs e)
        {
            this.CalendarViewModel.SelectedTransactionSettleDueTypeCommand.Execute();
        }

        private void SelectedTransaction_SettleDueTypeNewAmount_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //this.grdMain.Opacity = 0.5;
                //this.grdMain.Effect = new BlurEffect();

                //this.grdInputBox.Visibility = Visibility.Visible;
                //this.grdInputBox.Opacity = 1;
                //this.grdInputBox.Effect = null;

                InputWindow inputwindow = new InputWindow("Enter New Amount", "Enter new amount for this transaction");
                inputwindow.ShowDialog();

                if (!string.IsNullOrEmpty(inputwindow.InputTextBox.Text) && decimal.TryParse(inputwindow.InputTextBox.Text, out decimal newamount))
                {
                    this.CalendarViewModel.SelectedTransactionSettleDueTypeNewAmount = newamount;
                    this.CalendarViewModel.SelectedTransactionSettleDueTypeIsPosted = MessageBox.Show("Is this transaction posted?", "Posted?", MessageBoxButton.YesNo) == MessageBoxResult.Yes;

                    this.CalendarViewModel.SelectedTransactionSettleDueTypeCommand.Execute();
                }

                //this.Opacity = 1;
                //this.Effect = null;                
            }
            catch (Exception ex)
            {
                MoneyApplication.ErrorHandler(ex);
            }
        }
        #endregion

        #region Selection Box
        bool mouseDown = false; // Set to 'true' when mouse is held down.
        Point mouseDownPos; // The point where the mouse button was clicked down.

        private void Grid_MouseDown(object sender, MouseButtonEventArgs e)
        {
            // Capture and track the mouse.
            mouseDown = true;
            //mouseDownPos = e.GetPosition(this.itemsDateSets);
            mouseDownPos = e.GetPosition(this);
            //this.itemsDateSets.CaptureMouse();
            //this.CaptureMouse();

            // Initial placement of the drag selection box.         

            Canvas.SetLeft(selectionBox, mouseDownPos.X);
            Canvas.SetTop(selectionBox, mouseDownPos.Y);
            selectionBox.Width = 0;
            selectionBox.Height = 0;

            // Make the drag selection box visible.
            selectionBox.Visibility = Visibility.Visible;
        }

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            // Release the mouse capture and stop tracking it.
            mouseDown = false;
            this.itemsDateSets.ReleaseMouseCapture();

            // Hide the drag selection box.
            selectionBox.Visibility = Visibility.Collapsed;

            //Point mouseUpPos = e.GetPosition(this.itemsDateSets);
            Point mouseUpPos = e.GetPosition(this);


            // TODO: 
            //
            // The mouse has been released, check to see if any of the items 
            // in the other canvas are contained within mouseDownPos and 
            // mouseUpPos, for any that are, select them!
            //
        }

        private void Grid_MouseMove(object sender, MouseEventArgs e)
        {
            if (mouseDown)
            {
                // When the mouse is held down, reposition the drag selection box.

                //Point mousePos = e.GetPosition(this.itemsDateSets);
                Point mousePos = e.GetPosition(this);

                if (mouseDownPos.X < mousePos.X)
                {
                    Canvas.SetLeft(selectionBox, mouseDownPos.X);
                    selectionBox.Width = mousePos.X - mouseDownPos.X;
                }
                else
                {
                    Canvas.SetLeft(selectionBox, mousePos.X);
                    selectionBox.Width = mouseDownPos.X - mousePos.X;
                }

                if (mouseDownPos.Y < mousePos.Y)
                {
                    Canvas.SetTop(selectionBox, mouseDownPos.Y);
                    selectionBox.Height = mousePos.Y - mouseDownPos.Y;
                }
                else
                {
                    Canvas.SetTop(selectionBox, mousePos.Y);
                    selectionBox.Height = mouseDownPos.Y - mousePos.Y;
                }
            }
        }
        #endregion

        #region Drag and Drop
        private void DatedListView_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && sender != null
                && sender is ListView listview && listview.InputHitTest(e.GetPosition(listview)) is TextBlock textblock
                && textblock.GetBindingExpression(TextBlock.TextProperty).ResolvedSource is Transaction transaction)
            {
                DragDrop.DoDragDrop(listview, transaction, DragDropEffects.Move);
            }
        }

        private void DatedListView_DragOver(object sender, DragEventArgs e)
        {
            if (sender is ListView listview)
            {
                if (listview.GetBindingExpression(ListView.ItemsSourceProperty).ResolvedSource is DatedTransactionSet datedtransactionset
                    && e.Data.GetDataPresent(typeof(Transaction))
                    && e.Data.GetData(typeof(Transaction)) is Transaction transaction
                    && transaction.TransactionDate != datedtransactionset.SetDate)
                {
                    e.Effects = DragDropEffects.Move;
                }
                else
                    e.Effects = DragDropEffects.None;

                e.Handled = true;
            }
        }

        private void DatedListView_Drop(object sender, DragEventArgs e)
        {
            if (sender is ListView listview
                && e.Data.GetDataPresent(typeof(Transaction))
                && listview.GetBindingExpression(ListView.ItemsSourceProperty).ResolvedSource is DatedTransactionSet datedtransactionset)
            {
                this.CalendarViewModel.MoveSelectedTransactionCommand.Execute(datedtransactionset.SetDate);
            }
        }

        private void cboAccounts_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Move;
        }

        private void AccountItem_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Move;
            e.Handled = true;
        }

        private void AccountItem_Drop(object sender, DragEventArgs e)
        {
            System.Diagnostics.Debugger.Break();
            this.CalendarViewModel.SelectedTransactionMoveToAccountCommand.Execute(((sender as TextBlock).Tag as AccountBalance).AccountID);
        }
        #endregion

        private void DatedListView_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            this.Menu_Amount_Click(sender, e);
            this.Menu_DueAmount_Click(sender, e);
        }

        private void CalendarWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Source is TextBox)
                return;

            try
            {
                if (e.Key == Key.Enter && (Keyboard.Modifiers & ModifierKeys.Control) != ModifierKeys.Control)
                {
                    this.CalendarViewModel.SaveSelectedTransactionCommand.Execute();
                    e.Handled = true;
                }
                else if (e.Key == Key.PageUp || e.Key == Key.PageDown)
                {
                    int days = 28 * (e.Key == Key.PageUp ? -1 : 1);
                    this.CalendarViewModel.SelectStartDateCommand?.Execute(this.CalendarViewModel.StartDate + new TimeSpan(days, 0, 0, 0));
                    e.Handled = true;
                }
                else if (e.Key == Key.Down || e.Key == Key.Up)
                {
                    int days = 7 * (e.Key == Key.Up ? -1 : 1);
                    DateTime newdate = this.CalendarViewModel.SelectedCalenderDate.AddDays(days);
                    if (newdate > this.CalendarViewModel.EndDate || newdate < this.CalendarViewModel.StartDate)
                        //this.CalendarViewModel.SelectStartDate(newdate);
                        this.CalendarViewModel.SelectStartDateCommand?.Execute(newdate);

                    this.CalendarViewModel.DatedTransactionSets[newdate].SelectDatedTransactionSetCommand.Execute();

                    e.Handled = true;
                }
                else if (e.Key == Key.Left || e.Key == Key.Right)
                {
                    int days = 1 * (e.Key == Key.Left ? -1 : 1);
                    DateTime newdate = this.CalendarViewModel.SelectedCalenderDate.AddDays(days);
                    if (newdate > this.CalendarViewModel.EndDate || newdate < this.CalendarViewModel.StartDate)
                        this.CalendarViewModel.SelectStartDateCommand?.Execute(this.CalendarViewModel.StartDate + new TimeSpan(7 * days, 0, 0, 0));

                    this.CalendarViewModel.DatedTransactionSets[newdate].SelectDatedTransactionSetCommand.Execute();

                    e.Handled = true;
                }
                else if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
                {
                    //Shortcut keys

                    if (e.Key == Key.T)
                    {
                        this.CalendarViewModel.SelectStartDate(DateTime.Today);
                        this.CalendarViewModel.DatedTransactionSets[DateTime.Today].SelectDatedTransactionSetCommand.Execute();
                        e.Handled = true;
                    }
                    else if (e.Key == Key.X)
                    {
                        this.CalendarViewModel.CutSelectedTransactionCommand.Execute();
                        e.Handled = true;
                    }
                    else if (e.Key == Key.C)
                    {
                        this.CalendarViewModel.CopySelectedTransactionCommand.Execute();
                        e.Handled = true;
                    }
                    else if (e.Key == Key.V)
                    {
                        this.CalendarViewModel.PasteSelectedTransactionCommand.Execute();
                        e.Handled = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MoneyApplication.ErrorHandler(ex);
            }
        }

        private void Ribbon_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.ribbonMain.SelectedItem is RibbonTab selectedtab && this.DataContext is CalendarViewModel viewmodel)
            {
                if (selectedtab.Name == nameof(this.ribbontabReports))
                {
                    viewmodel.ShowReport = true;
                    this.ShowReport();
                }
                else
                    viewmodel.ShowCalendar = true;
            }
        }

        private void ShowReport()
        {
            try
            {
                if (this.DataContext is CalendarViewModel viewmodel)
                {
                    this.ReportViewer.LocalReport.DataSources.Clear();
                    this.ReportViewer.LocalReport.ReportEmbeddedResource = $"MoneyCalendar.Reports.MonthSummaryReport.rdlc";
                    this.ReportViewer.LocalReport.DisplayName = "Month Summary";

                    this.ReportViewer.SetDisplayMode(DisplayMode.PrintLayout);
                    this.ReportViewer.Refresh();
                    this.ReportViewer.RefreshReport();
                }
            }
            catch (Exception ex)
            {
                MoneyApplication.ErrorHandler(ex);
            }
        }
    }
}
