using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.UI;

public class Title : Label
{
	public Title( string text )
	{
		StyleSheet.Load( "/ui/spawnmenu/library/Title.scss" );
		Text = text;
	}
}
