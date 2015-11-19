using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Windows.Input;
using WpfAsyncDataSample.Model;

namespace WpfAsyncDataSample.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        #region Fields

        private readonly EmployeeDataProvider _dataProvider;
        private readonly ObservableCollection<Employee> _items;

        private CancellationTokenSource _cancellationTokenSource;
        private IProgress<double> _progress;
        private double _currentProgress;
        private string _currentStatus;

        #endregion

        #region Properties

        public ObservableCollection<Employee> Items
        {
            get { return _items; }
        }

        public double CurrentProgress
        {
            get { return _currentProgress; }
            private set
            {
                _currentProgress = value;
                OnPropertyChanged();
            }
        }

        public string CurrentStatus
        {
            get { return _currentStatus; }
            private set
            {
                _currentStatus = value;
                OnPropertyChanged();
            }
        }

        public ICommand LoadEmployeesCommand { get; private set; }
        public ICommand CancelCommand { get; private set; }

        #endregion

        public MainViewModel()
        {
            _dataProvider = new EmployeeDataProvider();
            _items = new ObservableCollection<Employee>();
            LoadEmployeesCommand = new RelayCommand(LoadEmployeesExecute, LoadEmployeesCanExecute);
            CancelCommand = new RelayCommand(CancelExecute, CancelCanExecute);
        }

        private bool CancelCanExecute(object o)
        {
            return _cancellationTokenSource != null;
        }

        private void CancelExecute(object o)
        {
            _cancellationTokenSource.Cancel();
        }

        private bool LoadEmployeesCanExecute(object o)
        {
            return _cancellationTokenSource == null;
        }

        private async void LoadEmployeesExecute(object o)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _progress = new Progress<double>(x => CurrentProgress = x);
            try
            {
                CurrentStatus = "Loading employees";

                Items.Clear();
                var result = await _dataProvider.LoadEmployeesAsync(_cancellationTokenSource.Token, _progress);

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
            catch (OperationCanceledException)
            {
                CurrentStatus = "Operation cancelled";
            }
            catch (Exception ex)
            {
                CurrentStatus = "Operation failed - " + ex.Message;
            }
            finally
            {
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
                _progress.Report(0);
            }
        }

    }
}