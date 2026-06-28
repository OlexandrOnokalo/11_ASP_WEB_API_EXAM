namespace Cars.API.Settings
{
    // Константи в одному місці — зміню тут, переподе скрізь весь код; узгоджую StorageDir+CarsDir → шлях, CarsUrl → URL
    public static class StaticFilesSettings
    {
        public const string StorageDir = "Storage";
        public const string CarsDir = "Cars";
        public const string CarsUrl = "/images/cars"; // URL-префікс для UseStaticFiles: /images/cars/ → Storage/Cars/
    }
}