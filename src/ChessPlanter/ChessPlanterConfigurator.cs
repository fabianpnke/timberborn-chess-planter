using Bindito.Core;
using Timberborn.BottomBarSystem;

namespace ChessPlanter {

  /// <summary>Wires the mod into the in-game dependency container.</summary>
  [Context("Game")]
  public class ChessPlanterConfigurator : Configurator {

    /// <summary>
    /// Anything above the Fields group's 30 works, since the injector adds no element of its
    /// own. Orders must be unique across all mods, so this one is deliberately odd.
    /// </summary>
    const int BottomBarOrder = 3017;

    protected override void Configure() {
      Bind<CropSelection>().AsSingleton();
      Bind<CropPicker>().AsSingleton();
      Bind<MixedPlantingTool>().AsSingleton();
      Bind<FieldsToolInjector>().AsSingleton();
      MultiBind<BottomBarModule>().ToProvider<BottomBarModuleProvider>().AsSingleton();
    }

    class BottomBarModuleProvider : IProvider<BottomBarModule> {

      readonly FieldsToolInjector _fieldsToolInjector;

      public BottomBarModuleProvider(FieldsToolInjector fieldsToolInjector) {
        _fieldsToolInjector = fieldsToolInjector;
      }

      public BottomBarModule Get() {
        var builder = new BottomBarModule.Builder();
        builder.AddLeftSectionElement(_fieldsToolInjector, BottomBarOrder);
        return builder.Build();
      }

    }

  }

}
