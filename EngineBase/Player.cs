using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

namespace IceThermical.EngineBase
{
	public class KeyboardIN
	{
		static KeyboardState currentKeyState;
		static KeyboardState previousKeyState;

		public static KeyboardState GetState()
		{
			previousKeyState = currentKeyState;
			currentKeyState = Microsoft.Xna.Framework.Input.Keyboard.GetState();
			return currentKeyState;
		}

		public static bool IsPressed(Keys key)
		{
			return currentKeyState.IsKeyDown(key);
		}

		public static bool HasBeenPressed(Keys key)
		{
			return currentKeyState.IsKeyDown(key) && !previousKeyState.IsKeyDown(key);
		}
	}
	public class Player : Entity
	{
		public Camera camera;
		Vector3 rot;
		bool paused;
		ITStaticModel model;

		Vector3 smoothRot;
		float time;
		Vector3 wishDir;
		bool crouching;

		public override void Start()
		{
			extents = new Vector3(0.25f,1f,0.25f);
			boxOffset = Vector3.Down * 0.3f;
			camera = new Camera();
			camera.Initialize(Engine.instance.GraphicsDevice);
			position = -Vector3.One*100;
			base.Start();
			position = Vector3.One;
			model = new ITStaticModel();
			model.model = Engine.instance.Content.Load<Model>("Models/dumbarm");
			model.Shine = 10f;
			model.ShineScale = 0.0f;
			model.modelTexture = Engine.instance.Content.Load<Texture2D>("Textures/Arms");
			Engine.instance.TargetElapsedTime = TimeSpan.FromSeconds(1d / 250d);
		}
		public override void Update(GameTime gt)
		{
			time += (float)gt.ElapsedGameTime.TotalSeconds*10;
			MouseState state = Mouse.GetState();

			Point mouseRelativeToCenter = new Point(state.X - Engine.instance.GraphicsDevice.Viewport.Width / 2, state.Y - Engine.instance.GraphicsDevice.Viewport.Height / 2);

			if(!paused)
			{
				rot.X += -mouseRelativeToCenter.X * 0.2f;
				rot.Y += mouseRelativeToCenter.Y * 0.2f;

				smoothRot.X += mouseRelativeToCenter.X * 10f * (float)gt.ElapsedGameTime.TotalSeconds;
				smoothRot.Y += mouseRelativeToCenter.Y * 10f * (float)gt.ElapsedGameTime.TotalSeconds;

				Mouse.SetPosition(Engine.instance.GraphicsDevice.Viewport.Width / 2, Engine.instance.GraphicsDevice.Viewport.Height / 2);
			}
			if (KeyboardIN.IsPressed(Keys.OemTilde))
			{
				paused = !paused;
			}

			rot.Y = MathF.Max(MathF.Min(rot.Y, 88f), -88f);

			float xPos = MathF.Sin(MathHelper.ToRadians(rot.X)) * MathF.Cos(MathHelper.ToRadians(rot.Y));
			float yPos = MathF.Sin(MathHelper.ToRadians(-rot.Y));
			float zPos = MathF.Cos(MathHelper.ToRadians(rot.Y)) * MathF.Cos(MathHelper.ToRadians(rot.X));

			camera.forward = new Vector3(xPos, yPos, zPos);
			camera.up = Vector3.Cross(camera.forward, Vector3.Up);

			camera.lookTarget = camera.camPos + new Vector3(xPos, yPos, zPos);

			rotation.Y = rot.X;

			KeyboardIN.GetState();

			var padState = GamePad.GetState(PlayerIndex.One);

			wishDir = Vector3.Zero;

			wishDir.X = padState.ThumbSticks.Left.X * -5;
			wishDir.Z = padState.ThumbSticks.Left.Y * 5;

			rot.X += padState.ThumbSticks.Right.X * -5;
			rot.Y+= padState.ThumbSticks.Right.Y * -5;
			smoothRot.X += padState.ThumbSticks.Right.X * -5;
			smoothRot.Y += padState.ThumbSticks.Right.Y * -5;

			if (KeyboardIN.IsPressed(Keys.W))
			{
				wishDir.Z = 5;
			}
			if (KeyboardIN.IsPressed(Keys.S))
			{
				wishDir.Z = -5;
			}
			if (KeyboardIN.IsPressed(Keys.A))
			{
				wishDir.X = 5;
			}
			if (KeyboardIN.IsPressed(Keys.D))
			{
				wishDir.X = -5;
			}
			if ((KeyboardIN.IsPressed(Keys.Space) || padState.IsButtonDown(Buttons.A)) && grounded)
			{
				gravity = -3f;
				position.Y += 0.04f;
				grounded = false;
			}

			camera.viewMatrix = Matrix.CreateLookAt(camera.camPos, camera.lookTarget, Vector3.Up) * Matrix.CreateFromYawPitchRoll(0, 0, (velocity.Length() * -0.002f) * wishDir.X / 5);

			if (KeyboardIN.IsPressed(Keys.LeftControl) || padState.IsButtonDown(Buttons.B))
			{
				bool needsReconstruction = extents != new Vector3(0.25f,0.25f,0.25f);

				extents = new Vector3(0.25f, 0.4f, 0.25f);
				boxOffset = Vector3.Down * 0.3f;

				if(needsReconstruction)
				{
					box = new BoundingBox(position - extents + boxOffset, position + extents + boxOffset);
				}
				crouching = true;
				camera.camPos = position + Vector3.Up * 0.05f;
			}
			else
			{
				bool needsReconstruction = extents != new Vector3(0.25f, 1f, 0.25f);

				if(CanUnCrouch())
				{
					extents = new Vector3(0.25f, 1f, 0.25f);
					boxOffset = Vector3.Down * 0.3f;

					if (needsReconstruction)
					{
						box = new BoundingBox(position - extents + boxOffset, position + extents + boxOffset);
						position += 0.6f * Vector3.Up;
					}
					crouching = false;
					camera.camPos = position + Vector3.Up * 0.6f;
				}
				else
				{
					needsReconstruction = extents != new Vector3(0.25f, 0.25f, 0.25f);

					extents = new Vector3(0.25f, 0.4f, 0.25f);
					boxOffset = Vector3.Down * 0.3f;

					if (needsReconstruction)
					{
						box = new BoundingBox(position - extents + boxOffset, position + extents + boxOffset);
					}
					crouching = true;
					camera.camPos = position + Vector3.Up * 0.05f;
				}
			}



			wishDir = Vector3.Clamp(wishDir,-Vector3.One*0.2f * (crouching? 0.5f:((KeyboardIN.IsPressed(Keys.LeftShift) || padState.IsButtonDown(Buttons.LeftStick)) ? 2 : 1)),
											Vector3.One*0.2f * (crouching ? 0.5f: ((KeyboardIN.IsPressed(Keys.LeftShift) || padState.IsButtonDown(Buttons.LeftStick)) ? 2 : 1)));



			velocity += (right * wishDir.X + forward * wishDir.Z) / (grounded? 1 : 15);

			base.Update(gt);

			smoothRot = Vector3.Lerp(smoothRot, Vector3.Zero, 18 * (float)gt.ElapsedGameTime.TotalSeconds);
		}
		public void RenderViewModel()
		{
			float xPos = MathF.Sin(MathHelper.ToRadians(rot.X)) * MathF.Cos(MathHelper.ToRadians(rot.Y));
			float yPos = MathF.Sin(MathHelper.ToRadians(-rot.Y));
			float zPos = MathF.Cos(MathHelper.ToRadians(rot.Y)) * MathF.Cos(MathHelper.ToRadians(rot.X));

			var vec = Vector3.Up * -0.3f + Vector3.Forward * MathF.Max(MathF.Min(MathF.Sin(time) * (velocity.Length() * 0.01f), 0.1f),-0.1f) + Vector3.Forward * -(rot.Y / 600);

			model.Render(camera,Engine.instance.tex,Matrix.CreateWorld(vec, Vector3.Forward, Vector3.Up), 
					Matrix.CreateFromYawPitchRoll(0,0, (velocity.Length() * -0.002f) * wishDir.X * -0.1f), new Vector3(-1f, -1f, 0.6f) + new Vector3(xPos,yPos,zPos));
		}
	}
}
