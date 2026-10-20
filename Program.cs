using WinFormsApp1;

namespace EmployeeApp;

internal static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        SQLitePCL.Batteries.Init();
        EmployeeRepository.Init();
        Application.Run(new MainForm());
    }
}