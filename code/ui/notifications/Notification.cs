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

	public Notification(string text, float lifetime )
	{
		Lifetime = -lifetime;
		Text = text;
	}

	public override void Tick()
	{
		base.Tick();

		if ( Lifetime >= 0 && !IsDeleting )
			Delete();
	}
}
