using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.UI;

[UseTemplate]
public class Slider : Panel
{
	public Action<float> OnValueChanged;
	public Action<float> OnFinalValue;
	public Panel Grabber { get; set; }
	bool Vertical = false;

	float sliderValue;
	float Min, Max;


	public float Value
	{
		get => sliderValue;
		set
		{
			sliderValue = value;
			UpdateUI();
		}
	}

	public Slider( float min, float max, bool vertical = false )
	{
		BindClass( "active", () => IsHolding || HasHovered );
		BindClass( "vertical", () => Vertical );

		Vertical = vertical;
		Max = max;
		Min = min;
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

namespace Sandbox.UI.Construct
{
	public static class SliderCreator 
	{
		public static Slider Slider( this PanelCreator self, float min, float max, bool vertical = false)
		{
			Slider nSlider = new( min, max, vertical );
			self.panel.AddChild( nSlider );

			return nSlider;
		}
	}
}
