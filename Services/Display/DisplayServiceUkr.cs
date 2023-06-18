using System;
namespace ASP_202.Services.Display
{
    public class DisplayServiceUkr : IDisplayService
    {
        public string DateString(DateTime dateTime)
        {
            return DateTime.Today == dateTime.Date
                ? "Сьогодні " + dateTime.ToString("HH:mm")
                : dateTime.ToString("dd.MM.yyyy HH:mm");
        }

        public string ReduceString(string source, int maxLenght)
        {
            if (source.Length <= maxLenght) return source;
            source = source[..(maxLenght - 3)];

            int lastSpaceIndex = source.LastIndexOf(' ');
            if(maxLenght - 3 - lastSpaceIndex < 15
                && maxLenght - 3 - lastSpaceIndex >0)
            {
                source = source[..(lastSpaceIndex+1)];
            }
            return source + "...";
         }
    }
}

