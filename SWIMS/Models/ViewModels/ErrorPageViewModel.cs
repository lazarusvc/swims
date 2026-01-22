namespace SWIMS.Models.ViewModels
{
    // Unsealed so ErrorViewModel can inherit it (keeps both names usable)
    public class ErrorPageViewModel
    {
        public int StatusCode { get; set; }

        public string Title { get; set; } = "";
        public string Message { get; set; } = "";

        public string? RequestId { get; set; }

        // Make this computed so you don’t have to set it manually
        public bool ShowRequestId => !string.IsNullOrWhiteSpace(RequestId);

        // Optional: if later you want “Go back” etc.
        public string? ReturnUrl { get; set; }

        // Optional for later: different image per status code
        public string? ImagePath { get; set; }
    }
}
