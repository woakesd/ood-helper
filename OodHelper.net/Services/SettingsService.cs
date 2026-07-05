namespace OodHelper.Services
{
    internal sealed class SettingsService : ISettingsService
    {
        public int BottomSeed
        {
            get { return Settings.BottomSeed; }
            set { Settings.BottomSeed = value; }
        }

        public int TopSeed
        {
            get { return Settings.TopSeed; }
            set { Settings.TopSeed = value; }
        }

        public double RhCoefficient
        {
            get { return Settings.RHCoefficieent; }
            set { Settings.RHCoefficieent = value; }
        }

        public double RsCoefficient
        {
            get { return Settings.RSCoefficieent; }
            set { Settings.RSCoefficieent = value; }
        }

        public string Mysql
        {
            get { return Settings.Mysql; }
            set { Settings.Mysql = value; }
        }

        public string DefaultDiscardProfile
        {
            get { return Settings.DefaultDiscardProfile; }
            set { Settings.DefaultDiscardProfile = value; }
        }

        public string ResultsWebServiceBaseURL
        {
            get { return Settings.ResultsWebServiceBaseURL; }
            set { Settings.ResultsWebServiceBaseURL = value; }
        }

        public string ResultsWebServiceBaseUsername
        {
            get { return Settings.ResultsWebServiceBaseUsername; }
            set { Settings.ResultsWebServiceBaseUsername = value; }
        }

        public string ResultsWebServiceBasePassword
        {
            get { return Settings.ResultsWebServiceBasePassword; }
            set { Settings.ResultsWebServiceBasePassword = value; }
        }

        public string PusherAppId
        {
            get { return Settings.PusherAppId; }
            set { Settings.PusherAppId = value; }
        }

        public string PusherAppKey
        {
            get { return Settings.PusherAppKey; }
            set { Settings.PusherAppKey = value; }
        }

        public string PusherAppSecret
        {
            get { return Settings.PusherAppSecret; }
            set { Settings.PusherAppSecret = value; }
        }
    }
}
