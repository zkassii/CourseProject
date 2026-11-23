namespace ShelterApp.Models;

public record ConfirmModel(string Label, string Handler, int Id, string Question, string Css = "btn btn-danger btn-sm");