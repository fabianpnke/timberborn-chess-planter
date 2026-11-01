using UnityEngine;

namespace ChessPlanter {

  /// <summary>
  /// Which of <c>n</c> crops goes on a tile: diagonal stripes, one crop per diagonal.
  /// <para>
  /// Every crop gets the same share of any area, and the four direct neighbours of a tile
  /// always hold a different crop. With two crops this is exactly a chessboard. The pattern
  /// hangs off world coordinates rather than the dragged rectangle, so two areas dragged
  /// side by side continue each other's stripes without a seam.
  /// </para>
  /// </summary>
  public static class StripePattern {

    public static int IndexAt(Vector3Int coordinates, int count) {
      var index = (coordinates.x + coordinates.y) % count;
      return index < 0 ? index + count : index;
    }

  }

}
