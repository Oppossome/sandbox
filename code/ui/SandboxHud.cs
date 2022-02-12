using Sandbox;
using Sandbox.UI;

[Library]
public partial class SandboxHud : HudEntity<RootPanel>
{
	public SandboxHud()
	{
		SetupHud();		
	}

	[Event.Hotload]
	public void SetupHud()
	{
		if ( !IsClient )
			return;

		RootPanel.BindClass( "camera", () => Local.Pawn is SandboxPlayer ply ? ply.InCameraTool : true );
		RootPanel.StyleSheet.Load( "/ui/SandboxHud.scss" );
		RootPanel.DeleteChildren();

		RootPanel.AddChild<NameTags>();
		RootPanel.AddChild<CrosshairCanvas>();
		RootPanel.AddChild<ChatBox>();
		RootPanel.AddChild<VoiceList>();
		RootPanel.AddChild<KillFeed>();
		RootPanel.AddChild<Playerlist>();
		RootPanel.AddChild<Health>();
		RootPanel.AddChild<InventoryBar>();
		RootPanel.AddChild<CurrentTool>();
		RootPanel.AddChild<SpawnMenu>();
		RootPanel.AddChild<Notifications>();
	}

}
