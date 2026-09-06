namespace OrcaFacil.Application.DTOs.Customers;

public record CreateCustomerDto(
    string Name,
    string? Phone,
    string? Email,
    string? Document,
    string? Address,
    string? Notes);

public record UpdateCustomerDto(
    string Name,
    string? Phone,
    string? Email,
    string? Document,
    string? Address,
    string? Notes);

public record CustomerResponseDto(
    Guid Id,
    string Name,
    string? Phone,
    string? Email,
    string? Document,
    string? Address,
    string? Notes,
    DateTime CreatedAt);
