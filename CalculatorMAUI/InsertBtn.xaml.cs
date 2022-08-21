namespace CalculatorMAUI;

public partial class InsertBtn : Button
{
	public InsertBtn()
	{
		InitializeComponent();
	}

	private void Button_Loaded(object sender, EventArgs e)
	{
        if (CommandParameter == null)
        {
            CommandParameter = Text;
        }
    }
}
