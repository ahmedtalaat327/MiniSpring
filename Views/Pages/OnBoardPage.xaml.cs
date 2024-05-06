

using Microcharts;
using MiniSpring.Views.Pages.ChartsPages;
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

        
    }

    private void OnBoardPage_Loaded(object? sender, EventArgs e)

    {
        VMCentral.DockingManagerViewModel.OverViewSubBoard.Add(
         new ChartBoard() { NameInRecord = "users", Title = "Users", Details = "Here are users in spring system.", Count = 21, });

        //solution here as I mentioned before using states as we use before in winforms
        VMCentral.BasePageViewModel.CheckViewStateOnRules.Execute(true);

        VMCentral.UsersStatisticsViewModel.GetCountingAndStoreThem.Execute(true);
      
        VMCentral.DockingManagerViewModel.LoadCurrentChartsCommands.Execute(true);



    }
}
