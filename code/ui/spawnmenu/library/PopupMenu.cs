using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

[UseTemplate]
public class PopupMenu : Panel
{
	public Panel PopupInner { get; set; }
	Panel Caller;


	public PopupMenu(Panel caller)
	{
		Parent = caller.FindRootPanel();
		PopupInner.Style.Left = Mouse.Position.x;
		PopupInner.Style.Top = Mouse.Position.y;
		Caller = caller;
	}

	public void AddButton(string text, Action callback )
	{
		PopupInner.Add.Button( text, () =>
		{
			callback();
			Delete();
		} );
	}

	protected override void OnClick( MousePanelEvent e )
	{
		base.OnClick( e );

		if( !PopupInner.IsInside( Mouse.Position ) )
		{
			Delete();

			Caller.FindRootPanel().CreateEvent( "onmousedown" );
		}
	}

	public override void Tick()
	{
		base.Tick();

		if ( !Caller.IsVisible )
		{
			Delete();
			return;
		}
	}
}
