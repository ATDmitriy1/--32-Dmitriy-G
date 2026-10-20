using Microsoft.Data.Sqlite;

namespace EmployeeApp;

public static class EmployeeRepository
{
    private const string ConnectionString = "Data Source=C:\\Users\\eklim\\OneDrive\\Desktop\\Гиревой д\\Практическое задание\\employees.db";

    public static void Init()
    {
        using var conn = new SqliteConnection(ConnectionString);
        conn.Open();

        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = @"
CREATE TABLE IF NOT EXISTS Employees (
    ""Id""          INTEGER PRIMARY KEY AUTOINCREMENT,
    ""FullName""    TEXT NOT NULL,
    ""Departament"" TEXT NOT NULL,
    ""Position""    TEXT NOT NULL,
    ""Salary""      INTEGER,
    ""HireDate""    INTEGER NOT NULL
);";
            cmd.ExecuteNonQuery();
        }

        long count;
        using (var cmd = conn.CreateCommand())
        {
            cmd.CommandText = "SELECT COUNT(*) FROM Employees";
            count = Convert.ToInt64(cmd.ExecuteScalar());
        }
        if (count > 0) return;

        var seed = new (string Name, string Dept, string Pos, int Salary, DateTime Hire)[]
        {
            ("Иванов Иван Иванович",     "Бухгалтерия", "Бухгалтер",         250000, new DateTime(2019, 3, 12)),
            ("Петрова Анна Сергеевна",   "Бухгалтерия", "Главный бухгалтер", 400000, new DateTime(2016, 9, 1)),
            ("Сидоров Пётр Алексеевич",  "IT",          "Программист",       550000, new DateTime(2021, 5, 20)),
            ("Кузнецова Мария Олеговна", "IT",          "Тестировщик",       380000, new DateTime(2022, 1, 10)),
            ("Ахметов Ерлан Нурланович", "IT",          "Программист",       620000, new DateTime(2018, 11, 5)),
            ("Смирнов Дмитрий Павлович", "Продажи",     "Менеджер",          300000, new DateTime(2020, 7, 15)),
            ("Белова Ольга Игоревна",    "Продажи",     "Менеджер",          320000, new DateTime(2017, 2, 28)),
            ("Омаров Данияр Султанович", "Склад",       "Кладовщик",         220000, new DateTime(2023, 4, 3)),
            ("Волкова Елена Андреевна",  "Кадры",       "Специалист по кадрам", 280000, new DateTime(2019, 8, 19)),
        };

        using var tx = conn.BeginTransaction();
        foreach (var s in seed)
        {
            using var cmd = conn.CreateCommand();
            cmd.Transaction = tx;
            cmd.CommandText = @"INSERT INTO Employees (FullName, Departament, Position, Salary, HireDate)
                                VALUES ($n, $d, $p, $s, $h)";
            cmd.Parameters.AddWithValue("$n", s.Name);
            cmd.Parameters.AddWithValue("$d", s.Dept);
            cmd.Parameters.AddWithValue("$p", s.Pos);
            cmd.Parameters.AddWithValue("$s", s.Salary);
            cmd.Parameters.AddWithValue("$h", new DateTimeOffset(s.Hire).ToUnixTimeSeconds());
            cmd.ExecuteNonQuery();
        }
        tx.Commit();
    }

    public static List<Employee> LoadAll()
    {
        var list = new List<Employee>();

        using var conn = new SqliteConnection(ConnectionString);
        conn.Open();

        using var cmd = conn.CreateCommand();
        cmd.CommandText = "SELECT Id, FullName, Departament, Position, Salary, HireDate FROM Employees";

        using var r = cmd.ExecuteReader();
        while (r.Read())
        {
            list.Add(new Employee
            {
                Id = r.GetInt32(0),
                FullName = r.GetString(1),
                Department = r.GetString(2),
                Position = r.GetString(3),
                Salary = r.IsDBNull(4) ? 0 : r.GetInt32(4),
                HireDate = DateTimeOffset.FromUnixTimeSeconds(r.GetInt64(5)).LocalDateTime
            });
        }
        return list;
    }
}