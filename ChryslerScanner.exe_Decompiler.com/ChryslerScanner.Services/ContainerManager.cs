using System;
using SimpleInjector;

namespace ChryslerScanner.Services;

public static class ContainerManager
{
	private static readonly Lazy<Container> _container = new Lazy<Container>((Func<Container>)ConfigureServices);

	public static Container Instance => _container.Value;

	private static Container ConfigureServices()
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		//IL_001b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0022: Expected O, but got Unknown
		Container val = new Container();
		val.Register<SerialService>((Lifestyle)(object)Lifestyle.Singleton);
		val.Register<MainForm>((Lifestyle)(object)Lifestyle.Singleton);
		val.Verify();
		return val;
	}
}
