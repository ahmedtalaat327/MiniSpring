using Spring.Helpers;
using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Threading.Tasks;


namespace Spring.ViewModel.Base
{
    /// <summary>
    /// Just base class to Interface the Fody property change mechanism
    /// all other view models will copy the same schema.
    /// </summary>
    public class BaseViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged = (s, e) => {/* event to be fired when setter of property happened*/ };

        /// <summary>
        /// Call this to fire a <see cref="PropertyChanged"/> event
        /// </summary>
        /// <param name="name"></param>
        public void OnPropertyChanged(string name)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(name));
        }


        /// <summary>
        /// Basically we will delgate the func that return property flag here and it's action and 
        /// manage them gere using Expressions [reason for this properties can not passed as args and wait to be changed] 
        /// so we wil compile[as setter for property] it and invoke[as getter] using Expressions!.
        /// </summary>
        /// <param name="updatingFlag">lambda expression body</param>
        /// <param name="action">the activity to be monitoring</param>
        /// <returns></returns>
        protected async Task RunCommand(Expression<Func<bool>> updatingFlag, Func<Task> action)
        {
            //if flag is true means the login action still active preventing you from doing any action[Load state] so skip it.
            if (updatingFlag.GetCompliledValue()) { return; }
            //else set flag to true then run the task of connectivity
            updatingFlag.SetCompliledValue(true);
            //run task
            try
            {
                await action();
            }
            finally
            {
                updatingFlag.SetCompliledValue(false);
            }

        }
    }
}
