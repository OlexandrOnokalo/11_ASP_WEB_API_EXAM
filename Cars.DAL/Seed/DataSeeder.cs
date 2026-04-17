using Cars.DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cars.DAL.Seed
{
    public static class DataSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            if (await context.Manufactures.AnyAsync() || await context.Cars.AnyAsync())
            {
                return;
            }

            var toyota = new ManufactureEntity { Name = "Toyota" };
            var bmw = new ManufactureEntity { Name = "BMW" };
            var audi = new ManufactureEntity { Name = "Audi" };

            await context.Manufactures.AddRangeAsync(toyota, bmw, audi);
            await context.SaveChangesAsync();

            var cars = new List<CarEntity>();
            cars.AddRange(CreateToyotaCars(toyota.Id));
            cars.AddRange(CreateBmwCars(bmw.Id));
            cars.AddRange(CreateAudiCars(audi.Id));

            await context.Cars.AddRangeAsync(cars);
            await context.SaveChangesAsync();
        }

        private static List<CarEntity> CreateToyotaCars(int manufactureId)
        {
            return new List<CarEntity>
            {
                new() { Name = "Camry", ManufactureId = manufactureId, Year = 2020, Volume = 2.50m, Price = 25000m, Color = "Black", Description = "Sedan", Image = "https://example.com/toyota-camry.jpg" },
                new() { Name = "Corolla", ManufactureId = manufactureId, Year = 2019, Volume = 1.80m, Price = 18000m, Color = "White", Description = "Compact sedan", Image = "https://example.com/toyota-corolla.jpg" },
                new() { Name = "RAV4", ManufactureId = manufactureId, Year = 2021, Volume = 2.50m, Price = 32000m, Color = "Gray", Description = "SUV", Image = "https://example.com/toyota-rav4.jpg" },
                new() { Name = "Highlander", ManufactureId = manufactureId, Year = 2022, Volume = 3.50m, Price = 42000m, Color = "Silver", Description = "Family SUV", Image = "https://example.com/toyota-highlander.jpg" },
                new() { Name = "Yaris", ManufactureId = manufactureId, Year = 2018, Volume = 1.50m, Price = 14000m, Color = "Blue", Description = "City hatchback", Image = "https://example.com/toyota-yaris.jpg" },
                new() { Name = "Prius", ManufactureId = manufactureId, Year = 2020, Volume = 1.80m, Price = 27000m, Color = "Green", Description = "Hybrid liftback", Image = "https://example.com/toyota-prius.jpg" },
                new() { Name = "Land Cruiser", ManufactureId = manufactureId, Year = 2023, Volume = 3.30m, Price = 85000m, Color = "White", Description = "Off-road SUV", Image = "https://example.com/toyota-land-cruiser.jpg" },
                new() { Name = "C-HR", ManufactureId = manufactureId, Year = 2021, Volume = 2.00m, Price = 26000m, Color = "Red", Description = "Compact crossover", Image = "https://example.com/toyota-c-hr.jpg" },
                new() { Name = "Avalon", ManufactureId = manufactureId, Year = 2019, Volume = 3.50m, Price = 36000m, Color = "Brown", Description = "Full-size sedan", Image = "https://example.com/toyota-avalon.jpg" },
                new() { Name = "Supra", ManufactureId = manufactureId, Year = 2022, Volume = 3.00m, Price = 52000m, Color = "Yellow", Description = "Sports coupe", Image = "https://example.com/toyota-supra.jpg" }
            };
        }

        private static List<CarEntity> CreateBmwCars(int manufactureId)
        {
            return new List<CarEntity>
            {
                new() { Name = "3 Series", ManufactureId = manufactureId, Year = 2021, Volume = 2.00m, Price = 41000m, Color = "Black", Description = "Sport sedan", Image = "https://example.com/bmw-3-series.jpg" },
                new() { Name = "5 Series", ManufactureId = manufactureId, Year = 2022, Volume = 2.00m, Price = 56000m, Color = "Gray", Description = "Business sedan", Image = "https://example.com/bmw-5-series.jpg" },
                new() { Name = "7 Series", ManufactureId = manufactureId, Year = 2023, Volume = 3.00m, Price = 98000m, Color = "Blue", Description = "Luxury sedan", Image = "https://example.com/bmw-7-series.jpg" },
                new() { Name = "X1", ManufactureId = manufactureId, Year = 2020, Volume = 2.00m, Price = 39000m, Color = "White", Description = "Compact SUV", Image = "https://example.com/bmw-x1.jpg" },
                new() { Name = "X3", ManufactureId = manufactureId, Year = 2021, Volume = 2.00m, Price = 49000m, Color = "Silver", Description = "Mid-size SUV", Image = "https://example.com/bmw-x3.jpg" },
                new() { Name = "X5", ManufactureId = manufactureId, Year = 2021, Volume = 3.00m, Price = 65000m, Color = "Gray", Description = "SUV", Image = "https://example.com/bmw-x5.jpg" },
                new() { Name = "X7", ManufactureId = manufactureId, Year = 2022, Volume = 3.00m, Price = 92000m, Color = "Black", Description = "Large luxury SUV", Image = "https://example.com/bmw-x7.jpg" },
                new() { Name = "M3", ManufactureId = manufactureId, Year = 2022, Volume = 3.00m, Price = 74000m, Color = "Green", Description = "Performance sedan", Image = "https://example.com/bmw-m3.jpg" },
                new() { Name = "M5", ManufactureId = manufactureId, Year = 2023, Volume = 4.40m, Price = 108000m, Color = "Red", Description = "High-performance sedan", Image = "https://example.com/bmw-m5.jpg" },
                new() { Name = "i4", ManufactureId = manufactureId, Year = 2023, Volume = 1.00m, Price = 62000m, Color = "White", Description = "Electric gran coupe", Image = "https://example.com/bmw-i4.jpg" }
            };
        }

        private static List<CarEntity> CreateAudiCars(int manufactureId)
        {
            return new List<CarEntity>
            {
                new() { Name = "A3", ManufactureId = manufactureId, Year = 2020, Volume = 1.50m, Price = 35000m, Color = "White", Description = "Compact sedan", Image = "https://example.com/audi-a3.jpg" },
                new() { Name = "A4", ManufactureId = manufactureId, Year = 2021, Volume = 2.00m, Price = 43000m, Color = "Black", Description = "Sedan", Image = "https://example.com/audi-a4.jpg" },
                new() { Name = "A5", ManufactureId = manufactureId, Year = 2022, Volume = 2.00m, Price = 47000m, Color = "Blue", Description = "Sportback", Image = "https://example.com/audi-a5.jpg" },
                new() { Name = "A6", ManufactureId = manufactureId, Year = 2022, Volume = 2.00m, Price = 57000m, Color = "Gray", Description = "Business sedan", Image = "https://example.com/audi-a6.jpg" },
                new() { Name = "A7", ManufactureId = manufactureId, Year = 2023, Volume = 3.00m, Price = 79000m, Color = "Silver", Description = "Luxury sportback", Image = "https://example.com/audi-a7.jpg" },
                new() { Name = "Q3", ManufactureId = manufactureId, Year = 2021, Volume = 2.00m, Price = 41000m, Color = "Red", Description = "Compact SUV", Image = "https://example.com/audi-q3.jpg" },
                new() { Name = "Q5", ManufactureId = manufactureId, Year = 2021, Volume = 2.00m, Price = 51000m, Color = "Black", Description = "Mid-size SUV", Image = "https://example.com/audi-q5.jpg" },
                new() { Name = "Q7", ManufactureId = manufactureId, Year = 2022, Volume = 3.00m, Price = 72000m, Color = "White", Description = "Large SUV", Image = "https://example.com/audi-q7.jpg" },
                new() { Name = "TT", ManufactureId = manufactureId, Year = 2019, Volume = 2.00m, Price = 49000m, Color = "Yellow", Description = "Sports coupe", Image = "https://example.com/audi-tt.jpg" },
                new() { Name = "e-tron GT", ManufactureId = manufactureId, Year = 2023, Volume = 1.00m, Price = 104000m, Color = "Gray", Description = "Electric performance sedan", Image = "https://example.com/audi-e-tron-gt.jpg" }
            };
        }
    }
}