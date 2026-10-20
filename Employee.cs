namespace EmployeeApp;

public class Employee
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Department { get; set; } = "";
    public string Position { get; set; } = "";
    public int Salary { get; set; }
    public DateTime HireDate { get; set; }
}