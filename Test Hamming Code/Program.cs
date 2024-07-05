using System;

namespace Test_Hamming_Code
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Введите строку:");
            string str = Console.ReadLine();
            int[,] infblocks = StringTransform(str);
            Console.WriteLine("\nДвоичное представление:");
            Show(infblocks);
            int[,] r = ControlBits(infblocks);
            Console.WriteLine("\nКонтрольные биты:");
            Show(r);
            int[,] blocks = MakeBlocks(infblocks, r);
            Console.WriteLine("\nКодированный блок:");
            Show(blocks);
            Console.WriteLine("\nБлок, полученный принимающей стороной:");
            int[,] blocks_rcv = Receiving(blocks);
            Console.WriteLine("\nПроверка:");
            int[,] z = Check(blocks_rcv);
            Show(z);
            int[,] blocks_corr = Correction(blocks_rcv, z);
            Console.WriteLine("\nИсправление ошибки:");
            Show(blocks_corr);
        }
        static void Show(int[,] Mas)
        {
            for (int i = 0; i < Mas.GetLength(0); i++)
            {
                for (int j = 0; j < Mas.GetLength(1); j++)
                {
                    Console.Write(Mas[i, j]);
                }
                Console.WriteLine();
            }
        }

        static int[,] StringTransform(string str)
        {
            char[] symbolArray = str.ToCharArray(); //Символы в виде массива
            Console.WriteLine("Десятичное представление:");
            foreach (int a in symbolArray)
                Console.WriteLine(a);

            int[,] infblocks = new int[symbolArray.Length, 8];
            for (int i = 0; i < symbolArray.Length; i++)
            {
                int dec = symbolArray[i];
                string BinCode = Convert.ToString(dec, 2); //преобразование в двоичный код, блок в виде строки
                int bin = Convert.ToInt32(BinCode); //блок в виде int
                for (int j = 7; j >= 0; j--)
                {
                    infblocks[i, j] = bin % 10;
                    bin /= 10;
                }
            }
            return infblocks;
        }

        static int[,] ControlBits(int[,] infblocks)
        {
            int[,] r = new int[infblocks.GetLength(0), 4]; //контрольные биты
            for (int i = 0; i < infblocks.GetLength(0); i++)
            {
                r[i, 0] = infblocks[i, 0] ^ infblocks[i, 1] ^ infblocks[i, 3] ^ infblocks[i, 4] ^ infblocks[i, 6];
                r[i, 1] = infblocks[i, 0] ^ infblocks[i, 2] ^ infblocks[i, 3] ^ infblocks[i, 5] ^ infblocks[i, 6];
                r[i, 2] = infblocks[i, 1] ^ infblocks[i, 2] ^ infblocks[i, 3] ^ infblocks[i, 7];
                r[i, 3] = infblocks[i, 4] ^ infblocks[i, 5] ^ infblocks[i, 6] ^ infblocks[i, 7];
            }
            return r;
        }
        static int[,] MakeBlocks(int[,] infblocks, int[,] r)
        {
            int[,] blocks = new int[infblocks.GetLength(0), infblocks.GetLength(1) + r.GetLength(1)];
            for (int i = 0; i < blocks.GetLength(0); i++)
            {
                blocks[i, 0] = r[i, 0];
                blocks[i, 1] = r[i, 1];
                blocks[i, 3] = r[i, 2];
                blocks[i, 7] = r[i, 3];
                blocks[i, 2] = infblocks[i, 0];
                blocks[i, 4] = infblocks[i, 1];
                blocks[i, 5] = infblocks[i, 2];
                blocks[i, 6] = infblocks[i, 3];
                blocks[i, 8] = infblocks[i, 4];
                blocks[i, 9] = infblocks[i, 5];
                blocks[i, 10] = infblocks[i, 6];
                blocks[i, 11] = infblocks[i, 7];
            }
            return blocks;
        }
        private static readonly Random rnd = new Random();
        private static readonly object syncLock = new object();
        static int[,] Receiving(int[,] blocks)
        {
            for (int i = 0; i < blocks.GetLength(0); i++)
            {
                int value;
                int value2;
                lock (syncLock)
                {
                    value = rnd.Next(1, 10);
                    value2 = rnd.Next(0, 11);
                }

                if (value > 5)
                {
                    if (blocks[i, value2] == 1)
                        blocks[i, value2] = 0;
                    else
                        blocks[i, value2] = 1;

                    for (int j = 0; j < blocks.GetLength(1); j++)
                    {
                        Console.Write(blocks[i, j]);
                    }
                    Console.WriteLine($" - Ошибка в бите {value2 + 1}");
                }
                else
                {
                    for (int j = 0; j < blocks.GetLength(1); j++)
                    {
                        Console.Write(blocks[i, j]);
                    }
                    Console.WriteLine($" - Без ошибок");
                }
            }
            return blocks;
        }
        static int[,] Check(int[,] blocks_rcv)
        {
            int[,] z = new int[blocks_rcv.GetLength(0), 4]; //контрольные биты
            for (int i = 0; i < blocks_rcv.GetLength(0); i++)
            {
                z[i, 0] = blocks_rcv[i, 0] ^ blocks_rcv[i, 2] ^ blocks_rcv[i, 4] ^ blocks_rcv[i, 6] ^ blocks_rcv[i, 8] ^ blocks_rcv[i, 10];
                z[i, 1] = blocks_rcv[i, 1] ^ blocks_rcv[i, 2] ^ blocks_rcv[i, 5] ^ blocks_rcv[i, 6] ^ blocks_rcv[i, 9] ^ blocks_rcv[i, 10];
                z[i, 2] = blocks_rcv[i, 3] ^ blocks_rcv[i, 4] ^ blocks_rcv[i, 5] ^ blocks_rcv[i, 6] ^ blocks_rcv[i, 11];
                z[i, 3] = blocks_rcv[i, 7] ^ blocks_rcv[i, 8] ^ blocks_rcv[i, 9] ^ blocks_rcv[i, 10] ^ blocks_rcv[i, 11];
            }
            return z;
        }
        static int[,] Correction(int[,] blocks_rcv, int[,] z)
        {
            for (int i = 0; i < blocks_rcv.GetLength(0); i++)
            {
                char[] symbolArray;
                string s = "";
                if (z[i, 0] == 1 | z[i, 1] == 1 | z[i, 2] == 1 | z[i, 3] == 1)
                {
                    s = s + z[i, 0] + z[i, 1] + z[i, 2] + z[i, 3];
                    symbolArray = s.ToCharArray();
                    Array.Reverse(symbolArray);
                    string s2 = String.Concat<char>(symbolArray);
                    int dec = Convert.ToInt32(s2, 2);
                    if (blocks_rcv[i, dec - 1] == 1)
                        blocks_rcv[i, dec - 1] = 0;
                    else blocks_rcv[i, dec - 1] = 1;
                }
            }
            return blocks_rcv;
        }
    }
}
