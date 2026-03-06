// See https://aka.ms/new-console-template for more information
class Calculator {
    static void CheckExit(string input)
    {
        if (input == "#")
        {
            Console.WriteLine("Выходим из программы. До свидания!");
            Environment.Exit(0);
        }
    }

    static void Main() {
        Console.WriteLine("Это Калькулятор. Вы можете выполнять операции сложения, вычитания, умножения и деления. Чтобы выйти, введите '#'.");
        while (true) {
            double num1;
            while (true) {
                Console.WriteLine("Введите a1:");
                string input1 = Console.ReadLine() ?? "";
                CheckExit(input1);
                if (double.TryParse(input1, out num1)) break;
                Console.WriteLine("Ошибка: введено не число! Попробуйте снова. Или введите '#' для выхода.");
            }
            double num2;
            while (true) {
                Console.WriteLine("Введите a2:");
                string input2 = Console.ReadLine() ?? "";
                CheckExit(input2);
                if (double.TryParse(input2, out num2)) break;
                Console.WriteLine("Ошибка: введено не число!");
            }

            string op;
            while (true) {
                Console.WriteLine("Введите операцию:");
                op = Console.ReadLine() ?? "";
                CheckExit(op);
                if (op=="+" || op=="-" || op=="*" || op=="/") break;
                Console.WriteLine("Ошибка: неизвестная операция! Попробуйте снова. Или введите '#' для выхода.");
            }

            double result=0;
            bool valid=true;

            switch (op) {
                case "+":
                    result = num1+num2;
                    break;

                case "-":
                    result = num1-num2;
                    break;

                case "*":
                    result = num1*num2;
                    break;

                case "/":
                    if (num2==0) {
                        Console.WriteLine("Ошибка: деление на ноль!");
                        valid=false;
                    } else result = num1/num2;
                    break;
            }

            if (valid) Console.WriteLine($"Результат: {result}");

            Console.WriteLine();
        }
    }
}
