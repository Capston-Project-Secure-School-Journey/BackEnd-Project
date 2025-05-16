using System.ComponentModel.DataAnnotations;

namespace Api.TransferDTOs.Requests;

public class ApproveApplicationRequest
{
    public string Note { get; set; } = string.Empty;
}