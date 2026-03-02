namespace RoutingV3.Api.Dtos;

public record MandateDto(int Id, string Code, string Name);
public record CreateMandateRequest(string Code, string Name);
public record UpdateMandateRequest(string Code, string Name);
