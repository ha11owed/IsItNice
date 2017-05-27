using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows.Input;

namespace ClientLogic.WinRT.Commands
{
    /// <summary>
    /// This is a command that simply aggregates other commands into a group.
    /// This command's CanExecute logic delegates to the CanExecute logic of 
    /// all the child commands.  When executed, it calls the Execute method
    /// on each child command sequentially.
    /// <see cref="http://www.codeproject.com/Articles/25808/Aggregating-WPF-Commands-with-CommandGroup"/>
    /// </summary>
    public class CommandGroup : ICommand
    {
        #region Constructor

        public CommandGroup()
        {
            // Parameterless public ctor required for XAML instantiation.
        }

        #endregion // Constructor

        #region Commands

        private ObservableCollection<ICommand> _commands;

        /// <summary>
        /// Returns the collection of child commands.  They are executed
        /// in the order that they exist in this collection.
        /// </summary>
        public ObservableCollection<ICommand> Commands
        {
            get
            {
                if (_commands == null)
                {
                    _commands = new ObservableCollection<ICommand>();
                    _commands.CollectionChanged += this.OnCommandsCollectionChanged;
                }

                return _commands;
            }
        }

        void OnCommandsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            // We have a new child command so our ability to execute may have changed.
            this.OnCanExecuteChanged();

            if (e.NewItems != null && 0 < e.NewItems.Count)
            {
                foreach (ICommand cmd in e.NewItems)
                    cmd.CanExecuteChanged += this.OnChildCommandCanExecuteChanged;
            }

            if (e.OldItems != null && 0 < e.OldItems.Count)
            {
                foreach (ICommand cmd in e.OldItems)
                    cmd.CanExecuteChanged -= this.OnChildCommandCanExecuteChanged;
            }
        }

        void OnChildCommandCanExecuteChanged(object sender, EventArgs e)
        {
            // Bubble up the child commands CanExecuteChanged event so that
            // it will be observed by WPF.
            this.OnCanExecuteChanged();
        }

        #endregion // Commands

        #region ICommand Members

        public bool CanExecute(object parameter)
        {
            foreach (ICommand cmd in this.Commands)
                if (!cmd.CanExecute(parameter))
                    return false;

            return true;
        }

        public event EventHandler CanExecuteChanged;

        protected virtual void OnCanExecuteChanged()
        {
            if (this.CanExecuteChanged != null)
                this.CanExecuteChanged(this, EventArgs.Empty);
        }

        public void Execute(object parameter)
        {
            foreach (ICommand cmd in this.Commands)
                cmd.Execute(parameter);
        }

        #endregion
    }
}