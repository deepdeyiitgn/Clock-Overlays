public partial class SplashForm : Form
{
    private static readonly string[] facts = {
        "Fact 1: The first fact.",
        "Fact 2: The second fact.",
        "Fact 3: The third fact."
    };

    public SplashForm()
    {
        InitializeComponent();
        // Show a random fact
        Random random = new Random();
        int index = random.Next(facts.Length);
        factLabel.Text = facts[index]; // Assuming there's a label to show the fact
    }
}