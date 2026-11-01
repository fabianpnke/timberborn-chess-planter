using System.Collections.Generic;
using System.Linq;
using Timberborn.AreaSelectionSystemUI;
using Timberborn.GameSound;
using Timberborn.Localization;
using Timberborn.Planting;
using Timberborn.SelectionSystem;
using Timberborn.SelectionToolSystem;
using Timberborn.SingletonSystem;
using Timberborn.TerrainQueryingSystem;
using Timberborn.ToolSystem;
using Timberborn.ToolSystemUI;
using UnityEngine;

namespace ChessPlanter {

  /// <summary>
  /// Marks a dragged area for planting with every picked crop at once, in diagonal stripes.
  /// <para>
  /// Built like the game's own <c>PlantingTool</c>, down to the cursor and the sound, with
  /// one difference besides the crop choice: tiles that are already marked for a crop are
  /// left as they are, so dragging over an existing field only fills its gaps.
  /// </para>
  /// </summary>
  public class MixedPlantingTool : ITool, IToolDescriptor {

    const string CursorKey = "PlantingCursor";

    readonly CropSelection _cropSelection;
    readonly CropPicker _cropPicker;
    readonly TerrainAreaService _terrainAreaService;
    readonly PlantingAreaValidator _plantingAreaValidator;
    readonly PlantingService _plantingService;
    readonly AreaHighlightingService _areaHighlightingService;
    readonly MeasurableAreaDrawer _measurableAreaDrawer;
    readonly GameUISoundController _gameUISoundController;
    readonly EventBus _eventBus;
    readonly ILoc _loc;
    readonly SelectionToolProcessor _selectionToolProcessor;

    public MixedPlantingTool(CropSelection cropSelection,
                             CropPicker cropPicker,
                             TerrainAreaService terrainAreaService,
                             PlantingAreaValidator plantingAreaValidator,
                             PlantingService plantingService,
                             AreaHighlightingService areaHighlightingService,
                             MeasurableAreaDrawer measurableAreaDrawer,
                             GameUISoundController gameUISoundController,
                             EventBus eventBus,
                             ILoc loc,
                             SelectionToolProcessorFactory selectionToolProcessorFactory) {
      _cropSelection = cropSelection;
      _cropPicker = cropPicker;
      _terrainAreaService = terrainAreaService;
      _plantingAreaValidator = plantingAreaValidator;
      _plantingService = plantingService;
      _areaHighlightingService = areaHighlightingService;
      _measurableAreaDrawer = measurableAreaDrawer;
      _gameUISoundController = gameUISoundController;
      _eventBus = eventBus;
      _loc = loc;
      _selectionToolProcessor = selectionToolProcessorFactory.Create(
          Preview, Plant, () => { }, CursorKey);
    }

    public void Enter() {
      _selectionToolProcessor.Enter();
    }

    public void Exit() {
      _selectionToolProcessor.Exit();
    }

    public ToolDescription DescribeTool() {
      return new ToolDescription.Builder(_loc.T("ChessPlanter.Tool.Title"))
          .AddSection(_loc.T("ChessPlanter.Tool.Description"))
          .AddUpdatableSection(_cropPicker.Root, _cropPicker.Update)
          .Build();
    }

    void Preview(IEnumerable<Vector3Int> inputBlocks, Ray ray) {
      foreach (var (coordinates, crop) in Assign(inputBlocks, ray)) {
        _areaHighlightingService.DrawTile(coordinates, _cropSelection.ColorOf(crop));
        _measurableAreaDrawer.AddMeasurableCoordinates(coordinates);
      }
    }

    void Plant(IEnumerable<Vector3Int> inputBlocks, Ray ray) {
      var assignments = Assign(inputBlocks, ray).ToList();
      if (assignments.Count == 0) {
        return;
      }
      foreach (var (coordinates, crop) in assignments) {
        _plantingService.SetPlantingCoordinates(coordinates, crop.TemplateName);
      }
      _gameUISoundController.PlayFieldPlacedSound();
      _eventBus.Post(new PlantingAreaMarkedEvent());
    }

    /// <summary>
    /// The crop each tile of the area gets, skipping tiles already marked and tiles where
    /// that crop cannot grow. A skipped tile keeps its place in the stripes, so a building
    /// in the middle of a field leaves a hole rather than shifting the pattern.
    /// </summary>
    IEnumerable<(Vector3Int Coordinates, PlantableSpec Crop)> Assign(IEnumerable<Vector3Int> inputBlocks,
                                                                     Ray ray) {
      var crops = _cropSelection.ActiveCrops();
      if (crops.Count == 0) {
        yield break;
      }
      foreach (var coordinates in _terrainAreaService.InMapLeveledCoordinates(inputBlocks, ray)) {
        var crop = crops[StripePattern.IndexAt(coordinates, crops.Count)];
        if (!_plantingService.IsResourceAt(coordinates)
            && _plantingAreaValidator.CanPlant(coordinates, crop.TemplateName)) {
          yield return (coordinates, crop);
        }
      }
    }

  }

}
