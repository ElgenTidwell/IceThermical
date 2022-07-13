using IceThermical.EngineBase;
using IceThermical.Map;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
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
		public Player player;
		public Texture2D tex;
		RenderTarget2D baseTarget;
		LoadedITM mapReader;

		bool VisCollision = false;

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
			_graphics.IsFullScreen = false;
			Window.IsBorderless = true;
			_graphics.GraphicsProfile = GraphicsProfile.Reach;

			_graphics.ApplyChanges();

			player = new Player();
			player.Start();

			base.Initialize();
		}

		protected override void LoadContent()
		{
			mapReader = new LoadedITM();
			_spriteBatch = new SpriteBatch(GraphicsDevice);

			mapReader.LoadMap("Maps/map");

			tex = Content.Load<Texture2D>("Textures/missing");
			//Song song = Content.Load<Song>("Audio/Music/bleepbloopy");
			//MediaPlayer.Play(song);
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
			GraphicsDevice.SamplerStates[0] = SamplerState.LinearWrap;

			GraphicsDevice.Clear(new Color(0, 0, 0, 0));

			player.camera.PrepareRender(gameTime,GraphicsDevice);
			//model.Render(player.camera, tex);
			mapReader.RenderMap(player.camera,GraphicsDevice);

			if(VisCollision)
			{
				BasicEffect effect = new BasicEffect(GraphicsDevice);
				effect.World = player.camera.worldMatrix;
				effect.View = player.camera.viewMatrix;
				effect.Projection = player.camera.projectionMatrix;

				List<BoundingBox> tovis = new List<BoundingBox>();
				tovis.Add(player.box);
				tovis.AddRange(boxes);

				foreach (BoundingBox box in tovis)
				{
					VertexPosition[] c = new VertexPosition[8];

					int i = 0;
					foreach(Vector3 vec in box.GetCorners())
					{
						c[i] = new VertexPosition(vec);
						i++;
					}


					//4) Define the vertices that the cube is composed of:
					//I have used 16 vertices (4 vertices per side). 
					//This is because I want the vertices of each side to have separate normals.
					//(so the object renders light/shade correctly) 
					VertexPosition[] verts = new VertexPosition[]
					{
						c[0], c[1], c[2], c[3], // Bottom
						c[7], c[4], c[0], c[3], // Left
						c[4], c[5], c[1], c[0], // Front
						c[6], c[7], c[3], c[2], // Back
						c[5], c[6], c[2], c[1], // Right
						c[7], c[6], c[5], c[4]  // Top
					};

					VertexBuffer buffer = new VertexBuffer(GraphicsDevice,typeof(VertexPosition), verts.Length, BufferUsage.WriteOnly);
					buffer.SetData(verts);

					GraphicsDevice.SetVertexBuffer(buffer);

					foreach(EffectPass pass in effect.CurrentTechnique.Passes)
					{
						pass.Apply();
						GraphicsDevice.DrawPrimitives(PrimitiveType.LineList,0, verts.Length);
					}
				}
			}

			player.RenderViewModel();

			base.Draw(gameTime);
		}
	}
}
