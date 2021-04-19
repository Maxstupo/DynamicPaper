namespace Maxstupo.DynamicPaper.Controls {

    using Maxstupo.DynamicPaper.Wallpaper.Players;

    public class LoopModeButton : CyclingButton<LoopMode> {

        public override string GetDisplayName(LoopMode value) {
            switch (value) {
                case LoopMode.None:
                    return "Loop None";
                case LoopMode.All:
                    return "Loop All";
                case LoopMode.Current:
                    return "Loop Current";
                default:
                    return value.ToString();
            }
        }

    }

    public class ShuffleModeButton : CyclingButton<ShuffleMode> {
    
        public override string GetDisplayName(ShuffleMode value) {
            switch (value) {
                case ShuffleMode.None:
                    return "No Shuffle";
                case ShuffleMode.All:
                    return "Shuffle All";
                default:
                    return value.ToString();
            }
        }

    }

}