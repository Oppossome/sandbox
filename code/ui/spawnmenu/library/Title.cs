using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.UI;

public class Title : Label
{
	public Title() {
		StyleSheet.Load( "/ui/spawnmenu/library/Title.scss" );
	}

	public Title( string text ) : this()
	{
		Text = text;
	}

	public override void SetProperty( string name, string value )
	{
		switch ( name )
		{
			case "html":
				Text = value;
				return;
		}

		base.SetProperty( name, value );
	}
}
