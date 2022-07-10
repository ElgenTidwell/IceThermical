using IceThermical.EngineBase;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace IceThermical
{
	public class Engine : Game
	{
		public const int BlockSize = 128;

		public static Engine instance;

		public List<BoundingBox> boxes;
		public List<OrientedBoundingBox> orientedBoxes;

		private GraphicsDeviceManager _graphics;
		private SpriteBatch _spriteBatch;
		private Vector2 rotation;
		private Player player;
		public Texture2D tex;
		RenderTarget2D baseTarget;

		private ITStaticModel model;

		public void AddBox(BoundingBox box)
		{
			boxes.Add(box);
		}

		public Engine()
		{
			orientedBoxes = new List<OrientedBoundingBox>();
			boxes = new List<BoundingBox>();
			_graphics = new GraphicsDeviceManager(this);
			Content.RootDirectory = "Content";
			IsMouseVisible = false;
			Window.AllowAltF4 = true;
			instance = this;
		}

		protected override void Initialize()
		{
			_graphics.PreferredBackBufferWidth = (int)(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width);
			_graphics.PreferredBackBufferHeight = (int)(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height);
			_graphics.PreferredBackBufferFormat = SurfaceFormat.Rgba64;
			_graphics.IsFullScreen = true;
			_graphics.GraphicsProfile = GraphicsProfile.Reach;

			_graphics.ApplyChanges();

			player = new Player();
			player.Start();

			base.Initialize();
		}

		protected override void LoadContent()
		{
			_spriteBatch = new SpriteBatch(GraphicsDevice);

			model = new ITStaticModel();
			model.model = Content.Load<Model>("Models/testlevel");
			model.modelTexture = Content.Load<Texture2D>("Textures/Lightmap_02");
			model.pos = Vector3.Forward*-4;

			OrientedBoundingBox obb = new OrientedBoundingBox(Vector3.Left*6,new Vector3(15 * MathF.PI/180,0,0),20,5,3);
			orientedBoxes.Add(obb);

			model.init();

			tex = Content.Load<Texture2D>("Textures/missing");
		}

		protected override void Update(GameTime gameTime)
		{
			if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
				Exit();

			//MouseState state = Mouse.GetState();

			//Point mouseRelativeToCenter = new Point(state.X - GraphicsDevice.Viewport.Width / 2, state.Y - GraphicsDevice.Viewport.Height / 2);

			//rotation.X += -mouseRelativeToCenter.X * 20f * (float)gameTime.ElapsedGameTime.TotalSeconds;
			//rotation.Y += mouseRelativeToCenter.Y * 20f * (float)gameTime.ElapsedGameTime.TotalSeconds;

			//Mouse.SetPosition(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);

			//rotation.Y = MathF.Max(MathF.Min(rotation.Y, 85f), -85f);

			//float xPos = MathF.Sin(MathHelper.ToRadians(rotation.X)) * MathF.Cos(MathHelper.ToRadians(rotation.Y));
			//float yPos = MathF.Sin(MathHelper.ToRadians(-rotation.Y));
			//float zPos = MathF.Cos(MathHelper.ToRadians(rotation.Y)) * MathF.Cos(MathHelper.ToRadians(rotation.X));

			//cam.forward = new Vector3(xPos, yPos, zPos);
			//cam.up = Vector3.Cross(cam.forward, Vector3.Up);

			//cam.lookTarget = cam.camPos + new Vector3(xPos, yPos, zPos);

			//cam.viewMatrix = Matrix.CreateLookAt(cam.camPos, cam.lookTarget, Vector3.Up);
			player.Update(gameTime);

			base.Update(gameTime);
		}

		protected override void Draw(GameTime gameTime)
		{
			SamplerState ss = new SamplerState();
			ss.Filter = TextureFilter.Linear;
			ss.FilterMode = TextureFilterMode.Comparison;
			GraphicsDevice.SamplerStates[0] = ss;

			GraphicsDevice.Clear(new Color(0, 0, 0, 0));
			player.camera.PrepareRender(gameTime,GraphicsDevice);
			model.Render(player.camera, tex);

			player.RenderViewModel();

			base.Draw(gameTime);
		}
	}
}
