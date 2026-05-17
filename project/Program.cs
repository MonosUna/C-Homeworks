using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace TaskHub {
    enum Priority { Low, Medium, High }
    enum TaskStatus { New, InProgress, Done }

    interface IEntity { int Id { get; } }

    class HubTask : IEntity {
        public int Id { get; set; }
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public Priority Priority { get; set; }
        public DateTime Deadline { get; set; }
        public TaskStatus Status { get; set; }
        public bool IsDone => Status == TaskStatus.Done;
        public bool IsOverdue => !IsDone && Deadline < DateTime.Now;
        public override string ToString() {
            return $"ID={Id}, Название={Title}, Описание={Description}, Приоритет={Priority}, Дедлайн={Deadline:dd.MM.yyyy HH:mm}, Статус={Status}" + (IsOverdue ? " ПРОСРОЧЕНА" : "");
        }
    }

    class Repository<T> where T : IEntity {
        private readonly Dictionary<int, T> _storage = new();
        public int Count => _storage.Count;
        public void Add(T item) {
            if (_storage.ContainsKey(item.Id)) throw new Exception($"Элемент с ID={item.Id} уже существует.");
            _storage[item.Id] = item;
        }
        public bool Remove(int id) => _storage.Remove(id);
        public T? GetById(int id) {
            _storage.TryGetValue(id, out T? item);
            return item;
        }
        public IReadOnlyList<T> GetAll() => new List<T>(_storage.Values);
        public IReadOnlyList<T> Find(Predicate<T> predicate) {
            var res = new List<T>();
            foreach (var item in _storage.Values)
                if (predicate(item)) res.Add(item);
            return res;
        }
        public void Clear() => _storage.Clear();
    }

    static class TaskUtils {
        public static void PrintTasks(IReadOnlyList<HubTask> tasks) {
            if (tasks.Count == 0) {
                Console.WriteLine("Задач нет.");
                return;
            }
            foreach (var task in tasks) Console.WriteLine(task);
        }
        public static int NextId(IReadOnlyList<HubTask> tasks) {
            int max = 0;
            foreach (var task in tasks)
                if (task.Id > max) max = task.Id;
            return max + 1;
        }
    }

    class TaskFileStorage : IDisposable {
        private readonly string _path;
        private bool _disposed;
        public TaskFileStorage(string path) { _path = path; }
        public async Task SaveAsync(IReadOnlyList<HubTask> tasks) {
            if (_disposed) throw new ObjectDisposedException(nameof(TaskFileStorage));
            await File.WriteAllTextAsync(_path, JsonSerializer.Serialize(tasks, new JsonSerializerOptions { WriteIndented = true }));
        }
        public async Task<List<HubTask>> LoadAsync() {
            if (_disposed) throw new ObjectDisposedException(nameof(TaskFileStorage));
            if (!File.Exists(_path)) return new List<HubTask>();
            string json = await File.ReadAllTextAsync(_path);
            if (string.IsNullOrWhiteSpace(json)) return new List<HubTask>();
            return JsonSerializer.Deserialize<List<HubTask>>(json) ?? new List<HubTask>();
        }
        public void Dispose() { _disposed = true; }
    }

    class DeadlineChecker : IDisposable {
        private readonly Repository<HubTask> _repo;
        private readonly CancellationTokenSource _cts = new();
        private Task? _task;
        public DeadlineChecker(Repository<HubTask> repo) { _repo = repo; }
        public void Start() {
            _task = Task.Run(async () => {
                while (!_cts.Token.IsCancellationRequested) {
                    try {
                        await Task.Delay(7000, _cts.Token);
                        var overdue = _repo.Find(t => t.IsOverdue);
                        if (overdue.Count == 0) continue;
                        Console.WriteLine("\n=== Есть просроченные задачи (づ•̀ᴗ•́)づ──☆*:・ﾟ ===");
                        foreach (var task in overdue) Console.WriteLine(task);
                        Console.WriteLine();
                    } catch (TaskCanceledException) { break; }
                }
            });
        }
        public void Dispose() {
            _cts.Cancel();
            try { _task?.Wait(); } catch {}
            _cts.Dispose();
        }
    }

    delegate bool TaskFilter(HubTask task);

    class Program {
        static Repository<HubTask> repo = new();
        static int nextId = 1;
        const string FilePath = "tasks.json";

        static async Task Main() {
            using var storage = new TaskFileStorage(FilePath);
            using var checker = new DeadlineChecker(repo);
            checker.Start();
            Console.WriteLine("=== TaskHub - менеджер задач (づ•̀ᴗ•́)づ──☆*:・ﾟ ===");
            while (true) {
                Console.WriteLine("1. Создать задачу");
                Console.WriteLine("2. Просмотр задач");
                Console.WriteLine("3. Редактировать задачу");
                Console.WriteLine("4. Удалить задачу");
                Console.WriteLine("5. Поиск задач");
                Console.WriteLine("6. Статистика");
                Console.WriteLine("7. Сохранить задачи в файл");
                Console.WriteLine("8. Загрузить задачи из файла");
                Console.WriteLine("0. Выход");
                try {
                    switch (ReadString("Выберите пункт: ")) {
                        case "1": CreateTask(); break;
                        case "2": ViewTasks(); break;
                        case "3": EditTask(); break;
                        case "4": DeleteTask(); break;
                        case "5": SearchTasks(); break;
                        case "6": ShowStats(); break;
                        case "7":
                            await storage.SaveAsync(repo.GetAll());
                            Console.WriteLine("Задачи сохранены.");
                            break;
                        case "8":
                            repo.Clear();
                            foreach (var task in await storage.LoadAsync()) repo.Add(task);
                            nextId = TaskUtils.NextId(repo.GetAll());
                            Console.WriteLine($"Загружено задач: {repo.Count}");
                            break;
                        case "0":
                            Console.WriteLine("Завершаемся. ПОКА!");
                            return;
                        default: Console.WriteLine("Не понимаю, что от меня хотят."); break;
                    }
                } catch (Exception ex) {
                    Console.WriteLine($"Ошибка: {ex.Message}");
                }
                Console.WriteLine();
            }
        }

        static void CreateTask() {
            Console.WriteLine("=== Создание задачи (づ•̀ᴗ•́)づ──☆*:・ﾟ ===");
            repo.Add(new HubTask {
                Id = nextId++,
                Title = ReadString("Введите название: "),
                Description = ReadString("Введите описание: "),
                Priority = ReadEnum<Priority>("Введите приоритет (Low / Medium / High): "),
                Deadline = ReadDate("Введите дедлайн (дд.мм.гггг чч:мм): "),
                Status = ReadEnum<TaskStatus>("Введите статус (New / InProgress / Done): ")
            });
            Console.WriteLine("Задача создана.");
        }

        static void ViewTasks() {
            Console.WriteLine("1. Все задачи");
            Console.WriteLine("2. Выполненные");
            Console.WriteLine("3. Невыполненные");
            Console.WriteLine("4. С высоким приоритетом");
            switch (ReadString("Выберите пункт: ")) {
                case "1": TaskUtils.PrintTasks(repo.GetAll()); break;
                case "2": TaskUtils.PrintTasks(repo.Find(t => t.IsDone)); break;
                case "3": TaskUtils.PrintTasks(repo.Find(t => !t.IsDone)); break;
                case "4": TaskUtils.PrintTasks(repo.Find(t => t.Priority == Priority.High)); break;
                default: Console.WriteLine("Неизвестный пункт."); break;
            }
        }

        static void EditTask() {
            HubTask? task = repo.GetById(ReadInt("Введите ID задачи: "));
            if (task is null) {
                Console.WriteLine("Задача не найдена.");
                return;
            }
            Console.WriteLine(task);
            string inp = ReadStringAllowEmpty("Новое название или Enter: ");
            if (!string.IsNullOrWhiteSpace(inp)) task.Title = inp;
            inp = ReadStringAllowEmpty("Новое описание или Enter: ");
            if (!string.IsNullOrWhiteSpace(inp)) task.Description = inp;
            inp = ReadStringAllowEmpty("Новый приоритет или Enter: ");
            if (!string.IsNullOrWhiteSpace(inp) && Enum.TryParse(inp, true, out Priority p)) task.Priority = p;
            inp = ReadStringAllowEmpty("Новый статус или Enter: ");
            if (!string.IsNullOrWhiteSpace(inp) && Enum.TryParse(inp, true, out TaskStatus s)) task.Status = s;
            Console.WriteLine("Задача обновлена.");
        }

        static void DeleteTask() {
            if (repo.Remove(ReadInt("Введите ID задачи: "))) Console.WriteLine("Задача удалена.");
            else Console.WriteLine("Задача не найдена.");
        }

        static void SearchTasks() {
            Console.WriteLine("1. По названию");
            Console.WriteLine("2. По статусу");
            Console.WriteLine("3. По приоритету");
            TaskFilter filter;
            switch (ReadString("Выберите пункт: ")) {
                case "1":
                    string title = ReadString("Введите название: ");
                    filter = t => t.Title.Contains(title, StringComparison.OrdinalIgnoreCase);
                    break;
                case "2":
                    TaskStatus status = ReadEnum<TaskStatus>("Введите статус: ");
                    filter = t => t.Status == status;
                    break;
                case "3":
                    Priority priority = ReadEnum<Priority>("Введите приоритет: ");
                    filter = t => t.Priority == priority;
                    break;
                default:
                    Console.WriteLine("Неизвестный пункт.");
                    return;
            }
            TaskUtils.PrintTasks(repo.Find(t => filter(t)));
        }

        static void ShowStats() {
            int done = 0, overdue = 0;
            var priorities = new Dictionary<Priority, int> {{Priority.Low, 0}, {Priority.Medium, 0}, {Priority.High, 0}};
            foreach (var task in repo.GetAll()) {
                if (task.IsDone) done++;
                if (task.IsOverdue) overdue++;
                priorities[task.Priority]++;
            }
            Console.WriteLine($"Всего задач: {repo.Count}");
            Console.WriteLine($"Выполненных: {done}");
            Console.WriteLine($"Просроченных: {overdue}");
            foreach (var kvp in priorities) Console.WriteLine($"{kvp.Key}: {kvp.Value}");
        }

        static string ReadString(string text) {
            while (true) {
                Console.Write(text);
                string inp = Console.ReadLine() ?? "";
                if (!string.IsNullOrWhiteSpace(inp)) return inp;
                Console.WriteLine("Пустой ввод, попробуйте снова.");
            }
        }

        static string ReadStringAllowEmpty(string text) {
            Console.Write(text);
            return Console.ReadLine() ?? "";
        }

        static int ReadInt(string text) {
            while (true) {
                Console.Write(text);
                if (int.TryParse(Console.ReadLine(), out int res)) return res;
                Console.WriteLine("Ошибка: введено не число.");
            }
        }

        static DateTime ReadDate(string text) {
            while (true) {
                Console.Write(text);
                string inp = Console.ReadLine() ?? "";
                string[] formats = {"dd.MM.yyyy HH:mm", "dd.MM.yyyy", "dd MM yyyy HH:mm", "dd MM yyyy", "d.M.yyyy H:mm", "d.M.yyyy"};
                if (DateTime.TryParseExact(inp, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime res)) return res;
                Console.WriteLine("Ошибка: введите дату в формате дд.мм.гггг чч:мм или дд мм гггг.");
            }
        }

        static T ReadEnum<T>(string text) where T : struct {
            while (true) {
                Console.Write(text);
                if (Enum.TryParse(Console.ReadLine(), true, out T res) && Enum.IsDefined(typeof(T), res)) return res;
                Console.WriteLine("Ошибка: неизвестное значение.");
            }
        }
    }
}