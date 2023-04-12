using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using CsvHelper;
using CsvHelper.Configuration;

namespace SpaceApp
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 4)
            {
                Console.WriteLine("Usage: WeatherReport <file-name> <sender-email> <password> <receiver-email>");
                return;
            }

            string fileName = args[0];
            string senderEmail = args[1];
            string password = args[2];
            string receiverEmail = args[3];

            List<WeatherData> weatherDataList = LoadWeatherDataFromFile(fileName);

            if (weatherDataList == null || weatherDataList.Count == 0)
            {
                Console.WriteLine("Failed to load weather data from file.");
                return;
            }

            List<WeatherData> filteredWeatherData = FilterWeatherData(weatherDataList);

            if (filteredWeatherData == null || filteredWeatherData.Count == 0)
            {
                Console.WriteLine("No suitable launch date found in weather data.");
                return;
            }

            WeatherDataAggregate aggregate = CalculateAggregates(filteredWeatherData);

            string reportFileName = "WeatherReport.csv";
            GenerateWeatherReportFile(reportFileName, aggregate);

            SendWeatherReportEmail(senderEmail, password, receiverEmail, reportFileName);

            Console.WriteLine("Weather report sent successfully!");
        }

        static List<WeatherData> LoadWeatherDataFromFile(string fileName)
        {
            try
            {
                using (var reader = new StreamReader(fileName))
                using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture) { HasHeaderRecord = true, Delimiter = "," }))
                {
                    return csv.GetRecords<WeatherData>().ToList();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to load weather data from file. {ex.Message}");
                return null;
            }
        }

        static List<WeatherData> FilterWeatherData(List<WeatherData> weatherDataList)
        {
            return weatherDataList.Where(data =>
                data.Temperature >= 2 && data.Temperature <= 31 &&
                data.WindSpeed <= 10 &&
                data.Humidity < 60 &&
                data.Precipitation == 0 &&
                data.Lightning == "No" &&
                (data.Clouds != "Cumulus" && data.Clouds != "Nimbus")
            ).ToList();
        }

        static WeatherDataAggregate CalculateAggregates(List<WeatherData> weatherDataList)
        {
            WeatherDataAggregate aggregate = new WeatherDataAggregate();

            aggregate.AverageTemperature = weatherDataList.Average(data => data.Temperature);
            aggregate.MaxTemperature = weatherDataList.Max(data => data.Temperature);
            aggregate.MinTemperature = weatherDataList.Min(data => data.Temperature);
            aggregate.MedianTemperature = CalculateMedian(weatherDataList.Select(data => data.Temperature));

            aggregate.AverageWindSpeed = weatherDataList.Average(data => data.WindSpeed);
            aggregate.MaxWindSpeed = weatherDataList.Max(data => data.WindSpeed);
            aggregate.MinWindSpeed = weatherDataList.Min(data => data.WindSpeed);
            aggregate.MedianWindSpeed = CalculateMedian(weatherDataList.Select(data => data.WindSpeed));

            aggregate.AverageHumidity = weatherDataList.Average(data => data.Humidity);
            aggregate.MaxHumidity = weatherDataList.Max(data => data.Humidity);
            aggregate.MinHumidity = weatherDataList.Min(data => data.Humidity);
            aggregate.MedianHumidity = CalculateMedian(weatherDataList.Select(data => data.Humidity));

            return aggregate;
        }

        static void GenerateWeatherReportFile(string fileName, WeatherDataAggregate aggregate)
        {
            try
            {
                using (var writer = new StreamWriter(fileName))
                using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                {
                    csv.WriteRecords(new List<WeatherDataAggregate> { aggregate });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to generate weather report file. {ex.Message}");
            }
        }




        static void SendWeatherReportEmail(string senderEmail, string password, string receiverEmail, string reportFileName)
        {
            try
            {
                using (var message = new MailMessage(senderEmail, receiverEmail))
                {
                    message.Subject = "Weather Report";
                    message.Body = "Please find the attached weather report.";
                    Attachment attachment = new Attachment(reportFileName);
                    message.Attachments.Add(attachment);

                    using (var client = new SmtpClient("smtp.gmail.com", 587))
                    {
                        client.EnableSsl = true;
                        client.Credentials = new NetworkCredential(senderEmail, password);
                        client.Send(message);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send weather report email. {ex.Message}");
            }
        }

        static double CalculateMedian(IEnumerable<double> values)
        {
            List<double> sortedValues = values.OrderBy(x => x).ToList();
            int count = sortedValues.Count;
            if (count % 2 == 0)
            {
                int middleIndex1 = count / 2 - 1;
                int middleIndex2 = count / 2;
                return (sortedValues[middleIndex1] + sortedValues[middleIndex2]) / 2;
            }
            else
            {
                int middleIndex = count / 2;
                return sortedValues[middleIndex];
            }
        }
    }

    class WeatherData
    {
        public double Temperature { get; set; }
        public double WindSpeed { get; set; }
        public double Humidity { get; set; }
        public int Precipitation { get; set; }
        public string Lightning { get; set; }
        public string Clouds { get; set; }
    }

    class WeatherDataAggregate
    {
        public double AverageTemperature { get; set; }
        public double MaxTemperature { get; set; }
        public double MinTemperature { get; set; }
        public double MedianTemperature { get; set; }
        public double AverageWindSpeed { get; set; }
        public double MaxWindSpeed { get; set; }
        public double MinWindSpeed { get; set; }
        public double MedianWindSpeed { get; set; }
        public double AverageHumidity { get; set; }
        public double MaxHumidity { get; set; }
        public double MinHumidity { get; set; }
        public double MedianHumidity { get; set; }
    }
}


