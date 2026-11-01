using Timberborn.ModManagerScene;
using UnityEngine;

namespace ChessPlanter {

  /// <summary>Entry point the game calls once, while the mod manager scene loads.</summary>
  public class ModStarter : IModStarter {

    public void StartMod(IModEnvironment modEnvironment) {
      Debug.Log("[ChessPlanter] mod loaded");
    }

  }

}
