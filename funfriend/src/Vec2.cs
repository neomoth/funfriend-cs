namespace funfriend;

public struct Vec2
{
	public float X { get; set; }
	public float Y { get; set; }
	
	public static Vec2 Zero => new(0);
	public static Vec2 One => new(1);

	public Vec2(float x, float y)
	{
		X = x;
		Y = y;
	}

	public Vec2(int x, int y) : this((float)x, y) {}

	public Vec2(float xy) : this(xy, xy) {}
	
	public Vec2(int xy) : this(xy, xy) {}
	
	
	
	//swizzle
	public Vec2 Xy() => new Vec2(X, Y);
	public Vec2 Yx() => new Vec2(Y, X);
	
	public Vec2 XyI() => new Vec2((int)X, (int)Y);
	public Vec2 YxI() => new Vec2((int)Y, (int)X);

	public int Xi() => (int)X;
	public int Yi() => (int)Y;

	public float Dot(Vec2 other) => X * other.X + Y * other.Y;

	public Vec2 Cross(Vec2 other) => new Vec2(X * other.Y - Y * other.X, X * other.X + Y * other.Y);

	public float Length() => (float)Math.Sqrt(X * X + Y * Y);
	public float LengthSquared() => X * X + Y * Y;
	
	public float Angle() => MathF.Atan2(Y, X);

	public float AngleTo(Vec2 other)
	{
		float dot = Dot(other);
		float lenProd = Length() * other.Length();
		return lenProd != 0 ? dot / lenProd : 0;
	}

	public Vec2 Clone() => new Vec2(X, Y);

	public void Normalize()
	{
		float len = Length();
		if (len == 0)
		{
			X = 0;
			Y = 0;
			return;
		}
		X /= len;
		Y /= len;
	}

	public Vec2 Normalized()
	{
		float len = Length();
		return len == 0 ? Zero : new Vec2(X / len, Y / len);
	}
	
	public void Scale(float scale)
	{
		Normalize();
		X *= scale;
		Y *= scale;
	}

	public Vec2 Scaled(float scale) => new Vec2(X*scale, Y*scale);
	
	public float Distance(Vec2 other) => (this - other).Length();

	public float SquareDistance(Vec2 other)
	{
		float dx = X - other.X;
		float dy = Y - other.Y;
		return dx * dx + dy * dy;
	}
	
	public static Vec2 FromPolar(float angle, float length) => new Vec2((float)Math.Cos(angle) * length, (float)Math.Sin(angle) * length);
	
	public static Vec2 Rand(float? length = null)
	{
		Random random = new();
		float len = length ?? (float)random.NextDouble();
		float angle = (float)random.NextDouble() * 360;
		float magnitude = (float)random.NextDouble() * len;
		
		return FromPolar(angle, magnitude);
	}
	
	public override string ToString() => $"({X}, {Y})";
	
	public static Vec2 operator +(Vec2 self, Vec2 other) => new Vec2(self.X + other.X, self.Y + other.Y);
	public static Vec2 operator +(Vec2 self, float num) => new Vec2(self.X + num, self.Y + num);
	public static Vec2 operator -(Vec2 self, Vec2 other) => new Vec2(self.X - other.X, self.Y - other.Y);
	public static Vec2 operator -(Vec2 self, float num) => new Vec2(self.X - num, self.Y - num);
	public static Vec2 operator *(Vec2 self, Vec2 other) => new Vec2(self.X * other.X, self.Y * other.Y);
	public static Vec2 operator *(Vec2 self, float scalar) => new Vec2(self.X * scalar, self.Y * scalar);
	public static Vec2 operator /(Vec2 self, Vec2 other) => new Vec2(self.X / other.X, self.Y / other.Y);
	public static Vec2 operator /(Vec2 self, float scalar) => new Vec2(self.X / scalar, self.Y / scalar);
	public static Vec2 operator -(Vec2 self) => new Vec2(-self.X, -self.Y);
	public static bool operator ==(Vec2 self, Vec2 other) => Math.Abs(self.X - other.X) < 0.1f && Math.Abs(self.Y - other.Y) < 0.1f;
	public static bool operator !=(Vec2 self, Vec2 other) => Math.Abs(self.X - other.X) > 0.1f || Math.Abs(self.Y - other.Y) > 0.1f;
}