using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Unicode;
using System.Threading.Tasks;

namespace Test_B1
{
    public static class RandomExt
    {
        public static string latinSymbols = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public static string russianSymbols = "абвгдеёжзийклмнопрстуфхцчшщьыъэюяАБВГДЕЁЖЗИЙКЛМНОПРСТУФХЦЧШЩЬЫЪЭЮЯ";
        //методы расширения для генерации случайных значений
        public static double NextDouble(this Random @this, double min, double max)
        {
            return Math.Round(@this.NextDouble() * (max - min) + min, 7);
        }
        public static DateTime NextDateTime(this Random @this, int yearsAgo)
        {
            DateTime start = DateTime.Today.AddYears(-yearsAgo);
            int range = (DateTime.Today - start).Days;
            return start.AddDays((new Random()).Next(range));
        }
        public static char NextRandomChar(this Random @this, string symbols)
        {
            var index = new Random().Next(symbols.Length);
            return symbols[index];
        }
    }
}
