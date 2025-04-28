using static app.shared.EnumGroup;

namespace app.shared.Dto
{
    public class ResultDto
    {
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
        public MessageType Type { get; set; }
    }
}
