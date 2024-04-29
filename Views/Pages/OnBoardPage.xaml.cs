
using Spring.StaticVM;

namespace MiniSpring.Views.Pages;

public partial class OnBoardPage : ContentPage
{
	 
	public OnBoardPage()
	{
		InitializeComponent();
		 
		 

        this.BindingContext = VMCentral.DockingManagerViewModel;
	}

    
}