using System;
using System.Collections.Generic;
using System.Linq;
using Timberborn.EntitySystem;
using Timberborn.Localization;
using Timberborn.Planting;
using Timberborn.PlantingUI;
using Timberborn.ToolSystem;
using UnityEngine;

namespace ChessPlanter {

  /// <summary>
  /// The crops this faction can plant, which of them the player has picked, and the colour
  /// each one is previewed in.
  /// <para>
  /// The crops are taken from the game's own planting tools rather than from the templates,
  /// so the list is exactly what the Fields group offers, in its order, and each crop's lock
  /// state comes for free from the tool that owns it.
  /// </para>
  /// </summary>
  public class CropSelection {

    /// <summary>Remembered across saves and sessions, as the crops the player switched off.</summary>
    const string DeselectedPrefsKey = "ChessPlanter.DeselectedCrops";

    static readonly Color[] Palette = {
        new(0.36f, 0.85f, 0.30f),
        new(0.98f, 0.78f, 0.15f),
        new(0.35f, 0.62f, 1.00f),
        new(0.95f, 0.38f, 0.30f),
        new(0.78f, 0.45f, 0.98f),
        new(0.20f, 0.86f, 0.84f),
        new(1.00f, 0.55f, 0.10f),
        new(0.98f, 0.50f, 0.78f),
    };

    readonly ToolUnlockingService _toolUnlockingService;
    readonly ILoc _loc;
    readonly List<PlantingTool> _crops = new();
    readonly HashSet<string> _deselected = new();

    public IReadOnlyList<PlantingTool> Crops => _crops;

    public CropSelection(ToolUnlockingService toolUnlockingService, ILoc loc) {
      _toolUnlockingService = toolUnlockingService;
      _loc = loc;
    }

    public void Initialize(IEnumerable<PlantingTool> plantingTools) {
      _crops.Clear();
      _crops.AddRange(plantingTools);
      LoadDeselected();
    }

    /// <summary>Picked and unlocked, in the Fields group's order: what actually gets planted.</summary>
    public List<PlantableSpec> ActiveCrops() {
      return _crops.Where(crop => IsSelected(crop) && !IsLocked(crop))
          .Select(crop => crop.PlantableSpec)
          .ToList();
    }

    public bool IsSelected(PlantingTool crop) {
      return !_deselected.Contains(crop.PlantableSpec.TemplateName);
    }

    public bool IsLocked(PlantingTool crop) {
      return _toolUnlockingService.IsLocked(crop);
    }

    public void Toggle(PlantingTool crop) {
      var name = crop.PlantableSpec.TemplateName;
      if (!_deselected.Remove(name)) {
        _deselected.Add(name);
      }
      SaveDeselected();
    }

    public Color ColorOf(PlantableSpec plantable) {
      var index = _crops.FindIndex(crop => crop.PlantableSpec == plantable);
      return Palette[Math.Max(index, 0) % Palette.Length];
    }

    public Color ColorOf(PlantingTool crop) {
      return ColorOf(crop.PlantableSpec);
    }

    public string DisplayName(PlantingTool crop) {
      return _loc.T(crop.PlantableSpec.GetSpec<LabeledEntitySpec>().DisplayNameLocKey);
    }

    public Sprite Icon(PlantingTool crop) {
      return crop.PlantableSpec.GetSpec<LabeledEntitySpec>().Icon.Asset;
    }

    void LoadDeselected() {
      _deselected.Clear();
      if (PlayerPrefs.HasKey(DeselectedPrefsKey)) {
        foreach (var name in PlayerPrefs.GetString(DeselectedPrefsKey).Split(',')) {
          if (name.Length > 0) {
            _deselected.Add(name);
          }
        }
        return;
      }
      // First run: everything that grows where most crops grow. Water crops such as
      // spadderdock belong to a different planter group and would only leave holes in a
      // field dragged on dry land, so they start switched off.
      var mainGroup = _crops.GroupBy(crop => crop.PlantableSpec.ResourceGroup)
          .OrderByDescending(group => group.Count())
          .Select(group => group.Key)
          .FirstOrDefault();
      foreach (var crop in _crops.Where(crop => crop.PlantableSpec.ResourceGroup != mainGroup)) {
        _deselected.Add(crop.PlantableSpec.TemplateName);
      }
    }

    void SaveDeselected() {
      PlayerPrefs.SetString(DeselectedPrefsKey, string.Join(",", _deselected));
      PlayerPrefs.Save();
    }

  }

}
