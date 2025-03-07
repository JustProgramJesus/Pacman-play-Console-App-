using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Management.Instrumentation;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace Learn
{
    class Program
    {
        static void Main(string[] args)
        {
            char[,] map = ReadMap("map.txt"); // Передаем аргумент (назавание файла карты) в наш метод ReadMap, который ее считает
            bool play = true; // Для бесконечного цикла, чтобы в дальнейшем расширить игру, например при прохождении, можно задать этой переменной false
            ConsoleKeyInfo pressedKey = new ConsoleKeyInfo('w', ConsoleKey.W, false, false, false); // По дефолту задаем кнопку, которая будет нажата

            int pacmanX = 1, pacmanY = 1; // Положение Pacman (его координаты)
            int score = 0; // Количество очков
            const int maxHealth = 20; // Макс HP
            int health = 10; // Defoult HP

            while (play)
            {
                if (health <= 0) // Поражение, когда hp стало равно 0
                {
                    play = false;
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write($"Вы закончили игру со счетом: {score}! Неплохо, но вы проиграли, попробуйте еще раз!\n\n\n\n");
                    Console.ForegroundColor = default;
                    break;
                }

                if (score == 32) // Победа, установите на свое значение, когда хотите вывести победу при сборе определенного количества очков
                {
                    play = false;
                    Console.Clear();
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.Write($"Поздравляю! Ваш счет: {score}! Вы выйграли, продолжайте в том же духе!\n\n\n\n");
                    Console.ForegroundColor = default;
                    break;
                }

                Console.Clear(); // Очищаем консоль

                Console.ForegroundColor = ConsoleColor.Blue; // Задаем цвет нашей карте
                DrawMap(map); // Передаем в карту нашу карту в виде двумерного массива map

                Console.ForegroundColor = ConsoleColor.Yellow; // Задаем цвет нашему Pacman, а именно желтый
                Console.SetCursorPosition(pacmanX, pacmanY); // Устанавливаем курсор в позицию pacmanX и pacmanY, в дальнейшем будем реализовывать логику движения
                Console.Write("@"); // Рисуем нашего Pacman
                 Console.CursorVisible = false; // Отключаем видимость курсора

                Console.ForegroundColor = ConsoleColor.Red; // Задаем цвет красный
                Console.SetCursorPosition(0, 15); // Устанавливаем позицию для отрисовки
                Console.Write($"Score: {score}"); // Отрисовка очков. (красным цветом)

                Console.SetCursorPosition(map.GetLength(0) + 2, 0);
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("[");
                DrawHealthBar(health, maxHealth, map.GetLength(0) + 3, 0);
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write("]");

                pressedKey = Console.ReadKey(); // Сохраняем текущую клавишу пользователя в переменную в формате char

                HandleInput(pressedKey, ref pacmanX, ref pacmanY, ref map, ref score, ref health); // Вся логика игры нашего героя
            }
        }

        private static char[,] ReadMap(string path)
        {
            string[] file = File.ReadAllLines(path); // Читаем построчно наш файл карты и записываем все в одномерный массив file. Он будет такого вида: {"######","#    #" ...}

            char[,] map = new char[GetMaxLenghtOfLine(file), file.Length]; // Мы устанавливаем границу нашей карты, паралельно вычисляя его размер. Он будет размера Макс. длины строчки и количеству этих строчек в массиве строк file

            for (int x = 0; x < map.GetLength(0); x++) // Перебираем координаты каждого элемента в массиве строк file и распихиваем их по координатам в двумерный массив map. В итоге получается полная карта в нужном нам формате
            {
                for (int y = 0; y < map.GetLength(1); y++)
                {
                    map[x, y] = file[y][x]; // Собираем карту Y - у нас это будет x, а X - будет y (Так как вывод массива обычно сверху вниз и потом сдвиг в сторону до конца)
                }
            }

            return map;
        }

        private static void DrawHealthBar(int health, int maxHealf, int positionX, int positionY) // Рисуем HP Bar чуть дальше границы нашей карты
        {
            for (int i = 0; i < health; i++) // Пробегаемся по нашему текущему HP и рисуем его
            {
                Console.BackgroundColor = ConsoleColor.Green; // Впринципе тут все понятно
                Console.ForegroundColor = ConsoleColor.White;
                Console.SetCursorPosition(positionX + i, positionY); // Сдвигаем курсор, чтобы рисовать не в одной координате
                Console.Write(" ");
                Console.BackgroundColor = default; // Ставим цвет фона на дефолтный
            }

            for (int j = health; j < maxHealf; j++) // Бежим по недостоющему HP до максимума
            {
                Console.BackgroundColor = ConsoleColor.Red;
                Console.ForegroundColor = ConsoleColor.White;
                Console.SetCursorPosition(positionX + j, 0); // Тут так же сдвигаем курсор, чтобы правильно рисовать
                Console.Write(" ");
                Console.BackgroundColor = default;
            }
        }

        private static void DrawMap(char[,] map) // Рисуем карту в косоли
        {
            for (int y = 0; y < map.GetLength(1); y++) // Бежим по каждому символу в массиве по X
            {
                for (int x = 0; x < map.GetLength(0); x++) // Бежим по каждому символу в массиве по Y
                {
                    Console.Write(map[x, y]); // Выводим этот символ. (Находим каждый символ по координатам)
                }
                Console.Write("\n"); // Делам перенос строки, когда вывели первую строчку и дальнейшие тоже. (Служит для красивого вывода карты).
            }
        }

        private static void HandleInput(ConsoleKeyInfo pressedKey, ref int pacmanX, ref int pacmanY, ref char[,] map, ref int score, ref int health)
        {
            switch (pressedKey.Key)
            {
                case ConsoleKey.UpArrow: // Проверка, что пользователь нажан на стрелку вверх
                    if (checkWall(map, pacmanX, pacmanY - 1))
                    {  // Метод, в котором отправляем данные для проверки, а нет ли стены куда мы собираемся пойти. Сразу передаем параметр смещенный в нужную нам сторону
                        pacmanY -= 1; // если метод проверил и там нет стены, то перемещаемся, если нет, то break
                        if (checkEat(map, pacmanX, pacmanY)) // Если мы сдвинулись, то смотрим, а не на еде ли мы стоим, если да, то кушаем ее
                        {
                            map[pacmanX, pacmanY] = ' ';
                            score += 1;
                        }
                        
                        if (checkSpikes(map, pacmanX, pacmanY)) // Смотрим, не на шипы ли мы встали, если да, то вычитаем HP
                        {
                            health -= 2;
                        }
                    }
                    break;

                case ConsoleKey.DownArrow: // Тоже самое, только с клавишей стелка вниз
                    if (checkWall(map, pacmanX, pacmanY + 1))
                    {
                        pacmanY += 1;
                        if (checkEat(map, pacmanX, pacmanY)) // Тоже самое, только для другого положения
                        {
                            map[pacmanX, pacmanY] = ' ';
                            score += 1;
                        }

                        if (checkSpikes(map, pacmanX, pacmanY))
                        {
                            health -= 2;
                        }
                    }
                    break;

                case ConsoleKey.LeftArrow: // Тоже самое, только с клавишей стрелка влево
                    if (checkWall(map, pacmanX - 1, pacmanY))
                    {
                        pacmanX -= 1;
                        if (checkEat(map, pacmanX, pacmanY)) // Тоже самое, только для другого положения
                        {
                            map[pacmanX, pacmanY] = ' ';
                            score += 1;
                        }

                        if (checkSpikes(map, pacmanX, pacmanY))
                        {
                            health -= 2;
                        }
                    }
                    break;

                case ConsoleKey.RightArrow: // Тоже самое, только с клавишей стрелка вправо
                    if (checkWall(map, pacmanX + 1, pacmanY))
                    {
                        pacmanX += 1;
                        if (checkEat(map, pacmanX, pacmanY)) // Тоже самое, только для другого положения
                        {
                            map[pacmanX, pacmanY] = ' ';
                            score += 1;
                        }

                        if (checkSpikes(map, pacmanX, pacmanY))
                        {
                            health -= 2;
                        }
                    }
                    break;
            }
        }

        private static bool checkWall(char[,] map, int pacmanX, int pacmanY) // Метод для проверки стены. Принимает саму карту и координаты, куда собираемся двигаться
        {
            if (map[pacmanX, pacmanY] != '#') // Это позиция, куда мы бы попали, если бы переместились, если там нет стены, то даем добро на перемещение
            {
                return true;
            }
            else
            {
                return false; // Если там стена, то не даем добро на перемещение.
            }
        }

        private static bool checkSpikes(char[,] map, int pacmanX, int pacmanY) // Метод для проверки шипов
        {
            if (map[pacmanX, pacmanY] == 'Ш') // Проверяем, на чем мы стоим, если это char "Ш", то мы встали на шипы
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static bool checkEat(char[,] map, int pacmanX, int pacmanY)
        {
            if (map[pacmanX, pacmanY] == '.') // Проверяем, если мы стоим на еде, то возвращаем true, иначе нет.
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static int GetMaxLenghtOfLine(string[] lines) // Метод для выявления границ карты. Вдруг у нас карта не ровная. Мы смотрим макс. длину строчек и возвращаем это число.
        {
            int MaxLength = lines[0].Length; // Макс. длина строчки - это первая строка

            foreach (var line in lines) // берем каждую строку из массива строк, который мы передали
            {
                if (line.Length > MaxLength) // смотрим, если у нас длина строки больше первой строчки, то мы перезаписываем макс. длину строки.
                {
                    MaxLength = line.Length;
                }
            }

            return MaxLength; // В конце концов возвращаем длину самой длинной строчки, это и будет ограничение нашего массива по X.
        }
    }
}
