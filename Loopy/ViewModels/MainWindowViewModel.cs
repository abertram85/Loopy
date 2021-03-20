using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Loopy.Models;
using Loopy.Support;

namespace Loopy.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        #region Fields
        private LoopGenerator _generator;
        private BackgroundWorker _loopWorker = null;
        private bool _isLoopInProgress = false;
        private ICommand _startLoopCommand;
        private ICommand _cancelLoopCommand;

        #endregion

        #region Constructor
        public MainWindowViewModel()
        {
            _generator = new LoopGenerator();
            getConfigSettings();

        }
        #endregion
        #region Properties

        public int UpperBound
        {
            get
            {
                return _generator.UpperBound;
            }
            set
            {
                _generator.UpperBound = value;
                ValidateBounds();
                RaisePropertyChanged("UpperBound");
            }
        }

        public int LowerBound
        {
            get
            {
                return _generator.LowerBound;
            }
            set
            {
                _generator.LowerBound = value;
                ValidateBounds();
                RaisePropertyChanged("LowerBound");

            }
        }


        public int PercentProgress
        {
            get
            {
                return _generator.PercentProgress;
            }
        }

        public string Status
        {
            get
            {
                return _generator.Status;
            }
        }
        public ObservableCollection<DigitDisplayText> Results
        {
            get
            {
                return new ObservableCollection<DigitDisplayText>(_generator.Results);
            }
        }

        public bool EnableStartButton
        {
            get
            {
                return !IsLoopInProgress && IsValid;
            }
        }
        public bool IsLoopInProgress {
            get
            {
                return _isLoopInProgress;
            }
            set
            {
                _isLoopInProgress = value;
                RaisePropertyChanged("IsLoopInProgress");
                RaisePropertyChanged("EnableStartButton");
            }
        }

        #endregion

        #region Methods


        public void StartLoop()
        {
            //Reset the loop generator results in case this is not the first run.
            //We will run the loop generator as a BackgroundWorker so we can display a progress bar
            //and allow the user to cancel the process if they don't want to see all the integers.
            _loopWorker = new BackgroundWorker();
            _loopWorker.WorkerSupportsCancellation = true;
            _loopWorker.WorkerReportsProgress = true;
            _loopWorker.ProgressChanged += loopWorker_ProgressChanged;
            _loopWorker.DoWork += loopWorker_DoWork;
            _loopWorker.RunWorkerCompleted += loopWorker_RunWorkerCompleted;

            if (!_loopWorker.IsBusy)
            {
                IsLoopInProgress = true;                
                _loopWorker.RunWorkerAsync();

            }

        }

        public void CancelLoop()
        {
            _loopWorker.CancelAsync();
        }

        private void ValidateBounds()
        {
            if (UpperBound <= LowerBound)
            {
                AddError(nameof(UpperBound), "Upper Bound must be >= Lower Bound.");
            }
            else
            {
                RemoveError(nameof(UpperBound));
            }
            RaisePropertyChanged("IsValid");
            RaisePropertyChanged("EnableStartButton");
        }
        private void getConfigSettings()
        {
            LoopyConfiguration config = (LoopyConfiguration)ConfigurationManager.GetSection("loopyConfig");
            _generator.DigitReplacements = new List<DigitReplacement>();
            foreach (DigitReplacementElement dre in config.DigitReplacements)
            {
                _generator.DigitReplacements.Add(new DigitReplacement { MultipleOf = dre.MultipleOf, ReplaceWith = dre.ReplaceWith, Color=dre.TextColor });
            }
        }

        #endregion

        #region BackgroundWorker Methods
        void loopWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            var bgw = (BackgroundWorker)sender;

            _generator.ProgressChanged = (sp, pe) => bgw.ReportProgress(pe.ProgressPercentage, pe.UserState);
            _generator.CheckCancel += (sc, ce) => ce.Cancel = bgw.CancellationPending;
            _generator.Execute();

        }

        void loopWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            IsLoopInProgress = false;
            if (e.Error != null)
            {
                System.Windows.MessageBox.Show(e.Error.Message, "Process Did Not Complete");
            }
            else if (_generator.Cancelled)
            {
                RaisePropertyChanged("Status");
                RaisePropertyChanged("PercentProgress");
            }
        }
        void loopWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            var bgw = (BackgroundWorker)sender;
            RaisePropertyChanged("Results");
            RaisePropertyChanged("PercentProgress");
            RaisePropertyChanged("Status");
        }


        #endregion

        #region ICommands 
        public ICommand BtnStartLoop
        {
            get
            {
                return _startLoopCommand ?? (_startLoopCommand = new RelayCommand(
               x =>
               {
                   StartLoop();
               }));
            }
        }

        public ICommand BtnCancelLoop
        {
            get
            {
                return _cancelLoopCommand ?? (_cancelLoopCommand = new RelayCommand(
               x =>
               {
                   CancelLoop();
               }));

            }
        }

        #endregion
    }
}
