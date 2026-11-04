public partial class Weapon
{
	/// <summary>
	/// Color of the bullet tracer line.
	/// </summary>
	[Property, Group( "Effects" )] public Color TracerColor { get; set; } = new Color( 1f, 0.85f, 0.6f, 0.5f );

	/// <summary>
	/// Width of the bullet tracer line.
	/// </summary>
	[Property, Group( "Effects" )] public float TracerWidth { get; set; } = 0.15f;

	/// <summary>
	/// How long the bullet tracer stays visible, in seconds.
	/// </summary>
	[Property, Group( "Effects" )] public float TracerLifetime { get; set; } = 0.05f;

	/// <summary>
	/// Destroys a GameObject after a delay, if it still exists by then.
	/// </summary>
	private static void DestroyAfter( GameObject gameObject, float seconds )
	{
		if ( !gameObject.IsValid() ) return;

		var timer = gameObject.Components.GetOrCreate<TimedDestroy>();
		timer.Lifetime = seconds;
	}

	/// <summary>
	/// Spawns the surface's bullet impact effect, decal and sound at a hit point.
	/// </summary>
	private void CreateImpactEffects( GameObject hitObject, Surface surface, Vector3 pos, Vector3 normal )
	{
		if ( surface is null ) return;

		if ( surface.PrefabCollection.BulletImpact is GameObject impactPrefab )
		{
			var impact = impactPrefab.Clone( new Transform( pos + normal, Rotation.LookAt( normal ) ) );
			DestroyAfter( impact, 5f );
		}

		if ( surface.PrefabCollection.BulletImpactDecal is GameObject decalPrefab )
		{
			var decal = decalPrefab.Clone( new Transform( pos + normal, Rotation.LookAt( -normal ) ) );

			// Follow whatever we hit, so decals stay on moving targets.
			if ( hitObject.IsValid() )
				decal.SetParent( hitObject, true );

			DestroyAfter( decal, 30f );
		}

		if ( surface.SoundCollection.Bullet is SoundEvent impactSound )
		{
			Sound.Play( impactSound, pos );
		}
	}

	/// <summary>
	/// Do shoot effects
	/// </summary>
	protected void DoShootEffects()
	{
		if ( ShootSound is not null )
		{
			if ( Sound.Play( ShootSound, MuzzleGameObject.WorldPosition ) is SoundHandle snd )
			{
				snd.ListenLocal = !IsProxy;
			}
		}
	}

	protected void DoTracer( Vector3 startPosition, Vector3 endPosition, float distance, int count )
	{
		// For when we have bullet penetration implemented.
		if ( count > 0 )
		{
			// Project backward
			Vector3 dir = (startPosition - endPosition).Normal;
			var tr = Scene.Trace.Ray( endPosition, startPosition + (dir * 50f) )
				.Radius( 1f )
				.WithoutTags( "weapon" )
				.Run();

			if ( tr.Hit )
			{
				CreateImpactEffects( tr.GameObject, tr.Surface, tr.StartPosition, dir );
			}
		}

		var origin = count == 0 && MuzzleGameObject.IsValid() ? MuzzleGameObject.WorldPosition : startPosition;

		var gameObject = Scene.CreateObject();
		gameObject.Name = "Tracer";
		gameObject.WorldPosition = origin;

		var line = gameObject.Components.Create<LineRenderer>();
		line.UseVectorPoints = true;
		line.VectorPoints = new() { origin, endPosition };
		line.Color = TracerColor;
		line.Width = TracerWidth;
		line.Additive = true;
		line.Opaque = false;
		line.Lighting = false;
		line.CastShadows = false;

		DestroyAfter( gameObject, TracerLifetime );
	}
}
