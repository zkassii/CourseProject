using ShelterApp.Models;

namespace ShelterApp.Services;

public static class RequestHelper
{
    // цвет плашки статуса заявки (классы из site.css)
    public static string StatusClass(string status) => status switch
    {
        RequestStatus.New => "status-warn",
        RequestStatus.Approved => "status-ok",
        RequestStatus.Rejected => "status-error",
        RequestStatus.Transferred => "status-pink",
        _ => "status-ok"
    };
}