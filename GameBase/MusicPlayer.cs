using IceThermical.EngineBase;
using Microsoft.Xna.Framework.Media;

namespace IceThermical.GameBase
{
	/// <summary>
	/// General music playing entity, the first data bit is a song title
	/// </summary>
	public class MusicPlayer : MapEntity
	{
		Song song;
		public override void OnSpawned()
		{
			isStatic = true; //this does not need physics at all.

			// Load our song based on our data, set in the map
			// we load this here to help keep all our content loading happen on map load,
			// instead of randomly throughout gameplay.
			song = Engine.instance.Content.Load<Song>("Audio/Music/" + data[0]);
		}

		//Pulsing this entity will cause it to play its music.
		public override void OnPulsed()
		{
			//Play our song.
			MediaPlayer.Play(song);
		}

		public override void OnDestroyed()
		{
			//Dispose our audio file and free up the memory.
			song.Dispose();
		}
	}
}
