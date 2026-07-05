namespace OodHelper.Services
{
    public interface ISettingsService
    {
        int BottomSeed { get; set; }
        int TopSeed { get; set; }
        double RhCoefficient { get; set; }
        double RsCoefficient { get; set; }
        string Mysql { get; set; }
        string DefaultDiscardProfile { get; set; }
        string ResultsWebServiceBaseURL { get; set; }
        string ResultsWebServiceBaseUsername { get; set; }
        string ResultsWebServiceBasePassword { get; set; }
        string PusherAppId { get; set; }
        string PusherAppKey { get; set; }
        string PusherAppSecret { get; set; }
    }
}
