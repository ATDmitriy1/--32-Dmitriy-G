namespace EmployeeApp;

public class MainForm : Form
{
    private const string AllDepartments = "Все отделы";

    private readonly ComboBox cbDept = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 150 };
    private readonly TextBox txtMin = new() { Width = 80 };
    private readonly TextBox txtMax = new() { Width = 80 };
    private readonly TextBox txtSearch = new() { Width = 160 };
    private readonly ComboBox cbSort = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 300 };
    private readonly DataGridView grid = new()
    {
        Dock = DockStyle.Fill,
        ReadOnly = true,
        AllowUserToAddRows = false,
        AllowUserToDeleteRows = false,
        AutoGenerateColumns = false,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        RowHeadersVisible = false
    };
    private readonly Label lblCount = new() { Dock = DockStyle.Bottom, Height = 24, TextAlign = ContentAlignment.MiddleLeft };

    private List<Employee> employees = new();
    private bool loading = true;

    public MainForm()
    {
        Text = "Сотрудники (LINQ: фильтрация и сортировка)";
        Width = 950;
        Height = 560;
        StartPosition = FormStartPosition.CenterScreen;

        BuildUi();
        LoadData();
    }

    private void BuildUi()
    {
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            Padding = new Padding(8),
            WrapContents = true
        };

        panel.Controls.Add(MakeLabel("Отдел:"));
        panel.Controls.Add(cbDept);
        panel.Controls.Add(MakeLabel("Оклад от:"));
        panel.Controls.Add(txtMin);
        panel.Controls.Add(MakeLabel("до:"));
        panel.Controls.Add(txtMax);
        panel.Controls.Add(MakeLabel("Поиск по ФИО:"));
        panel.Controls.Add(txtSearch);
        panel.Controls.Add(MakeLabel("Сортировка:"));
        panel.Controls.Add(cbSort);

        cbSort.Items.AddRange(new object[]
        {
            "ФИО (А → Я)",
            "Оклад (по убыванию)",
            "Отдел ↑, затем Оклад ↓",
            "Должность ↑, затем Дата приема ↑"
        });
        cbSort.SelectedIndex = 0;

        grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(Employee.Id), HeaderText = "Id", FillWeight = 30 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(Employee.FullName), HeaderText = "ФИО", FillWeight = 130 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(Employee.Department), HeaderText = "Отдел", FillWeight = 70 });
        grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = nameof(Employee.Position), HeaderText = "Должность", FillWeight = 90 });
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(Employee.Salary),
            HeaderText = "Оклад",
            FillWeight = 60,
            DefaultCellStyle = new DataGridViewCellStyle { Format = "N0", Alignment = DataGridViewContentAlignment.MiddleRight }
        });
        grid.Columns.Add(new DataGridViewTextBoxColumn
        {
            DataPropertyName = nameof(Employee.HireDate),
            HeaderText = "Дата приема",
            FillWeight = 60,
            DefaultCellStyle = new DataGridViewCellStyle { Format = "dd.MM.yyyy" }
        });

        Controls.Add(grid);
        Controls.Add(lblCount);
        Controls.Add(panel);

        cbDept.SelectedIndexChanged += (_, _) => ApplyFilters();
        cbSort.SelectedIndexChanged += (_, _) => ApplyFilters();
        txtMin.TextChanged += (_, _) => ApplyFilters();
        txtMax.TextChanged += (_, _) => ApplyFilters();
        txtSearch.TextChanged += (_, _) => ApplyFilters();
    }

    private static Label MakeLabel(string text) =>
        new() { Text = text, AutoSize = true, Margin = new Padding(8, 6, 2, 0) };

    private void LoadData()
    {
        employees = EmployeeRepository.LoadAll();

        cbDept.Items.Clear();
        cbDept.Items.Add(AllDepartments);
        foreach (var d in employees.Select(e => e.Department).Distinct().OrderBy(d => d))
            cbDept.Items.Add(d);
        cbDept.SelectedIndex = 0;

        loading = false;
        ApplyFilters();
    }

    private void ApplyFilters()
    {
        if (loading) return;

        IEnumerable<Employee> query = employees;

        if (cbDept.SelectedItem is string selectedDept && selectedDept != AllDepartments)
            query = query.Where(e => e.Department == selectedDept);

        int minSalary = int.TryParse(txtMin.Text.Trim(), out var mn) ? mn : int.MinValue;
        int maxSalary = int.TryParse(txtMax.Text.Trim(), out var mx) ? mx : int.MaxValue;
        query = query.Where(e => e.Salary >= minSalary && e.Salary <= maxSalary);

        string searchText = txtSearch.Text.Trim();
        if (searchText.Length > 0)
            query = query.Where(e => e.FullName.Contains(searchText, StringComparison.OrdinalIgnoreCase));

        IOrderedEnumerable<Employee> sorted = cbSort.SelectedIndex switch
        {
            1 => query.OrderByDescending(e => e.Salary),
            2 => query.OrderBy(e => e.Department).ThenByDescending(e => e.Salary),
            3 => query.OrderBy(e => e.Position).ThenBy(e => e.HireDate),
            _ => query.OrderBy(e => e.FullName)
        };

        var result = sorted.ToList();
        grid.DataSource = result;
        lblCount.Text = $"  Найдено: {result.Count} из {employees.Count}";
    }
}