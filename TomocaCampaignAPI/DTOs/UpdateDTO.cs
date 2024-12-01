using System.ComponentModel.DataAnnotations;

public class UpdateEmployeeDto
{
    [Required]
    public int Id { get; set; }

    [Required]
    public required string Name { get; set; }

    [Required]
    public required string EmployeeID { get; set; }

    [Required]
    public required string UserName { get; set; }

    public string? NewPassword { get; set; }

    public string? ConfirmPassword { get; set; }
}
