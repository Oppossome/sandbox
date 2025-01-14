﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Tests;

[UseTemplate]
public class ModelSelector : Panel
{
	protected Dictionary<string, Action> callbacks = new();
	public List<string> Models = new();
	public VirtualScrollPanel Canvas;
	public bool IsSingular = true;

	public ModelSelector()
	{
		Canvas = new();
		Canvas.Layout.AutoColumns = true;
		Canvas.Layout.ItemHeight = Length.ViewWidth( 6 ).Value;
		Canvas.Layout.ItemWidth = Length.ViewWidth( 6 ).Value;
		Canvas.AddClass( "canvas" );
		AddChild( Canvas );

		Canvas.OnCreateCell += ( cell, data ) =>
		{
			string modelPath = (string)data;
			var spIcon = new SpawnIcon( GetFileName( modelPath ) );
			spIcon.BindClass("active", () => Models.Contains(modelPath));

			if ( Texture.Load( FileSystem.Mounted, $"{modelPath}_c.png", false) is Texture tx ) spIcon.WithIcon( tx );
			else spIcon.WithRenderedIcon( modelPath );

			cell.AddChild( spIcon );
			spIcon.WithCallback( (bool isLeftClick) =>
			 {
				 if ( IsSingular ) Models.Clear();
				 if ( Models.Contains( modelPath ) ) Models.Remove( modelPath );
				 else Models.Add( modelPath );

				 CreateEvent( "onFinalValue" );
				 if ( callbacks.ContainsKey( modelPath ) )
					 callbacks[modelPath].Invoke();
			 } );
		};
	}

	public override void SetProperty( string name, string value )
	{
		switch ( name )
		{
			case "singular":
				IsSingular = bool.Parse(value);
				return;
		}

		base.SetProperty( name, value );
	}

	public void AddEntry(string modelPath, Action callback )
	{
		callbacks[modelPath] = callback;
		Canvas.AddItem( modelPath );
	}

	private string GetFileName( string name )
	{
		Match fileMatch = Regex.Match( name, @"(\w+)\.\w+$" );
		return fileMatch.Groups[1].Value;
	}
}


namespace Sandbox.UI.Construct
{
	public static class ModelSelectorCreator
	{
		public static ModelSelector ModelSelector( this PanelCreator self )
		{
			ModelSelector modelSelector = new();
			self.panel.AddChild( modelSelector );
			return modelSelector;
		}
	}
}
