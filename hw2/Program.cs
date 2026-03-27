using System;


enum CarType {
    Tesla,
    Mercedes,
    Audi,
    Lada
}


interface ICar {
    string GetDescription();
}
interface IElectric {}
interface IMechanicalEngine {}
interface IAutomatical {}
interface IManual {}


abstract class ACar : ICar {
    protected string Model;
    protected string Color;
    protected double Volume;
    protected int Seats;
    protected int MaxSpeed;
    protected bool Android;

    public virtual string GetDescription() {
        string EngineDesc;
        string Transmission;
        string AndroidDesc;
        string VolumeDesc;
        if (this is IElectric) {
            EngineDesc="электрический двигатель";
        } else {
            EngineDesc="бензиновый двигатель";
        }
        if (this is IAutomatical) {
            Transmission="на автомате";
        } else {
            Transmission="на механике";
        }
        if (Android) {
            AndroidDesc="есть Android система";
        } else {
            AndroidDesc="нет Android системы";
        }
        if (Volume == 0.0) {
            VolumeDesc="";
        } else {
            VolumeDesc=$"объем двигателя {Volume}л,";
        }
        return $"{Model}: {EngineDesc}, {Transmission}, {Seats} мест, макс скорость {MaxSpeed} км/ч, цвет {Color}, {VolumeDesc} {AndroidDesc}";
    }
}

class Tesla : ACar, IElectric, IAutomatical {
    public Tesla()
    {
        Model="Tesla model 3";
        Seats=5;
        MaxSpeed=210;
        Color="черный";
        Android=true;
        Volume=0.0;
    }
}

class Mercedes : ACar, IMechanicalEngine, IAutomatical {
    public Mercedes() {
        Model="Mercedes-benz";
        Seats=6;
        MaxSpeed=200;
        Color="белый";
        Android=false;
        Volume=2.5;
    }
}

class Audi : ACar, IMechanicalEngine, IAutomatical {
    public Audi() {
        Model="Audi Q7";
        Seats=5;
        MaxSpeed=240;
        Color="красный";
        Android=true;
        Volume=3.5;
    }
}

class Lada : ACar, IMechanicalEngine, IManual {
    public Lada() {
        Model="Lada Granta";
        Seats=10;
        MaxSpeed=100500;
        Color="черный хром";
        Android=true;
        Volume=112.0;
    }
}


class CarFactory {
    public static ICar Create(CarType type) {
        return type switch {
            CarType.Tesla => new Tesla(),
            CarType.Mercedes => new Mercedes(),
            CarType.Audi => new Audi(),
            CarType.Lada => new Lada(),
            _ => throw new Exception()
        };
    }
}

class Program {
    static void Main() {
        while (true) {
            Console.WriteLine("Введите марку автомобиля или done для остановки ввода: ");
            string input = Console.ReadLine();
            if (input.ToLower() == "done") break;
            try {
                CarType type = (CarType)Enum.Parse(typeof(CarType), input, true);
                Console.WriteLine(CarFactory.Create(type).GetDescription());
            } catch {
                Console.WriteLine("Неизвестная нам марка автомобиля");
            }
        }
    }
}