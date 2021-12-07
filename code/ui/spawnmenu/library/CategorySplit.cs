using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

public class CategorySplit : SplitBase
{
	public CategorySplit()
	{
		StyleSheet.Load( "/ui/spawnmenu/library/CategorySplit.scss" );
		BindClass( "bodyHidden", () => CurrentContent is null );
	}	
}
