using ShareProject.Common;

namespace ShareProject.Response
{
    public class CardResponse : BaseResponse
    {
        public CardDto Card { get; set; } = new CardDto(); // 卡片資料
    }
}
