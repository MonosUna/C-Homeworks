
using System;
using System.Collections.Generic;

namespace GenericRepository
{
    public interface IEntity {int Id { get; }}

    public class Product : IEntity {
        public int Id { get; init; }
        public string Name { get; init; }
        public int Price { get; init; }

        public override string ToString() => $"Товар: ID={Id}, Название={Name}, Цена={Price}";
    }

    public class User : IEntity {
        public int Id { get; init; }
        public string Name { get; init; }

        public override string ToString() => $"Пользователь: ID={Id}, Имя={Name}";
    }

    public class Repository<T> where T : IEntity {
        private readonly Dictionary<int, T> _storage = new();

        public int Count => _storage.Count;

        public void Add(T item) {
            if (item is null) throw new ArgumentNullException(nameof(item));

            if (_storage.ContainsKey(item.Id))
                throw new InvalidOperationException($"Элемент с ID={item.Id} уже существует. Введите другой ID.");
            _storage[item.Id] = item;
        }

        public bool Remove(int id) {
            return _storage.Remove(id);
        }

        public T? GetById(int id) {
            _storage.TryGetValue(id, out T? item);
            return item;
        }

        public IReadOnlyList<T> GetAll() {
            var res = new List<T>(_storage.Values);
            return res;
        }

        public IReadOnlyList<T> Find(Predicate<T> predicate) {
            if (predicate is null) throw new ArgumentNullException(nameof(predicate));
            var res = new List<T>();
            foreach (var item in _storage.Values) {
                if (predicate(item)) {res.Add(item);}
            }
            return res;
        }
    }

    internal class Program {
        static void Main() {
            var productRepo = new Repository<Product>();
            var userRepo = new Repository<User>();

            Console.WriteLine("=== Проверяем функциональность кода (づ•̀ᴗ•́)づ──☆*:・ﾟ ===");
            Console.WriteLine("\n=== Добавляем товары (づ•̀ᴗ•́)づ──☆*:・ﾟ ===");

            productRepo.Add(new Product{Id = 0, Name = "Нож", Price = 1200});
            productRepo.Add(new Product{Id = 1, Name = "Освежитель воздуха", Price = 500});
            productRepo.Add(new Product{Id = 2, Name = "Веревка", Price = 800});
            productRepo.Add(new Product{Id = 3, Name = "Новенький пикап", Price = 25000});
            productRepo.Add(new Product{Id = 4, Name = "Пакеты", Price = 200});
            Console.WriteLine($"Все товаров: {productRepo.Count}");
            Console.WriteLine("\n=== Добавляем людей (づ•̀ᴗ•́)づ──☆*:・ﾟ ===");
            userRepo.Add(new User {Id = 0, Name = "Саша"});
            userRepo.Add(new User {Id = 1, Name = "Маша"});
            userRepo.Add(new User {Id = 2, Name = "Колян"});
            Console.WriteLine($"Пользователей добавлено: {userRepo.Count}");
            Console.WriteLine("\n=== Get (づ•̀ᴗ•́)づ──☆*:・ﾟ ===");

            Product? p1 = productRepo.GetById(2);
            Console.WriteLine($"Товар с Id=2 : {p1 ?? (object)"не найден"}");
            Product? p2 = productRepo.GetById(67);
            Console.WriteLine($"Товар с Id=67: {p2 ?? (object)"не найден"}");
            User? u1 = userRepo.GetById(1);
            Console.WriteLine($"Пользователь с Id=1: {u1}");

            Console.WriteLine("\n=== GetAll (づ•̀ᴗ•́)づ──☆*:・ﾟ ===");

            foreach (var p in productRepo.GetAll())
                Console.WriteLine($"{p}");

            Console.WriteLine("\n=== Find (づ•̀ᴗ•́)づ──☆*:・ﾟ ===");
            Console.WriteLine("=== Ищем продукты дороже 500 ===");
            var expensive = productRepo.Find(p => p.Price > 500);
            foreach (var p in expensive)
                Console.WriteLine($"  {p}");

            Console.WriteLine("\n=== Find (づ•̀ᴗ•́)づ──☆*:・ﾟ ===");
            Console.WriteLine("=== Ищем людей с именем кончающимся на 'аша' ===");
            var filtered = userRepo.Find(u => u.Name.EndsWith("аша"));
            foreach (var u in filtered)
                Console.WriteLine($"  {u}");

            Console.WriteLine("\n=== Remove (づ•̀ᴗ•́)づ──☆*:・ﾟ ===");

            bool removed = productRepo.Remove(2);
            if (removed) {
                Console.WriteLine("Товар с Id=2 удалён.");
            } else {
                Console.WriteLine("Товар с Id=2 не найден для удаления.");
            }
            Console.WriteLine($"Товаров осталось: {productRepo.Count}");

            bool removedMissing = productRepo.Remove(67);
            if (removedMissing) {
                Console.WriteLine("Товар с Id=67 удалён.");
            } else {
                Console.WriteLine("Товар с Id=67 не найден для удаления.");
            }
            Console.WriteLine("\n=== Попытка добавить дубликат (づ•̀ᴗ•́)づ──☆*:・ﾟ ===");
            try {
                productRepo.Add(new Product { Id = 1, Name = "Перчатки", Price = 10 });
            }
            catch (InvalidOperationException ex) {
                Console.WriteLine($"Поймали исключение: {ex.Message}");
            }
        }
    }
}