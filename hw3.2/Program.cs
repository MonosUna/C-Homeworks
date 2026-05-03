using System;
using System.Collections.Generic;

namespace GenericCollections {
    public class Product {
        public string Name { get; init; }
        public decimal Price { get; init; }

        public override string ToString() => $"{Name} ({Price:C})";
    }

    public static class CollectionUtils {
        public static List<T> Distinct<T>(List<T> source) {
            if (source is null) throw new ArgumentNullException(nameof(source));

            var seen = new HashSet<T>();
            var res = new List<T>();

            foreach (T item in source) {
                if (seen.Add(item)) {res.Add(item);}
            }

            return res;
        }

        public static Dictionary<TKey, List<TValue>> GroupBy<TValue, TKey>(
            List<TValue> source,
            Func<TValue, TKey> keySelector) where TKey : notnull {
            if (source is null) throw new ArgumentNullException(nameof(source));
            if (keySelector is null) throw new ArgumentNullException(nameof(keySelector));

            var res = new Dictionary<TKey, List<TValue>>();

            foreach (TValue item in source) {
                if (!res.TryGetValue(keySelector(item), out List<TValue>? group)) {
                    group = new List<TValue>();
                    res[keySelector(item)] = group;
                }
                group.Add(item);
            }

            return res;
        }

        public static Dictionary<TKey, TValue> Merge<TKey, TValue>(
            Dictionary<TKey, TValue> first,
            Dictionary<TKey, TValue> second,
            Func<TValue, TValue, TValue> conflictResolver) where TKey : notnull {
            if (first is null) throw new ArgumentNullException(nameof(first));
            if (second is null) throw new ArgumentNullException(nameof(second));
            if (conflictResolver is null) throw new ArgumentNullException(nameof(conflictResolver));

            var res = new Dictionary<TKey, TValue>(first);

            foreach (var kvp in second) {
                var key = kvp.Key;
                var value = kvp.Value;
                if (res.TryGetValue(key, out TValue? existing)) {
                    res[key] = conflictResolver(existing, value);
                } else {
                    res[key] = value;
                }
            }

            return res;
        }

        public static T MaxBy<T, TKey>(List<T> source, Func<T, TKey> selector)
            where TKey : IComparable<TKey> {
            if (source is null) throw new ArgumentNullException(nameof(source));
            if (selector is null) throw new ArgumentNullException(nameof(selector));
            if (source.Count == 0) {throw new InvalidOperationException("В коллекции нет элементов.");}

            var comparator = Comparer<TKey>.Default;

            T ans = source[0];
            TKey maxKey = selector(ans);

            foreach (var item in source) {
                if (comparator.Compare(selector(item), maxKey) > 0) {
                    ans = item;
                    maxKey = selector(item);
                }
            }

            return ans;
        }
    }

    internal class Program {
        static void Main() {
            Console.WriteLine("=== Distinct (づ•̀ᴗ•́)づ──☆*:・ﾟ ===");

            var ints = new List<int> {1, 2, 3, 4, 3, 4, 1, 5, 6, 6, 7};
            var c1 = CollectionUtils.Distinct(ints);
            Console.WriteLine($"Входные числа: [{string.Join(", ", ints)}]");
            Console.WriteLine($"Без дублей: [{string.Join(", ", c1)}]");

            var words = new List<string> {"саша", "каша", "паша", "каша", "саша", "каша"};
            var c2 = CollectionUtils.Distinct(words);
            Console.WriteLine($"\nВходные Строки: [{string.Join(", ", words)}]");
            Console.WriteLine($"Без дублей: [{string.Join(", ", c2)}]");

            Console.WriteLine("\n=== GroupBy (づ•̀ᴗ•́)づ──☆*:・ﾟ ===");
            Console.WriteLine("\n=== Сортируем слова по длине ===");
            var wordList = new List<string> {"а", "б", "ггг", "бб", "дд", "в", "аа", "сссс"};

            Dictionary<int, List<string>> r1 = CollectionUtils.GroupBy(wordList, w=>w.Length);

            foreach (var (len, group) in r1){
                Console.WriteLine($"длина {len}: [{string.Join(", ", group)}]");
            }

            Console.WriteLine("\n=== Merge (づ•̀ᴗ•́)づ──☆*:・ﾟ ===");
            var c3 = new Dictionary<string, int> {["саша"] = 1, ["сша"] = 2, ["ашан"] = 3};
            var c4 = new Dictionary<string, int> {["сша"] = 1, ["ашан"] = 2, ["шашна"] = 3};

            var merged = CollectionUtils.Merge(c3, c4, (a, b) => a + b);

            Console.WriteLine("Первый: " + PrintDict(c3));
            Console.WriteLine("Второй: " + PrintDict(c4));
            Console.WriteLine("Результат: " + PrintDict(merged));

            Console.WriteLine("\n=== MaxBy (づ•̀ᴗ•́)づ──☆*:・ﾟ ===");

            var products = new List<Product> {
                new() {Name = "Нож", Price = 1500},
                new() {Name = "Веревка", Price = 200},
                new() {Name = "Пакеты", Price = 150},
                new() {Name = "Перчатки", Price = 40},
            };

            Product expensiveOne = CollectionUtils.MaxBy(products, p=>p.Price);
            Console.WriteLine($"Самый дорогой товар: {expensiveOne}");

            Console.WriteLine("\n=== MaxBy на пустом списке (づ•̀ᴗ•́)づ──☆*:・ﾟ ===");
            try {
                CollectionUtils.MaxBy(new List<Product>(), p=>p.Price);
            }
            catch (InvalidOperationException ex) {
                Console.WriteLine($"Исключение: {ex.Message}");
            }
        }

        static string PrintDict<TKey, TValue>(Dictionary<TKey, TValue> dict)
            where TKey : notnull {
            var pairs = new List<string>();
            foreach (var kvp in dict) {
                pairs.Add($"{kvp.Key}={kvp.Value}");
            }
            var res = "{" + string.Join(", ", pairs) + "}";
            return res;
        }
    }
}