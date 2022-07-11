namespace IceThermical.EngineBase
{
	public abstract class MapEntity : Entity
	{
		public string[] data;
		public abstract void OnSpawned();

		public abstract void OnPulsed();

		public abstract void OnDestroyed();
	}
}
