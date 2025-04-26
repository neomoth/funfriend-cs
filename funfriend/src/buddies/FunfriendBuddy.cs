namespace funfriend.buddies;

public class FunfriendBuddy : Buddy
{
	private static readonly List<List<string>> ChatterDialog =
	[
		new()
		{
			"HELLO AGAIN",
		},
		new()
		{
			"HI INTERLOPER"
		},
		new()
		{
			"HELLO!", "IS THE AUTH LAYER STILL DISSOCIATED?", "I MISS THEM"
		},
		new()
		{
			"INTERLOPER!", "WELCOME", "BUT ALSO DO NOT BOTHER ME", "VERY BUSY"
		}
	];

	private static readonly List<List<string>> TouchedDialog =
	[
		new()
		{
			"HI INTERLOPER!"
		},
		new()
		{
			"HELLO!"
		},
		new()
		{
			"HI!"
		}
	];
	
	private static readonly List<List<string>> MovedDialog =
	[
		new()
		{
			"OK I'LL BE HERE",
		},
	];
	
	public override string Name() => "FUNFRIEND";

	public override List<List<string>> Dialog(DialogType type)
	{
		return type switch
		{
			DialogType.Chatter => ChatterDialog,
			DialogType.Moved => MovedDialog,
			DialogType.Touched => TouchedDialog,
			_ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
		};
	}

	public override TextureManager.TextureBasket Textures() => new TextureManager.TextureBasket(
		Enumerable.Range(0, 40)
			.Select(i => TextureManager.LoadTexture($"buddies/funfriend_{i:D2}.png"))
			.ToList(), 10);

	public override TextureManager.SizedTexture? BgTexture() => null;

	public override void TalkSound() => SoundManager.PlaySound($"sfx/talk{new Random().Next(1, 9)}.ogg");

	public override string Font() => "fonts/SpaceMono";
}