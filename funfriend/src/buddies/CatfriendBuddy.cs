namespace funfriend.buddies;

public class CatfriendBuddy : FunfriendBuddy
{
	public override TextureManager.TextureBasket Textures() => new TextureManager.TextureBasket(
		Enumerable.Range(0, 40)
			.Select(i => TextureManager.LoadTexture($"buddies/catfriend_{i:D2}.png"))
			.ToList(), 10);
}