using System;
using System.Linq;
using System.Threading.Tasks;

namespace csharp_console
{
    public class Program
    {
        public static void Main(string[] args)
        {
            const int size = 20;
            const int speed = 100;

            var gameOfLife = new GameOfLife(size);

            gameOfLife.SetWorldData(GenerateRandomData(size));
            //gameOfLife.SetWorldData(new bool[size,size]);
            //gameOfLife.UpdateWorldData(5, 5, true);
            //gameOfLife.UpdateWorldData(5, 6, true);
            //gameOfLife.UpdateWorldData(5, 7, true);

            PrintWorld(gameOfLife.WorldData);

            for (var cycles = 0; cycles <= 1000; cycles++)
            {
                Task.Delay(speed).Wait();
                gameOfLife.Update().Wait();
                PrintWorld(gameOfLife.WorldData);
            }

            Console.WriteLine();
            Console.WriteLine();
            Console.Write("Press any key to continue...");
            Console.ReadKey();
        }

        private static void PrintWorld(bool[,] worldData)
        {
            Console.Clear();

            for (var x = 0; x < worldData.GetLength(0); x++)
            {
                for (var y = 0; y < worldData.GetLength(1); y++)
                {
                    Console.Write(worldData[x, y] ? "#" : " ");
                }

                Console.WriteLine();
            }
        }

        private static bool[,] GenerateRandomData(int size)
        {
            var data = new bool[size, size];
            var random = new Random();

            for (var w = 0; w < size; w++)
            {
                for (var h = 0; h < size; h++)
                {
                    if (random.Next(0, 30) > 20)
                    {
                        data[w, h] = true;
                    }
                }
            }

            return data;
        }
    }

    public class GameOfLife
    {
        public int Size { get; }

        public bool[,] WorldData { get; private set; }

        public bool[,] NextWorldData { get; private set; }

        public GameOfLife(int size)
        {
            if (size <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(size), "Size must be greater than zero.");
            }

            Size = size;
        }

        public void SetWorldData(bool[,] data)
        {
            if (!IsDataValid(data))
            {
                throw new ArgumentException("The provided intial data is invalid, pleas ensure it's size matches with provided width and height.");
            }

            WorldData = data;
        }

        public void UpdateWorldData(int x, int y, bool value)
        {
            EnsureWorldDataHasBeenInitiallySet();
            WorldData[x, y] = value;
        }

        public Task Update()
        {
            EnsureWorldDataHasBeenInitiallySet();

            if (NextWorldData == null)
            {
                NextWorldData = new bool[Size, Size];
            }

            return Task.Factory.StartNew(() =>
            {
                Parallel.For(0, Size, x =>
                {
                    Parallel.For(0, Size, y =>
                    {
                        var numNeighbors = new[]
                        {
                            IsNeighborAlive(x, y, -1, 0),
                            IsNeighborAlive(x, y, -1, 1),
                            IsNeighborAlive(x, y, 0, 1),
                            IsNeighborAlive(x, y, 1, 1),
                            IsNeighborAlive(x, y, 1, 0),
                            IsNeighborAlive(x, y, 1, -1),
                            IsNeighborAlive(x, y, 0, -1),
                            IsNeighborAlive(x, y, -1, -1)
                        }.Count(state => state);

                        NextWorldData[x, y] = false;

                        if (WorldData[x, y] && (numNeighbors == 2 || numNeighbors == 3))
                        {
                            NextWorldData[x, y] = true;
                        }
                        else if (!WorldData[x, y] && numNeighbors == 3)
                        {
                            NextWorldData[x, y] = true;
                        }
                    });
                });

                var flip = WorldData;
                WorldData = NextWorldData;
                NextWorldData = flip;
            });
        }

        private void EnsureWorldDataHasBeenInitiallySet()
        {
            if (!IsDataValid(WorldData))
            {
                throw new InvalidOperationException("This method can only be called when world data has been set initially via 'SetWorldData(...)' or passed into the constrcutor.");
            }
        }

        private bool IsDataValid(bool[,] data)
        {
            return data != null &&
                   data.GetLength(0) == Size &&
                   data.GetLength(1) == Size;
        }

        private bool IsNeighborAlive(int x, int y, int offsetX, int offsetY)
        {
            var neighborX = x + offsetX;
            var neighborY = y + offsetY;

            return neighborX >= 0 &&
                   neighborX < Size &&
                   neighborY >= 0 &&
                   neighborY < Size &&
                   WorldData[neighborX, neighborY];
        }
    }
}
