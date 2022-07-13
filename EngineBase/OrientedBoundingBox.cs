using Microsoft.Xna.Framework;

namespace IceThermical.EngineBase
{
	public class OrientedBoundingBox
	{
		public Vector3 pos;
		public Vector3 rot;
		public Matrix orientation;
		public Vector3 extents;

		public OrientedBoundingBox(Vector3 p, Vector3 r, float w, float h, float l)
		{
			pos = p;
			rot = r;
			extents = new Vector3(w/2,h/2,l/2);
			UpdateRotation();
		}

		void UpdateRotation()
		{
			orientation = Matrix.CreateRotationX(rot.X) * Matrix.CreateRotationY(rot.Y) * Matrix.CreateRotationZ(rot.Z);
		}
	}
	public class Collision
	{
		public struct Interval { public float min; public float max; }
		public Vector3[] testVertices = new Vector3[8];

		public static Interval GetInterval(BoundingBox AABB,Vector3 axis)
		{
			Vector3[] AABBVertices = new Vector3[8];
			AABBVertices[0] = new Vector3(AABB.Min.X,AABB.Max.Y,AABB.Max.Z);
			AABBVertices[1] = new Vector3(AABB.Min.X,AABB.Max.Y,AABB.Min.Z);
			AABBVertices[2] = new Vector3(AABB.Min.X,AABB.Min.Y,AABB.Max.Z);
			AABBVertices[3] = new Vector3(AABB.Min.X,AABB.Min.Y,AABB.Min.Z);			
			AABBVertices[4] = new Vector3(AABB.Max.X,AABB.Max.Y,AABB.Max.Z);
			AABBVertices[5] = new Vector3(AABB.Max.X,AABB.Max.Y,AABB.Min.Z);
			AABBVertices[6] = new Vector3(AABB.Max.X,AABB.Min.Y,AABB.Max.Z);
			AABBVertices[7] = new Vector3(AABB.Max.X,AABB.Min.Y,AABB.Min.Z);

			Interval result;
			result.min = result.max = Vector3.Dot(axis,AABBVertices[0]);

			for(int i = 0; i < AABBVertices.Length; i ++)
			{
				float projection = Vector3.Dot(axis, AABBVertices[i]);
				if (projection < result.min)
					result.min = projection;
				if (projection > result.max)
					result.max = projection;
			}
			return result;
		}

		public static Interval GetInterval(OrientedBoundingBox OBB, Vector3 axis)
		{
			Vector3[] OBBVertices = new Vector3[8];

			Vector3 C = OBB.pos;
			Vector3 E = OBB.extents;

			Vector3 A0 = OBB.orientation.Right;
			Vector3 A1 = OBB.orientation.Up;
			Vector3 A2 = OBB.orientation.Forward;

			OBBVertices[0] = C + A0 * E.X + A1 * E.Y + A2 * E.Z;
			OBBVertices[1] = C - A0 * E.X + A1 * E.Y + A2 * E.Z;
			OBBVertices[2] = C + A0 * E.X - A1 * E.Y + A2 * E.Z;
			OBBVertices[3] = C + A0 * E.X + A1 * E.Y - A2 * E.Z;
			OBBVertices[4] = C - A0 * E.X - A1 * E.Y - A2 * E.Z;
			OBBVertices[5] = C + A0 * E.X - A1 * E.Y - A2 * E.Z;
			OBBVertices[6] = C - A0 * E.X + A1 * E.Y - A2 * E.Z;
			OBBVertices[7] = C - A0 * E.X - A1 * E.Y + A2 * E.Z;

			Interval result;
			result.min = result.max = Vector3.Dot(axis, OBBVertices[0]);

			for (int i = 0; i < OBBVertices.Length; i++)
			{
				float projection = Vector3.Dot(axis, OBBVertices[i]);
				if (projection < result.min)
					result.min = projection;
				if (projection > result.max)
					result.max = projection;
			}
			return result;
		}

		public static bool OverlapOnAxis(BoundingBox AABB, OrientedBoundingBox OBB, Vector3 axis)
		{
			Interval a = GetInterval(AABB, axis);
			Interval b = GetInterval(OBB, axis);
			return ((b.min <= a.max) && (a.min <= b.max));
		}

		public static bool IsAABBToOBBCollision(BoundingBox AABB, OrientedBoundingBox OBB)
		{
			Vector3[] test = new Vector3[15];

			test[0] = new Vector3(1, 0, 0); //AABB Axis
			test[1] = new Vector3(0, 1, 0);
			test[2] = new Vector3(0, 0, 1);
			test[3] = OBB.orientation.Right; //OBB Axis
			test[4] = OBB.orientation.Up;
			test[5] = OBB.orientation.Forward;

			//Cross Products
			test[6] =  Vector3.Cross(test[0], test[3]);
			test[7] =  Vector3.Cross(test[0], test[4]);
			test[8] =  Vector3.Cross(test[0], test[5]);
			test[9] =  Vector3.Cross(test[1], test[3]);
			test[10] = Vector3.Cross(test[1], test[4]);
			test[11] = Vector3.Cross(test[1], test[5]);
			test[12] = Vector3.Cross(test[2], test[3]);
			test[13] = Vector3.Cross(test[2], test[4]);
			test[14] = Vector3.Cross(test[2], test[5]);

			for (int i = 0; i < 15; i++)
				if (!OverlapOnAxis(AABB, OBB, test[i]))
					return false;
			return true;
		}
	}
}
