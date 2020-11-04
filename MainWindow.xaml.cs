using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.CodeDom;
using System.Windows.Media.TextFormatting;

namespace Hamnen
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        static List<Space> harbour = new List<Space>();
        
        static int dayCounter = 0;
        static int rejected = 0;
        static int numberOfNewBoats = 10;
        
        public MainWindow()
        {
            InitializeComponent();
            Random rnd = new Random();
            
            for (int i = 0; i < 64; i++)
            {
                harbour.Add(new Space(i));
               
            }


            LoadSaved();
            //Update();
            DisplayHarbour();
            displayData();
           
        }
        void SaveToFile()
        {
            StreamWriter sw = new StreamWriter("hamnen.txt", false);
            foreach (var space in harbour)
            {
                if (!space.isFree)
                {
                    foreach (var boat in space.OccupiedBy)
                    {
                        sw.WriteLine($"{space.Number},{space.isFree},{space.IsHalfFull},{space.TimeToFree},{boat.Id},{boat.maxSpeed},{boat.Weight},{boat.BoatType},{GetUniqueProp(boat)}");
                    }
                }
                else
                {
                    sw.WriteLine($"{space.Number}");
                }
            }
            sw.Close();
        }
        void LoadSaved()
        {
            var data = File.ReadAllText("hamnen.txt");
            var lines = data.Split("\n");
            foreach (var item in lines)
            {
                var line = item.Split(",");

                if (line[0] == "")
                {
                    continue;
                }

                if (line.Length > 1)
                {
                    harbour[int.Parse(line[0])].isFree = line[1] == "True";
                    harbour[int.Parse(line[0])].IsHalfFull = line[2] == "True";
                    harbour[int.Parse(line[0])].TimeToFree = int.Parse(line[3]);

                    switch (line[7])
                    {
                        case "Roddbåt":
                            harbour[int.Parse(line[0])].OccupiedBy.Add(new RowBoat
                            {
                                Id = line[4],
                                maxSpeed = int.Parse(line[5]),
                                Weight = int.Parse(line[6]),
                                BoatType = line[7],
                                MaxPassengers = int.Parse(line[8].Split(":")[1])

                            });
                            break;
                        case "Motorbåt":
                            harbour[int.Parse(line[0])].OccupiedBy.Add(new MotorBoat
                            {
                                Id = line[4],
                                maxSpeed = int.Parse(line[5]),
                                Weight = int.Parse(line[6]),
                                BoatType = line[7],
                                Hp = int.Parse(line[8].Split(":")[1])

                            });
                            break;
                        case "Segelbåt":
                            harbour[int.Parse(line[0])].OccupiedBy.Add(new SailBoat
                            {
                                Id = line[4],
                                maxSpeed = int.Parse(line[5]),
                                Weight = int.Parse(line[6]),
                                BoatType = line[7],
                                Length = int.Parse(line[8].Split(":")[1])

                            });
                            break;
                        case "Katamaran":
                            harbour[int.Parse(line[0])].OccupiedBy.Add(new Catamaran
                            {
                                Id = line[4],
                                maxSpeed = int.Parse(line[5]),
                                Weight = int.Parse(line[6]),
                                BoatType = line[7],
                                Bunks = int.Parse(line[8].Split(":")[1])

                            });
                            break;
                        case "Lastfartyg":
                            harbour[int.Parse(line[0])].OccupiedBy.Add(new Freighter
                            {
                                Id = line[4],
                                maxSpeed = int.Parse(line[5]),
                                Weight = int.Parse(line[6]),
                                BoatType = line[7],
                                Cargo = int.Parse(line[8].Split(":")[1])

                            });
                            break;

                    }
                }
                else
                {
                    harbour[int.Parse(line[0])].isFree = true;
                    harbour[int.Parse(line[0])].IsHalfFull = false;
                    harbour[int.Parse(line[0])].TimeToFree = 0;
                }

                
                

                

                
            
            
                           

            }
                    
        }
        static List<Space> findSpace(int size)
        {
            List<List<Space>> freeSpaces = new List<List<Space>>();
            List<Space> freeSpace = new List<Space>();
            List<int> luckor = new List<int>();
            for(int i = 0; i <= harbour.Count - size; i++)
            {
                freeSpace = harbour.GetRange(i, size);
                if(freeSpace.All(s => s.isFree))
                {
                    List<Space> tmpList = new List<Space>();
                    freeSpace.ForEach(s => tmpList.Add(s));
                    
                    
                    int pos = i;
                    if (i < 32)
                    {
                        while (pos + size < 32 && harbour[pos + size].isFree)
                        {
                            tmpList.Add(harbour[pos + size]);
                            pos++;
                        }
                        pos = i;

                        while (pos - 1 >= 0 && harbour[pos - 1].isFree)
                        {
                            tmpList.Add(harbour[pos - 1]);
                            pos--;
                        }
                    }
                    else
                    {
                        while (pos + size < 64 && harbour[pos + size].isFree)
                        {
                            tmpList.Add(harbour[pos + size]);
                            pos++;
                        }
                        pos = i;

                        while (pos - 1 >= 32 && harbour[pos - 1].isFree)
                        {
                            tmpList.Add(harbour[pos - 1]);
                            pos--;
                        }

                    }
                    freeSpaces.Add(tmpList);

                }
                freeSpace.Clear();
            }

            try
            {
                return freeSpaces
                .Where(fs => !(fs.Any(s => s.Number == 31) && fs.Any(s => s.Number == 32)))
                .OrderBy(fs => fs.Count())
                .First()
                .Take(size)
                .ToList();
            }
            catch
            {
                return freeSpace;
            }
        }

        static List<Boat> generateIncoming(int numberOfBoats, Random rnd)
        {
            List<Boat> incoming = new List<Boat>();
            for (int i = 0; i < numberOfBoats; i++)
            {
                int r = rnd.Next(0, 5);
                switch (r)
                {
                    case 0:
                        incoming.Add(new RowBoat(rnd));
                        break;
                    case 1:
                        incoming.Add(new MotorBoat(rnd));
                        break;
                    case 2:
                        incoming.Add(new SailBoat(rnd));
                        break;
                    case 3:
                        incoming.Add(new Catamaran(rnd));
                        break;
                    case 4:
                        incoming.Add(new Freighter(rnd));
                        break;
                }
            }
            return incoming;

               





        }

        static string GetUniqueProp(Boat boat)
        {
            string x = "";
            if (boat is RowBoat)
            {
                var tmp = (RowBoat)boat;
                x = tmp.GetU();
            }
            if (boat is MotorBoat)
            {
                var tmp = (MotorBoat)boat;
                x = tmp.GetU();
            }
            if (boat is SailBoat)
            {
                var tmp = (SailBoat)boat;
                x = tmp.GetU();
            }
            if (boat is Catamaran)
            {
                var tmp = (Catamaran)boat;
                x = tmp.GetU();
            }
            if (boat is Freighter)
            {
                var tmp = (Freighter)boat;
                x = tmp.GetU();
            }
            return x;

        }

        void DisplayHarbour()
        { 
            harbourInfo.Text = "";
            gridInfo1.Items.Clear();
            gridInfo2.Items.Clear();
            foreach (var space in harbour) //Iterate over harbour
            {
               
                if (space.Number < 32) //If harbour 1 (left)
                {
                    if (space.isFree)
                    {
                        gridInfo1.Items.Add(new //Assign free space 
                        {
                            spaceno = space.Number,
                            boatType = "Tom plats",
                            Id = "",
                            maxSpeed = "0",
                            Weight = "",
                            ColorSet = "Green",
                            Other = ""
                        });


                    }
                    else
                    {
                        foreach (var boat in space.OccupiedBy) //There can be two boats in one space, if so, draw the space for both boats
                        {
                            gridInfo1.Items.Add(new
                            {
                                spaceno = space.Number,
                                boatType = boat.BoatType,
                                Id = boat.Id,
                                maxSpeed = boat.maxSpeed,
                                Weight = boat.Weight,
                                ColorSet = space.IsNew ? "Magenta" : "Yellow",
                                Other = GetUniqueProp(boat)

                            });
                            
                        }
                    }
                }

                else //if harbour 2 (right), do the same thing
                {
                    if (space.isFree)
                    {
                        gridInfo2.Items.Add(new
                        {
                            spaceno = space.Number,
                            boatType = "Tom plats",
                            Id = "",
                            maxSpeed = "",
                            Weight = "",
                            ColorSet = "Green",
                            Other = ""
                        });


                    }
                    else
                    {
                        foreach (var boat in space.OccupiedBy)
                        {
                            gridInfo2.Items.Add(new
                            {
                                spaceno = space.Number,
                                boatType = boat.BoatType,
                                Id = boat.Id,
                                maxSpeed = boat.maxSpeed,
                                Weight = boat.Weight,
                                ColorSet = space.IsNew ? "Magenta" : "Yellow",
                                Other = GetUniqueProp(boat)

                            });

                        }
                    }
                }
            }








            


        }
        void displayData()
        { //Display stats in middle column
            var ids = new List<Boat>();
            
            foreach (var space in harbour)
            {
                if (space.OccupiedBy.Any())
                {
                    foreach (var boat in space.OccupiedBy)
                    {
                        if (!ids.Any(b => b.Id == boat.Id))
                        {
                            ids.Add(boat);
                        }
                        
                    }
                }
            }
            totalBoats.Content = ids
                .Count();

            totalRowboats.Content = ids
                .Where(b => b is RowBoat)
                .Count();
            totalMotorboats.Content = ids
                .Where(b => b is MotorBoat)
                .Count();
            totalSailboats.Content = ids
                .Where(b => b is SailBoat)
                .Count();
            totalCatamarans.Content = ids
                .Where(b => b is Catamaran)
                .Count();
            totalFreighters.Content = ids
                .Where(b => b is Freighter)
                .Count();

            totalWeight.Content = ids
                .Sum(b => b.Weight);
            try
            {
                avgMaxSpeed.Content = (ids.Sum(b => b.maxSpeed) / ids.Count()) * 1.852;
            }
            catch
            {
                avgMaxSpeed.Content = 0;
            }
            numberOfFreeSpaces.Content = harbour
                .Count(s => s.isFree);

            dayCounterLabel.Content = dayCounter;

        }

        void Update()
        {
            Random rnd = new Random();
            IncomingGrid.Items.Clear();

            //Get new boats
            numberOfNewBoats = newBoatsSelector.SelectedIndex + 1;
            var incoming = generateIncoming(numberOfNewBoats, rnd);
            dayCounter++;
            
            foreach (var space in harbour)
            {
                space.TimeToFree--;
                space.IsNew = false;
                if (space.TimeToFree == 0)
                {
                    space.isFree = true;
                    space.IsHalfFull = false;
                    space.OccupiedBy.Clear();
                }
            }
            
            //Iterate over incoming boats
            foreach (var boat in incoming)
            {
                if (boat is RowBoat)
                {
                    //For rowboat, find free or halffree space, first look for half-free...

                    if (harbour.Any(s => s.IsHalfFull))
                    {
                        var activeSpace =
                        harbour
                            .Where(s => s.IsHalfFull)
                            .First();
                        activeSpace.IsHalfFull = false;
                        activeSpace.TimeToFree = 1;
                        activeSpace.OccupiedBy.Add(boat);
                        activeSpace.IsNew = true;

                        IncomingGrid.Items.Add(new
                        {
                            Id = boat.Id,
                            BoatType = boat.BoatType,
                            ToSpace = activeSpace.Number,
                            ColorSet = "Magenta"
                        });
                    }
                    else //...then look for free spaces
                    {
                        

                        if (findSpace(1).Count() > 0)
                        {
                            var activeSpace = findSpace(1)[0];

                            activeSpace.isFree = false;
                            activeSpace.IsHalfFull = true;
                            activeSpace.TimeToFree = 1;
                            activeSpace.OccupiedBy.Add(boat);
                            activeSpace.IsNew = true;


                            IncomingGrid.Items.Add(new //Add to datagrid
                            {
                                Id = boat.Id,
                                BoatType = boat.BoatType,
                                ToSpace = activeSpace.Number,
                                ColorSet = "Magenta"
                            });
                        }
                        else
                        {
                            //if no space found, reject.

                            IncomingGrid.Items.Add(new
                            {
                                Id = boat.Id,
                                BoatType = boat.BoatType,
                                ToSpace = "Avvisad",
                                ColorSet = "Red"
                            });
                            rejected++;
                        }
                    }

                }
                else
                { //Now check other boat types

                    var freeSpace = findSpace(boat.Size); //Call the spacefinding method
                    if (freeSpace.Count == 0) //Reject if no space found
                    {
                        IncomingGrid.Items.Add(new
                        {
                            Id = boat.Id,
                            BoatType = boat.BoatType,
                            ToSpace = "Avvisad",
                            ColorSet = "Red"
                        });
                        
                        
                        rejected++;
                        rejectedLabel.Content = rejected;
                    }
                    else
                    { //Assign boat to space
                        foreach (var space in freeSpace)
                        {
                            space.isFree = false;
                            space.OccupiedBy.Add(boat);
                            space.TimeToFree = boat.TimeToDeparture;
                            space.IsNew = true;
                        }
                        IncomingGrid.Items.Add(new //Add to Datagrid for new boats
                        {
                            Id = boat.Id,
                            BoatType = boat.BoatType,
                            ToSpace = $"#{freeSpace[0].Number} - {freeSpace[freeSpace.Count-1].Number}",
                            ColorSet ="Magenta"
                        });
                        
                    }
                }


                
            }
            
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Update();
            displayData();
            DisplayHarbour();
            SaveToFile();
        }
    }
    class Boat
    {
        public string Id { get; set; }
        public int maxSpeed { get; set; }
        public int Weight { get; set; }
        public int TimeToDeparture { get; set; }
        public int Size { get; set; }
        public string BoatType { get; set; }
        public string U { get; set; }
        

        


        public string GenerateId(Random rnd)
        {
            string id = "";
            for (int i = 0; i < 3; i++)
            {
                id += (char)rnd.Next(65, 91);
            }
            return id;
        }
       
        public Boat()
        {
        }


    }
    class RowBoat : Boat
    {
        public int MaxPassengers { get; set; }
        public RowBoat(Random rnd)
        {
            Id = $"R - {GenerateId(rnd)}";
            Weight = rnd.Next(100, 301);
            maxSpeed = rnd.Next(0, 4);
            MaxPassengers = rnd.Next(1, 7);
            TimeToDeparture = 1;
            BoatType = "Roddbåt";
            
        }
        public string GetU()
        {
            return $"Max passagerare : {MaxPassengers}";
        }
        public RowBoat()
        {

        }
    }

    class MotorBoat : Boat
    {
        public int Hp { get; set; }
        public MotorBoat(Random rnd)
        {
            Id = $"M - {GenerateId(rnd)}";
            Weight = rnd.Next(200, 3001);
            maxSpeed = rnd.Next(0, 61);
            Hp = rnd.Next(10, 1001);
            TimeToDeparture = 3;
            Size = 1;
            BoatType = "Motorbåt";
            

        }
        public string GetU()
        {
            return $"Hästkrafter : {Hp}";
        }

        public MotorBoat()
        {
        }

    }
    class SailBoat : Boat
    {
        public int Length { get; set; }
        public SailBoat(Random rnd)
        {
            Id = $"S - {GenerateId(rnd)}";
            Weight = rnd.Next(800, 6001);
            maxSpeed = rnd.Next(0, 12);
            Length = rnd.Next(10, 61);
            TimeToDeparture = 4;
            Size = 2;
            BoatType = "Segelbåt";
        }
        public string GetU()
        {
            return $"Längd : {Length}";
        }
        public SailBoat()
        {

        }
    }
    class Catamaran : Boat
    {
        public int Bunks { get; set; }
        public Catamaran (Random rnd)
        {
            Id = $"K - {GenerateId(rnd)}";
            Weight = rnd.Next(1200, 8001);
            maxSpeed = rnd.Next(0, 12);
            Bunks = rnd.Next(1, 5);
            TimeToDeparture = 3;
            Size = 3;
            BoatType = "Katamaran";
        }
        public string GetU()
        {
            return $"Sängplatser : {Bunks}";
        }
        public Catamaran()
        {

        }
    }
    class Freighter : Boat
    {
        public int Cargo { get; set; }
        public Freighter(Random rnd)
        {
            Id = $"L - {GenerateId(rnd)}";
            Weight = rnd.Next(3000, 20001);
            maxSpeed = rnd.Next(0, 21);
            Cargo = rnd.Next(0, 501);
            TimeToDeparture = 6;
            Size = 4;
            BoatType = "Lastfartyg";
        }
        public string GetU()
        {
            return $"Containrar : {Cargo}";
        }
        public Freighter()
        {

        }
    }
    class Space
    {
        public bool isFree { get; set; }
        public bool IsHalfFull { get; set; }
        public int TimeToFree { get; set; }
        public List<Boat> OccupiedBy { get; set; }
        public int Number { get; set; }
        public bool IsNew { get; set; }
        public Space(int number)
        {

            isFree = true;
            IsHalfFull = false;
            TimeToFree = 0;
            OccupiedBy = new List<Boat>();
            Number = number;
        }
    }
}
