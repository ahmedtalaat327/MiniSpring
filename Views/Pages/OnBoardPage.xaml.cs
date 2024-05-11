

using Microcharts;
using MiniSpring.Views.Pages.ChartsPages;
using MiniSpring.Views.Pages.ChartsPages.ViewModel;
using SkiaSharp;
using Spring.StaticVM;
using Spring.ViewModel;
using Spring.ViewModel.Command;
using System.ComponentModel;


namespace MiniSpring.Views.Pages;

public partial class OnBoardPage : ContentPage
{
     
    public OnBoardPage()
	{

		InitializeComponent();
		 
        this.BindingContext = VMCentral.DockingManagerViewModel;

        this.Loaded += OnBoardPage_Loaded;

        VMCentral.UsersStatisticsViewModel.PropertyChanged += UsersStatisticsViewModel_PropertyChanged;


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
        
        VMCentral.DockingManagerViewModel.OverViewSubBoard.Clear();

        VMCentral.DockingManagerViewModel.OverViewSubBoard.Add(
         new ChartBoard() { NameInRecord = "userschart", Title = "Users", Details = "Here are users in spring system." });


        VMCentral.DockingManagerViewModel.OverViewSubBoard.Add(
         new ChartBoard() { NameInRecord = "empschart", Title = "Employees", Details = "Here are employees in company system."});


       

        //start action here 
        VMCentral.UsersStatisticsViewModel.GetCountingAndStoreThem.Execute(true);
    }
}
