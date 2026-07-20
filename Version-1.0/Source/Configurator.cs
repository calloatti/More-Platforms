using Bindito.Core;
using Timberborn.BlockSystem;
using Timberborn.TemplateInstantiation;

namespace Tobbert.MorePlatforms
{
  [Context("Game")]
  public class MorePlatformsConfigurator : Configurator
  {
    protected override void Configure()
    {
      Bind<SidePlatform>().AsTransient();

      // MISSING LINE ADDED HERE:
      Bind<SidePlatformSupportBlocker>().AsTransient();

      MultiBind<IBlockObjectValidator>().To<SidePlatformValidator>().AsSingleton();

      MultiBind<TemplateModule>().ToProvider(ProvideTemplateModule).AsSingleton();
    }

    private static TemplateModule ProvideTemplateModule()
    {
      var builder = new TemplateModule.Builder();
      builder.AddDecorator<SidePlatformSpec, SidePlatform>();

      // This attaches our deletion blocker to EVERY building in the game
      builder.AddDecorator<BlockObject, SidePlatformSupportBlocker>();

      return builder.Build();
    }
  }
}