using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.UI;

[UseTemplate]
public class Notification : Panel
{
	public string Text { get; set; } 
	public TimeSince Lifetime;
	public TimeSince Jiggle;
	public string Type;
	public Object Data;

	public Notification(string text, float lifetime, string type = "generic" )
	{
		BindClass( "jiggle", () => Jiggle < 0 );
		Lifetime = -lifetime;
		Text = text;
		Type = type;
	}

	public override void Tick()
	{
		base.Tick();

		if ( Lifetime >= 0 && !IsDeleting )
			Delete();
	}
}
