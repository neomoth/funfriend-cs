namespace funfriend.buddies;

public enum DialogType
{
	Chatter,
	Moved,
	Touched,
}

public abstract class Buddy
{
	public abstract String Name();
	public abstract List<List<String>> Dialog(DialogType type);
	public abstract TextureManager.TextureBasket Textures();
	public abstract TextureManager.SizedTexture? BgTexture();
	public abstract void TalkSound();
	public abstract String Font();
}