/// <summary>
/// Destroys its GameObject once <see cref="Lifetime"/> seconds have passed.
/// </summary>
public sealed class TimedDestroy : Component
{
	/// <summary>
	/// Seconds before the GameObject is destroyed.
	/// </summary>
	[Property] public float Lifetime { get; set; } = 1f;

	TimeSince TimeSinceCreated { get; set; }

	protected override void OnStart()
	{
		TimeSinceCreated = 0;
	}

	protected override void OnUpdate()
	{
		if ( TimeSinceCreated >= Lifetime )
			GameObject.Destroy();
	}
}
