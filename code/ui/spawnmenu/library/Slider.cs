using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

[UseTemplate]
public class Slider : Panel
{
	public Action<float> OnValueChanged;
	public Action<float> OnFinalValue;
	public Panel Grabber { get; set; }
	public float Min, Max;
	bool Vertical = false;

	float sliderValue;


	public float Value
	{
		get => sliderValue;
		set
		{
			sliderValue = value;
			UpdateUI();
		}
	}

	public Slider()
	{
		OnValueChanged += ( float val ) => CreateEvent( "onValueChanged" );
		OnFinalValue += ( float val ) => CreateEvent( "onFinalValue" );
		BindClass( "active", () => IsHolding || HasHovered );
		BindClass( "vertical", () => Vertical );
	}

	public Slider( float min, float max, bool vertical = false ) : this()
	{
		Vertical = vertical;
		Max = max;
		Min = min;
	}

	public override void SetProperty( string name, string value )
	{
		switch( name )
		{
			case "min":
				Min = float.Parse( value );
				return;
			case "max":
				Max = float.Parse( value );
				return;
			case "vertical":
				Vertical = bool.Parse( value );
				return;
		}

		base.SetProperty( name, value );
	}

	protected void UpdateUI()
	{
		float percentage = (Value - Min ) / (Max - Min);
		if( Vertical ) Grabber.Style.Top = Length.Percent( percentage * 100 );
		else Grabber.Style.Left = Length.Percent( percentage * 100 );
	}

	bool IsHolding;
	public void MouseClicked(bool isClicking)
	{
		IsHolding = isClicking;
		
		if ( !isClicking ) OnFinalValue?.Invoke( Value );
		else MouseMoved();
	}

	public void MouseMoved()
	{
		if ( !IsHolding ) return;
		Vector2 pickerBounds = new( Box.Right - Box.Left, Box.Bottom - Box.Top );
		Vector2 localPos = ScreenPositionToPanelPosition( Mouse.Position ) / pickerBounds;
		if ( Vertical ) Value = Math.Clamp( Min + localPos.y * (Max - Min), Min, Max ); 
		else Value = Math.Clamp(Min + localPos.x * (Max - Min), Min, Max);
		OnValueChanged?.Invoke( Value );
	}
}

public static class SliderCreator
{
	public static Slider Slider( this PanelCreator self, float min, float max, bool vertical = false )
	{
		Slider nSlider = new Slider( min, max, vertical );
		self.panel.AddChild( nSlider );

		return nSlider;
	}
}
