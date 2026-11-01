using System.Collections.Generic;
using System.Linq;
using Timberborn.Localization;
using Timberborn.PlantingUI;
using UnityEngine;
using UnityEngine.UIElements;

namespace ChessPlanter {

  /// <summary>
  /// The crop picker shown in the tool's description panel: one tile per crop, clicked to
  /// switch it on or off. The coloured frame of a picked tile is the colour its fields are
  /// previewed in while dragging, which is what ties the picker to the stripes on the map.
  /// </summary>
  public class CropPicker {

    const int TileSize = 44;
    const int IconSize = 34;
    const int FrameWidth = 3;
    const float DeselectedOpacity = 0.35f;
    const float LockedOpacity = 0.15f;

    readonly CropSelection _cropSelection;
    readonly ILoc _loc;
    readonly List<(PlantingTool Crop, VisualElement Tile)> _tiles = new();
    VisualElement? _root;
    Label? _summary;

    /// <summary>
    /// Built on first use: the description panel asks for it the first time the tool is
    /// hovered, which is after the crops have been collected from the Fields group.
    /// </summary>
    public VisualElement Root => _root ??= Build();

    public CropPicker(CropSelection cropSelection, ILoc loc) {
      _cropSelection = cropSelection;
      _loc = loc;
    }

    /// <summary>Called every frame while the panel is shown; crops can unlock at any time.</summary>
    public void Update() {
      foreach (var (crop, tile) in _tiles) {
        var locked = _cropSelection.IsLocked(crop);
        var selected = _cropSelection.IsSelected(crop) && !locked;
        tile.style.opacity = locked ? LockedOpacity : selected ? 1f : DeselectedOpacity;
        SetFrame(tile, selected ? _cropSelection.ColorOf(crop) : Color.clear);
      }
      if (_summary != null) {
        _summary.text = Summary();
      }
    }

    VisualElement Build() {
      var root = new VisualElement { style = { paddingTop = 4, paddingBottom = 4 } };

      var hint = new Label(_loc.T("ChessPlanter.Picker.Hint"));
      hint.AddToClassList("game-text-normal");
      root.Add(hint);

      var row = new VisualElement {
          style = { flexDirection = FlexDirection.Row, flexWrap = Wrap.Wrap, marginTop = 4 },
      };
      foreach (var crop in _cropSelection.Crops) {
        var tile = CreateTile(crop);
        _tiles.Add((crop, tile));
        row.Add(tile);
      }
      root.Add(row);

      _summary = new Label { style = { marginTop = 4, whiteSpace = WhiteSpace.Normal } };
      _summary.AddToClassList("game-text-normal");
      root.Add(_summary);

      Update();
      return root;
    }

    VisualElement CreateTile(PlantingTool crop) {
      var tile = new VisualElement {
          style = {
              width = TileSize, height = TileSize, marginRight = 4, marginBottom = 4,
              alignItems = Align.Center, justifyContent = Justify.Center,
              borderTopLeftRadius = 4, borderTopRightRadius = 4,
              borderBottomLeftRadius = 4, borderBottomRightRadius = 4,
              borderTopWidth = FrameWidth, borderBottomWidth = FrameWidth,
              borderLeftWidth = FrameWidth, borderRightWidth = FrameWidth,
              backgroundColor = new Color(0f, 0f, 0f, 0.25f),
          },
          tooltip = _cropSelection.DisplayName(crop),
      };
      var icon = new Image {
          sprite = _cropSelection.Icon(crop),
          pickingMode = PickingMode.Ignore,
          style = { width = IconSize, height = IconSize },
      };
      tile.Add(icon);
      tile.RegisterCallback<ClickEvent>(_ => {
        if (!_cropSelection.IsLocked(crop)) {
          _cropSelection.Toggle(crop);
          Update();
        }
      });
      return tile;
    }

    static void SetFrame(VisualElement tile, Color color) {
      tile.style.borderTopColor = color;
      tile.style.borderBottomColor = color;
      tile.style.borderLeftColor = color;
      tile.style.borderRightColor = color;
    }

    string Summary() {
      var picked = _cropSelection.Crops
          .Where(crop => _cropSelection.IsSelected(crop) && !_cropSelection.IsLocked(crop))
          .Select(_cropSelection.DisplayName)
          .ToList();
      var text = picked.Count == 0
          ? _loc.T("ChessPlanter.Picker.None")
          : string.Join(", ", picked);
      var locked = _cropSelection.Crops
          .Where(_cropSelection.IsLocked)
          .Select(_cropSelection.DisplayName)
          .ToList();
      if (locked.Count > 0) {
        text += "\n" + _loc.T("ChessPlanter.Picker.Locked", string.Join(", ", locked));
      }
      return text;
    }

  }

}
