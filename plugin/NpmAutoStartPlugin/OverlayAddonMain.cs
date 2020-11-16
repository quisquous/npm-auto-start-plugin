using RainbowMage.OverlayPlugin;

namespace NpmAutoStart {
  public class OverlayAddonMain : IOverlayAddonV2 {
    public string Name {
      get { return "NpmAutoStart"; }
    }

    public string Description {
      get { return "Automatically runs `npm start`"; }
    }

    public void Init()
    {
    }
  }
}
