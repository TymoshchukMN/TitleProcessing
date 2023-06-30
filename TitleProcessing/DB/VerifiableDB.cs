namespace TitleProcessing.DB
{
    public class VerifiableDB
    {
        private static VerifiableDB _instance;

        /// <summary>
        /// Databases for checking for having access.
        /// </summary>
        private static string[] _verifiableDBValue =
        {
            "1C7Dymerka",
            "1C7Shops",
            "1C7Torg",
            "Zoom",
        };

        private VerifiableDB() { }

        public string[] VerifiableDBValue
        {
            get { return _verifiableDBValue; }
        }

        public static VerifiableDB GetInstance()
        {
            if (_instance == null)
            {
                _instance = new VerifiableDB();
            }
            return _instance;
        }
    }
}
