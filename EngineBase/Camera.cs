using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Text;

namespace IceThermical.EngineBase
{
	public class Camera
	{
		public Vector3 lookTarget;
		public Vector3 camPos, viewOffset;
		public Matrix projectionMatrix;
		public Matrix viewMatrix;
		public Matrix worldMatrix;
		public Vector3 up, forward;

		public BasicEffect basicEffect;
		public BasicEffect transparentVerts;
		BlendState _blendState;

		public void Initialize(GraphicsDevice d)
		{
			lookTarget = new Vector3(0, 0, 0);
			camPos = new Vector3(0, 0.8f, 0);

			projectionMatrix = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(90f), d.DisplayMode.AspectRatio, 0.1f, 300f);
			viewMatrix = Matrix.CreateFromYawPitchRoll(lookTarget.X, lookTarget.Y, lookTarget.Z);
			worldMatrix = Matrix.CreateWorld(Vector3.Zero - viewOffset, Vector3.Forward, Vector3.Up);

			_blendState = new BlendState
			{
				ColorSourceBlend = Blend.One, // multiplier of the source color
				ColorBlendFunction = BlendFunction.Add, // function to combine colors
				ColorDestinationBlend = Blend.InverseSourceAlpha, // multiplier of the destination color
				AlphaSourceBlend = Blend.One, // multiplier of the source alpha
				AlphaBlendFunction = BlendFunction.ReverseSubtract, // function to combine alpha
				AlphaDestinationBlend = Blend.One, // multiplier of the destination alpha
			};
		}
		public void PrepareRender(GameTime gt, GraphicsDevice g)
		{
			worldMatrix = Matrix.CreateWorld(Vector3.Zero - viewOffset, Vector3.Forward, Vector3.Up);
		}
	}
}
