

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
               new ChartBoard() { NameInRecord = "users", Title = "Users", Details = "Here are users in spring system.", Count = 21, }

               );


        ChartEntry[] entrs = new ChartEntry[] { 
        new ChartEntry(21){Label="HVC",ValueLabel="112",Color = SKColor.Parse("#c64a44")},
        new ChartEntry(73){Label="Engineering",ValueLabel="432",Color = SKColor.Parse("#0099ff")},
        new ChartEntry(6){Label="IT",ValueLabel="3",Color = SKColor.Parse("#b3db86")},


        };
        VMCentral.DockingManagerViewModel.OverViewSubBoard[0].MyChart = new DonutChart()
        {
            Entries = entrs,
            BackgroundColor = SKColors.Transparent
        };
    }
}
