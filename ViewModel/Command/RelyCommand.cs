using System;
using System.Windows.Input;

namespace Spring.ViewModel.Command
{
    public class RelyCommand : ICommand
    {
        #region Members
        /// <summary>
        /// Property change if only <see cref="CanExecute(object)"/> is changed value to T/F
        /// </summary>
        public event EventHandler CanExecuteChanged = (sdr,evt) => { };
        /// <summary>
        /// Instance for storing current action
        /// </summary>
        private Action miniAction = null;
        #endregion
        #region Constructor
        public RelyCommand(Action ac) {
        this.miniAction = ac;
        }
        #endregion
        #region Funcs
        /// <summary>
        /// Marked as True in return means always can execute
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public bool CanExecute(object parameter){
            return true;
        }

        /// <summary>
        /// Load my funcs from outside to be as Action
        /// </summary>
        /// <param name="parameter"></param>
        /// <exception cref="NotImplementedException"></exception>
        public void Execute(object parameter)
        {
            
                miniAction();
        }
        #endregion
    }


    public class RelayParameterizedCommand : ICommand
    {
        #region Private Members

        /// <summary>
        /// The action to run
        /// </summary>
        private Action<object> mAction;

        #endregion

        #region Public Events

        /// <summary>
        /// The event thats fired when the <see cref="CanExecute(object)"/> value has changed
        /// </summary>
        public event EventHandler CanExecuteChanged = (sender, e) => { };

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        public RelayParameterizedCommand(Action<object> action)
        {
            mAction = action;
        }

        #endregion

        #region Command Methods

        /// <summary>
        /// A relay command can always execute
        /// </summary>
        /// <param name="parameter"></param>
        /// <returns></returns>
        public bool CanExecute(object parameter)
        {
            return true;
        }

        /// <summary>
        /// Executes the commands Action
        /// </summary>
        /// <param name="parameter"></param>
        public void Execute(object parameter)
        {
            mAction(parameter);
        }

        #endregion
    }
}
