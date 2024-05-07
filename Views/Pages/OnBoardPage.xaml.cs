

using Microcharts;
using MiniSpring.Views.Pages.ChartsPages;
using MiniSpring.Views.Pages.ChartsPages.ViewModel;
using SkiaSharp;
using Spring.StaticVM;
using Spring.ViewModel;
using System.ComponentModel;


namespace MiniSpring.Views.Pages;

public partial class OnBoardPage : ContentPage
{
	 
    public OnBoardPage()
	{

		InitializeComponent();
		 
        this.BindingContext = VMCentral.DockingManagerViewModel;

        this.Loaded += OnBoardPage_Loaded;


        VMCentral.BasePageViewModel.PropertyChanged += BasePageViewModel_PropertyChanged;
        VMCentral.UsersStatisticsViewModel.PropertyChanged += UsersStatisticsViewModel_PropertyChanged;


    }

   

    private void BasePageViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        //make sure we are in same VM and same property.
        if (e.PropertyName == nameof(VMCentral.BasePageViewModel.Loading) && VMCentral.BasePageViewModel.GetType() == typeof(BasePageViewModel))
        {
            if (!VMCentral.BasePageViewModel.Loading)
            {
                if (VMCentral.BasePageViewModel.CheckRPhase == CheckRulesPhase.InChecking)
                {
                    //we ended checking
                    VMCentral.UsersStatisticsViewModel.GetCountingAndStoreThem.Execute(true);
                    VMCentral.BasePageViewModel.CheckRPhase = CheckRulesPhase.Non;
                }
            }
        }
     }
    private void UsersStatisticsViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        //make sure we are in same VM and same property.
        if (e.PropertyName == nameof(VMCentral.UsersStatisticsViewModel.LoadingChartData) && VMCentral.UsersStatisticsViewModel.GetType() == typeof(UsersStatisticsViewModel))
        {
            if (!VMCentral.UsersStatisticsViewModel.LoadingChartData)
            {
                if (VMCentral.UsersStatisticsViewModel.LoadPhase == LoadDataPhase.InLoad)
                {
                    //we ended checking
                    VMCentral.DockingManagerViewModel.LoadCurrentChartsCommands.Execute(true);
                    VMCentral.UsersStatisticsViewModel.LoadPhase = LoadDataPhase.Non;
                }
            }
        }
    }

    private void OnBoardPage_Loaded(object? sender, EventArgs e)

    {
        VMCentral.DockingManagerViewModel.OverViewSubBoard.Add(
         new ChartBoard() { NameInRecord = "users", Title = "Users", Details = "Here are users in spring system.", Count = 21, });

        //solution here as I mentioned before using states as we use before in winforms
        VMCentral.BasePageViewModel.CheckViewStateOnRules.Execute(true);

       
      



    }
}
