using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Loopy.Models
{
    //Loops through integers from LowerBound to UpperBound and returns them to the caller. If the current integer is divisible
    //by one of the divisors specified in the config file, replaces it with the value specified.
    //Intended to be called as a BackgroundWorker.
    public class LoopGenerator
    {
        public int LowerBound { get; set; }
        public int UpperBound { get; set; }
        public List<DigitDisplayText> Results { get; set; }
        public List<DigitReplacement> DigitReplacements { get; set; }

        public bool Cancelled = false;

        public int PercentProgress { get; set; }
        public string Status { get; set; }
        public ProgressChangedEventHandler ProgressChanged;

        public LoopGenerator()
        {
            Results = new List<DigitDisplayText>();
            PercentProgress = 0;
            Status = "Ready";
        }
        public void Execute()
        {
            Status = "Working";
            var e = new ProgressChangedEventArgs(PercentProgress, Status);            
            OnProgressChanged(e);
            Results = new List<DigitDisplayText>();
            decimal counter = 0;
            decimal totalIntegersToDisplay = UpperBound - LowerBound + 1;
            for (int i = LowerBound; i <= UpperBound; i++)
            {
                if (OnCheckCancel())
                {
                    Cancelled = true;
                    Status = "Cancelled";
                    PercentProgress = 0;
                    return;
                }
                else
                {
                    Cancelled = false;
                }
                try
                {
                    bool digitWasReplaced = false;
                    string textToDisplay = i.ToString();
                    string colorToDisplay = "Black";
                    string fontWeightToDisplay = "Normal";
                    foreach (DigitReplacement replace in DigitReplacements)
                    {
                        
                        if (i % replace.MultipleOf == 0)
                        {
                            if (digitWasReplaced)
                            {
                                textToDisplay += " " + replace.ReplaceWith;
                            }
                            else
                            {
                                textToDisplay = replace.ReplaceWith;
                            }
                            if (!String.IsNullOrEmpty(replace.Color))
                            {
                                colorToDisplay = replace.Color;
                            }
                            fontWeightToDisplay = "Bold";
                            digitWasReplaced = true;
                        }
                    }
                    Results.Add(new DigitDisplayText
                    {
                        DisplayText = textToDisplay,
                        Color = colorToDisplay,
                        FontWeight = fontWeightToDisplay
                    }) ;
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    counter++;
                    decimal pct = counter / totalIntegersToDisplay;
                    PercentProgress = Convert.ToInt32(Math.Round(pct * 100, 0));
                    e = new ProgressChangedEventArgs(PercentProgress, Status);                       
                    OnProgressChanged(e);
                    
                    Thread.Sleep(500);

                }
            }
            PercentProgress = 0;
            Status = "Complete";
            e = new ProgressChangedEventArgs(PercentProgress, Status);
            OnProgressChanged(e);
        }

        #region Event Handlers
        protected virtual void OnProgressChanged(ProgressChangedEventArgs e)
        {
            ProgressChanged?.Invoke(this, e);
        }

        public event EventHandler<CancelEventArgs> CheckCancel;

        private bool OnCheckCancel()
        {
            EventHandler<CancelEventArgs> handler = CheckCancel;

            if (handler != null)
            {
                CancelEventArgs e = new CancelEventArgs();

                handler(this, e);

                return e.Cancel;
            }

            return false;
        }

        #endregion

    }

    public class DigitReplacement
    {
        public int MultipleOf { get; set; }
        public string ReplaceWith { get; set; }
        public string Color { get; set; }
    }
}
