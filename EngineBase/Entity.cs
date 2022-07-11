using Microsoft.Xna.Framework;
using System;

namespace IceThermical.EngineBase
{
	public class Entity
	{
		public Vector3 position, rotation, forward, right, extents, boxOffset;

		public Vector3 velocity;
		public float gravity;

		protected BoundingBox box;

		public bool grounded = false;
		public bool isStatic = false;

		public virtual void Start() 
		{
			box = new BoundingBox(position-extents+boxOffset,position+extents+boxOffset);

			Engine.instance.boxes.Add(box);

			velocity = Vector3.Zero;
			gravity = 0;
		}
		public virtual void Update(GameTime gt) 
		{
			if(!isStatic)
				RunPhysics(gt);
			//Update pos

			float x = MathF.Sin((rotation.Y) * MathF.PI / 180) * MathF.Cos((rotation.X) * MathF.PI / 180);
			float y = MathF.Sin((-rotation.X) * MathF.PI / 180);
			float z = MathF.Cos((rotation.X) * MathF.PI / 180) * MathF.Cos((rotation.Y) * MathF.PI / 180);

			forward = new Vector3(x, y, z);

			x = MathF.Sin((rotation.Y + 90)*MathF.PI/180) * MathF.Cos((rotation.X) * MathF.PI / 180);
			y = MathF.Sin((-rotation.X) * MathF.PI / 180);
			z = MathF.Cos((rotation.X) * MathF.PI / 180) * MathF.Cos((rotation.Y+90) * MathF.PI / 180);

			right = new Vector3(x, y, z);

			if(!isStatic)
				CollisionCheck(gt);
		}
		public virtual void RunPhysics(GameTime gt)
		{
			gravity += (float)gt.ElapsedGameTime.TotalSeconds * 9;

			//if(position.Y < 0)
			//{
			//	position.Y = 0;
			//	grounded = true;
			//}

			if (grounded)
			{
				gravity = 1f;
				velocity = Vector3.Lerp(velocity, Vector3.Zero, (float)gt.ElapsedGameTime.TotalSeconds * 12);
			}
		}
		public void CollisionCheck(GameTime gt)
		{
			Vector3 pos = (velocity + (Vector3.Down * gravity)) * (float)gt.ElapsedGameTime.TotalSeconds;

			bool hashitground = false;
			foreach (BoundingBox _box in Engine.instance.boxes) //temp, soon to implement blockmap
			{
				if (_box == box) continue; // dont collide with ourselves

				box.Min = (position + new Vector3(pos.X, 0, 0)) - extents + boxOffset; //check the x dir for collision
				box.Max = (position + new Vector3(pos.X, 0, 0)) + extents + boxOffset;

				if (box.Intersects(_box)) //check collision
				{
					if (_box.Max.Y - box.Min.Y < 0.25f) //step up if applicable
					{
						position.Y += MathF.Abs(box.Min.Y - _box.Max.Y);
					}
					else //else stop movement in this direction.
					{
						if(MathF.Abs(box.Max.Y - _box.Min.Y) < 0.1f)
						{
							position.Y += MathF.Abs(box.Max.Y - _box.Min.Y);
						}
						pos.X = 0;
						velocity.X = 0;
					}
				}

				box.Min = (position + new Vector3(0, pos.Y, 0)) - extents - Vector3.Up * 0.2f + boxOffset; //check y dir for collision
				box.Max = (position + new Vector3(0, pos.Y, 0)) + extents + Vector3.Up * 0.2f + boxOffset;

				if (box.Intersects(_box)) //check collision
				{
					grounded = true; //we are touching the ground

					if (pos.Y > 0)
						gravity *= -1;

					hashitground = true;
					pos.Y = 0;
				}
				else if (!hashitground)//there's nothing, we're in the air
				{
					grounded = false;
				}

				box.Min = (position + new Vector3(0, 0, pos.Z)) - extents + boxOffset; // z dir
				box.Max = (position + new Vector3(0, 0, pos.Z)) + extents + boxOffset;

				if (box.Intersects(_box)) // collision
				{
					if (_box.Max.Y - box.Min.Y < 0.25f) // step up
					{
						position.Y += MathF.Abs(box.Min.Y - _box.Max.Y);
					}
					else // stop movement
					{
						pos.Z = 0;
						velocity.Z = 0;
					}
				}
			}
			foreach (OrientedBoundingBox _box in Engine.instance.orientedBoxes) //temp, soon to implement blockmap
			{
				box.Min = (position + new Vector3(pos.X, 0, 0)) - extents + boxOffset; //check the x dir for collision
				box.Max = (position + new Vector3(pos.X, 0, 0)) + extents + boxOffset;

				if (Collision.IsAABBToOBBCollision(box,_box)) //check collision
				{
					box.Min = (position + new Vector3(pos.X, 0.3f, 0)) - extents - Vector3.Up * 0.2f + boxOffset; //check if we can step up
					box.Max = (position + new Vector3(pos.X, 0.3f, 0)) + extents + Vector3.Up * 0.2f + boxOffset;
					if (Collision.IsAABBToOBBCollision(box, _box))
					{
						pos.X = 0;
						velocity.X = 0;
					}
					else
					{
						position.Y += 0.05f;
					}
				}

				box.Min = (position + new Vector3(0, pos.Y, 0)) - extents - Vector3.Up * 0.2f + boxOffset; //check y dir for collision
				box.Max = (position + new Vector3(0, pos.Y, 0)) + extents + Vector3.Up * 0.2f + boxOffset;

				if (Collision.IsAABBToOBBCollision(box, _box)) //check collision
				{
					grounded = true; //we are touching the ground

					if (pos.Y > 0)
						gravity *= -1;

					hashitground = true;
					pos.Y = 0;
				}
				else if (!hashitground)//there's nothing, we're in the air
				{
					grounded = false;
				}

				box.Min = (position + new Vector3(0, 0, pos.Z)) - extents + boxOffset; // z dir
				box.Max = (position + new Vector3(0, 0, pos.Z)) + extents + boxOffset;

				if (Collision.IsAABBToOBBCollision(box, _box)) // collision
				{
					box.Min = (position + new Vector3(0, 0.3f, pos.Z)) - extents - Vector3.Up * 0.2f + boxOffset; //check if we can step up
					box.Max = (position + new Vector3(0, 0.3f, pos.Z)) + extents + Vector3.Up * 0.2f + boxOffset;
					if (Collision.IsAABBToOBBCollision(box, _box))
					{
						pos.Z = 0;
						velocity.Z = 0;
					}else
					{
						position.Y += 0.05f;
					}
				}
			}

			position += pos;
		}
	}
}
