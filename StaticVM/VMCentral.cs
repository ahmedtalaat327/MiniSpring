////You can replace this class with injet [memory locker mechanism]

using MiniSpring.Views.Pages.ChartsPages.ViewModel;
using Spring.ViewModel;

namespace Spring.StaticVM
{
    public static class VMCentral
    {
        /// <summary>
        /// singelton of main board VM
        /// </summary>
        public static DockingManagerViewModel DockingManagerViewModel = new DockingManagerViewModel();
        /// <summary>
        /// singelton of base page VM
        /// </summary>
        public static BasePageViewModel BasePageViewModel = new BasePageViewModel();

        public static UsersStatisticsViewModel UsersStatisticsViewModel = new UsersStatisticsViewModel();   
        

    }
}
