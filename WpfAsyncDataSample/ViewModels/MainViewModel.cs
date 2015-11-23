using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Input;
using Prism.Commands;
using Prism.Mvvm;
using WpfAsyncDataSample.Model;

namespace WpfAsyncDataSample.ViewModels
{
    public class MainViewModel : BindableBase
    {
        #region Fields

        private readonly EmployeeDataProvider _dataProvider = new EmployeeDataProvider();
        private readonly ObservableCollection<Employee> _items = new ObservableCollection<Employee>();
        private CancellationTokenSource _cancellationTokenSource;
        private double _currentProgress;
        private string _currentStatus;

        #endregion

        #region Properties

        public ObservableCollection<Employee> Items
        {
            get { return _items; }
        }

        protected CancellationTokenSource CancellationTokenSource
        {
            get { return _cancellationTokenSource; }
            set { SetProperty(ref _cancellationTokenSource, value); }
        }

        public double CurrentProgress
        {
            get { return _currentProgress; }
            private set { SetProperty(ref _currentProgress, value); }
        }

        public string CurrentStatus
        {
            get { return _currentStatus; }
            private set { SetProperty(ref _currentStatus, value); }
        }

        public ICommand LoadEmployeesCommand
        {
            get; private set;
        }

        public ICommand CancelCommand
        {
            get; private set;
        }

        #endregion

        public MainViewModel()
        {
            LoadEmployeesCommand = new DelegateCommand(LoadEmployees, CanLoadEmployees)
                .ObservesProperty(() => CancellationTokenSource);

            CancelCommand = new DelegateCommand(Cancel, CanCancel)
                .ObservesProperty(() => CancellationTokenSource);
        }

        private bool CanCancel()
        {
            return CancellationTokenSource != null;
        }

        private void Cancel()
        {
            CancellationTokenSource.Cancel();
        }

        private bool CanLoadEmployees()
        {
            return CancellationTokenSource == null;
        }

        private async void LoadEmployees()
        {
            CancellationTokenSource = new CancellationTokenSource();
            IProgress<double> progress = new Progress<double>(x => CurrentProgress = x);
            try {
                CurrentStatus = "Loading employees";

                Items.Clear();
                var result = await _dataProvider.LoadEmployeesAsync(CancellationTokenSource.Token, progress);

                if (result.Result != null) {
                    foreach (var item in result.Result) {
                        Items.Add(item);
                    }
                }
                if (result.Exception != null) {
                    throw result.Exception;
                }

                CurrentStatus = "Operation completed";
            }
            catch (OperationCanceledException) {
                CurrentStatus = "Operation cancelled";
            }
            catch (Exception ex) {
                CurrentStatus = "Operation failed - " + ex.Message;
            }
            finally {
                CancellationTokenSource.Dispose();
                CancellationTokenSource = null;
                progress.Report(0);
            }
        }

    }
}