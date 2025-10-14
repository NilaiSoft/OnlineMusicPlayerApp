using OnlineMusicPlayerApp.Views;

namespace OnlineMusicPlayerApp
{
    public static class PlayerManager
    {
        public static MiniPlayerView MiniPlayerInstance { get; } = new MiniPlayerView();
    }
}
