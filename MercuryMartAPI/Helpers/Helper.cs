using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MercuryMartAPI.Helpers
{
    public class Helper
    {
        // Generate a random number between two numbers    
        public int RandomNumber(int min, int max)
        {
            Random random = new Random();
            return random.Next(min, max);
        }

        // Generate a random string with a given size    
        public string RandomString(int size, bool lowerCase)
        {
            StringBuilder builder = new StringBuilder();
            Random random = new Random();
            char ch;
            for (int i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * random.NextDouble() + 65)));
                builder.Append(ch);
            }
            if (lowerCase)
                return builder.ToString().ToLower();
            return builder.ToString();
        }

        // Generate a random password    
        public string RandomPassword()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(RandomString(4, true));
            builder.Append(RandomNumber(1000, 9999));
            builder.Append(RandomString(2, false));
            return builder.ToString();
        }

        public string GenerateTicketNumber()
        {
            var currentDate = DateTime.Now.ToShortDateString().Trim().Split('/');
            var currentTime = (DateTime.Now.TimeOfDay.ToString().Split('.')[0]).Split(':');
            var ticketNumber = currentTime[2] + currentTime[1] + currentDate[1] + currentDate[2].Substring(0, 2) + currentDate[0] + currentTime[0] + currentDate[2].Substring(2, 2) + RandomString(4, true);
            return ticketNumber;
        }
        public static float Sigmoid(float x)
        {
            return (float)(100 / (1 + Math.Exp(-x)));
        }
    }
}
