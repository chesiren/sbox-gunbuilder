
/// <summary>
/// A laser pointer .
/// </summary>
public partial class LaserPointerComponent : Component, Component.ExecuteInEditor
{
	/// <summary>
	/// The color of the laser.
	/// </summary>
	[Property] public Color LaserColor { get; set; }

	/// <summary>
	/// Width of the laser line.
	/// </summary>
	[Property] public float LineWidth { get; set; } = 0.05f;

	/// <summary>
	/// Diameter of the laser dot.
	/// </summary>
	[Property] public float DotSize { get; set; } = 0.6f;

	/// <summary>
	/// Renders the laser line.
	/// </summary>
	LineRenderer LineRenderer { get; set; }

	/// <summary>
	/// Renders the laser dot.
	/// </summary>
	LineRenderer DotRenderer { get; set; }

	/// <summary>
	/// Is this thing on? Handles turning on the line and dot.
	/// </summary>
	[Property, Change( nameof( OnIsEnabledChanged ) )] public bool IsEnabled { get; set; } = true;

	/// <summary>
	/// A reference to the grab point that we want to respond to input events for.
	/// </summary>
	[Property] public GrabPoint GrabPoint { get; set; }

	/// <summary>
	/// How long has it been since we toggled the laser pointer?
	/// </summary>
	TimeSince TimeSinceToggled { get; set; } = 1;

	/// <summary>
	/// How far forward do laser pointers go?
	/// </summary>
	const float Dist = 100000;

	/// <summary>
	/// Called whenever IsEnabled changes.
	/// </summary>
	private void OnIsEnabledChanged( bool oldValue, bool newValue )
	{
		if ( Active )
			SetEnabled( newValue );
	}

	/// <summary>
	/// Gets an end position for the trace of the laser pointer.
	/// </summary>
	/// <returns></returns>
	private Vector3 GetTraceEnd()
	{
		var tr = Scene.Trace.Ray( WorldPosition, WorldPosition + ( WorldRotation.Forward * Dist ) ).Run();
		return tr.EndPosition;
	}

	/// <summary>
	/// Creates a hidden, unsaved line renderer used to draw part of the laser.
	/// </summary>
	private LineRenderer CreateRenderer( float width, bool roundCaps )
	{
		var renderer = Components.Create<LineRenderer>();
		renderer.Flags = ComponentFlags.NotSaved | ComponentFlags.Hidden;
		renderer.UseVectorPoints = true;
		renderer.Color = LaserColor;
		renderer.Width = width;
		renderer.Additive = true;
		renderer.Opaque = false;
		renderer.Lighting = false;
		renderer.CastShadows = false;

		if ( roundCaps )
		{
			renderer.StartCap = SceneLineObject.CapStyle.Rounded;
			renderer.EndCap = SceneLineObject.CapStyle.Rounded;
		}

		return renderer;
	}

	/// <summary>
	/// Updates the laser line and dot positions.
	/// </summary>
	private void UpdateRenderers()
	{
		if ( !LineRenderer.IsValid() && !DotRenderer.IsValid() )
			return;

		var end = GetTraceEnd();

		if ( LineRenderer.IsValid() )
		{
			LineRenderer.VectorPoints = new() { WorldPosition, end };
		}

		if ( DotRenderer.IsValid() )
		{
			// A tiny segment with round caps reads as a dot.
			var back = (WorldPosition - end).Normal * 0.01f;
			DotRenderer.VectorPoints = new() { end + back, end + back * 2f };
		}
	}

	/// <summary>
	/// Turns on/off the laser pointer.
	/// </summary>
	/// <param name="enabled"></param>
	public void SetEnabled( bool enabled )
	{
		LineRenderer?.Destroy();
		DotRenderer?.Destroy();
		LineRenderer = null;
		DotRenderer = null;

		if ( !enabled )
			return;

		LineRenderer = CreateRenderer( LineWidth, false );
		DotRenderer = CreateRenderer( DotSize, true );
		UpdateRenderers();
	}

	protected override void OnEnabled()
	{
		SetEnabled( IsEnabled );
	}

	protected override void OnDisabled()
	{
		// Mainly for the editor. Since I want to see them there.
		SetEnabled( false );
	}

	protected override void OnUpdate()
	{
		UpdateRenderers();

		// Grab point input parsing.
		if ( GrabPoint.IsValid() && GrabPoint.HeldHand.IsValid() )
		{
			var controller = GrabPoint.HeldHand.GetController();
			if ( controller.JoystickPress.IsPressed && TimeSinceToggled > 0.5f )
			{
				TimeSinceToggled = 0;

				// Toggling runs OnIsEnabledChanged, which turns the line and dot on/off.
				IsEnabled ^= true;
			}
		}
	}
}
