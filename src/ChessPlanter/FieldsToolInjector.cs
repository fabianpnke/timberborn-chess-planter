using System;
using System.Collections.Generic;
using System.Linq;
using Timberborn.BottomBarSystem;
using Timberborn.PlantingUI;
using Timberborn.ToolButtonSystem;
using Timberborn.ToolSystem;
using UnityEngine;

namespace ChessPlanter {

  /// <summary>
  /// Puts the mixed planting tool into the game's own Fields group, next to the crop tools.
  /// <para>
  /// The Fields group is built by an internal bottom bar provider and offers no way in. But
  /// the bottom bar asks its providers for elements in order, all before the tool buttons are
  /// wired up in PostLoad, so a provider registered right after Fields can find the group's
  /// buttons, add one of its own and contribute no element itself.
  /// </para>
  /// </summary>
  public class FieldsToolInjector : IBottomBarElementsProvider {

    const string FieldsToolGroupId = "Fields";

    /// <summary>Under <c>Sprites/BottomBar/</c>, where the tool button factory looks.</summary>
    const string IconName = "ChessPlanterTool";

    readonly ToolGroupService _toolGroupService;
    readonly ToolButtonService _toolButtonService;
    readonly ToolButtonFactory _toolButtonFactory;
    readonly CropSelection _cropSelection;
    readonly MixedPlantingTool _tool;

    public FieldsToolInjector(ToolGroupService toolGroupService,
                              ToolButtonService toolButtonService,
                              ToolButtonFactory toolButtonFactory,
                              CropSelection cropSelection,
                              MixedPlantingTool tool) {
      _toolGroupService = toolGroupService;
      _toolButtonService = toolButtonService;
      _toolButtonFactory = toolButtonFactory;
      _cropSelection = cropSelection;
      _tool = tool;
    }

    public IEnumerable<BottomBarElement> GetElements() {
      Inject();
      return Array.Empty<BottomBarElement>();
    }

    void Inject() {
      var group = _toolGroupService.GetGroup(FieldsToolGroupId);
      var fieldButtons = _toolButtonService.ToolButtons
          .Where(button => _toolGroupService.IsAssignedToGroup(button.Tool, group))
          .ToList();
      if (fieldButtons.Count == 0) {
        Debug.LogWarning("[ChessPlanter] the Fields tool group has no buttons yet; tool not added");
        return;
      }
      _cropSelection.Initialize(fieldButtons.Select(button => button.Tool).OfType<PlantingTool>());

      var groupButton = _toolButtonService.GetToolGroupButton(fieldButtons[0]);
      var parent = groupButton.ToolButtonsElement;
      var button = _toolButtonFactory.Create(_tool, IconName, parent);
      // Just before the cancel tool, which the game always adds last.
      parent.Insert(parent.IndexOf(fieldButtons[fieldButtons.Count - 1].Root), button.Root);
      groupButton.AddTool(button);
      _toolGroupService.AssignToGroup(group, _tool);
    }

  }

}
